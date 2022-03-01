using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyClient.Migrations
{
    public partial class updateOutboxMessageDataTypr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Message",
                schema: "RedisOutbox",
                table: "OutboxMessages",
                type: "VARCHAR(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Message",
                schema: "RedisOutbox",
                table: "OutboxMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(MAX)");
        }
    }
}
