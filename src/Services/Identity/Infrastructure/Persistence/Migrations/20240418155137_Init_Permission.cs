using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IDP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init_Permission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "e1f9e918-db7c-4d8e-8cbc-40cfd38e9582");

            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "f424f123-c473-4e0f-97d6-a7d670a2fad7");

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Function = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Command = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    RoleId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "8737c8a8-69c8-493a-99ab-7e1eba4a322f", null, "Administrator", "ADMINISTRATOR" },
                    { "95e8c2fa-024f-487e-96f9-1f28e1dbd632", null, "Customer", "CUSTOMER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Function_Command_RoleId",
                schema: "Identity",
                table: "Permissions",
                columns: new[] { "Function", "Command", "RoleId" },
                unique: true,
                filter: "[Function] IS NOT NULL AND [Command] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleId",
                schema: "Identity",
                table: "Permissions",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "Identity");

            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "8737c8a8-69c8-493a-99ab-7e1eba4a322f");

            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "95e8c2fa-024f-487e-96f9-1f28e1dbd632");

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "e1f9e918-db7c-4d8e-8cbc-40cfd38e9582", null, "Administrator", "ADMINISTRATOR" },
                    { "f424f123-c473-4e0f-97d6-a7d670a2fad7", null, "Customer", "CUSTOMER" }
                });
        }
    }
}
