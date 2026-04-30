using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    FullName = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(180)", maxLength: 180, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CATEGORIES",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    UserId = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CATEGORIES_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRANSACTIONS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    UserId = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    CategoryId = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    Amount = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDateUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Type = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: false),
                    IsDeleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACTIONS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRANSACTIONS_CATEGORIES_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CATEGORIES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRANSACTIONS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRANSACTION_HISTORY",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    UserId = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    TransactionId = table.Column<string>(type: "VARCHAR2(36)", nullable: false),
                    Action = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: false),
                    PreviousValues = table.Column<string>(type: "CLOB", nullable: true),
                    CurrentValues = table.Column<string>(type: "CLOB", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACTION_HISTORY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRANSACTION_HISTORY_TRANSACTIONS_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "TRANSACTIONS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRANSACTION_HISTORY_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "USERS",
                columns: new[] { "Id", "CreatedAtUtc", "Email", "FullName", "UpdatedAtUtc" },
                values: new object[] { "11111111-1111-1111-1111-111111111111", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "demo@fintrack.local", "FinTrack Demo User", null });

            migrationBuilder.InsertData(
                table: "CATEGORIES",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "Name", "UpdatedAtUtc", "UserId" },
                values: new object[,]
                {
                    { "22222222-2222-2222-2222-222222222221", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Default income category", "Salary", null, "11111111-1111-1111-1111-111111111111" },
                    { "22222222-2222-2222-2222-222222222222", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Default expense category", "Bills", null, "11111111-1111-1111-1111-111111111111" },
                    { "22222222-2222-2222-2222-222222222223", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Default savings category", "Savings", null, "11111111-1111-1111-1111-111111111111" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORIES_UserId",
                table: "CATEGORIES",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTION_HISTORY_TransactionId",
                table: "TRANSACTION_HISTORY",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTION_HISTORY_UserId",
                table: "TRANSACTION_HISTORY",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTIONS_CategoryId",
                table: "TRANSACTIONS",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTIONS_UserId",
                table: "TRANSACTIONS",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TRANSACTION_HISTORY");

            migrationBuilder.DropTable(
                name: "TRANSACTIONS");

            migrationBuilder.DropTable(
                name: "CATEGORIES");

            migrationBuilder.DropTable(
                name: "USERS");
        }
    }
}
