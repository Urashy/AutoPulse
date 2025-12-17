using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    /// 
    [ExcludeFromCodeCoverage]

    public partial class modifetatsignalementplainte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_etatsignalement_ets_ets_id",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropTable(
                name: "t_e_etatsignalement_ets",
                schema: "public");

            migrationBuilder.AddColumn<double>(
                name: "voi_cylindrermoteur",
                schema: "public",
                table: "t_e_voiture_voi",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "voi_interieurcuire",
                schema: "public",
                table: "t_e_voiture_voi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "voi_positionvolant",
                schema: "public",
                table: "t_e_voiture_voi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ets_id",
                schema: "public",
                table: "t_e_plainte_pla",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "t_e_etatsignalementplainte_ets",
                schema: "public",
                columns: table => new
                {
                    ets_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    eta_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_etatsignalementplainte_ets", x => x.ets_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_plainte_pla_ets_id",
                schema: "public",
                table: "t_e_plainte_pla",
                column: "ets_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_plainte_pla_t_e_etatsignalementplainte_ets_ets_id",
                schema: "public",
                table: "t_e_plainte_pla",
                column: "ets_id",
                principalSchema: "public",
                principalTable: "t_e_etatsignalementplainte_ets",
                principalColumn: "ets_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_etatsignalementplainte_ets_ets_id",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ets_id",
                principalSchema: "public",
                principalTable: "t_e_etatsignalementplainte_ets",
                principalColumn: "ets_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_plainte_pla_t_e_etatsignalementplainte_ets_ets_id",
                schema: "public",
                table: "t_e_plainte_pla");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_etatsignalementplainte_ets_ets_id",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropTable(
                name: "t_e_etatsignalementplainte_ets",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_t_e_plainte_pla_ets_id",
                schema: "public",
                table: "t_e_plainte_pla");

            migrationBuilder.DropColumn(
                name: "voi_cylindrermoteur",
                schema: "public",
                table: "t_e_voiture_voi");

            migrationBuilder.DropColumn(
                name: "voi_interieurcuire",
                schema: "public",
                table: "t_e_voiture_voi");

            migrationBuilder.DropColumn(
                name: "voi_positionvolant",
                schema: "public",
                table: "t_e_voiture_voi");

            migrationBuilder.DropColumn(
                name: "ets_id",
                schema: "public",
                table: "t_e_plainte_pla");

            migrationBuilder.CreateTable(
                name: "t_e_etatsignalement_ets",
                schema: "public",
                columns: table => new
                {
                    ets_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    eta_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_etatsignalement_ets", x => x.ets_id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_etatsignalement_ets_ets_id",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ets_id",
                principalSchema: "public",
                principalTable: "t_e_etatsignalement_ets",
                principalColumn: "ets_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
