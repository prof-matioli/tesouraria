using System;
using System.Windows;
using System.Windows.Controls;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities; // Para acessar o Enum TipoTransacao

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CategoriaWindow : Window
    {
        private readonly IBaseService<CategoriaFinanceira, CategoriaFinanceiraDTO> _service;

        public CategoriaWindow(IBaseService<CategoriaFinanceira, CategoriaFinanceiraDTO> service)
        {
            InitializeComponent();
            _service = service;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Carrega o Enum no ComboBox
            cbTipo.ItemsSource = Enum.GetValues(typeof(TipoTransacao));
            cbTipo.SelectedIndex = 0; // Seleciona o primeiro por padrão

            CarregarGrid();
        }

        private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validação simples
                if (cbTipo.SelectedItem == null)
                {
                    MessageBox.Show("Selecione um tipo de transação.");
                    return;
                }

                var dto = new CategoriaFinanceiraDTO
                {
                    Nome = txtNome.Text,
                    // Cast seguro do item selecionado no ComboBox para o Enum
                    Tipo = (TipoTransacao)cbTipo.SelectedItem,
                    DedutivelIR = chkDedutivel.IsChecked == true
                };

                if (!string.IsNullOrEmpty(txtId.Text) && int.TryParse(txtId.Text, out int id) && id > 0)
                {
                    dto.Id = id;
                    await _service.UpdateAsync(dto);
                    MessageBox.Show("Categoria atualizada!");
                }
                else
                {
                    await _service.AddAsync(dto);
                    MessageBox.Show("Categoria criada!");
                }

                LimparCampos();
                CarregarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private async void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;

            if (MessageBox.Show("Deseja excluir esta categoria?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _service.DeleteAsync(int.Parse(txtId.Text));
                    LimparCampos();
                    CarregarGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}");
                }
            }
        }

        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        private void DgDados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgDados.SelectedItem is CategoriaFinanceiraDTO item)
            {
                txtId.Text = item.Id.ToString();
                txtNome.Text = item.Nome;

                // Atribui o Enum diretamente, o ComboBox sabe selecionar o item correspondente
                cbTipo.SelectedItem = item.Tipo;

                chkDedutivel.IsChecked = item.DedutivelIR;
            }
        }

        private async void CarregarGrid()
        {
            DgDados.ItemsSource = await _service.GetAllAsync();
        }

        private void LimparCampos()
        {
            txtId.Text = "";
            txtNome.Text = "";
            cbTipo.SelectedIndex = 0; // Reseta para o primeiro
            chkDedutivel.IsChecked = false;

            DgDados.SelectedItem = null;
            txtNome.Focus();
        }
    }
}