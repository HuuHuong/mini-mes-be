using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini_mes_be.Migrations.CreateModels
{
    /// <inheritdoc />
    public partial class SupportMultipleProductsInWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk__work_orders__products_product_id",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "ix__work_orders_product_id",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "defect_quantity",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "produced_quantity",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "target_quantity",
                table: "WorkOrders");

            migrationBuilder.AddColumn<int>(
                name: "product_id",
                table: "QualityChecks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WorkOrderProducts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    work_order_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    target_quantity = table.Column<int>(type: "int", nullable: false),
                    produced_quantity = table.Column<int>(type: "int", nullable: false),
                    defect_quantity = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk__work_order_products", x => x.id);
                    table.ForeignKey(
                        name: "fk__work_order_products__products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk__work_order_products__work_orders_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "WorkOrders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__quality_checks_product_id",
                table: "QualityChecks",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix__work_order_products_product_id",
                table: "WorkOrderProducts",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix__work_order_products_work_order_id",
                table: "WorkOrderProducts",
                column: "work_order_id");

            migrationBuilder.AddForeignKey(
                name: "fk__quality_checks__products_product_id",
                table: "QualityChecks",
                column: "product_id",
                principalTable: "Products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk__quality_checks__products_product_id",
                table: "QualityChecks");

            migrationBuilder.DropTable(
                name: "WorkOrderProducts");

            migrationBuilder.DropIndex(
                name: "ix__quality_checks_product_id",
                table: "QualityChecks");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "QualityChecks");

            migrationBuilder.AddColumn<int>(
                name: "defect_quantity",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "produced_quantity",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "product_id",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "target_quantity",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix__work_orders_product_id",
                table: "WorkOrders",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "fk__work_orders__products_product_id",
                table: "WorkOrders",
                column: "product_id",
                principalTable: "Products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
