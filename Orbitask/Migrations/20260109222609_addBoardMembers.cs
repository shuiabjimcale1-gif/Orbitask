using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbitask.Migrations
{
    /// <inheritdoc />
    public partial class addBoardMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Workbenches",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BoardMember",
                columns: table => new
                {
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardMember", x => new { x.BoardId, x.UserId });
                    table.ForeignKey(
                        name: "FK_BoardMember_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BoardMember_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workbenches_OwnerId",
                table: "Workbenches",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardMember_UserId",
                table: "BoardMember",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workbenches_AspNetUsers_OwnerId",
                table: "Workbenches",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workbenches_AspNetUsers_OwnerId",
                table: "Workbenches");

            migrationBuilder.DropTable(
                name: "BoardMember");

            migrationBuilder.DropIndex(
                name: "IX_Workbenches_OwnerId",
                table: "Workbenches");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Workbenches");
        }
    }
}
