using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DayClaim.AR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMenuAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users_menu_access",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuPath = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_menu_access", x => new { x.UserId, x.MenuPath });
                    table.ForeignKey(
                        name: "FK_users_menu_access_users_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users_menu_access");
        }
    }
}
