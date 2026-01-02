namespace Tesouraria.Application.Services
{
    using Tesouraria.Domain.Entities;
    using Tesouraria.Domain.Interfaces;
    using System;

    public class FornecedorService
    {
        private readonly IFornecedorRepository _repo;

        public FornecedorService(IFornecedorRepository repo)
        {
            _repo = repo;
        }

        // --- CREATE ---
        public void CadastrarFornecedor(Fornecedor fornecedor)
        {
            // 1. Validação de campos obrigatórios
            if (string.IsNullOrEmpty(fornecedor.RazaoSocial))
                throw new Exception("A Razão Social é obrigatória.");

            if (string.IsNullOrEmpty(fornecedor.CNPJ))
                throw new Exception("O CNPJ é obrigatório.");

            // 2. Regra de Negócio: Não permitir CNPJ duplicado
            var existente = _repo.ObterPorCNPJ(fornecedor.CNPJ);
            if (existente != null)
                throw new Exception("Já existe um fornecedor cadastrado com este CNPJ.");

            // 3. (Opcional) Validar se o CNPJ é matematicamente válido
            // if (!ValidarCNPJ(fornecedor.CNPJ)) throw...

            _repo.Adicionar(fornecedor);
        }

        // --- READ ---
        public IEnumerable<Fornecedor> ListarTodos()
        {
            return _repo.ObterTodos();
        }

        public Fornecedor BuscarParaEdicao(int id)
        {
            return _repo.ObterPorId(id);
        }

        // --- UPDATE ---
        public void EditarFornecedor(Fornecedor fornecedorEditado)
        {
            // 1. Verificar existência
            var fornecedorBanco = _repo.ObterPorId(fornecedorEditado.Id);
            if (fornecedorBanco == null)
                throw new Exception("Fornecedor não encontrado.");

            // 2. ATUALIZAÇÃO SEGURA (Transferência de valores)
            // Pegamos os valores que vieram da tela e jogamos no objeto rastreado pelo EF.

            fornecedorBanco.RazaoSocial = fornecedorEditado.RazaoSocial;
            fornecedorBanco.NomeFantasia = fornecedorEditado.NomeFantasia; // Se tiver
            fornecedorBanco.CNPJ = fornecedorEditado.CNPJ;
            fornecedorBanco.Email = fornecedorEditado.Email;
            fornecedorBanco.Telefone = fornecedorEditado.Telefone;

            // 2. Validação de Duplicidade no Update
            // CUIDADO: O usuário pode estar editando o próprio registro.
            // Só é erro se o CNPJ pertencer a OUTRO fornecedor (ID diferente).
            var cnpjExistente = _repo.ObterPorCNPJ(fornecedorEditado.CNPJ);

            if (cnpjExistente != null && cnpjExistente.Id != fornecedorEditado.Id)
            {
                throw new Exception("Este CNPJ já está sendo usado por outro fornecedor.");
            }

            // 3. Persistir
            // Como buscamos 'fornecedorBanco' (com tracking), podemos atualizar suas propriedades
            // ou mandar o objeto editado direto pro Update.

            _repo.Atualizar(fornecedorBanco);
        }

        // --- DELETE ---
        public void RemoverFornecedor(int id)
        {
            // Regra extra: Talvez não possa excluir se tiver produtos vinculados
            // if (_produtoRepo.ExisteProdutoDeFornecedor(id)) throw new Exception(...)

            _repo.Excluir(id);
        }
    }
}