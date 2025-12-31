using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
    [ExcludeFromCodeCoverage]

    /// <inheritdoc />
    public partial class ajoutnbairbags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "voi_nbairbag",
                schema: "public",
                table: "t_e_voiture_voi",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "voi_nbairbag",
                schema: "public",
                table: "t_e_voiture_voi");
        }
    }
}
