namespace Tesouraria.Domain.Interfaces
{
    using Tesouraria.Domain.Entities;
    using System.Collections.Generic;

    public interface IFornecedorRepository
    {
        void Adicionar(Fornecedor fornecedor);
        void Atualizar(Fornecedor fornecedor);
        void Excluir(int id);

        Fornecedor ObterPorId(int id);
        IEnumerable<Fornecedor> ObterTodos();

        // Método extra útil para validação
        Fornecedor ObterPorCNPJ(string cnpj);
    }
}