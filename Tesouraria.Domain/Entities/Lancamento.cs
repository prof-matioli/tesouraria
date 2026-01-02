using Tesouraria.Domain.Common;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Domain.Entities
{
    public class Lancamento : BaseEntity
    {
        public string Descricao { get; private set; } = string.Empty;
        public decimal ValorOriginal { get; private set; }
        public decimal ValorPago { get; private set; } // Valor final com juros/multa/desconto
        public DateTime DataVencimento { get; private set; }
        public DateTime? DataPagamento { get; private set; } // Data da Baixa
        public FormaPagamento FormaPagamento { get; set; }

        public string? Observacao { get; private set; }

        public TipoTransacao Tipo { get; private set; }
        public StatusLancamento Status { get; private set; }

        // Relacionamentos
        public int CategoriaId { get; private set; }
        public virtual CategoriaFinanceira Categoria { get; private set; } = null!;

        public int CentroCustoId { get; private set; }
        public virtual CentroCusto CentroCusto { get; private set; } = null!;

        // Quem registrou o lançamento (Auditoria/Rastreabilidade)
        public int UsuarioId { get; private set; }
        public virtual Usuario Usuario { get; private set; } = null!;

        // Opcionais: Vinculação com Fiel (Receita) ou Fornecedor (Despesa)
        public int? FielId { get; private set; }
        public virtual Fiel? Fiel { get; private set; }

        public int? FornecedorId { get; private set; }
        public virtual Fornecedor? Fornecedor { get; private set; }

        // Construtor para EF
        protected Lancamento() { }

        // Factory Method / Construtor rico para garantir consistência
        public Lancamento(
            string descricao,
            decimal valorOriginal,
            DateTime dataVencimento,
            TipoTransacao tipo,
            int categoriaId,
            int centroCustoId,
            int usuarioId,
            int? fielId = null,
            int? fornecedorId = null)
        {
            Descricao = descricao;
            ValorOriginal = valorOriginal;
            DataVencimento = dataVencimento;
            Tipo = tipo;
            Status = StatusLancamento.Pendente;
            CategoriaId = categoriaId;
            CentroCustoId = centroCustoId;
            UsuarioId = usuarioId;
            FielId = fielId;
            FornecedorId = fornecedorId;

            Validar();
        }

        public void EstornarBaixa()
        {
            if (Status != StatusLancamento.Pago)
            {
                throw new InvalidOperationException("Apenas lançamentos pagos podem ser estornados.");
            }

            // Retorna ao estado original
            Status = StatusLancamento.Pendente;
            ValorPago = 0; // Ou null, dependendo de como você mapeou no EF (no Dto usamos null, aqui decimal)
            DataPagamento = null;

            // Opcional: Registrar em log de auditoria que houve um estorno
        }

        public void Baixar(decimal valorPago, DateTime dataPagamento)
        {
            if (Status == StatusLancamento.Cancelado)
                throw new InvalidOperationException("Não é possível baixar um lançamento cancelado.");

            ValorPago = valorPago;
            DataPagamento = dataPagamento;
            Status = StatusLancamento.Pago;
        }

        public void Cancelar()
        {
            if (Status == StatusLancamento.Pago)
                throw new InvalidOperationException("Não é possível cancelar um lançamento já pago/recebido. Realize o estorno.");

            Status = StatusLancamento.Cancelado;
        }

        private void Validar()
        {
            if (string.IsNullOrWhiteSpace(Descricao)) throw new ArgumentException("A descrição é obrigatória.");
            if (ValorOriginal <= 0) throw new ArgumentException("O valor deve ser maior que zero.");
            if (CategoriaId <= 0) throw new ArgumentException("Categoria inválida.");
            if (CentroCustoId <= 0) throw new ArgumentException("Centro de Custo inválido.");
        }

        // Dentro da classe Lancamento
        public void AtualizarDados(
            string descricao,
            decimal valor,
            DateTime dataVencimento,
            FormaPagamento formaPagamento,
            TipoTransacao tipo,
            int categoriaId,
            int centroCustoId,
            int? fielId,
            int? fornecedorId,
            string? observacao)
        {
            // Regra de negócio: Não permitir alterar valor de algo já pago sem estornar antes
            // (Optei por ser flexível aqui, mas poderíamos bloquear se Status == Pago)

            if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("A descrição é obrigatória.");
            if (valor <= 0) throw new ArgumentException("O valor deve ser maior que zero.");

            Descricao = descricao;
            ValorOriginal = valor;
            // Se ainda não foi pago, o ValorPago (se houver lógica de pré-visualização) poderia acompanhar, 
            // mas por segurança mantemos o original atualizado.

            DataVencimento = dataVencimento;
            FormaPagamento = formaPagamento;
            Tipo = tipo;
            CategoriaId = categoriaId;
            CentroCustoId = centroCustoId;
            FielId = fielId;
            FornecedorId = fornecedorId;
            Observacao = observacao;

            // Auditabilidade (se tiver DataAtualizacao na BaseEntity)
            // DataAtualizacao = DateTime.Now; 
        }
    }
}