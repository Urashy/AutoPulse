using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class AjoutDescriptionAnnonce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ann_description",
                schema: "public",
                table: "t_e_annonce_ann",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ann_description",
                schema: "public",
                table: "t_e_annonce_ann");
        }
    }
}
