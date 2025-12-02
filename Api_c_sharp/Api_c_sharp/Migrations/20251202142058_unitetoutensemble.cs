using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class unitetoutensemble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_e_bloque_blo",
                schema: "public",
                columns: table => new
                {
                    blo_id = table.Column<int>(type: "integer", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    blo_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_bloque_blo", x => new { x.com_id, x.blo_id });
                    table.ForeignKey(
                        name: "FK_t_e_bloque_blo_t_e_compte_com_blo_id",
                        column: x => x.blo_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_bloque_blo_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_bloque_blo_blo_id",
                schema: "public",
                table: "t_e_bloque_blo",
                column: "blo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_e_bloque_blo",
                schema: "public");
        }
    }
}
