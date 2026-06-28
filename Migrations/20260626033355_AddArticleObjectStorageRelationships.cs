using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medium.Api.Migrations
{
  /// <inheritdoc />
  public partial class AddArticleObjectStorageRelationships : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<int>(
          name: "size",
          table: "object_storages",
          type: "int",
          nullable: true,
          oldClrType: typeof(int),
          oldType: "int");

      migrationBuilder.AddColumn<Guid>(
          name: "article_id",
          table: "object_storages",
          type: "uniqueidentifier",
          nullable: true);

      migrationBuilder.AddColumn<Guid>(
          name: "thumbnail_id",
          table: "articles",
          type: "uniqueidentifier",
          nullable: true);

      migrationBuilder.CreateIndex(
          name: "i_x_object_storages_article_id",
          table: "object_storages",
          column: "article_id");

      migrationBuilder.CreateIndex(
          name: "i_x_articles_thumbnail_id",
          table: "articles",
          column: "thumbnail_id");

      migrationBuilder.AddForeignKey(
          name: "f_k_articles_object_storages_thumbnail_id",
          table: "articles",
          column: "thumbnail_id",
          principalTable: "object_storages",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);

      migrationBuilder.AddForeignKey(
          name: "f_k_object_storages_articles_article_id",
          table: "object_storages",
          column: "article_id",
          principalTable: "articles",
          principalColumn: "id",
          onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "f_k_articles_object_storages_thumbnail_id",
          table: "articles");

      migrationBuilder.DropForeignKey(
          name: "f_k_object_storages_articles_article_id",
          table: "object_storages");

      migrationBuilder.DropIndex(
          name: "i_x_object_storages_article_id",
          table: "object_storages");

      migrationBuilder.DropIndex(
          name: "i_x_articles_thumbnail_id",
          table: "articles");

      migrationBuilder.DropColumn(
          name: "article_id",
          table: "object_storages");

      migrationBuilder.DropColumn(
          name: "thumbnail_id",
          table: "articles");

      migrationBuilder.AlterColumn<int>(
          name: "size",
          table: "object_storages",
          type: "int",
          nullable: false,
          defaultValue: 0,
          oldClrType: typeof(int),
          oldType: "int",
          oldNullable: true);
    }
  }
}