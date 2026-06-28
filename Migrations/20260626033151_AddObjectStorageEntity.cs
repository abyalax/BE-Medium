using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medium.Api.Migrations
{
  /// <inheritdoc />
  public partial class AddObjectStorageEntity : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "object_storages",
          columns: table => new
          {
            id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            author_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            bucket = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            object_key = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            original_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
            size = table.Column<int>(type: "int", nullable: false),
            access_types = table.Column<string>(type: "nvarchar(max)", nullable: false),
            created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
            updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
            deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("p_k_object_storages", x => x.id);
            table.ForeignKey(
                      name: "f_k_object_storages_users_author_id",
                      column: x => x.author_id,
                      principalTable: "users",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
          });

      migrationBuilder.CreateIndex(
          name: "i_x_object_storages_author_id",
          table: "object_storages",
          column: "author_id");

      migrationBuilder.CreateIndex(
          name: "i_x_object_storages_object_key",
          table: "object_storages",
          column: "object_key",
          unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "object_storages");
    }
  }
}