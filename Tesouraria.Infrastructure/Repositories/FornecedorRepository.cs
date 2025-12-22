namespace Tesouraria.Infra.Data.Repositories
{
    using Tesouraria.Domain.Entities;
    using Tesouraria.Domain.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Tesouraria.Infrastructure.Data;

    public class FornecedorRepository : IFornecedorRepository
    {
        private readonly AppDbContext _context;

        public FornecedorRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Adicionar(Fornecedor fornecedor)
        {
            _context.Fornecedores.Add(fornecedor);
            //_context.SaveChanges();
            _context.SaveChangesAsync();
        }

        public void Atualizar(Fornecedor fornecedor)
        {
            _context.Fornecedores.Update(fornecedor);
            //_context.SaveChanges();
            _context.SaveChangesAsync();
        }

        public void Excluir(int id)
        {
            var fornecedor = _context.Fornecedores.Find(id);
            if (fornecedor != null)
            {
                _context.Fornecedores.Remove(fornecedor);
                //_context.SaveChanges();
                _context.SaveChangesAsync();
            }
        }

        // LEITURA OTIMIZADA (Para Grids/Listas)
        public IEnumerable<Fornecedor> ObterTodos()
        {
            return _context.Fornecedores
                           .AsNoTracking() // Mais rápido
                           .ToList();
        }

        // LEITURA COMUM (Pode ser usado para editar depois, então sem AsNoTracking)
        public Fornecedor ObterPorId(int id)
        {
            return _context.Fornecedores.Find(id);
        }

        // Útil para verificar duplicidade
        public Fornecedor ObterPorCNPJ(string cnpj)
        {
            return _context.Fornecedores
                           .AsNoTracking()
                           .FirstOrDefault(f => f.CNPJ == cnpj);
        }
    }
}