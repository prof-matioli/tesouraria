using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public RelatorioPdfService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void GerarPdf(IEnumerable<LancamentoDto> dados, FiltroRelatorioDto filtro, string caminhoArquivo)
        {
            var nomeEmpresa = _configuration["ConfiguracoesAplicacao:NomeEmpresa"] ?? "Minha Empresa";

            // 1. Agrupamento por Centro de Custo
            var dadosAgrupadosCC = dados
                .GroupBy(x => x.CentroCustoNome)
                .OrderBy(g => g.Key)
                .ToList();

            // 2. Agrupamento por Categoria
            var dadosAgrupadosCategoria = dados
                .GroupBy(x => x.CategoriaNome)
                .OrderBy(g => g.Key)
                .ToList();

            // Totais Gerais
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
                            col.Item().Text(nomeEmpresa).FontSize(16).SemiBold().FontColor(Colors.Blue.Darken3);
                            col.Item().Text("Prestação de Contas (com Saldo Progressivo)").FontSize(12);
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

                    // --- CONTEÚDO PRINCIPAL ---
                    page.Content().PaddingVertical(10).Column(colunaPrincipal =>
                    {
                        // ====================================================================================
                        // PARTE 1: TABELA DETALHADA (Com Saldo Acumulado)
                        // ====================================================================================
                        foreach (var grupo in dadosAgrupadosCC)
                        {
                            // Totais do Grupo (para o totalizador final)
                            var totalReceitaCC = grupo.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago ?? x.ValorOriginal);
                            var totalDespesaCC = grupo.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago ?? x.ValorOriginal);
                            var saldoFinalCC = totalReceitaCC - totalDespesaCC;

                            // Variável de Saldo Progressivo (Zera a cada Centro de Custo)
                            decimal saldoAcumulado = 0;

                            colunaPrincipal.Item().PaddingBottom(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(35); // Venc.
                                    columns.ConstantColumn(35); // Pgto.
                                    columns.RelativeColumn();   // Descrição
                                    columns.ConstantColumn(90); // Categoria
                                    columns.ConstantColumn(60); // Entrada
                                    columns.ConstantColumn(60); // Saída
                                    columns.ConstantColumn(65); // Saldo
                                });

                                table.Header(header =>
                                {
                                    header.Cell().ColumnSpan(7)
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(5)
                                        .Text(t =>
                                        {
                                            t.Span("Centro de Custo: ").Bold();
                                            t.Span(grupo.Key.ToUpper()).Bold().FontColor(Colors.Black);
                                        });

                                    header.Cell().Element(EstiloCabecalhoColuna).Text("Venc.");
                                    header.Cell().Element(EstiloCabecalhoColuna).Text("Pgto.");
                                    header.Cell().Element(EstiloCabecalhoColuna).Text("Descrição / Pessoa");
                                    header.Cell().Element(EstiloCabecalhoColuna).Text("Categoria");
                                    header.Cell().Element(EstiloCabecalhoColuna).AlignRight().Text("Entradas");
                                    header.Cell().Element(EstiloCabecalhoColuna).AlignRight().Text("Saídas");
                                    header.Cell().Element(EstiloCabecalhoColuna).AlignRight().Text("Saldo");
                                });

                                // --- LINHAS DE DADOS ---
                                foreach (var item in grupo.OrderBy(x => x.DataVencimento))
                                {
                                    var valor = item.ValorPago ?? item.ValorOriginal;
                                    var isReceita = item.Tipo == TipoTransacao.Receita;

                                    if (isReceita) saldoAcumulado += valor;
                                    else saldoAcumulado -= valor;

                                    table.Cell().Element(EstiloCelula).Text(item.DataVencimento.ToString("dd/MM"));
                                    table.Cell().Element(EstiloCelula).Text(item.DataPagamento?.ToString("dd/MM") ?? "-").FontColor(Colors.Grey.Darken2);

                                    table.Cell().Element(EstiloCelula).Column(c =>
                                    {
                                        c.Item().Text(item.Descricao).SemiBold();
                                        if (!string.IsNullOrEmpty(item.PessoaNome))
                                            c.Item().Text(item.PessoaNome).FontSize(7).FontColor(Colors.Grey.Darken2);
                                    });

                                    table.Cell().Element(EstiloCelula).Text(item.CategoriaNome).FontSize(7);

                                    table.Cell().Element(EstiloCelula).AlignRight().Text(isReceita ? valor.ToString("N2") : "-").FontColor(Colors.Green.Darken3);
                                    table.Cell().Element(EstiloCelula).AlignRight().Text(!isReceita ? valor.ToString("N2") : "-").FontColor(Colors.Red.Darken3);

                                    table.Cell().Element(EstiloCelula).AlignRight().Text(saldoAcumulado.ToString("N2"))
                                        .SemiBold().FontColor(saldoAcumulado >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                                }

                                // --- LINHA DE TOTALIZAÇÃO (Aparece apenas uma vez no final) ---
                                // Substitui o uso de table.Footer() para não repetir em quebras de página

                                table.Cell().ColumnSpan(6)
                                     .BorderTop(1).BorderColor(Colors.Grey.Medium) // Borda para separar
                                     .AlignRight().PaddingRight(5).PaddingVertical(6)
                                     .Text("Saldo Final do Centro de Custo:").Bold();

                                table.Cell()
                                     .BorderTop(1).BorderColor(Colors.Grey.Medium)
                                     .AlignRight().PaddingVertical(6)
                                     .Text(saldoFinalCC.ToString("N2"))
                                     .ExtraBold().FontSize(10)
                                     .FontColor(saldoFinalCC >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
                            });
                        }

                        // ====================================================================================
                        // PARTE 2: RESUMO POR CENTRO DE CUSTO
                        // ====================================================================================
                        colunaPrincipal.Item().PageBreak();
                        colunaPrincipal.Item().Text("RESUMO POR CENTRO DE CUSTO").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                        colunaPrincipal.Item().PaddingBottom(10).Text("Visão consolidada.").FontSize(10).FontColor(Colors.Grey.Darken1);

                        colunaPrincipal.Item().Table(t =>
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
                                h.Cell().Element(EstiloCabecalhoResumo).Text("Centro de Custo").Bold();
                                h.Cell().Element(EstiloCabecalhoResumo).AlignRight().Text("Receitas").Bold();
                                h.Cell().Element(EstiloCabecalhoResumo).AlignRight().Text("Despesas").Bold();
                                h.Cell().Element(EstiloCabecalhoResumo).AlignRight().Text("Saldo").Bold();
                            });

                            foreach (var g in dadosAgrupadosCC)
                            {
                                var r = g.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago ?? x.ValorOriginal);
                                var d = g.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago ?? x.ValorOriginal);
                                var s = r - d;

                                t.Cell().Element(EstiloCelulaResumo).Text(g.Key.ToUpper());
                                t.Cell().Element(EstiloCelulaResumo).AlignRight().Text(r.ToString("N2"));
                                t.Cell().Element(EstiloCelulaResumo).AlignRight().Text(d.ToString("N2"));
                                t.Cell().Element(EstiloCelulaResumo).AlignRight().Text(s.ToString("N2")).FontColor(s >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                            }

                            // Linha de Total Geral (Movida para o corpo para evitar repetição em quebra de página)
                            t.Cell().ColumnSpan(4).Element(EstiloRodapeResumo).Padding(0); // Espaçador ou linha divisória se necessário

                            t.Cell().Element(EstiloRodapeResumo).Text("TOTAL GERAL");
                            t.Cell().Element(EstiloRodapeResumo).AlignRight().Text(totalGeralReceitas.ToString("C2"));
                            t.Cell().Element(EstiloRodapeResumo).AlignRight().Text(totalGeralDespesas.ToString("C2"));
                            t.Cell().Element(EstiloRodapeResumo).AlignRight().Text(saldoGeral.ToString("C2")).FontColor(saldoGeral >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
                        });

                        // ====================================================================================
                        // PARTE 3: RESUMO POR CATEGORIA
                        // ====================================================================================
                        colunaPrincipal.Item().PageBreak();
                        colunaPrincipal.Item().Text("RESUMO POR CATEGORIA").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                        colunaPrincipal.Item().PaddingBottom(10).Text("Visão consolidada por natureza.").FontSize(10).FontColor(Colors.Grey.Darken1);

                        colunaPrincipal.Item().Table(t =>
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
                                h.Cell().Element(EstiloCabecalhoResumo).Text("Categoria").Bold();
                                h.Cell().Element(EstiloCabecalhoResumo).AlignRight().Text("Receitas").Bold();
                                h.Cell().Element(EstiloCabecalhoResumo).AlignRight().Text("Despesas").Bold();
                                h.Cell().Element(EstiloCabecalhoResumo).AlignRight().Text("Saldo").Bold();
                            });

                            foreach (var g in dadosAgrupadosCategoria)
                            {
                                var r = g.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago ?? x.ValorOriginal);
                                var d = g.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago ?? x.ValorOriginal);
                                var s = r - d;

                                t.Cell().Element(EstiloCelulaResumo).Text(g.Key);
                                t.Cell().Element(EstiloCelulaResumo).AlignRight().Text(r.ToString("N2"));
                                t.Cell().Element(EstiloCelulaResumo).AlignRight().Text(d.ToString("N2"));
                                t.Cell().Element(EstiloCelulaResumo).AlignRight().Text(s.ToString("N2")).FontColor(s >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                            }

                            // Linha de Total Geral
                            t.Cell().Element(EstiloRodapeResumo).Text("TOTAL GERAL");
                            t.Cell().Element(EstiloRodapeResumo).AlignRight().Text(totalGeralReceitas.ToString("C2"));
                            t.Cell().Element(EstiloRodapeResumo).AlignRight().Text(totalGeralDespesas.ToString("C2"));
                            t.Cell().Element(EstiloRodapeResumo).AlignRight().Text(saldoGeral.ToString("C2")).FontColor(saldoGeral >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
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

        // --- HELPERS DE ESTILO ---

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

        static IContainer EstiloCabecalhoResumo(IContainer container)
        {
            return container.BorderBottom(2).BorderColor(Colors.Blue.Darken3).Padding(4);
        }

        static IContainer EstiloCelulaResumo(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4);
        }

        static IContainer EstiloRodapeResumo(IContainer container)
        {
            return container.BorderTop(2).Padding(6).DefaultTextStyle(x => x.ExtraBold().FontSize(11));
        }
    }
}