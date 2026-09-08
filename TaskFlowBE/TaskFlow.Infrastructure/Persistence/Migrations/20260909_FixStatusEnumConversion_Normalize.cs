using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    public partial class FixStatusEnumConversion_Normalize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalize and map textual enum names (trim, remove non-breaking spaces, case-insensitive)
            migrationBuilder.Sql(@"
-- Normalize WorkOrders.Status and map known names to numeric values
UPDATE dbo.WorkOrders
SET Status = CASE normalized
    WHEN 'new' THEN '0'
    WHEN 'open' THEN '1'
    WHEN 'inprogress' THEN '2'
    WHEN 'completed' THEN '3'
    WHEN 'closed' THEN '4'
    ELSE Status
END
FROM (
    SELECT Id,
           LOWER(LTRIM(RTRIM(REPLACE(REPLACE(COALESCE(Status, ''), CHAR(160), ' '), CHAR(9), ' ')))) AS normalized
    FROM dbo.WorkOrders
) AS sub
WHERE dbo.WorkOrders.Id = sub.Id
  AND TRY_CONVERT(int, dbo.WorkOrders.Status) IS NULL
  AND sub.normalized IN ('new','open','inprogress','completed','closed');

-- Normalize StatusChanges.FromStatus
UPDATE dbo.StatusChanges
SET FromStatus = CASE normalized
    WHEN 'new' THEN '0'
    WHEN 'open' THEN '1'
    WHEN 'inprogress' THEN '2'
    WHEN 'completed' THEN '3'
    WHEN 'closed' THEN '4'
    ELSE FromStatus
END
FROM (
    SELECT Id,
           LOWER(LTRIM(RTRIM(REPLACE(REPLACE(COALESCE(FromStatus, ''), CHAR(160), ' '), CHAR(9), ' ')))) AS normalized
    FROM dbo.StatusChanges
) AS sub
WHERE dbo.StatusChanges.Id = sub.Id
  AND TRY_CONVERT(int, dbo.StatusChanges.FromStatus) IS NULL
  AND sub.normalized IN ('new','open','inprogress','completed','closed');

-- Normalize StatusChanges.ToStatus
UPDATE dbo.StatusChanges
SET ToStatus = CASE normalized
    WHEN 'new' THEN '0'
    WHEN 'open' THEN '1'
    WHEN 'inprogress' THEN '2'
    WHEN 'completed' THEN '3'
    WHEN 'closed' THEN '4'
    ELSE ToStatus
END
FROM (
    SELECT Id,
           LOWER(LTRIM(RTRIM(REPLACE(REPLACE(COALESCE(ToStatus, ''), CHAR(160), ' '), CHAR(9), ' ')))) AS normalized
    FROM dbo.StatusChanges
) AS sub
WHERE dbo.StatusChanges.Id = sub.Id
  AND TRY_CONVERT(int, dbo.StatusChanges.ToStatus) IS NULL
  AND sub.normalized IN ('new','open','inprogress','completed','closed');
");

            // Safety check: abort migration if any non-convertible values remain
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE TRY_CONVERT(int, Status) IS NULL)
BEGIN
    THROW 51000, 'Non-convertible WorkOrders.Status values detected. Inspect data and retry.', 1;
END
IF EXISTS (SELECT 1 FROM dbo.StatusChanges WHERE TRY_CONVERT(int, FromStatus) IS NULL OR TRY_CONVERT(int, ToStatus) IS NULL)
BEGIN
    THROW 51000, 'Non-convertible StatusChanges status values detected. Inspect data and retry.', 1;
END
");

            // Alter columns from string to int
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
            // Revert columns back to string
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

            // Map numeric values back to names
            migrationBuilder.Sql(@"
UPDATE dbo.WorkOrders
SET Status = CASE Status
    WHEN '0' THEN 'New'
    WHEN '1' THEN 'Open'
    WHEN '2' THEN 'InProgress'
    WHEN '3' THEN 'Completed'
    WHEN '4' THEN 'Closed'
    ELSE Status
END;

UPDATE dbo.StatusChanges
SET FromStatus = CASE FromStatus
    WHEN '0' THEN 'New'
    WHEN '1' THEN 'Open'
    WHEN '2' THEN 'InProgress'
    WHEN '3' THEN 'Completed'
    WHEN '4' THEN 'Closed'
    ELSE FromStatus
END;

UPDATE dbo.StatusChanges
SET ToStatus = CASE ToStatus
    WHEN '0' THEN 'New'
    WHEN '1' THEN 'Open'
    WHEN '2' THEN 'InProgress'
    WHEN '3' THEN 'Completed'
    WHEN '4' THEN 'Closed'
    ELSE ToStatus
END;
");
        }
    }
}
