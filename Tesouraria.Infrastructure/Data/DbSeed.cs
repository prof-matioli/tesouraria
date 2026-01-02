using System;
using System.Linq;
using Tesouraria.Application.Utils; // Onde está o SenhaHelper
using Tesouraria.Domain.Entities;
using Tesouraria.Infrastructure.Data;

namespace Tesouraria.Infrastructure.Data
{
    public static class DbSeed
    {
        public static void Seed(AppDbContext context)
        {
            // 1. Garante que o Perfil de Administrador existe
            var perfilAdmin = context.Perfis.FirstOrDefault(p => p.Nome == "Administrador");

            if (perfilAdmin == null)
            {
                perfilAdmin = new Perfil
                {
                    Nome = "Administrador",
                    Descricao = "Acesso total ao sistema",
                    // Se tiver DataCriacao na BaseEntity
                    DataCriacao = DateTime.Now
                };
                context.Perfis.Add(perfilAdmin);
                context.SaveChanges(); // Salva para gerar o ID
            }

            // 2. Garante que o Usuário Admin existe
            var usuarioAdmin = context.Usuarios.FirstOrDefault(u => u.Email == "admin@paroquia.com");

            if (usuarioAdmin == null)
            {
                var admin = new Usuario
                {
                    Nome = "Administrador do Sistema",
                    Email = "admin@paroquia.com",
                    Ativo = true,
                    PerfilId = perfilAdmin.Id, // Vincula ao perfil criado acima
                    DataCriacao = DateTime.Now,

                    // GERA O HASH DA SENHA PADRÃO "admin123"
                    SenhaHash = PasswordHelper.GenerateHash("admin123")
                };

                context.Usuarios.Add(admin);
                context.SaveChanges();
            }
        }
    }
}