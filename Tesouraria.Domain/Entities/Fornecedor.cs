namespace Tesouraria.Domain.Entities
{
    public class Fornecedor
    {
        public int Id { get; set; }

        public string RazaoSocial { get; set; }  // Nome oficial
        public string NomeFantasia { get; set; } // Nome comercial
        public string CNPJ { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }

        public bool Ativo { get; set; } = true;

        public Fornecedor() { }

        public Fornecedor(string razaoSocial, string cnpj, string email)
        {
            RazaoSocial = razaoSocial;
            CNPJ = cnpj;
            Email = email;
        }
    }
}