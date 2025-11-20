using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class modifs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_adresse_adr_t_e_ville_vil_vil_id",
                schema: "public",
                table: "t_e_adresse_adr");

            migrationBuilder.DropTable(
                name: "t_e_ville_vil",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_apouradresse_apa",
                schema: "public");

            migrationBuilder.RenameColumn(
                name: "vil_id",
                schema: "public",
                table: "t_e_adresse_adr",
                newName: "pays_id");

            migrationBuilder.RenameIndex(
                name: "IX_t_e_adresse_adr_vil_id",
                schema: "public",
                table: "t_e_adresse_adr",
                newName: "IX_t_e_adresse_adr_pays_id");

            migrationBuilder.AlterColumn<string>(
                name: "cpr_siret",
                schema: "public",
                table: "t_e_compte_com",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.AddColumn<int>(
                name: "ann_pri",
                schema: "public",
                table: "t_e_annonce_ann",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "adr_codepostal",
                schema: "public",
                table: "t_e_adresse_adr",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "adr_libelleville",
                schema: "public",
                table: "t_e_adresse_adr",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "adr_nom",
                schema: "public",
                table: "t_e_adresse_adr",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "com_id",
                schema: "public",
                table: "t_e_adresse_adr",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_t_e_adresse_adr_com_id",
                schema: "public",
                table: "t_e_adresse_adr",
                column: "com_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_adresse_adr_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_adresse_adr",
                column: "com_id",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_adresse_adr_t_e_pays_pay_pays_id",
                schema: "public",
                table: "t_e_adresse_adr",
                column: "pays_id",
                principalSchema: "public",
                principalTable: "t_e_pays_pay",
                principalColumn: "pay_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_adresse_adr_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_adresse_adr");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_adresse_adr_t_e_pays_pay_pays_id",
                schema: "public",
                table: "t_e_adresse_adr");

            migrationBuilder.DropIndex(
                name: "IX_t_e_adresse_adr_com_id",
                schema: "public",
                table: "t_e_adresse_adr");

            migrationBuilder.DropColumn(
                name: "ann_pri",
                schema: "public",
                table: "t_e_annonce_ann");

            migrationBuilder.DropColumn(
                name: "adr_codepostal",
                schema: "public",
                table: "t_e_adresse_adr");

            migrationBuilder.DropColumn(
                name: "adr_libelleville",
                schema: "public",
                table: "t_e_adresse_adr");

            migrationBuilder.DropColumn(
                name: "adr_nom",
                schema: "public",
                table: "t_e_adresse_adr");

            migrationBuilder.DropColumn(
                name: "com_id",
                schema: "public",
                table: "t_e_adresse_adr");

            migrationBuilder.RenameColumn(
                name: "pays_id",
                schema: "public",
                table: "t_e_adresse_adr",
                newName: "vil_id");

            migrationBuilder.RenameIndex(
                name: "IX_t_e_adresse_adr_pays_id",
                schema: "public",
                table: "t_e_adresse_adr",
                newName: "IX_t_e_adresse_adr_vil_id");

            migrationBuilder.AlterColumn<string>(
                name: "cpr_siret",
                schema: "public",
                table: "t_e_compte_com",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "t_e_ville_vil",
                schema: "public",
                columns: table => new
                {
                    vil_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pay_id = table.Column<int>(type: "integer", nullable: false),
                    vil_codepostal = table.Column<string>(type: "text", nullable: false),
                    vil_libelle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_ville_vil", x => x.vil_id);
                    table.ForeignKey(
                        name: "FK_t_e_ville_vil_t_e_pays_pay_pay_id",
                        column: x => x.pay_id,
                        principalSchema: "public",
                        principalTable: "t_e_pays_pay",
                        principalColumn: "pay_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_j_apouradresse_apa",
                schema: "public",
                columns: table => new
                {
                    adr_id = table.Column<int>(type: "integer", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_j_apouradresse_apa", x => new { x.adr_id, x.com_id });
                    table.ForeignKey(
                        name: "FK_t_j_apouradresse_apa_t_e_adresse_adr_adr_id",
                        column: x => x.adr_id,
                        principalSchema: "public",
                        principalTable: "t_e_adresse_adr",
                        principalColumn: "adr_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_j_apouradresse_apa_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_ville_vil_pay_id",
                schema: "public",
                table: "t_e_ville_vil",
                column: "pay_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_ville_vil_vil_codepostal",
                schema: "public",
                table: "t_e_ville_vil",
                column: "vil_codepostal");

            migrationBuilder.CreateIndex(
                name: "IX_t_j_apouradresse_apa_com_id",
                schema: "public",
                table: "t_j_apouradresse_apa",
                column: "com_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_adresse_adr_t_e_ville_vil_vil_id",
                schema: "public",
                table: "t_e_adresse_adr",
                column: "vil_id",
                principalSchema: "public",
                principalTable: "t_e_ville_vil",
                principalColumn: "vil_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
