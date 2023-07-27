using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace truyenchu.Migrations
{
    /// <inheritdoc />
    public partial class StoryAdd_Published : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Story_StorySlug",
                table: "Story");

            migrationBuilder.AlterColumn<string>(
                name: "StorySlug",
                table: "Story",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<bool>(
                name: "Published",
                table: "Story",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Story_StorySlug",
                table: "Story",
                column: "StorySlug",
                unique: true,
                filter: "[StorySlug] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Story_StorySlug",
                table: "Story");

            migrationBuilder.DropColumn(
                name: "Published",
                table: "Story");

            migrationBuilder.AlterColumn<string>(
                name: "StorySlug",
                table: "Story",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Story_StorySlug",
                table: "Story",
                column: "StorySlug",
                unique: true);
        }
    }
}
