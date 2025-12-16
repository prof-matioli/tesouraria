using System;
using System.Windows;
using System.Windows.Controls;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Desktop.Views.Cadastros
{
    public partial class CentroCustoWindow : Window
    {
        private readonly IBaseService<CentroCusto, CentroCustoDTO> _service;

        public CentroCustoWindow(IBaseService<CentroCusto, CentroCustoDTO> service)
        {
            InitializeComponent();
            _service = service;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarregarGrid();
        }

        private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dto = new CentroCustoDTO
                {
                    Nome = txtNome.Text,
                    Descricao = txtDescricao.Text
                };

                if (!string.IsNullOrEmpty(txtId.Text) && int.TryParse(txtId.Text, out int id) && id > 0)
                {
                    dto.Id = id;
                    await _service.UpdateAsync(dto);
                    MessageBox.Show("Atualizado com sucesso!");
                }
                else
                {
                    await _service.AddAsync(dto);
                    MessageBox.Show("Cadastrado com sucesso!");
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

            if (MessageBox.Show("Confirmar exclusão?", "Atenção", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
            if (DgDados.SelectedItem is CentroCustoDTO item)
            {
                txtId.Text = item.Id.ToString();
                txtNome.Text = item.Nome;
                txtDescricao.Text = item.Descricao;
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
            txtDescricao.Text = "";
            DgDados.SelectedItem = null;
            txtNome.Focus();
        }
    }
}