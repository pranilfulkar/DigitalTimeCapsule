using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapsuleAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedChangesInDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_TimeCapsules_TimeCapsuleId",
                table: "Contributions");

            migrationBuilder.DropIndex(
                name: "IX_Contributions_TimeCapsuleId",
                table: "Contributions");

            migrationBuilder.DropColumn(
                name: "TimeCapsuleId",
                table: "Contributions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeCapsuleId",
                table: "Contributions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contributions_TimeCapsuleId",
                table: "Contributions",
                column: "TimeCapsuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_TimeCapsules_TimeCapsuleId",
                table: "Contributions",
                column: "TimeCapsuleId",
                principalTable: "TimeCapsules",
                principalColumn: "Id");
        }
    }
}
