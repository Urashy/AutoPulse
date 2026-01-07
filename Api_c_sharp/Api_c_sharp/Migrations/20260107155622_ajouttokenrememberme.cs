using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class ajouttokenrememberme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_e_refresh_token_rft",
                schema: "public",
                columns: table => new
                {
                    rft_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rft_token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    rft_date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rft_date_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rft_est_revoque = table.Column<bool>(type: "boolean", nullable: false),
                    rft_date_revocation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rft_remember_me = table.Column<bool>(type: "boolean", nullable: false),
                    rft_ip_creation = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    rft_user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_refresh_token_rft", x => x.rft_id);
                    table.ForeignKey(
                        name: "FK_t_e_refresh_token_rft_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_refresh_token_rft_com_id",
                schema: "public",
                table: "t_e_refresh_token_rft",
                column: "com_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_e_refresh_token_rft",
                schema: "public");
        }
    }
}
