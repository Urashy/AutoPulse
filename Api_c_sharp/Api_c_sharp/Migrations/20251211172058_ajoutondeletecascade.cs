using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Api_c_sharp.Migrations
{
        [ExcludeFromCodeCoverage] 
    /// <inheritdoc />
    public partial class ajoutondeletecascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_image_img_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_image_img");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_image_img_t_e_voiture_voi_voi_id",
                schema: "public",
                table: "t_e_image_img");

            migrationBuilder.CreateTable(
                name: "t_e_notification_not",
                schema: "public",
                columns: table => new
                {
                    not_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    not_titre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    not_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    not_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    not_url_navigation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    not_est_lue = table.Column<bool>(type: "boolean", nullable: false),
                    not_date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ann_id = table.Column<int>(type: "integer", nullable: true),
                    not_ancien_prix = table.Column<double>(type: "double precision", nullable: true),
                    not_nouveau_prix = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_notification_not", x => x.not_id);
                    table.ForeignKey(
                        name: "FK_t_e_notification_not_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id");
                    table.ForeignKey(
                        name: "FK_t_e_notification_not_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_notification_not_ann_id",
                schema: "public",
                table: "t_e_notification_not",
                column: "ann_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_notification_not_com_id",
                schema: "public",
                table: "t_e_notification_not",
                column: "com_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_image_img_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_image_img",
                column: "com_id",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_image_img_t_e_voiture_voi_voi_id",
                schema: "public",
                table: "t_e_image_img",
                column: "voi_id",
                principalSchema: "public",
                principalTable: "t_e_voiture_voi",
                principalColumn: "voi_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_e_image_img_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_image_img");

            migrationBuilder.DropForeignKey(
                name: "FK_t_e_image_img_t_e_voiture_voi_voi_id",
                schema: "public",
                table: "t_e_image_img");

            migrationBuilder.DropTable(
                name: "t_e_notification_not",
                schema: "public");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_image_img_t_e_compte_com_com_id",
                schema: "public",
                table: "t_e_image_img",
                column: "com_id",
                principalSchema: "public",
                principalTable: "t_e_compte_com",
                principalColumn: "com_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_e_image_img_t_e_voiture_voi_voi_id",
                schema: "public",
                table: "t_e_image_img",
                column: "voi_id",
                principalSchema: "public",
                principalTable: "t_e_voiture_voi",
                principalColumn: "voi_id");
        }
    }
}
