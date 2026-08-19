using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations;

public partial class AddMonthlyPlansModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Tables are currently created via idempotent SQL in DatabaseMigrationExtensions.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // No-op by design for now.
    }
}
