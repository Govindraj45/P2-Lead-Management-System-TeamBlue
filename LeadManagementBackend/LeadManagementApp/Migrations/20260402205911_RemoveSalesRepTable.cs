using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSalesRepTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_SalesRepresentatives_AssignedToRepId",
                table: "Leads");

            migrationBuilder.DropTable(
                name: "SalesRepresentatives");

            migrationBuilder.DropColumn(
                name: "SalesRepId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "AssignedToRepId",
                table: "Leads",
                newName: "AssignedSalesRepId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_AssignedToRepId",
                table: "Leads",
                newName: "IX_Leads_AssignedSalesRepId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Users_AssignedSalesRepId",
                table: "Leads",
                column: "AssignedSalesRepId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Users_AssignedSalesRepId",
                table: "Leads");

            migrationBuilder.RenameColumn(
                name: "AssignedSalesRepId",
                table: "Leads",
                newName: "AssignedToRepId");

            migrationBuilder.RenameIndex(
                name: "IX_Leads_AssignedSalesRepId",
                table: "Leads",
                newName: "IX_Leads_AssignedToRepId");

            migrationBuilder.AddColumn<int>(
                name: "SalesRepId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesRepresentatives",
                columns: table => new
                {
                    RepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesRepresentatives", x => x.RepId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_SalesRepresentatives_AssignedToRepId",
                table: "Leads",
                column: "AssignedToRepId",
                principalTable: "SalesRepresentatives",
                principalColumn: "RepId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
