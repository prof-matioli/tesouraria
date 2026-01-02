namespace Tesouraria.Domain.Enums
{
    public enum PerfilUsuario
    {
        Usuario = 1,        // Consulta e Relatórios básicos
        Secretaria = 2,     // + Cadastros (Fieis/Fornecedores) e Lançamentos
        Tesoureira = 3,     // + Centros de Custo e Categorias Financeiras
        Paroco = 4,         // Equivalente a Tesoureira (semanticamente distinto)
        Admin = 99          // Acesso Total
    }
}