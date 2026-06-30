using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medium.Api.Migrations
{
  /// <inheritdoc />
  public partial class AddSummaryToArticle : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "summary",
          table: "articles",
          type: "nvarchar(1000)",
          maxLength: 1000,
          nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "summary",
          table: "articles");
    }
  }
}