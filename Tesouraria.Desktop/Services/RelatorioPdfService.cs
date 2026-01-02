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

        // Injeção de dependência para acessar o appsettings
        public RelatorioPdfService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void GerarPdf(IEnumerable<LancamentoDto> dados, FiltroRelatorioDto filtro, string caminhoArquivo)
        {
            // Ler o nome do arquivo de configuração. Se não achar, usa um valor padrão.
            var nomeEmpresa = _configuration["ConfiguracoesAplicacao:NomeEmpresa"]
                              ?? "Minha Empresa";

            // 1. Agrupamento por Centro de Custo (para o detalhamento e primeiro resumo)
            var dadosAgrupadosCC = dados
                .GroupBy(x => x.CentroCustoNome)
                .OrderBy(g => g.Key)
                .ToList();

            // 2. Agrupamento por Categoria (para o novo resumo)
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
                            col.Item().Text("Prestação de Contas").FontSize(12);
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
                        // PARTE 1: TABELA DETALHADA (Agrupada por Centro de Custo)
                        // ====================================================================================
                        foreach (var grupo in dadosAgrupadosCC)
                        {
                            var subtotalReceita = grupo.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago ?? x.ValorOriginal);
                            var subtotalDespesa = grupo.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago ?? x.ValorOriginal);

                            colunaPrincipal.Item().PaddingBottom(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40); // Venc.
                                    columns.ConstantColumn(40); // Pgto.
                                    columns.RelativeColumn();   // Descrição
                                    columns.ConstantColumn(100);// Categoria
                                    columns.ConstantColumn(65); // Entrada
                                    columns.ConstantColumn(65); // Saída
                                });

                                table.Header(header =>
                                {
                                    header.Cell().ColumnSpan(6)
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
                                });

                                foreach (var item in grupo.OrderBy(x => x.DataVencimento))
                                {
                                    var valor = item.ValorPago ?? item.ValorOriginal;
                                    var isReceita = item.Tipo == TipoTransacao.Receita;

                                    table.Cell().Element(EstiloCelula).Text(item.DataVencimento.ToString("dd/MM"));
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

                                table.Footer(footer =>
                                {
                                    footer.Cell().ColumnSpan(4).AlignRight().PaddingRight(5).PaddingVertical(3).Text("Subtotal do Centro de Custo:").Bold();
                                    footer.Cell().AlignRight().PaddingVertical(3).Text(subtotalReceita.ToString("N2")).Bold().FontColor(Colors.Green.Darken3);
                                    footer.Cell().AlignRight().PaddingVertical(3).Text(subtotalDespesa.ToString("N2")).Bold().FontColor(Colors.Red.Darken3);
                                });
                            });
                        }

                        // ====================================================================================
                        // PARTE 2: RESUMO POR CENTRO DE CUSTO (Nova Página)
                        // ====================================================================================
                        colunaPrincipal.Item().PageBreak();

                        colunaPrincipal.Item().Text("RESUMO POR CENTRO DE CUSTO").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                        colunaPrincipal.Item().PaddingBottom(10).Text("Visão consolidada dos saldos agrupados por centro de custo.").FontSize(10).FontColor(Colors.Grey.Darken1);

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
                                t.Cell().Element(EstiloCelulaResumo).AlignRight().Text(s.ToString("N2"))
                                    .FontColor(s >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                            }

                            t.Footer(f =>
                            {
                                f.Cell().Element(EstiloRodapeResumo).Text("TOTAL GERAL");
                                f.Cell().Element(EstiloRodapeResumo).AlignRight().Text(totalGeralReceitas.ToString("C2"));
                                f.Cell().Element(EstiloRodapeResumo).AlignRight().Text(totalGeralDespesas.ToString("C2"));
                                f.Cell().Element(EstiloRodapeResumo).AlignRight().Text(saldoGeral.ToString("C2"))
                                    .FontColor(saldoGeral >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2);
                            });
                        });

                        // ====================================================================================
                        // PARTE 3: RESUMO POR CATEGORIA FINANCEIRA (Nova Página)
                        // ====================================================================================
                        colunaPrincipal.Item().PageBreak();

                        colunaPrincipal.Item().Text("RESUMO POR CATEGORIA FINANCEIRA").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                        colunaPrincipal.Item().PaddingBottom(10).Text("Visão consolidada dos saldos agrupados por natureza (categoria).").FontSize(10).FontColor(Colors.Grey.Darken1);

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
                                t.Cell().Element(EstiloCelulaResumo).AlignRight().Text(s.ToString("N2"))
                                    .FontColor(s >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                            }

                            // O Rodapé (Totais) é o mesmo, pois a soma das categorias deve bater com a soma dos centros de custo
                            t.Footer(f =>
                            {
                                f.Cell().Element(EstiloRodapeResumo).Text("TOTAL GERAL");
                                f.Cell().Element(EstiloRodapeResumo).AlignRight().Text(totalGeralReceitas.ToString("C2"));
                                f.Cell().Element(EstiloRodapeResumo).AlignRight().Text(totalGeralDespesas.ToString("C2"));
                                f.Cell().Element(EstiloRodapeResumo).AlignRight().Text(saldoGeral.ToString("C2"))
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

        // --- HELPERS DE ESTILO (Refatorado para limpar o código principal) ---

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

        // Estilos específicos para as tabelas de Resumo
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