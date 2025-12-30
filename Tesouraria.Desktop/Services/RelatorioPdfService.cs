using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;
using System.Diagnostics;
using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Enums;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Desktop.Services
{
    public class RelatorioPdfService
    {
        public void GerarPdf(IEnumerable<LancamentoDto> dados, FiltroRelatorioDto filtro, string caminhoArquivo)
        {
            // Agrupamento dos dados por Centro de Custo
            var dadosAgrupados = dados
                .GroupBy(x => x.CentroCustoNome)
                .OrderBy(g => g.Key)
                .ToList();

            // Totais Gerais para o Resumo Final
            var totalGeralReceitas = dados.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago ?? x.ValorOriginal);
            var totalGeralDespesas = dados.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago ?? x.ValorOriginal);
            var saldoGeral = totalGeralReceitas - totalGeralDespesas;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    // --- CABEÇALHO ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Paróquia São Benedito - Limeira").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken3);
                            col.Item().Text("Prestação de Contas por Centro de Custo").FontSize(12);
                            col.Item().Text($"Período: {filtro.DataInicio:dd/MM/yyyy} a {filtro.DataFim:dd/MM/yyyy}").FontSize(9).Italic();

                            var textoFiltro = filtro.CentroCustoId > 0 ? "Filtro: Centro de Custo Específico" : "Visualização: Todos os Centros de Custo";
                            col.Item().Text(textoFiltro).FontSize(9);
                        });

                        row.ConstantItem(120).AlignRight().Column(col =>
                        {
                            col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                            col.Item().Text($"Total Registros: {dados.Count()}").FontSize(8);
                        });
                    });

                    // --- CONTEÚDO ---
                    page.Content().PaddingVertical(10).Column(colunaPrincipal =>
                    {
                        // 1. Imprime todos os grupos (Centros de Custo)
                        foreach (var grupo in dadosAgrupados)
                        {
                            var subtotalReceita = grupo.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago ?? x.ValorOriginal);
                            var subtotalDespesa = grupo.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago ?? x.ValorOriginal);

                            // Tabela Individual por Centro de Custo
                            colunaPrincipal.Item().PaddingBottom(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40); // Vencimento (NOVO)
                                    columns.ConstantColumn(40); // Pagamento (Antigo "Data")
                                    columns.RelativeColumn();   // Descrição
                                    columns.ConstantColumn(100);// Categoria
                                    columns.ConstantColumn(65); // Entrada (Ajustado larguras)
                                    columns.ConstantColumn(65); // Saída   (Ajustado larguras)
                                });

                                // Cabeçalho da Tabela
                                table.Header(header =>
                                {
                                    // Título do Grupo
                                    header.Cell().ColumnSpan(6) // Ajustado para 6 colunas
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(5)
                                        .Text(t =>
                                        {
                                            t.Span("Centro de Custo: ").Bold();
                                            t.Span(grupo.Key.ToUpper()).Bold().FontColor(Colors.Black);
                                        });

                                    // Títulos das Colunas
                                    header.Cell().Element(EstiloCabecalhoColuna).Text("Venc.");
                                    header.Cell().Element(EstiloCabecalhoColuna).Text("Pgto.");
                                    header.Cell().Element(EstiloCabecalhoColuna).Text("Descrição / Pessoa");
                                    header.Cell().Element(EstiloCabecalhoColuna).Text("Categoria");
                                    header.Cell().Element(EstiloCabecalhoColuna).AlignRight().Text("Entradas");
                                    header.Cell().Element(EstiloCabecalhoColuna).AlignRight().Text("Saídas");
                                });

                                // Itens - ORDENADOS POR DATA DE VENCIMENTO
                                foreach (var item in grupo.OrderBy(x => x.DataVencimento))
                                {
                                    var valor = item.ValorPago ?? item.ValorOriginal;
                                    var isReceita = item.Tipo == TipoTransacao.Receita;

                                    // Coluna Vencimento
                                    table.Cell().Element(EstiloCelula).Text(item.DataVencimento.ToString("dd/MM"));

                                    // Coluna Pagamento
                                    table.Cell().Element(EstiloCelula).Text(item.DataPagamento?.ToString("dd/MM") ?? "-").FontColor(Colors.Grey.Darken2);

                                    table.Cell().Element(EstiloCelula).Column(c =>
                                    {
                                        c.Item().Text(item.Descricao).SemiBold();
                                        if (!string.IsNullOrEmpty(item.PessoaNome))
                                            c.Item().Text(item.PessoaNome).FontSize(7).FontColor(Colors.Grey.Darken2);
                                    });

                                    table.Cell().Element(EstiloCelula).Text(item.CategoriaNome).FontSize(8);
                                    table.Cell().Element(EstiloCelula).AlignRight().Text(isReceita ? valor.ToString("N2") : "-").FontColor(Colors.Green.Darken3);
                                    table.Cell().Element(EstiloCelula).AlignRight().Text(!isReceita ? valor.ToString("N2") : "-").FontColor(Colors.Red.Darken3);
                                }

                                // Rodapé do Grupo (Subtotais)
                                table.Footer(footer =>
                                {
                                    footer.Cell().ColumnSpan(4).AlignRight().PaddingRight(5).PaddingVertical(3).Text("Subtotal do Centro de Custo:").Bold();
                                    footer.Cell().AlignRight().PaddingVertical(3).Text(subtotalReceita.ToString("N2")).Bold().FontColor(Colors.Green.Darken3);
                                    footer.Cell().AlignRight().PaddingVertical(3).Text(subtotalDespesa.ToString("N2")).Bold().FontColor(Colors.Red.Darken3);
                                });
                            });
                        }

                        // 2. QUEBRA DE PÁGINA PARA O RESUMO
                        colunaPrincipal.Item().PageBreak();

                        // --- QUADRO DE RESUMO GERAL ---
                        colunaPrincipal.Item().Text("RESUMO CONSOLIDADO").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                        colunaPrincipal.Item().Text("Visão geral dos saldos por centro de custo no período.").FontSize(10).FontColor(Colors.Grey.Darken1);

                        colunaPrincipal.Item().PaddingTop(10).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.ConstantColumn(90);
                                c.ConstantColumn(90);
                                c.ConstantColumn(90);
                            });

                            t.Header(h =>
                            {
                                h.Cell().BorderBottom(2).BorderColor(Colors.Blue.Darken3).Padding(4).Text("Centro de Custo").Bold();
                                h.Cell().BorderBottom(2).BorderColor(Colors.Blue.Darken3).Padding(4).AlignRight().Text("Receitas").Bold();
                                h.Cell().BorderBottom(2).BorderColor(Colors.Blue.Darken3).Padding(4).AlignRight().Text("Despesas").Bold();
                                h.Cell().BorderBottom(2).BorderColor(Colors.Blue.Darken3).Padding(4).AlignRight().Text("Saldo").Bold();
                            });

                            foreach (var g in dadosAgrupados)
                            {
                                var r = g.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago ?? x.ValorOriginal);
                                var d = g.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago ?? x.ValorOriginal);
                                var s = r - d;

                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(g.Key.ToUpper());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(r.ToString("N2"));
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(d.ToString("N2"));
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(s.ToString("N2"))
                                    .FontColor(s >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                            }

                            // Rodapé do Resumo
                            t.Footer(f =>
                            {
                                f.Cell().BorderTop(2).Padding(6).Text("TOTAL GERAL").ExtraBold().FontSize(11);
                                f.Cell().BorderTop(2).Padding(6).AlignRight().Text(totalGeralReceitas.ToString("C2")).ExtraBold().FontSize(11);
                                f.Cell().BorderTop(2).Padding(6).AlignRight().Text(totalGeralDespesas.ToString("C2")).ExtraBold().FontSize(11);
                                f.Cell().BorderTop(2).Padding(6).AlignRight().Text(saldoGeral.ToString("C2"))
                                    .ExtraBold().FontSize(11)
                                    .FontColor(saldoGeral >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
                            });
                        });
                    });

                    // --- RODAPÉ DA PÁGINA ---
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).AlignRight().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                });
            })
            .GeneratePdf(caminhoArquivo);

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(caminhoArquivo) { UseShellExecute = true };
            p.Start();
        }

        static IContainer EstiloCabecalhoColuna(IContainer container)
        {
            return container.DefaultTextStyle(x => x.SemiBold().FontSize(8))
                            .PaddingVertical(2)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Medium);
        }

        static IContainer EstiloCelula(IContainer container)
        {
            return container.BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten4)
                            .PaddingVertical(4);
        }
    }
}