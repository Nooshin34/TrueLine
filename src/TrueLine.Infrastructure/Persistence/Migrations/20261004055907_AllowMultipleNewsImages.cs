using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueLine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleNewsImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsImages_NewsId",
                table: "NewsImages");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "NewsImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NewsImages_NewsId",
                table: "NewsImages",
                column: "NewsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsImages_NewsId",
                table: "NewsImages");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "NewsImages");

            migrationBuilder.CreateIndex(
                name: "IX_NewsImages_NewsId",
                table: "NewsImages",
                column: "NewsId",
                unique: true);
        }
    }
}
