using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookNeighbors.API.Migrations
{
    /// <inheritdoc />
    public partial class UniqueLibraryPerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_user_id",
                table: "Libraries");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_user_id",
                table: "Libraries",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_user_id",
                table: "Libraries");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_user_id",
                table: "Libraries",
                column: "user_id");
        }
    }
}
