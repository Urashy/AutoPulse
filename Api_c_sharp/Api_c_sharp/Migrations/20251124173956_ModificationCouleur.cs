using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class ModificationCouleur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cou_id",
                schema: "public",
                table: "t_e_voiture_voi");

            migrationBuilder.AddColumn<string>(
                name: "cou_codehexa",
                schema: "public",
                table: "t_e_couleur_cou",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cou_codehexa",
                schema: "public",
                table: "t_e_couleur_cou");

            migrationBuilder.AddColumn<int>(
                name: "cou_id",
                schema: "public",
                table: "t_e_voiture_voi",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
