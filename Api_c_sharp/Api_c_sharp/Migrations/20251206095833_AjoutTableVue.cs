using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class AjoutTableVue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_bloque_blo_t_e_compte_com_blo_id",
                schema: "public",
                table: "t_e_bloque_blo");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_bloque_blo_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_bloque_blo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_t_e_bloque_blo",
                schema: "public",
                table: "t_e_bloque_blo");

            migrationBuilder.RenameTable(
                name: "t_e_bloque_blo",
                schema: "public",
                newName: "t_j_bloque_blo",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_t_e_bloque_blo_blo_id",
                schema: "public",
                table: "t_j_bloque_blo",
                newName: "IX_t_j_bloque_blo_blo_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_t_j_bloque_blo",
                schema: "public",
                table: "t_j_bloque_blo",
                columns: new[] { "com_id", "blo_id" });

            migrationBuilder.CreateTable(
                name: "t_j_vue_vue",
                schema: "public",
                columns: table => new
                {
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    ann_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_j_vue_vue", x => new { x.com_id, x.ann_id });
                    table.ForeignKey(
                        name: "FK_t_j_vue_vue_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_j_vue_vue_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_j_vue_vue_ann_id",
                schema: "public",
                table: "t_j_vue_vue",
                column: "ann_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_j_bloque_blo_t_e_compte_com_blo_id",
                schema: "public",
                table: "t_j_bloque_blo",
                column: "blo_id",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_j_bloque_blo_t_e_compte_com_com_id",
                schema: "public",
                table: "t_j_bloque_blo",
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
                name: "FK_t_j_bloque_blo_t_e_compte_com_blo_id",
                schema: "public",
                table: "t_j_bloque_blo");

            migrationBuilder.DropForeignKey(
                name: "FK_t_j_bloque_blo_t_e_compte_com_com_id",
                schema: "public",
                table: "t_j_bloque_blo");

            migrationBuilder.DropTable(
                name: "t_j_vue_vue",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "PK_t_j_bloque_blo",
                schema: "public",
                table: "t_j_bloque_blo");

            migrationBuilder.RenameTable(
                name: "t_j_bloque_blo",
                schema: "public",
                newName: "t_e_bloque_blo",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_t_j_bloque_blo_blo_id",
                schema: "public",
                table: "t_e_bloque_blo",
                newName: "IX_t_e_bloque_blo_blo_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_t_e_bloque_blo",
                schema: "public",
                table: "t_e_bloque_blo",
                columns: new[] { "com_id", "blo_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_bloque_blo_t_e_compte_com_blo_id",
                schema: "public",
                table: "t_e_bloque_blo",
                column: "blo_id",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_bloque_blo_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_bloque_blo",
                column: "com_id",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
