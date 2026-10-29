using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddPartitionKeyToOutboxMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "partition_key",
                schema: "orderflow_payments",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "partition_key",
                schema: "orderflow_payments",
                table: "outbox_messages");
        }
    }
}
