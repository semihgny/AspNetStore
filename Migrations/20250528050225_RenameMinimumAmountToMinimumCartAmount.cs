using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet.Migrations
{
    /// <inheritdoc />
    public partial class RenameMinimumAmountToMinimumCartAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Coupons");

            migrationBuilder.RenameColumn(
                name: "UsageCount",
                table: "Coupons",
                newName: "UsedCount");

            migrationBuilder.RenameColumn(
                name: "MinimumAmount",
                table: "Coupons",
                newName: "MinimumCartAmount");

            migrationBuilder.RenameColumn(
                name: "MaximumDiscountAmount",
                table: "Coupons",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "UsageLimit",
                table: "Coupons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UsedCount",
                table: "Coupons",
                newName: "UsageCount");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Coupons",
                newName: "MaximumDiscountAmount");

            migrationBuilder.RenameColumn(
                name: "MinimumCartAmount",
                table: "Coupons",
                newName: "MinimumAmount");

            migrationBuilder.AlterColumn<int>(
                name: "UsageLimit",
                table: "Coupons",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Coupons",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
