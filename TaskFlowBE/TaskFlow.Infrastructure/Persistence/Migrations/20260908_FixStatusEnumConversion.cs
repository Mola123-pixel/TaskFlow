using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    public partial class FixStatusEnumConversion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Map string enum names to numeric values
            migrationBuilder.Sql(@"UPDATE dbo.WorkOrders
SET Status =
    CASE Status
        WHEN 'New' THEN '0'
        WHEN 'Open' THEN '1'
        WHEN 'InProgress' THEN '2'
        WHEN 'Completed' THEN '3'
        WHEN 'Closed' THEN '4'
        ELSE Status
    END");

            migrationBuilder.Sql(@"UPDATE dbo.StatusChanges
SET FromStatus =
    CASE FromStatus
        WHEN 'New' THEN '0'
        WHEN 'Open' THEN '1'
        WHEN 'InProgress' THEN '2'
        WHEN 'Completed' THEN '3'
        WHEN 'Closed' THEN '4'
        ELSE FromStatus
    END");

            migrationBuilder.Sql(@"UPDATE dbo.StatusChanges
SET ToStatus =
    CASE ToStatus
        WHEN 'New' THEN '0'
        WHEN 'Open' THEN '1'
        WHEN 'InProgress' THEN '2'
        WHEN 'Completed' THEN '3'
        WHEN 'Closed' THEN '4'
        ELSE ToStatus
    END");

            // Alter columns from nvarchar to int
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "FromStatus",
                table: "StatusChanges",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "ToStatus",
                table: "StatusChanges",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert columns back to string and map numeric values to names
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorkOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FromStatus",
                table: "StatusChanges",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ToStatus",
                table: "StatusChanges",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"UPDATE dbo.WorkOrders
SET Status =
    CASE Status
        WHEN '0' THEN 'New'
        WHEN '1' THEN 'Open'
        WHEN '2' THEN 'InProgress'
        WHEN '3' THEN 'Completed'
        WHEN '4' THEN 'Closed'
        ELSE Status
    END");

            migrationBuilder.Sql(@"UPDATE dbo.StatusChanges
SET FromStatus =
    CASE FromStatus
        WHEN '0' THEN 'New'
        WHEN '1' THEN 'Open'
        WHEN '2' THEN 'InProgress'
        WHEN '3' THEN 'Completed'
        WHEN '4' THEN 'Closed'
        ELSE FromStatus
    END");

            migrationBuilder.Sql(@"UPDATE dbo.StatusChanges
SET ToStatus =
    CASE ToStatus
        WHEN '0' THEN 'New'
        WHEN '1' THEN 'Open'
        WHEN '2' THEN 'InProgress'
        WHEN '3' THEN 'Completed'
        WHEN '4' THEN 'Closed'
        ELSE ToStatus
    END");
        }
    }
}
