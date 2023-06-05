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
            string uri;
            if (rota.Tipo == MetodoHttp.GET)
            {
                if (rota.TemId)
                {
                    var rotaSemId = rota.Nome.Substring(0, rota.Nome.Length - 4);
                    if (rota.Id == Guid.Empty)
                    {
                        // Tenta pegar Id através da rota GET
                        uri = TratarUri(rotaSemId);
                        string result = _httpClient.GetStringAsync(uri).Result.ToLower();
                        var indexFirstId = result.IndexOf("id");
                        rota.Id = Guid.Parse(result.Substring(indexFirstId + 5, 36));
                    }
                    uri = TratarUri(rotaSemId + rota.Id);
                    resposta = _httpClient.GetAsync(uri).Result;
                }
                else
                {
                    uri = TratarUri(rota.Nome);
                    resposta = _httpClient.GetAsync(uri).Result;
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
                uri = TratarUri(rotaSemId + rota.Id);
                resposta = _httpClient.PatchAsync(uri, content).Result;
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
            Console.WriteLine("   ------");
        }

        private string TratarUri(string recurso)
        {
            string enderecoBase = _httpClient.BaseAddress.ToString();
            if (_httpClient.BaseAddress!.ToString().Last() == '/')
                enderecoBase = enderecoBase.Remove(enderecoBase.Length - 1);
            if (recurso[0] != '/')
                recurso = '/' + recurso;
            return enderecoBase + recurso;
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
                throw new Exception($"Erro tentando obter Id para a rota: '{rota.Nome}'.");
            }
        }
    }
}
