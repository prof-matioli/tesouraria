using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Tesouraria.Application.DTOs;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Tesouraria.Application.Services
{
    public class ExtratoService
    {
        public List<TransacaoExtratoDto> LerArquivoPdf(string caminhoArquivo)
        {
            var listaTransacoes = new List<TransacaoExtratoDto>();

            var regexData = new Regex(@"^(\d{2}/\d{2})");
            var regexAno = new Regex(@"PERÍODO:.*\/(\d{4})");
            int anoCorrente = DateTime.Now.Year;

            using (var pdf = PdfDocument.Open(caminhoArquivo))
            {
                foreach (var page in pdf.GetPages())
                {
                    var words = page.GetWords().ToList();
                    if (!words.Any()) continue;

                    // A. Reconstrução de Linhas
                    var linhasReconstruidas = ReconstruirLinhas(words);

                    // B. Captura do Ano
                    foreach (var l in linhasReconstruidas)
                    {
                        var matchAno = regexAno.Match(l);
                        if (matchAno.Success && int.TryParse(matchAno.Groups[1].Value, out int ano))
                            anoCorrente = ano;
                    }

                    // C. Processamento de Blocos
                    TransacaoExtratoDto? transacaoAtual = null;
                    var linhasDoBloco = new List<string>();

                    foreach (var linha in linhasReconstruidas)
                    {
                        string linhaLimpa = linha.Trim();

                        if (string.IsNullOrWhiteSpace(linhaLimpa)) continue;
                        if (linhaLimpa.StartsWith("SICOOB")) continue;
                        if (linhaLimpa.Contains("EXTRATO CONTA CORRENTE", StringComparison.OrdinalIgnoreCase)) continue;
                        if (linhaLimpa.StartsWith("SALDO", StringComparison.OrdinalIgnoreCase)) continue;

                        var matchData = regexData.Match(linhaLimpa);

                        if (matchData.Success)
                        {
                            if (transacaoAtual != null)
                            {
                                ProcessarBlocoFinal(transacaoAtual, linhasDoBloco);
                                // Só adiciona se conseguiu ler um valor válido (> 0)
                                if (transacaoAtual.Valor > 0)
                                    listaTransacoes.Add(transacaoAtual);

                                transacaoAtual = null;
                            }

                            string restoDaLinha = linhaLimpa.Substring(matchData.Length).Trim();
                            if (restoDaLinha.StartsWith("SALDO", StringComparison.OrdinalIgnoreCase))
                            {
                                linhasDoBloco.Clear();
                                continue;
                            }

                            transacaoAtual = new TransacaoExtratoDto();
                            linhasDoBloco.Clear();

                            string diaMes = matchData.Groups[1].Value;
                            if (DateTime.TryParseExact($"{diaMes}/{anoCorrente}", "dd/MM/yyyy",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataValida))
                            {
                                transacaoAtual.Data = dataValida;
                            }

                            if (!string.IsNullOrEmpty(restoDaLinha)) linhasDoBloco.Add(restoDaLinha);
                        }
                        else if (transacaoAtual != null)
                        {
                            linhasDoBloco.Add(linhaLimpa);
                        }
                    }

                    if (transacaoAtual != null)
                    {
                        ProcessarBlocoFinal(transacaoAtual, linhasDoBloco);
                        if (transacaoAtual.Valor > 0)
                            listaTransacoes.Add(transacaoAtual);
                    }
                }
            }
            return listaTransacoes;
        }

        private List<string> ReconstruirLinhas(List<Word> words)
        {
            var linhas = new List<string>();
            var palavrasOrdenadas = words.OrderByDescending(w => w.BoundingBox.Bottom).ThenBy(w => w.BoundingBox.Left).ToList();

            if (palavrasOrdenadas.Any())
            {
                var linhaAtual = new List<Word> { palavrasOrdenadas[0] };
                double yAtual = palavrasOrdenadas[0].BoundingBox.Bottom;

                for (int i = 1; i < palavrasOrdenadas.Count; i++)
                {
                    var w = palavrasOrdenadas[i];
                    if (Math.Abs(w.BoundingBox.Bottom - yAtual) < 2.0)
                        linhaAtual.Add(w);
                    else
                    {
                        linhas.Add(string.Join(" ", linhaAtual.OrderBy(x => x.BoundingBox.Left).Select(x => x.Text)));
                        linhaAtual = new List<Word> { w };
                        yAtual = w.BoundingBox.Bottom;
                    }
                }
                linhas.Add(string.Join(" ", linhaAtual.OrderBy(x => x.BoundingBox.Left).Select(x => x.Text)));
            }
            return linhas;
        }

        private void ProcessarBlocoFinal(TransacaoExtratoDto t, List<string> linhas)
        {
            var historicoFinal = new List<string>();

            // Regex Ajustado: Aceita ponto ou vírgula antes das 2 casas decimais finais
            // Ex: Aceita 1.000,20 e também 1000.20 e 0.20
            var regexValor = new Regex(@"(\d{1,3}(?:[.,]\d{3})*[.,]\d{2})\s*([CD])\s*$");

            for (int i = 0; i < linhas.Count; i++)
            {
                string linha = linhas[i];

                if (t.Valor == 0)
                {
                    var matchValor = regexValor.Match(linha);
                    if (matchValor.Success)
                    {
                        string valorTexto = matchValor.Groups[1].Value; // Ex: "0.20" ou "1.200,50"
                        string tipo = matchValor.Groups[2].Value;

                        t.Valor = ConverterValorSeguro(valorTexto);
                        t.Tipo = tipo[0];

                        // Remove o valor da linha para não ir para o histórico
                        linha = linha.Substring(0, matchValor.Index).Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(linha) || linha.StartsWith("***")) continue;

                if (linha.StartsWith("DOC.:", StringComparison.OrdinalIgnoreCase))
                {
                    string conteudo = linha.Substring(5).Trim();
                    if (conteudo.Equals("Pix", StringComparison.OrdinalIgnoreCase)) continue;
                    linha = conteudo;
                }
                if (linha.StartsWith("REM.:", StringComparison.OrdinalIgnoreCase))
                    linha = "REM: " + linha.Substring(5).Trim();

                if (!string.IsNullOrWhiteSpace(linha)) historicoFinal.Add(linha);
            }
            t.Historico = string.Join(" | ", historicoFinal).Trim();
        }

        // Método auxiliar robusto para converter "0.20", "0,20", "1.000,00" ou "1,000.00"
        private decimal ConverterValorSeguro(string valorTexto)
        {
            try
            {
                // Verifica qual é o último separador (decimal)
                int lastComma = valorTexto.LastIndexOf(',');
                int lastDot = valorTexto.LastIndexOf('.');

                // Caso 1: Formato Brasileiro (1.200,50 ou 0,20) -> Último separador é vírgula
                if (lastComma > lastDot)
                {
                    // Remove os pontos de milhar e troca vírgula decimal por ponto (para Invariant)
                    string normalizado = valorTexto.Replace(".", "").Replace(",", ".");
                    return decimal.Parse(normalizado, CultureInfo.InvariantCulture);
                }
                // Caso 2: Formato Americano (1,200.50 ou 0.20) -> Último separador é ponto
                else if (lastDot > lastComma)
                {
                    // Remove as vírgulas de milhar
                    string normalizado = valorTexto.Replace(",", "");
                    return decimal.Parse(normalizado, CultureInfo.InvariantCulture);
                }
                // Caso 3: Sem separador (ex: "100")
                else
                {
                    return decimal.Parse(valorTexto, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                return 0m; // Retorna 0 em caso de falha grave
            }
        }
    }
}