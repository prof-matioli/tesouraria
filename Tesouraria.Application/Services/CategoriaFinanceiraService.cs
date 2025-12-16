using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Application.Services
{
    public class CategoriaFinanceiraService : IBaseService<CategoriaFinanceira, CategoriaFinanceiraDTO>
    {
        private readonly ICategoriaFinanceiraRepository _repository;

        public CategoriaFinanceiraService(ICategoriaFinanceiraRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoriaFinanceiraDTO>> GetAllAsync()
        {
            var dados = await _repository.ObterTodosAsync();
            return dados.Select(x => new CategoriaFinanceiraDTO
            {
                Id = x.Id,
                Nome = x.Nome,
                Tipo = x.Tipo,
                DedutivelIR = x.DedutivelIR
            }).ToList();
        }

        public async Task<CategoriaFinanceiraDTO> GetByIdAsync(int id)
        {
            var entity = await _repository.ObterPorIdAsync(id);
            if (entity == null) return null;

            return new CategoriaFinanceiraDTO
            {
                Id = entity.Id,
                Nome = entity.Nome,
                Tipo = entity.Tipo,
                DedutivelIR = entity.DedutivelIR
            };
        }

        public async Task<CategoriaFinanceiraDTO> AddAsync(CategoriaFinanceiraDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new Exception("Nome da categoria é obrigatório.");

            var entity = new CategoriaFinanceira
            {
                Nome = dto.Nome,
                Tipo = dto.Tipo,
                DedutivelIR = dto.DedutivelIR
            };

            await _repository.AdicionarAsync(entity);
            dto.Id = entity.Id;
            return dto;
        }

        public async Task UpdateAsync(CategoriaFinanceiraDTO dto)
        {
            var entity = await _repository.ObterPorIdAsync(dto.Id);
            if (entity == null) throw new Exception("Categoria não encontrada.");

            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new Exception("Nome da categoria é obrigatório.");

            // Atualiza campos
            entity.Nome = dto.Nome;
            entity.Tipo = dto.Tipo;
            entity.DedutivelIR = dto.DedutivelIR;

            await _repository.AtualizarAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.ExcluirAsync(id);
        }
    }
}