using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class ajoutetatcompteetplainte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "com_numero_telephone",
                schema: "public",
                table: "t_e_compte_com",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "etc_id",
                schema: "public",
                table: "t_e_compte_com",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "t_e_etatcompte_etc",
                schema: "public",
                columns: table => new
                {
                    etc_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    etc_libelle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_etatcompte_etc", x => x.etc_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_plainte_pla",
                schema: "public",
                columns: table => new
                {
                    pla_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pla_description = table.Column<string>(type: "text", nullable: false),
                    pla_date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sig_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_plainte_pla", x => x.pla_id);
                    table.ForeignKey(
                        name: "FK_t_e_plainte_pla_t_e_signalement_sig_sig_id",
                        column: x => x.sig_id,
                        principalSchema: "public",
                        principalTable: "t_e_signalement_sig",
                        principalColumn: "sig_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_compte_com_etc_id",
                schema: "public",
                table: "t_e_compte_com",
                column: "etc_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_plainte_pla_sig_id",
                schema: "public",
                table: "t_e_plainte_pla",
                column: "sig_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_compte_com_t_e_etatcompte_etc_etc_id",
                schema: "public",
                table: "t_e_compte_com",
                column: "etc_id",
                principalSchema: "public",
                principalTable: "t_e_etatcompte_etc",
                principalColumn: "etc_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_compte_com_t_e_etatcompte_etc_etc_id",
                schema: "public",
                table: "t_e_compte_com");

            migrationBuilder.DropTable(
                name: "t_e_etatcompte_etc",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_plainte_pla",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_t_e_compte_com_etc_id",
                schema: "public",
                table: "t_e_compte_com");

            migrationBuilder.DropColumn(
                name: "com_numero_telephone",
                schema: "public",
                table: "t_e_compte_com");

            migrationBuilder.DropColumn(
                name: "etc_id",
                schema: "public",
                table: "t_e_compte_com");
        }
    }
}
