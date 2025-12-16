using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tesouraria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteColunasBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Fornecedores",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Fornecedores",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Fornecedores");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Fornecedores");
        }
    }
}
