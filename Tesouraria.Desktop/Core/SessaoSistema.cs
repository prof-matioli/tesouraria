using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Desktop.Core
{
    // Classe estática simples para manter o estado do usuário logado na memória
    public static class SessaoSistema
    {
        public static UsuarioDTO? UsuarioLogado { get; set; }
        public static int UsuarioId => UsuarioLogado?.Id ?? 0;
        public static string NomeUsuario => UsuarioLogado?.Nome ?? "Usuário";

        public static bool IsLogado => UsuarioLogado != null;

        public static void Logout()
        {
            UsuarioLogado = null;
        }
    }
}