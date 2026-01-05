using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class ajoutvacances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_e_reinitialisationmotdepasse_rei",
                schema: "public");

            migrationBuilder.AddColumn<bool>(
                name: "com_a2f_actif",
                schema: "public",
                table: "t_e_compte_com",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "com_date_derniere_activation_a2f",
                schema: "public",
                table: "t_e_compte_com",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "t_e_tokenemail_tke",
                schema: "public",
                columns: table => new
                {
                    tke_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    com_email = table.Column<string>(type: "text", nullable: false),
                    tke_token = table.Column<string>(type: "text", nullable: false),
                    tke_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tke_utilise = table.Column<bool>(type: "boolean", nullable: false),
                    tke_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_tokenemail_tke", x => x.tke_id);
                    table.ForeignKey(
                        name: "FK_t_e_tokenemail_tke_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_tokenemail_tke_com_id",
                schema: "public",
                table: "t_e_tokenemail_tke",
                column: "com_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_e_tokenemail_tke",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "com_a2f_actif",
                schema: "public",
                table: "t_e_compte_com");

            migrationBuilder.DropColumn(
                name: "com_date_derniere_activation_a2f",
                schema: "public",
                table: "t_e_compte_com");

            migrationBuilder.CreateTable(
                name: "t_e_reinitialisationmotdepasse_rei",
                schema: "public",
                columns: table => new
                {
                    rei_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_email = table.Column<string>(type: "text", nullable: false),
                    rei_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    rei_token = table.Column<string>(type: "text", nullable: false),
                    rei_utilise = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_reinitialisationmotdepasse_rei", x => x.rei_id);
                });
        }
    }
}
