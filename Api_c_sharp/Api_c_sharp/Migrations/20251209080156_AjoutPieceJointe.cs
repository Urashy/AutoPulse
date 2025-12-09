using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class AjoutPieceJointe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_annonce_ann_ann_id",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropIndex(
                name: "IX_t_e_signalement_sig_ann_id",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropColumn(
                name: "ann_id",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.CreateTable(
                name: "t_e_piecejointe_pj",
                schema: "public",
                columns: table => new
                {
                    pj_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mes_id = table.Column<int>(type: "integer", nullable: false),
                    pj_nom_fichier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    pj_type_mime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pj_extension = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    pj_taille_fichier = table.Column<long>(type: "bigint", nullable: false),
                    pj_contenu = table.Column<byte[]>(type: "bytea", nullable: false),
                    pj_date_upload = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_piecejointe_pj", x => x.pj_id);
                    table.ForeignKey(
                        name: "FK_t_e_piecejointe_pj_t_e_message_mes_mes_id",
                        column: x => x.mes_id,
                        principalSchema: "public",
                        principalTable: "t_e_message_mes",
                        principalColumn: "mes_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ann_idannoncesignale");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_piecejointe_pj_mes_id",
                schema: "public",
                table: "t_e_piecejointe_pj",
                column: "mes_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_annonce_ann_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ann_idannoncesignale",
                principalSchema: "public",
                principalTable: "t_e_annonce_ann",
                principalColumn: "ann_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_annonce_ann_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropTable(
                name: "t_e_piecejointe_pj",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_t_e_signalement_sig_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.AddColumn<int>(
                name: "ann_id",
                schema: "public",
                table: "t_e_signalement_sig",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_ann_id",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ann_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_annonce_ann_ann_id",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ann_id",
                principalSchema: "public",
                principalTable: "t_e_annonce_ann",
                principalColumn: "ann_id");
        }
    }
}
