REM --- Camada de Infraestrutura ---
REM EF Core para SQL Server
dotnet add Tesouraria.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.11
REM Ferramentas do EF Core (para Migrations)
dotnet add Tesouraria.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.0.11
REM Serilog (Logs)
dotnet add Tesouraria.Infrastructure package Serilog
dotnet add Tesouraria.Infrastructure package Serilog.Sinks.File

REM --- Camada de Application ---
REM AutoMapper (Mapeamento Entidade <-> DTO)
dotnet add Tesouraria.Application package AutoMapper
REM FluentValidation (Validação de dados)
dotnet add Tesouraria.Application package FluentValidation

REM --- Camada Desktop (Presentation) ---
REM Injeção de Dependência e Host Genérico
dotnet add Tesouraria.Desktop package Microsoft.Extensions.Hosting --version 9.0.11
dotnet add Tesouraria.Desktop package Microsoft.Extensions.DependencyInjection --version 9.0.11
REM Configuração (appsettings.json)
dotnet add Tesouraria.Desktop package Microsoft.Extensions.Configuration.Json --version 9.0.11
REM EF
dotnet add Tesouraria.Desktop package  Microsoft.EntityFrameworkCore.Design --version 9.0.11
REM Serilog na UI
dotnet add Tesouraria.Desktop package Serilog.Extensions.Hosting --version 9.0
dotnet add Tesouraria.Desktop package Serilog.Settings.Configuration --version 10.0