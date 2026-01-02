using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Application.Services
{
    public class CentroCustoService : IBaseService<CentroCusto, CentroCustoDTO>
    {
        private readonly ICentroCustoRepository _repository;

        public CentroCustoService(ICentroCustoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CentroCustoDTO>> GetAllAsync()
        {
            var dados = await _repository.ObterTodosAsync();

            // Mapeamento Entity -> DTO
            return dados.Select(c => new CentroCustoDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao
            }).ToList();
        }

        public async Task<CentroCustoDTO> GetByIdAsync(int id)
        {
            var entity = await _repository.ObterPorIdAsync(id);
            if (entity == null) return null;

            return new CentroCustoDTO
            {
                Id = entity.Id,
                Nome = entity.Nome,
                Descricao = entity.Descricao
            };
        }

        public async Task<CentroCustoDTO> AddAsync(CentroCustoDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new Exception("O Nome do Centro de Custo é obrigatório.");

            var entity = new CentroCusto
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao
            };

            await _repository.AdicionarAsync(entity);
            dto.Id = entity.Id; // Atualiza ID gerado
            return dto;
        }

        public async Task UpdateAsync(CentroCustoDTO dto)
        {
            // Busca o original para manter o Tracking correto
            var entity = await _repository.ObterPorIdAsync(dto.Id);
            if (entity == null) throw new Exception("Registro não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new Exception("O Nome do Centro de Custo é obrigatório.");

            // Atualiza propriedades
            entity.Nome = dto.Nome;
            entity.Descricao = dto.Descricao;

            await _repository.AtualizarAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.ExcluirAsync(id);
        }
    }
}