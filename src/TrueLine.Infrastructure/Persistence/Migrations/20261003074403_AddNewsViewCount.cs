using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueLine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "News",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "News");
        }
    }
}
