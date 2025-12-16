using System;
using System.Windows;
using System.Windows.Controls;
using Tesouraria.Application.Services;


// -------------------------------------------------------------
// IMPORTS CRUCIAIS: Define onde estão suas classes de negócio
// -------------------------------------------------------------
using Tesouraria.Domain.Entities;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CadastroFornecedor : Window
    {
        // O Service que vai validar e chamar o repositório
        private readonly FornecedorService _service;

        public CadastroFornecedor(FornecedorService service)
        {
            InitializeComponent();
            _service = service;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarregarGrid();
        }

        // --- MÉTODOS DE AÇÃO (CRUD) ---

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Monta o objeto com dados da tela
                // O compilador sabe que 'Fornecedor' é do Domain por causa do using
                var fornecedor = new Fornecedor
                {
                    RazaoSocial = txtRazaoSocial.Text,
                    NomeFantasia = txtNomeFantasia.Text,
                    CNPJ = txtCNPJ.Text,
                    Telefone = txtTelefone.Text,
                    Email = txtEmail.Text
                };

                if (!string.IsNullOrEmpty(txtId.Text))
                {
                    fornecedor.Id = int.Parse(txtId.Text);
                    _service.EditarFornecedor(fornecedor); // Usa o serviço injetado
                }
                else
                {
                    _service.CadastrarFornecedor(fornecedor); // Usa o serviço injetado
                }

                LimparCampos();
                CarregarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text))
            {
                MessageBox.Show("Selecione um fornecedor para excluir.");
                return;
            }

            var confirmacao = MessageBox.Show("Tem certeza que deseja excluir?", "Confirmação", MessageBoxButton.YesNo);

            if (confirmacao == MessageBoxResult.Yes)
            {
                try
                {
                    int id = int.Parse(txtId.Text);
                    _service.RemoverFornecedor(id);

                    LimparCampos();
                    CarregarGrid();
                    MessageBox.Show("Fornecedor removido.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao excluir: " + ex.Message);
                }
            }
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        // --- EVENTOS DE INTERFACE ---

        // Quando o usuário clica em uma linha da tabela
        private void DgFornecedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // O DataGrid.SelectedItem retorna um 'object', precisamos fazer o cast
            var itemSelecionado = dgFornecedores.SelectedItem as Fornecedor;

            if (itemSelecionado != null)
            {
                // Popula os campos para edição
                txtId.Text = itemSelecionado.Id.ToString();
                txtRazaoSocial.Text = itemSelecionado.RazaoSocial;
                txtNomeFantasia.Text = itemSelecionado.NomeFantasia;
                txtCNPJ.Text = itemSelecionado.CNPJ;
                txtTelefone.Text = itemSelecionado.Telefone;
                txtEmail.Text = itemSelecionado.Email;
            }
        }

        // --- MÉTODOS AUXILIARES ---

        private void CarregarGrid()
        {
            // Buscamos a lista do serviço
            var lista = _service.ListarTodos();

            // No WPF, usamos ItemsSource
            dgFornecedores.ItemsSource = null; // Limpa binding anterior
            dgFornecedores.ItemsSource = lista;
        }

        private void LimparCampos()
        {
            txtId.Text = string.Empty;
            txtRazaoSocial.Text = string.Empty;
            txtNomeFantasia.Text = string.Empty;
            txtCNPJ.Text = string.Empty;
            txtTelefone.Text=string.Empty;
            txtEmail.Text = string.Empty;

            // Tira a seleção do grid
            dgFornecedores.SelectedItem = null;
            txtRazaoSocial.Focus();
        }
    }
}