namespace ApiScrapping
{
    public class Config
    {
        private const string VerifiqueConfig = "Verifique arquivo de configuralção";

        public string? Bearer { get; set; }
        public string? EnderecoBase { get; set; }
        public List<Rota> Rotas { get; set; } = new List<Rota>();

        private bool _temBearer = false;
        private bool _temEnderecoBase = false;
        private bool _temRotas = false;

        public Config(string[] args)
        {
            if (File.Exists("config.txt"))
                LeArquivoConfig();

            if (!_temBearer)
                ObtemBearer(args);

            if (!_temEnderecoBase)
                ObtemEnderecoBase();

            if (!_temRotas)
                ObtemRotas();
        }

        private void LeArquivoConfig()
        {
            var arquivo = File.ReadLines("config.txt");

            if (arquivo == null)
                return;

            var arq = arquivo.ToList();
            for (var l = 0; l < arq.Count; l++)
            {
                var linha = arq[l].Trim();
                if (linha == null || linha.Length == 0 || linha[0] == '#')
                    continue;

                if (VerificaJsonBody(arq, ref l))
                    continue;

                AtribuiParametros(linha);
            }
        }

        private void AtribuiParametros(string linha)
        {
            var words = linha.Split(':');
            var param = words[0].Trim();
            var value = words[1].Trim();
            for (int i = 2; i < words.Length; i++)
            {
                value += ":" + words[i].Trim();
            }

            if (param.Length > 1)
            {
                if (param.ToLower() == "enderecobase")
                {
                    EnderecoBase = value;
                    _temEnderecoBase = true;
                }
                if (param.ToLower() == "rota")
                {
                    Rota r = GetRota(value);
                    Rotas.Add(r);

                }
                if (param.ToLower() == "bearer")
                {
                    Bearer = value;
                    _temBearer = true;
                }
            }
        }

        private bool VerificaJsonBody(List<string> arq, ref int l)
        {
            try
            {
                if (l + 1 == arq.Count)
                    return false;

                var proxLinha = arq[l + 1].Trim();
                if (proxLinha == null || proxLinha.Length == 0 || proxLinha[0] != '{')
                    return false;

                var linha = arq[l].Trim();
                AtribuiParametros(linha);

                string body = "";
                bool primeiraVisitaJson = true;
                for (; ; )
                {
                    l++;

                    linha = arq[l].Trim();   // '{'

                    if (primeiraVisitaJson)
                    {
                        body = "";   // reseta body
                        primeiraVisitaJson = false;
                    }

                    if (linha[0] == '}')
                    {
                        body += '}';
                        Rotas.Last().Body = body;
                        return true;
                    }

                    body += TrataLinhaJson(linha);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Erro buscando corpo JSON. {VerifiqueConfig}\nMensagem: {e.Message}");
            }
        }

        private string TrataLinhaJson(string linha)
        {
            // Substitui " por \" na string
            char str = '\"';
            if (!linha.Contains(str))
                return linha;
            char barra = '\\';
            var subst = linha.Replace($"{str}", $"{barra}{str}");
            return subst;
        }

        private Rota GetRota(string value)
        {
            string recurso;
            MetodoHttp metodo;
            try
            {
                metodo = GetMetodoHttp(value);
                recurso = value.Split(" ")[1].Trim();
                _temRotas = true;
                bool temId = recurso.Contains("id");
                if (temId)
                {
                    var id = value.Split(" ")[2].Trim();
                    return new Rota(recurso, metodo, temId, id);
                }
                else
                {
                    return new Rota(recurso, metodo, temId);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Erro lendo rotas. {VerifiqueConfig}.\nMensagem: {e.Message}");
            }
        }

        private MetodoHttp GetMetodoHttp(string value)
        {
            var tipo = value.Split(" ")[0].Trim();
            if (tipo.ToLower() == "get") return MetodoHttp.GET;
            if (tipo.ToLower() == "post") return MetodoHttp.POST;
            if (tipo.ToLower() == "patch") return MetodoHttp.PATCH;
            throw new Exception($"Erro obtendo método HTTP da rota. {VerifiqueConfig}");
        }

        private void ObtemBearer(string[] args)
        {
            string? bearer;
            if (args.Length == 0)
            {
                Console.WriteLine("Cole o Bearer token:");
                bearer = Console.ReadLine();
            }
            else
            {
                bearer = args[0];
            }
            Bearer = bearer;
        }
        private void ObtemEnderecoBase()
        {
            EnderecoBase = "https://localhost:5001/assistencia-core-netcore";
        }
        private void ObtemRotas()
        {
            Rotas = new List<Rota>
            {
                new Rota("/casos"),
                new Rota("/casos/{id}", MetodoHttp.GET, true),
            };
        }
    }
}
