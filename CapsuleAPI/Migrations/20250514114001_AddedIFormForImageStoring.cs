using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapsuleAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedIFormForImageStoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "TimeCapsules",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "TimeCapsules");
        }
    }
}
