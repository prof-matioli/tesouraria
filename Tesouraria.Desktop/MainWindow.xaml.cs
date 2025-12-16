using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using Tesouraria.Application.DTOs;
using Tesouraria.Application.Interfaces;
using Tesouraria.Desktop.Views;
using Tesouraria.Desktop.Views.Cadastros;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Desktop
{
    public partial class MainWindow : Window
    {
        // Serviço injetado para manipular Fieis
        private readonly IBaseService<Fiel, FielDTO> _fielService;

        // Construtor com Injeção de Dependência
        public MainWindow(IBaseService<Fiel, FielDTO> fielService)
        {
            InitializeComponent();
        }

        private void BtnAbrirCentroCusto_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current is App app)
            {
                var janela = app.Host.Services.GetRequiredService<CentroCustoWindow>();
                janela.Owner = this;
                janela.ShowDialog();
            }
        }
        private void BtnAbrirPlanoContas_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current is App app)
            {
                var janela = app.Host.Services.GetRequiredService<CategoriaWindow>();
                janela.Owner = this;
                janela.ShowDialog();
            }
        }

        // Botão para abrir a Gestão de Fieis
        private void BtnAbrirFieis_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current is App app)
            {
                // O container resolve a nova janela e injeta o Service nela automaticamente
                var janelaGestao = app.Host.Services.GetRequiredService<CadastroFielWindow>();
                janelaGestao.ShowDialog();
            }
        }

        private void BtnAbrirFornecedores_Click(object sender, RoutedEventArgs e)
        {
            // Forma correta de abrir janelas com DI no WPF sem frameworks complexos:

            // 1. Pega a instância atual da aplicação
            var app = (App)System.Windows.Application.Current;

            // 2. Pede ao container a janela pronta
            var janela = app.Host.Services.GetRequiredService<CadastroFornecedor>();

            // 3. Abre
            janela.ShowDialog();
        }

        private void BtnAbrirTesouraria_Click(object sender, RoutedEventArgs e)
        {
            // Forma correta de abrir janelas com DI no WPF sem frameworks complexos:

            // 1. Pega a instância atual da aplicação
            var app = (App)System.Windows.Application.Current;

            // 2. Pede ao container a janela pronta
            var janela = app.Host.Services.GetRequiredService<LancamentoListaView>();

            // 3. Abre
            janela.ShowDialog();
        }
    }
}