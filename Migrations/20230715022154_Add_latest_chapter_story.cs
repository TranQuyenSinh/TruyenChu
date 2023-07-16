using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace truyenchu.Migrations
{
    /// <inheritdoc />
    public partial class Add_latest_chapter_story : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LatestChapterOrder",
                table: "Story",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatestChapterOrder",
                table: "Story");
        }
    }
}
