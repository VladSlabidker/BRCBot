using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.SQL.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkToReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Receipts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "Receipts");
        }
    }
}
