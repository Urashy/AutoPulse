using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class ajouttableoffre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_e_offre_off",
                schema: "public",
                columns: table => new
                {
                    off_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ann_id = table.Column<int>(type: "integer", nullable: false),
                    mes_id = table.Column<int>(type: "integer", nullable: false),
                    off_valeur = table.Column<decimal>(type: "numeric", nullable: false),
                    off_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    off_estaccepte = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_offre_off", x => x.off_id);
                    table.ForeignKey(
                        name: "FK_t_e_offre_off_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_offre_off_t_e_message_mes_mes_id",
                        column: x => x.mes_id,
                        principalSchema: "public",
                        principalTable: "t_e_message_mes",
                        principalColumn: "mes_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_offre_off_ann_id",
                schema: "public",
                table: "t_e_offre_off",
                column: "ann_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_offre_off_mes_id",
                schema: "public",
                table: "t_e_offre_off",
                column: "mes_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_e_offre_off",
                schema: "public");
        }
    }
}
