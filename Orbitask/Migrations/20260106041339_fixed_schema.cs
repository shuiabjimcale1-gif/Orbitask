using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orbitask.Migrations
{
    /// <inheritdoc />
    public partial class fixed_schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_Workbenches_WorkbenchId",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Boards_BoardId",
                table: "Tags");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_Workbenches_WorkbenchId",
                table: "Boards",
                column: "WorkbenchId",
                principalTable: "Workbenches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Boards_BoardId",
                table: "Tags",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_Workbenches_WorkbenchId",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Boards_BoardId",
                table: "Tags");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_Workbenches_WorkbenchId",
                table: "Boards",
                column: "WorkbenchId",
                principalTable: "Workbenches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Boards_BoardId",
                table: "Tags",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
