using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini_mes_be.Migrations.CreateModels
{
    /// <inheritdoc />
    public partial class AddMesCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk__machines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk__products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    machine_id = table.Column<int>(type: "int", nullable: false),
                    target_quantity = table.Column<int>(type: "int", nullable: false),
                    produced_quantity = table.Column<int>(type: "int", nullable: false),
                    defect_quantity = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    planned_start = table.Column<long>(type: "bigint", nullable: true),
                    planned_end = table.Column<long>(type: "bigint", nullable: true),
                    actual_start = table.Column<long>(type: "bigint", nullable: true),
                    actual_end = table.Column<long>(type: "bigint", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_by_user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk__work_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk__work_orders__machines_machine_id",
                        column: x => x.machine_id,
                        principalTable: "Machines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk__work_orders__products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk__work_orders__users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    work_order_id = table.Column<int>(type: "int", nullable: true),
                    reference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk__inventory_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk__inventory_transactions__products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk__inventory_transactions__users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk__inventory_transactions__work_orders_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "WorkOrders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "QualityChecks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    work_order_id = table.Column<int>(type: "int", nullable: false),
                    inspected_quantity = table.Column<int>(type: "int", nullable: false),
                    passed_quantity = table.Column<int>(type: "int", nullable: false),
                    failed_quantity = table.Column<int>(type: "int", nullable: false),
                    result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    inspector_user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk__quality_checks", x => x.id);
                    table.ForeignKey(
                        name: "fk__quality_checks__users_inspector_user_id",
                        column: x => x.inspector_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk__quality_checks__work_orders_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "WorkOrders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderLogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    work_order_id = table.Column<int>(type: "int", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    old_value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk__work_order_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk__work_order_logs__users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk__work_order_logs__work_orders_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "WorkOrders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__inventory_transactions_product_id",
                table: "InventoryTransactions",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix__inventory_transactions_user_id",
                table: "InventoryTransactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix__inventory_transactions_work_order_id",
                table: "InventoryTransactions",
                column: "work_order_id");

            migrationBuilder.CreateIndex(
                name: "ix__machines_code",
                table: "Machines",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__products_sku",
                table: "Products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__quality_checks_inspector_user_id",
                table: "QualityChecks",
                column: "inspector_user_id");

            migrationBuilder.CreateIndex(
                name: "ix__quality_checks_work_order_id",
                table: "QualityChecks",
                column: "work_order_id");

            migrationBuilder.CreateIndex(
                name: "ix__work_order_logs_user_id",
                table: "WorkOrderLogs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix__work_order_logs_work_order_id",
                table: "WorkOrderLogs",
                column: "work_order_id");

            migrationBuilder.CreateIndex(
                name: "ix__work_orders_created_by_user_id",
                table: "WorkOrders",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix__work_orders_machine_id",
                table: "WorkOrders",
                column: "machine_id");

            migrationBuilder.CreateIndex(
                name: "ix__work_orders_order_number",
                table: "WorkOrders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__work_orders_product_id",
                table: "WorkOrders",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "QualityChecks");

            migrationBuilder.DropTable(
                name: "WorkOrderLogs");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
