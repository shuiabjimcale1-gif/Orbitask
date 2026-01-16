using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbitask.Migrations
{
    /// <inheritdoc />
    public partial class removednavproperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardMember_AspNetUsers_UserId",
                table: "BoardMember");

            migrationBuilder.DropForeignKey(
                name: "FK_BoardMember_Boards_BoardId",
                table: "BoardMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Workbenches_AspNetUsers_OwnerId",
                table: "Workbenches");

            migrationBuilder.DropIndex(
                name: "IX_Workbenches_OwnerId",
                table: "Workbenches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BoardMember",
                table: "BoardMember");

            migrationBuilder.RenameTable(
                name: "BoardMember",
                newName: "BoardMembers");

            migrationBuilder.RenameIndex(
                name: "IX_BoardMember_UserId",
                table: "BoardMembers",
                newName: "IX_BoardMembers_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Workbenches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoardMembers",
                table: "BoardMembers",
                columns: new[] { "BoardId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BoardMembers_AspNetUsers_UserId",
                table: "BoardMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardMembers_Boards_BoardId",
                table: "BoardMembers",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardMembers_AspNetUsers_UserId",
                table: "BoardMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_BoardMembers_Boards_BoardId",
                table: "BoardMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BoardMembers",
                table: "BoardMembers");

            migrationBuilder.RenameTable(
                name: "BoardMembers",
                newName: "BoardMember");

            migrationBuilder.RenameIndex(
                name: "IX_BoardMembers_UserId",
                table: "BoardMember",
                newName: "IX_BoardMember_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Workbenches",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoardMember",
                table: "BoardMember",
                columns: new[] { "BoardId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Workbenches_OwnerId",
                table: "Workbenches",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardMember_AspNetUsers_UserId",
                table: "BoardMember",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardMember_Boards_BoardId",
                table: "BoardMember",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workbenches_AspNetUsers_OwnerId",
                table: "Workbenches",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
