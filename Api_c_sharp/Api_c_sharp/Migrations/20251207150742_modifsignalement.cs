using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class modifsignalement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_signalement_sig_t_e_annonce_ann_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig");

            migrationBuilder.DropIndex(
                name: "IX_t_e_signalement_sig_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig");
        }
    }
}
