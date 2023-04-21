namespace ApiScrapping
{
    public class Rota
    {
        public string Nome { get; set; }
        public MetodoHttp Tipo { get; set; }
        public string? Body { get; set; }
        public bool TemId { get; set; }
        public Guid Id { get; set; }

        public Rota(string nome, MetodoHttp tipo = MetodoHttp.GET, bool temId = false, string? id = null)
        {
            Nome = nome;
            Tipo = tipo;
            TemId = temId;
            if (id is not null)
            {
                Id = Guid.Parse(id);
            }
        }
    }

    public enum MetodoHttp
    {
        GET,
        POST,
        PATCH
    }
}
