using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolveMachine.Migrations
{
    /// <inheritdoc />
    public partial class DeletedisCompletedfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_completed",
                table: "Problem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_completed",
                table: "Problem",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
