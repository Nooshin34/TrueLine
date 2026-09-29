using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueLine.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "News",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "News");
        }
    }
}
