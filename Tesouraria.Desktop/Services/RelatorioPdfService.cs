using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Entities;
using Tesouraria.Domain.Enums;

namespace Tesouraria.Desktop.Services
{
    public class RelatorioPdfService
    {
        public void GerarPdf(IEnumerable<LancamentoDto> dados, FiltroRelatorioDto filtro, string caminhoArquivo)
        {
            // Cálculos de Totais
            var totalReceitas = dados.Where(x => x.Tipo == TipoTransacao.Receita).Sum(x => x.ValorPago ?? x.ValorOriginal);
            var totalDespesas = dados.Where(x => x.Tipo == TipoTransacao.Despesa).Sum(x => x.ValorPago ?? x.ValorOriginal);
            var saldo = totalReceitas - totalDespesas;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // --- CABEÇALHO ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Paróquia [Nome da Sua Paróquia]").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"Relatório de Fluxo de Caixa").FontSize(14);
                            col.Item().Text($"Período: {filtro.DataInicio:dd/MM/yyyy} a {filtro.DataFim:dd/MM/yyyy}").FontSize(10).Italic();
                            if (filtro.CentroCustoId > 0)
                                col.Item().Text("Filtro: Centro de Custo Específico").FontSize(9);
                        });

                        row.ConstantItem(100).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });

                    // --- CONTEÚDO (TABELA) ---
                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        // Definição das Colunas
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60); // Data
                            columns.RelativeColumn();   // Descrição
                            columns.ConstantColumn(100);// Categoria
                            columns.ConstantColumn(80); // Entrada
                            columns.ConstantColumn(80); // Saída
                        });

                        // Cabeçalho da Tabela
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Data");
                            header.Cell().Element(CellStyle).Text("Descrição / Pessoa");
                            header.Cell().Element(CellStyle).Text("Categoria");
                            header.Cell().Element(CellStyle).AlignRight().Text("Entradas");
                            header.Cell().Element(CellStyle).AlignRight().Text("Saídas");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        // Linhas da Tabela
                        foreach (var item in dados)
                        {
                            var valor = item.ValorPago ?? item.ValorOriginal;
                            var isReceita = item.Tipo == TipoTransacao.Receita;

                            table.Cell().Element(CellStyle).Text(item.DataPagamento?.ToString("dd/MM") ?? item.DataVencimento.ToString("dd/MM"));

                            table.Cell().Element(CellStyle).Column(c =>
                            {
                                c.Item().Text(item.Descricao).SemiBold();
                                if (!string.IsNullOrEmpty(item.PessoaNome))
                                    c.Item().Text(item.PessoaNome).FontSize(8).FontColor(Colors.Grey.Darken1);
                            });

                            table.Cell().Element(CellStyle).Text(item.CategoriaNome);

                            // Coluna Entrada
                            table.Cell().Element(CellStyle).AlignRight().Text(isReceita ? valor.ToString("N2") : "-").FontColor(Colors.Green.Darken2);

                            // Coluna Saída
                            table.Cell().Element(CellStyle).AlignRight().Text(!isReceita ? valor.ToString("N2") : "-").FontColor(Colors.Red.Darken2);

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(5);
                            }
                        }
                    });

                    // --- RODAPÉ COM TOTAIS ---
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("TOTAIS DO PERÍODO").FontSize(12).Bold();

                            row.AutoItem().Column(c =>
                            {
                                c.Item().Text($"Total Entradas: {totalReceitas:C2}").FontColor(Colors.Green.Darken2).AlignRight();
                                c.Item().Text($"Total Saídas: {totalDespesas:C2}").FontColor(Colors.Red.Darken2).AlignRight();
                                c.Item().Text($"SALDO FINAL: {saldo:C2}").FontSize(14).Bold().FontColor(saldo >= 0 ? Colors.Blue.Darken2 : Colors.Red.Darken2).AlignRight();
                            });
                        });

                        col.Item().PaddingTop(20).AlignRight().Text(x =>
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

            // Abre o PDF automaticamente
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(caminhoArquivo) { UseShellExecute = true };
            p.Start();
        }
    }
}