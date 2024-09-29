using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreMVCApp.Models.Migrations
{
    /// <inheritdoc />
    public partial class Eight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Project",
            //    table: "CashIn");

            migrationBuilder.AddColumn<int>(
                name: "CashInId",
                table: "ProjectDatabase",
                type: "int",
                nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "ProjectId",
            //    table: "CashIn",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "ProjectName",
            //    table: "CashIn",
            //    type: "nvarchar(max)",
            //    nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDatabase_CashInId",
                table: "ProjectDatabase",
                column: "CashInId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDatabase_CashIn_CashInId",
                table: "ProjectDatabase",
                column: "CashInId",
                principalTable: "CashIn",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDatabase_CashIn_CashInId",
                table: "ProjectDatabase");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDatabase_CashInId",
                table: "ProjectDatabase");

            migrationBuilder.DropColumn(
                name: "CashInId",
                table: "ProjectDatabase");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "CashIn");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "CashIn");

            migrationBuilder.AddColumn<string>(
                name: "Project",
                table: "CashIn",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
