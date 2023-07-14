using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace truyenchu.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLuotXemStory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Story",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Story");
        }
    }
}
