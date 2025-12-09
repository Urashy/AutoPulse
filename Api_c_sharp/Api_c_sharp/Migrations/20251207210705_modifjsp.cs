using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class modifjsp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_annonce_ann_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ann_idannoncesignale");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_annonce_ann_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ann_idannoncesignale",
                principalSchema: "public",
                principalTable: "t_e_annonce_ann",
                principalColumn: "ann_id");
        }
    }
}
