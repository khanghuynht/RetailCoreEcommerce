using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailCoreEcommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
