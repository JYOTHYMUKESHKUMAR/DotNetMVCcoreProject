using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCoreMVCApp.Models.Migrations
{
    /// <inheritdoc />
    public partial class Third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDatabase_AspNetUsers_CreatedBy",
                table: "ProjectDatabase");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDatabase_AspNetUsers_DeletedBy",
                table: "ProjectDatabase");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDatabase_AspNetUsers_UpdatedBy",
                table: "ProjectDatabase");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDatabase_CreatedBy",
                table: "ProjectDatabase");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDatabase_DeletedBy",
                table: "ProjectDatabase");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDatabase_UpdatedBy",
                table: "ProjectDatabase");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "ProjectDatabase",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "ProjectDatabase",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ProjectDatabase",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "ProjectDatabase",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "ProjectDatabase",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ProjectDatabase",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDatabase_CreatedBy",
                table: "ProjectDatabase",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDatabase_DeletedBy",
                table: "ProjectDatabase",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDatabase_UpdatedBy",
                table: "ProjectDatabase",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDatabase_AspNetUsers_CreatedBy",
                table: "ProjectDatabase",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDatabase_AspNetUsers_DeletedBy",
                table: "ProjectDatabase",
                column: "DeletedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDatabase_AspNetUsers_UpdatedBy",
                table: "ProjectDatabase",
                column: "UpdatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
