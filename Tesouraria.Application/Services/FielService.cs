using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces; // Onde está IBaseService
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Interfaces;      // Onde está IFielRepository

namespace Tesouraria.Application.Services
{
    public class FielService : IBaseService<Fiel, FielDTO>
    {
        private readonly IFielRepository _repository;

        // Injeção de Dependência do Repositório
        public FielService(IFielRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<FielDTO>> GetAllAsync()
        {
            var fieis = await _repository.ObterTodosAsync();

            // Mapeamento Manual (Entity -> DTO)
            // Em projetos maiores, usamos AutoMapper aqui.
            return fieis.Select(f => new FielDTO
            {
                Id = f.Id,
                Nome = f.Nome,
                CPF = f.CPF,
                DataNascimento = f.DataNascimento,
                Dizimista = f.Dizimista,
                Endereco = f.Endereco,
                Email = f.Email,
                Telefone = f.Telefone
                // Mapeie outros campos necessários
            }).ToList();
        }

        public async Task<FielDTO> GetByIdAsync(int id)
        {
            var f = await _repository.ObterPorIdAsync(id);

            if (f == null) return null;

            return new FielDTO
            {
                Id = f.Id,
                Nome = f.Nome,
                CPF = f.CPF,
                DataNascimento = f.DataNascimento
            };
        }

        public async Task<FielDTO> AddAsync(FielDTO dto)
        {
            // 1. Validação de Negócio
            if (string.IsNullOrEmpty(dto.Nome))
                throw new Exception("O nome do fiel é obrigatório.");

            // 2. Mapeamento (DTO -> Entity)
            var novoFiel = new Fiel
            {
                Nome = dto.Nome,
                CPF = dto.CPF,
                DataNascimento = dto.DataNascimento
            };

            // 3. Persistência
            await _repository.AdicionarAsync(novoFiel);

            // Retorna o DTO atualizado (com o ID gerado pelo banco)
            dto.Id = novoFiel.Id;
            return dto;
        }

        public async Task UpdateAsync(FielDTO dto)
        {
            // --- CORREÇÃO DO ERRO DE TRACKING ---

            // Passo 1: Buscar a entidade ORIGINAL que o EF Core está rastreando (ou trazer do banco)
            var fielBanco = await _repository.ObterPorIdAsync(dto.Id);

            if (fielBanco == null)
                throw new Exception("Fiel não encontrado para edição.");

            // Passo 2: Atualizar APENAS os campos, sem substituir o objeto inteiro
            fielBanco.Nome = dto.Nome;
            fielBanco.CPF = dto.CPF;
            fielBanco.DataNascimento = dto.DataNascimento;

            // Passo 3: Salvar (O Repositório apenas dá SaveChanges)
            await _repository.AtualizarAsync(fielBanco);
        }

        public async Task DeleteAsync(int id)
        {
            var fiel = await _repository.ObterPorIdAsync(id);
            if (fiel == null) throw new Exception("Fiel não encontrado.");

            await _repository.RemoverAsync(id);
        }
    }
}