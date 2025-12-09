using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class modifsjesaispasoumathieuveuxpasfairedemigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "tco_cherchable",
                schema: "public",
                table: "t_e_typecompte_tco",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "com_id",
                schema: "public",
                table: "t_e_message_mes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_t_e_message_mes_com_id",
                schema: "public",
                table: "t_e_message_mes",
                column: "com_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_commande_cmd_t_e_compte_com_com_id_acheteur",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "com_id_acheteur",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_commande_cmd_t_e_compte_com_com_id_vendeur",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "com_id_vendeur",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_message_mes_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_message_mes",
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
                name: "FK_t_e_commande_cmd_t_e_compte_com_com_id_acheteur",
                schema: "public",
                table: "t_e_commande_cmd");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_commande_cmd_t_e_compte_com_com_id_vendeur",
                schema: "public",
                table: "t_e_commande_cmd");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_message_mes_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_message_mes");

            migrationBuilder.DropIndex(
                name: "IX_t_e_message_mes_com_id",
                schema: "public",
                table: "t_e_message_mes");

            migrationBuilder.DropColumn(
                name: "tco_cherchable",
                schema: "public",
                table: "t_e_typecompte_tco");

            migrationBuilder.DropColumn(
                name: "com_id",
                schema: "public",
                table: "t_e_message_mes");
        }
    }
}
