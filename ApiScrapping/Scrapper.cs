using System.Net.Http.Headers;
using System.Text;

namespace ApiScrapping
{
    public class Scrapper
    {
        private readonly HttpClient _httpClient;
        public string EnderecoBase { get; }
        public HashSet<string> RotasComErro { get; set; } = new HashSet<string>();
        public bool TemErro => RotasComErro.Count > 0;

        public Scrapper(Config config)
        {
            EnderecoBase = config.EnderecoBase;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(config.EnderecoBase);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + config.Bearer);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Navega(Rota rota)
        {
            Console.WriteLine();
            Console.Write("   Rota: " + rota.Nome);

            HttpResponseMessage resposta;
            if (rota.Tipo == MetodoHttp.GET)
            {
                if (rota.TemId)
                {
                    var rotaSemId = rota.Nome.Substring(0, rota.Nome.Length - 4);
                    if (rota.Id == Guid.Empty)
                    {
                        // Tenta pegar Id através da rota GET
                        string result = _httpClient.GetStringAsync(EnderecoBase + rotaSemId).Result.ToLower();
                        var indexFirstId = result.IndexOf("id");
                        rota.Id = Guid.Parse(result.Substring(indexFirstId + 5, 36));
                    }
                    resposta = _httpClient.GetAsync(_httpClient.BaseAddress + rotaSemId + rota.Id).Result;
                }
                else
                {
                    resposta = _httpClient.GetAsync(_httpClient.BaseAddress + rota.Nome).Result;
                }
            }
            else if (rota.Tipo == MetodoHttp.PATCH)
            {
                var rotaSemId = rota.Nome.Substring(0, rota.Nome.Length - 4);
                if (rota.Id == Guid.Empty)
                {
                    // Tenta pegar Id através da rota GET
                    rota.Id = ObterPrimeiroId(rota);
                }
                var content = new StringContent(rota.Body ?? "", Encoding.UTF8, "application/json");
                resposta = _httpClient.PatchAsync(_httpClient.BaseAddress + rotaSemId + rota.Id, content).Result;
            }
            else
            {
                resposta = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }

            if (resposta.StatusCode != System.Net.HttpStatusCode.OK)
            {
                RotasComErro.Add(rota.Nome);
                Console.WriteLine();
                Console.WriteLine("=> Erro!");
                Console.WriteLine("   Código: " + (int)resposta.StatusCode);
            }
            else
            {
                Console.Write("   ...OK!");
            }

            Console.WriteLine("\n   URI: " + resposta.RequestMessage.Method.ToString() + " " + resposta?.RequestMessage?.RequestUri?.ToString() ?? "");
            if (rota.Body != null)
            {
                Console.WriteLine("   Body: " + rota.Body.ToString());
            }
            Console.WriteLine("---------");
        }

        private Guid ObterPrimeiroId(Rota rota)
        {
            try
            {
                var rotaGet = "/" + rota.Nome.Split('/')[1];
                string result = _httpClient.GetStringAsync(EnderecoBase + rotaGet).Result.ToLower();
                var indexFirstId = result.IndexOf("id");
                return Guid.Parse(result.Substring(indexFirstId + 5, 36));
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro tentando obter Id para a rota '{rota.Nome}'");
            }
        }
    }
}
