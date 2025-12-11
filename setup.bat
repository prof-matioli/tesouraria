REM 1. Cria a pasta raiz e entra nela
REM mkdir Tesouraria
REM cd Tesouraria

REM 2. Cria a Solução vazia
dotnet new sln -n Tesouraria

REM 3. Cria o projeto de Domínio (Class Library)
dotnet new classlib -n Tesouraria.Domain
dotnet sln add Tesouraria.Domain

REM 4. Cria o projeto de Aplicação (Class Library)
dotnet new classlib -n Tesouraria.Application
dotnet sln add Tesouraria.Application
REM Referência: Application usa Domain
dotnet add Tesouraria.Application reference Tesouraria.Domain

REM 5. Cria o projeto de Infraestrutura (Class Library)
dotnet new classlib -n Tesouraria.Infrastructure
dotnet sln add Tesouraria.Infrastructure
REM Referência: Infra usa Domain e Application
dotnet add Tesouraria.Infrastructure reference Tesouraria.Domain
dotnet add Tesouraria.Infrastructure reference Tesouraria.Application

REM 6. Cria o projeto de Interface Desktop (WPF)
dotnet new wpf -n Tesouraria.Desktop
dotnet sln add Tesouraria.Desktop
REM Referência: Desktop usa Application e Infra (para Injeção de Dependência)
dotnet add Tesouraria.Desktop reference Tesouraria.Application
dotnet add Tesouraria.Desktop reference Tesouraria.Infrastructure

REM 7. Cria o projeto de Testes (xUnit)
dotnet new xunit -n Tesouraria.Tests
dotnet sln add Tesouraria.Tests
dotnet add Tesouraria.Tests reference Tesouraria.Domain
dotnet add Tesouraria.Tests reference Tesouraria.Application

REM 8. Cria o arquivo .gitignore padrão para .NET
dotnet new gitignore