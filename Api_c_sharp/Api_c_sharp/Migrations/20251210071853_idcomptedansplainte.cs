using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
        [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class idcomptedansplainte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "com_id",
                schema: "public",
                table: "t_e_plainte_pla",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_t_e_plainte_pla_com_id",
                schema: "public",
                table: "t_e_plainte_pla",
                column: "com_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_plainte_pla_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_plainte_pla",
                column: "com_id",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_plainte_pla_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_plainte_pla");

            migrationBuilder.DropIndex(
                name: "IX_t_e_plainte_pla_com_id",
                schema: "public",
                table: "t_e_plainte_pla");

            migrationBuilder.DropColumn(
                name: "com_id",
                schema: "public",
                table: "t_e_plainte_pla");
        }
    }
}
