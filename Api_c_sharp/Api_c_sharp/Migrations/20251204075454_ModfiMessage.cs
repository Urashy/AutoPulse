using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class ModfiMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "mes_estlu",
                schema: "public",
                table: "t_e_message_mes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mes_estlu",
                schema: "public",
                table: "t_e_message_mes");
        }
    }
}
