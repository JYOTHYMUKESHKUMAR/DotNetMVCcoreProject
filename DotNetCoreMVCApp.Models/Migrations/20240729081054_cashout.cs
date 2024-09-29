using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreMVCApp.Models.Migrations
{
    /// <inheritdoc />
    public partial class cashout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Project",
                table: "CashOut");

            migrationBuilder.AddColumn<int>(
                name: "CashOutId",
                table: "ProjectDatabase",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CostCenter",
                table: "CashOut",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "CashOut",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "CashOut",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDatabase_CashOutId",
                table: "ProjectDatabase",
                column: "CashOutId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDatabase_CashOut_CashOutId",
                table: "ProjectDatabase",
                column: "CashOutId",
                principalTable: "CashOut",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDatabase_CashOut_CashOutId",
                table: "ProjectDatabase");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDatabase_CashOutId",
                table: "ProjectDatabase");

            migrationBuilder.DropColumn(
                name: "CashOutId",
                table: "ProjectDatabase");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "CashOut");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "CashOut");

            migrationBuilder.AlterColumn<string>(
                name: "CostCenter",
                table: "CashOut",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Project",
                table: "CashOut",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
