using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class correctionpkfacture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_facture_fac_t_e_commande_cmd_fac_id",
                schema: "public",
                table: "t_e_facture_fac");

            migrationBuilder.AlterColumn<int>(
                name: "fac_id",
                schema: "public",
                table: "t_e_facture_fac",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_t_e_facture_fac_cmd_id",
                schema: "public",
                table: "t_e_facture_fac",
                column: "cmd_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_facture_fac_t_e_commande_cmd_cmd_id",
                schema: "public",
                table: "t_e_facture_fac",
                column: "cmd_id",
                principalSchema: "public",
                principalTable: "t_e_commande_cmd",
                principalColumn: "cmd_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_facture_fac_t_e_commande_cmd_cmd_id",
                schema: "public",
                table: "t_e_facture_fac");

            migrationBuilder.DropIndex(
                name: "IX_t_e_facture_fac_cmd_id",
                schema: "public",
                table: "t_e_facture_fac");

            migrationBuilder.AlterColumn<int>(
                name: "fac_id",
                schema: "public",
                table: "t_e_facture_fac",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_facture_fac_t_e_commande_cmd_fac_id",
                schema: "public",
                table: "t_e_facture_fac",
                column: "fac_id",
                principalSchema: "public",
                principalTable: "t_e_commande_cmd",
                principalColumn: "cmd_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
