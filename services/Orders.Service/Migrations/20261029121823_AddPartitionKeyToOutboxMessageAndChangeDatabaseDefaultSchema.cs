using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddPartitionKeyToOutboxMessageAndChangeDatabaseDefaultSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orderflow_orders");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                newName: "outbox_messages",
                newSchema: "orderflow_orders");

            migrationBuilder.RenameTable(
                name: "orders",
                newName: "orders",
                newSchema: "orderflow_orders");

            migrationBuilder.RenameTable(
                name: "order_saga_states",
                newName: "order_saga_states",
                newSchema: "orderflow_orders");

            migrationBuilder.RenameTable(
                name: "order_lines",
                newName: "order_lines",
                newSchema: "orderflow_orders");

            migrationBuilder.RenameTable(
                name: "inbox_messages",
                newName: "inbox_messages",
                newSchema: "orderflow_orders");

            migrationBuilder.AddColumn<string>(
                name: "partition_key",
                schema: "orderflow_orders",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "orderflow_orders",
                table: "orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "partition_key",
                schema: "orderflow_orders",
                table: "outbox_messages");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                schema: "orderflow_orders",
                newName: "outbox_messages");

            migrationBuilder.RenameTable(
                name: "orders",
                schema: "orderflow_orders",
                newName: "orders");

            migrationBuilder.RenameTable(
                name: "order_saga_states",
                schema: "orderflow_orders",
                newName: "order_saga_states");

            migrationBuilder.RenameTable(
                name: "order_lines",
                schema: "orderflow_orders",
                newName: "order_lines");

            migrationBuilder.RenameTable(
                name: "inbox_messages",
                schema: "orderflow_orders",
                newName: "inbox_messages");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
