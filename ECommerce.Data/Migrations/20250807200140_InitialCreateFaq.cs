using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateFaq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FAQs_Category",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_IsActive_Category",
                table: "FAQs");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "FAQs",
                newName: "Author");

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "FAQs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "FAQs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HelpfulCount",
                table: "FAQs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHelpful",
                table: "FAQs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NotHelpfulCount",
                table: "FAQs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "FAQs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FAQCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_CategoryId",
                table: "FAQs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_IsActive_CategoryId",
                table: "FAQs",
                columns: new[] { "IsActive", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_ViewCount",
                table: "FAQs",
                column: "ViewCount");

            migrationBuilder.CreateIndex(
                name: "IX_FAQCategories_DisplayOrder",
                table: "FAQCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FAQCategories_IsActive",
                table: "FAQCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FAQCategories_Name",
                table: "FAQCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FAQs_FAQCategories_CategoryId",
                table: "FAQs",
                column: "CategoryId",
                principalTable: "FAQCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FAQs_FAQCategories_CategoryId",
                table: "FAQs");

            migrationBuilder.DropTable(
                name: "FAQCategories");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_CategoryId",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_IsActive_CategoryId",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_ViewCount",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "HelpfulCount",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "IsHelpful",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "NotHelpfulCount",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "FAQs");

            migrationBuilder.RenameColumn(
                name: "Author",
                table: "FAQs",
                newName: "Category");

            migrationBuilder.AlterColumn<string>(
                name: "Answer",
                table: "FAQs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_Category",
                table: "FAQs",
                column: "Category",
                filter: "[Category] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_IsActive_Category",
                table: "FAQs",
                columns: new[] { "IsActive", "Category" },
                filter: "[Category] IS NOT NULL");
        }
    }
}
