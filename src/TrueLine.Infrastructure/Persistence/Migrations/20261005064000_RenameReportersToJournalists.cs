using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrueLine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameReportersToJournalists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Reporters",
                newName: "Journalists");

            migrationBuilder.RenameIndex(
                name: "IX_Reporters_Email",
                table: "Journalists",
                newName: "IX_Journalists_Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Journalists_Email",
                table: "Journalists",
                newName: "IX_Reporters_Email");

            migrationBuilder.RenameTable(
                name: "Journalists",
                newName: "Reporters");
        }
    }
}
