using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedDate",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ApprovedByUserId",
                table: "Tasks",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_ApprovedByUserId",
                table: "Tasks",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_ApprovedByUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ApprovedByUserId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StartedDate",
                table: "Tasks");
        }
    }
}
