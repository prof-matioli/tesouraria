// Tesouraria/Base/BaseCadastroWindow.cs
using System.Windows;
using Tesouraria.Application.Interfaces;
using Tesouraria.Domain.Common;

namespace Tesouraria.Base
{
    // TEntity: A entidade do banco (ex: Fiel)
    // TDto: O objeto de tela (ex: FielDTO)
    public class BaseCadastroWindow<TEntity, TDto> : Window
        where TEntity : BaseEntity
        where TDto : class, new()
    {
        protected readonly IBaseService<TEntity, TDto> _service;
        protected int? _idRegistro; // Se nulo, é inclusão. Se tem valor, é edição.

        public BaseCadastroWindow(IBaseService<TEntity, TDto> service)
        {
            _service = service;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.Height; // Ajusta altura automaticamente
            Width = 600; // Largura padrão
        }

        // Método chamado pelo construtor da janela filha para carregar dados
        public async void CarregarRegistro(int id)
        {
            _idRegistro = id;
            var dto = await _service.GetByIdAsync(id);
            if (dto != null)
            {
                PreencherTela(dto);
            }
        }

        // Métodos Virtuais que as janelas filhas DEVEM ou PODEM implementar
        protected virtual void PreencherTela(TDto dto) { }
        protected virtual TDto MontarDTO() { return new TDto(); }
        protected virtual void LimparCampos() { }

        // Lógica do Botão Salvar
        protected async void Salvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dto = MontarDTO();

                if (_idRegistro.HasValue) // Edição
                {
                    // Aqui você poderia usar Reflection para setar o ID no DTO se necessário,
                    // ou garantir que o MontarDTO já pegue o ID oculto.
                    await _service.UpdateAsync(dto);
                    MessageBox.Show("Registro atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // Inclusão
                {
                    await _service.AddAsync(dto);
                    MessageBox.Show("Registro inserido com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Close(); // Fecha a janela após salvar
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Lógica do Botão Excluir
        protected async void Excluir_Click(object sender, RoutedEventArgs e)
        {
            if (!_idRegistro.HasValue) return;

            var result = MessageBox.Show("Tem certeza que deseja excluir este registro?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _service.DeleteAsync(_idRegistro.Value);
                    MessageBox.Show("Registro excluído.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}