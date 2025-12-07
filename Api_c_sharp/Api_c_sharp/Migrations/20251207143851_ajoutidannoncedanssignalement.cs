using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class ajoutidannoncedanssignalement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_compte_com_com_id_signalant",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignale",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.RenameColumn(
                name: "com_id_signalant",
                schema: "public",
                table: "t_e_signalement_sig",
                newName: "com_idsignalant");

            migrationBuilder.RenameIndex(
                name: "IX_t_e_signalement_sig_com_id_signalant",
                schema: "public",
                table: "t_e_signalement_sig",
                newName: "IX_t_e_signalement_sig_com_idsignalant");

            migrationBuilder.AlterColumn<int>(
                name: "com_idsignale",
                schema: "public",
                table: "t_e_signalement_sig",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignalant",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "com_idsignalant",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "com_idsignale",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignalant",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignale",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropColumn(
                name: "ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.RenameColumn(
                name: "com_idsignalant",
                schema: "public",
                table: "t_e_signalement_sig",
                newName: "com_id_signalant");

            migrationBuilder.RenameIndex(
                name: "IX_t_e_signalement_sig_com_idsignalant",
                schema: "public",
                table: "t_e_signalement_sig",
                newName: "IX_t_e_signalement_sig_com_id_signalant");

            migrationBuilder.AlterColumn<int>(
                name: "com_idsignale",
                schema: "public",
                table: "t_e_signalement_sig",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_compte_com_com_id_signalant",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "com_id_signalant",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "com_idsignale",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
