using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbitask.Migrations
{
    /// <inheritdoc />
    public partial class refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Boards_BoardId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_BoardId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Workbenches");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "TaskItems");

            migrationBuilder.AddColumn<int>(
                name: "WorkbenchId",
                table: "TaskTags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkbenchId",
                table: "Tags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkbenchMembers_WorkbenchId",
                table: "WorkbenchMembers",
                column: "WorkbenchId",
                unique: true,
                filter: "[Role] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkbenchMembers_WorkbenchId",
                table: "WorkbenchMembers");

            migrationBuilder.DropColumn(
                name: "WorkbenchId",
                table: "TaskTags");

            migrationBuilder.DropColumn(
                name: "WorkbenchId",
                table: "Tags");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Workbenches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BoardId",
                table: "TaskItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_BoardId",
                table: "TaskItems",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Boards_BoardId",
                table: "TaskItems",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");
        }
    }
}
