using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class modifcommandeidoffre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cmd_montant",
                schema: "public",
                table: "t_e_commande_cmd");

            migrationBuilder.AddColumn<int>(
                name: "off_idoffre",
                schema: "public",
                table: "t_e_commande_cmd",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_t_e_commande_cmd_off_idoffre",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "off_idoffre");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_commande_cmd_t_e_offre_off_off_idoffre",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "off_idoffre",
                principalSchema: "public",
                principalTable: "t_e_offre_off",
                principalColumn: "off_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_commande_cmd_t_e_offre_off_off_idoffre",
                schema: "public",
                table: "t_e_commande_cmd");

            migrationBuilder.DropIndex(
                name: "IX_t_e_commande_cmd_off_idoffre",
                schema: "public",
                table: "t_e_commande_cmd");

            migrationBuilder.DropColumn(
                name: "off_idoffre",
                schema: "public",
                table: "t_e_commande_cmd");

            migrationBuilder.AddColumn<decimal>(
                name: "cmd_montant",
                schema: "public",
                table: "t_e_commande_cmd",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
