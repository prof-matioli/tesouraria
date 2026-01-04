using System;
using System.IO;
using System.Windows;
using Microsoft.Win32; // Necessário para o SaveFileDialog

namespace Tesouraria.Application.Services
{
    public class BackupService
    {
        // Nome do arquivo de banco de dados definido na ConnectionString
        private const string NomeBanco = "tesouraria.db";

        public void RealizarBackup()
        {
            try
            {
                // 1. Localiza o arquivo do banco na pasta do executável
                string pastaOrigem = AppDomain.CurrentDomain.BaseDirectory;
                string caminhoOrigem = Path.Combine(pastaOrigem, NomeBanco);

                if (!File.Exists(caminhoOrigem))
                {
                    MessageBox.Show("Arquivo de banco de dados não encontrado!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 2. Abre a janela para o usuário escolher onde salvar
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Escolha onde salvar o Backup",
                    Filter = "Arquivo de Banco de Dados SQLite (*.db)|*.db",
                    FileName = $"Backup_Tesouraria_{DateTime.Now:yyyy-MM-dd_HH-mm}.db" // Sugere um nome com data/hora
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string caminhoDestino = saveFileDialog.FileName;

                    // 3. Faz a cópia (O SQLite permite copiar mesmo em uso, mas o ideal é garantir que não haja gravação pesada no momento)
                    File.Copy(caminhoOrigem, caminhoDestino, true);

                    MessageBox.Show($"Backup realizado com sucesso!\nLocal: {caminhoDestino}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha ao realizar backup: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // NOVO MÉTODO: Backup Automático
        public void RealizarBackupAutomatico()
        {
            try
            {
                string pastaOrigem = AppDomain.CurrentDomain.BaseDirectory;
                string caminhoOrigem = Path.Combine(pastaOrigem, NomeBanco);

                if (!File.Exists(caminhoOrigem)) return;

                // --- 1. Tenta descobrir a pasta do OneDrive ou Google Drive ---
                string pastaDestino = ObterPastaNuvem();

                if (string.IsNullOrEmpty(pastaDestino) || !Directory.Exists(pastaDestino))
                {
                    // Se não achar nuvem, não faz nada ou salva em uma pasta "Backups" local
                    return;
                }

                // Cria uma subpasta "Tesouraria_Backups" para organizar
                string pastaBackupFinal = Path.Combine(pastaDestino, "Tesouraria_Backups");
                if (!Directory.Exists(pastaBackupFinal))
                    Directory.CreateDirectory(pastaBackupFinal);

                // --- 2. Define o nome do arquivo (com data e hora) ---
                string nomeArquivo = $"AutoBackup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db";
                string caminhoDestino = Path.Combine(pastaBackupFinal, nomeArquivo);

                // --- 3. Copia o arquivo ---
                File.Copy(caminhoOrigem, caminhoDestino, true);

                // Opcional: Limpeza de backups muito antigos (ex: manter apenas os últimos 30 dias)
                LimparBackupsAntigos(pastaBackupFinal);
            }
            catch (Exception)
            {
                // Em backup automático silencioso, geralmente ignoramos erros ou logamos em arquivo txt
                // Não mostramos MessageBox para não assustar o usuário
            }
        }

        private string ObterPastaNuvem()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // 1. Tenta OneDrive (Padrão do Windows)
            string oneDrivePath = Path.Combine(userProfile, "OneDrive");
            if (Directory.Exists(oneDrivePath)) return oneDrivePath;

            // 2. Tenta Google Drive (Geralmente montado como unidade G: ou na pasta do usuário)
            // O Google Drive Desktop cria um drive virtual, verifique a letra, mas geralmente é difícil prever.
            // Uma aposta segura é usar a pasta Documentos se não achar OneDrive.

            return Path.Combine(userProfile, "Documents");
        }

        private void LimparBackupsAntigos(string pasta)
        {
            try
            {
                var diretorio = new DirectoryInfo(pasta);
                var arquivos = diretorio.GetFiles("AutoBackup_*.db");

                foreach (var arquivo in arquivos)
                {
                    // Se o arquivo for mais velho que 30 dias, deleta
                    if (arquivo.CreationTime < DateTime.Now.AddDays(-30))
                    {
                        arquivo.Delete();
                    }
                }
            }
            catch { }
        }
    }
}