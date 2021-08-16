using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace vue_expenses_api.Migrations
{
    public partial class PaidForTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseTypes_Expenses_ExpenseId",
                table: "ExpenseTypes");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseTypes_ExpenseId",
                table: "ExpenseTypes");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "ExpenseTypes");

            migrationBuilder.CreateTable(
                name: "ExpensePaidFor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExpenseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExpenseTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Archived = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpensePaidFor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpensePaidFor_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpensePaidFor_ExpenseTypes_ExpenseTypeId",
                        column: x => x.ExpenseTypeId,
                        principalTable: "ExpenseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpensePaidFor_ExpenseId",
                table: "ExpensePaidFor",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpensePaidFor_ExpenseTypeId",
                table: "ExpensePaidFor",
                column: "ExpenseTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpensePaidFor");

            migrationBuilder.AddColumn<int>(
                name: "ExpenseId",
                table: "ExpenseTypes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseTypes_ExpenseId",
                table: "ExpenseTypes",
                column: "ExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseTypes_Expenses_ExpenseId",
                table: "ExpenseTypes",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
