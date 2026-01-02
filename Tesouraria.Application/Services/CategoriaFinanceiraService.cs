using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;

namespace Tesouraria.Application.Services
{
    public class CategoriaFinanceiraService: IBaseService<CategoriaFinanceira, CategoriaFinanceiraDTO>
    {
        private readonly ICategoriaFinanceiraRepository _repository;

        // Injeção da Interface, não da implementação concreta (Dependency Inversion)
        public CategoriaFinanceiraService(ICategoriaFinanceiraRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoriaFinanceiraDTO>> GetAllAsync()
        {
            var dados = await _repository.GetAllAsync();

            // Mapeamento Entity -> DTO
            return dados.Select(c => new CategoriaFinanceiraDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                Tipo = c.Tipo,
                DedutivelIR = c.DedutivelIR
            }).ToList();
        }

        public async Task UpdateAsync(CategoriaFinanceiraDTO dto)
        {
            // Busca o original para manter o Tracking correto
            var entity = await _repository.GetByIdAsync(dto.Id);
            if (entity == null) throw new Exception("Registro não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new Exception("O Nome da Categoria Financeira é obrigatório.");

            // Atualiza propriedades
            entity.Nome = dto.Nome;
            entity.Tipo = dto.Tipo;

            await _repository.UpdateAsync(entity);
        }

        public Task DeleteAsync(int id)
        {
            return _repository.DeleteAsync(id);
        }

        public async Task<CategoriaFinanceiraDTO> AddAsync(CategoriaFinanceiraDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new Exception("O Nome da Categoria Financeira é obrigatório.");

            var entity = new CategoriaFinanceira
            {
                Nome = dto.Nome,
                Tipo = dto.Tipo,
                DedutivelIR = dto.DedutivelIR   
            };

            await _repository.AddAsync(entity);
            dto.Id = entity.Id; // Atualiza ID gerado
            return dto;
        }


        public async Task<CategoriaFinanceiraDTO?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;

            return new CategoriaFinanceiraDTO
            {
                Id = entity.Id,
                Nome = entity.Nome,
                Tipo = entity.Tipo,
                DedutivelIR = entity.DedutivelIR
            };
        }
    }
}