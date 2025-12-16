using System;
using System.Windows;
using System.Windows.Controls;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroFielWindow : Window
    {
        // Serviço Genérico ou Específico
        private readonly IBaseService<Fiel, FielDTO> _service;

        // Injeção de Dependência no Construtor
        public CadastroFielWindow(IBaseService<Fiel, FielDTO> service)
        {
            InitializeComponent();
            _service = service;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarregarGrid();
        }

        // --- MÉTODOS DE AÇÃO ---

        private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Validação Básica
                if (string.IsNullOrWhiteSpace(txtNome.Text))
                {
                    MessageBox.Show("O nome é obrigatório.");
                    return;
                }

                // 2. Montar o DTO com dados da tela
                var dto = new FielDTO
                {
                    Nome = txtNome.Text,
                    CPF = txtCPF.Text,
                    // Verifica se tem data, senão usa data atual ou trata como nulo
                    DataNascimento = dtpNascimento.SelectedDate ?? DateTime.Now
                };

                // 3. Verifica se é Edição ou Inserção pelo ID
                if (!string.IsNullOrEmpty(txtId.Text) && int.TryParse(txtId.Text, out int id) && id > 0)
                {
                    // EDIÇÃO
                    dto.Id = id;
                    await _service.UpdateAsync(dto); // Assume que seu service tem UpdateAsync
                    MessageBox.Show("Fiel atualizado com sucesso!");
                }
                else
                {
                    // NOVO CADASTRO
                    await _service.AddAsync(dto); // Assume que seu service tem AddAsync
                    MessageBox.Show("Fiel cadastrado com sucesso!");
                }

                LimparCampos();
                CarregarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro");
            }
        }

        private async void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text))
            {
                MessageBox.Show("Selecione um fiel para excluir.");
                return;
            }

            if (MessageBox.Show("Tem certeza?", "Confirmação", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    int id = int.Parse(txtId.Text);
                    await _service.DeleteAsync(id);

                    LimparCampos();
                    CarregarGrid();
                    MessageBox.Show("Registro removido.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir: {ex.Message}");
                }
            }
        }

        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        // --- EVENTOS DE SELEÇÃO ---

        private void DgDados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Quando clica na linha, joga os dados para cima (Formulário)
            if (DgDados.SelectedItem is FielDTO item)
            {
                txtId.Text = item.Id.ToString();
                txtNome.Text = item.Nome;
                txtCPF.Text = item.CPF;
                dtpNascimento.SelectedDate = item.DataNascimento;
            }
        }

        // --- MÉTODOS AUXILIARES ---

        private async void CarregarGrid()
        {
            try
            {
                var lista = await _service.GetAllAsync();
                DgDados.ItemsSource = lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar lista: {ex.Message}");
            }
        }

        private void LimparCampos()
        {
            txtId.Text = string.Empty;
            txtNome.Text = string.Empty;
            txtCPF.Text = string.Empty;
            dtpNascimento.SelectedDate = null;

            DgDados.SelectedItem = null; // Tira seleção da grid
            txtNome.Focus();
        }
    }
}