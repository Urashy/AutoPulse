using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class modifbd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fac_id",
                schema: "public",
                table: "t_e_commande_cmd");

            migrationBuilder.AddColumn<DateTime>(
                name: "con_date_dernier_message",
                schema: "public",
                table: "t_e_conversation_con",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "com_auth_provider",
                schema: "public",
                table: "t_e_compte_com",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "com_google_id",
                schema: "public",
                table: "t_e_compte_com",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "t_e_reinitialisationmotdepasse_rei",
                schema: "public",
                columns: table => new
                {
                    rei_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    com_email = table.Column<string>(type: "text", nullable: false),
                    rei_token = table.Column<string>(type: "text", nullable: false),
                    rei_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rei_utilise = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_reinitialisationmotdepasse_rei", x => x.rei_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_e_reinitialisationmotdepasse_rei",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "con_date_dernier_message",
                schema: "public",
                table: "t_e_conversation_con");

            migrationBuilder.DropColumn(
                name: "com_auth_provider",
                schema: "public",
                table: "t_e_compte_com");

            migrationBuilder.DropColumn(
                name: "com_google_id",
                schema: "public",
                table: "t_e_compte_com");

            migrationBuilder.AddColumn<int>(
                name: "fac_id",
                schema: "public",
                table: "t_e_commande_cmd",
                type: "integer",
                nullable: true);
        }
    }
}
