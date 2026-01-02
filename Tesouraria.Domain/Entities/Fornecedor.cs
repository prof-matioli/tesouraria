using Tesouraria.Domain.Common; // Importante

namespace Tesouraria.Domain.Entities
{
    // ALTERAÇÃO: Adicionado herança de BaseEntity
    public class Fornecedor : BaseEntity
    {
        // REMOVA a propriedade Id, pois ela já existe na BaseEntity
        // public int Id { get; set; } <--- APAGAR ISSO

        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;

        //public bool Ativo { get; set; } = true;

        public Fornecedor() { }

        public Fornecedor(string razaoSocial, string cnpj, string email)
        {
            RazaoSocial = razaoSocial;
            CNPJ = cnpj;
            Email = email;
            DataCriacao = DateTime.Now; // Garantir data de criação
        }
    }
}