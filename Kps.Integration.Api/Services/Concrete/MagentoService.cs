using System.Data;
using Kps.Integration.Api.Models.Inventories;
using Kps.Integration.Proxy.Magento.Models.Orders;

namespace Kps.Integration.Api.Services.Concrete
{
    using Dapper;
    using Kps.Integration.Api.Services.Models.Magento;
    using MySql.Data.MySqlClient;
    using System.Linq;

    public class MagentoService :
        Kps.Integration.Api.Services.IMagentoService
    {
        private const int VirtualQty = 8888;
        public string ConnectionString { get; private set; }

        public MagentoService(string connectionString)
        {
            this.ConnectionString = connectionString;

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public async Task<SalesOrder[]> GetOrderList(DateTime? lastOrderOn = null, int limit = 100)
        {
            string str;

            var sb = new System.Text.StringBuilder();

            sb.Append("select entity_id, created_at from `sales_order` ");

            if (lastOrderOn != null)
            {
                str = lastOrderOn.Value.ToString("yyyy-MM-dd HH:mm:ss");

                sb.Append("where `created_at` > '");
                sb.Append(str);
                sb.Append("' ");
            }

            sb.Append("order by `created_at` desc limit ");
            sb.Append(limit);
            sb.Append(";");

            str = sb.ToString();

            var queryResult = await this.QueryAsync<SalesOrder>(str);

            return queryResult.ToArray();
        }

        public async Task<SalesOrder[]> GetOrderForWms(DateTime? lastOrderOn = null, int limit = 50)
        {
            lastOrderOn = lastOrderOn ?? DateTime.Now.AddDays(-30);
            var query = @"
                select entity_id, created_at from `sales_order` 
                where `created_at` > @fromDate and status = 'confirmed'
                order by `created_at` desc limit @limit
            ";

            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("fromDate", lastOrderOn);
            dynamicParams.Add("limit", limit);

            using (var conn = this.CreateConnection())
            {
                var queryResult =  await conn.QueryAsync<SalesOrder>(query, dynamicParams).ConfigureAwait(false);
                return queryResult.ToArray();
            }
        }

        public async Task UpdateSaleOrderSyncStatus(int entityId, string wmsDo)
        {
            string sql = @"
                UPDATE `sales_order`
                SET synced_to_wms = 1, wms_do_code = @wmsDo
                where entity_id = @entityId;

                UPDATE `sales_order_grid`
                SET synced_to_wms = 1
                where entity_id = @entityId;
            ";

            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("entityId", entityId);
            dynamicParams.Add("wmsDo", wmsDo);

            using (var conn = this.CreateConnection())
            {
                await conn.ExecuteAsync(sql, dynamicParams).ConfigureAwait(false);
            }
        }

        public async Task UpdateSaleOrderProcessing(int entityId)
        {
            string sql = @"
                UPDATE `sales_order`
                SET status = 'processing', state = 'processing'
                where entity_id = @entityId;

                UPDATE `sales_order_grid`
                SET status = 'processing'
                where entity_id = @entityId;
            ";

            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("entityId", entityId);

            using (var conn = this.CreateConnection())
            {
                await conn.ExecuteAsync(sql, dynamicParams).ConfigureAwait(false);
            }
        }

        public async Task<int> UpdateInventoryQuantity(string sku, double quantity)
        {
            string sql = @"
                UPDATE cataloginventory_stock_item item_stock, cataloginventory_stock_status status_stock
                SET item_stock.qty = @Qty, item_stock.is_in_stock = IF(@Qty > 0, 1, 0),
                status_stock.qty = @Qty, status_stock.stock_status = IF(@Qty > 0, 1, 0)
                WHERE 
	                item_stock.product_id = status_stock.product_id
	                AND
                    item_stock.product_id in (select entity_id from `catalog_product_entity` where sku = @SKU);
                UPDATE  `inventory_source_item`
                SET quantity = @Qty 
                where sku = @SKU;
            ";

            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("SKU", sku);
            dynamicParams.Add("Qty", quantity);

            using (var conn = this.CreateConnection())
            {
                return await conn.ExecuteAsync(sql, dynamicParams).ConfigureAwait(false);

            }
        }

        public async Task<int> AddInventory(UpdateInventoryParams request)
        {
            string sql = @"
                UPDATE cataloginventory_stock_item item_stock, cataloginventory_stock_status status_stock
                SET item_stock.qty = CASE
                            WHEN item_stock.qty < @VirtualQty THEN  item_stock.qty + @Qty 
                            ELSE @Qty
                        END
                        , item_stock.is_in_stock = IF(item_stock.qty + @Qty > 0, 1, 0)
                        , status_stock.qty = CASE  
                            WHEN status_stock.qty < @VirtualQty THEN  status_stock.qty + @Qty 
                            ELSE @Qty
                        END, 
                    status_stock.stock_status = IF(status_stock.qty + @Qty > 0, 1, 0)
                WHERE 
	                item_stock.product_id = status_stock.product_id
	                AND item_stock.product_id in (select entity_id from `catalog_product_entity` where sku = @SKU);

                UPDATE  `inventory_source_item`
                SET quantity = CASE  
                            WHEN quantity < @VirtualQty THEN  quantity + @Qty 
                            ELSE @Qty
                        END
                where sku = @SKU;
            ";

            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("SKU", request.Sku);
            dynamicParams.Add("Qty", request.Quantity);
            dynamicParams.Add("VirtualQty", VirtualQty);

            using (var conn = this.CreateConnection())
            {
                return await conn.ExecuteAsync(sql, dynamicParams).ConfigureAwait(false);
            }
        }

        public async Task<int> SubtractInventory(UpdateInventoryParams request)
        {
            string sql = @"
                UPDATE cataloginventory_stock_item item_stock, cataloginventory_stock_status status_stock
                SET item_stock.qty = CASE
                            WHEN item_stock.qty < @VirtualQty THEN  item_stock.qty - @Qty 
                            ELSE item_stock.qty
                        END
                    , item_stock.is_in_stock = IF(item_stock.qty - @Qty > 0, 1, 0)
                    , status_stock.qty =  CASE
                            WHEN status_stock.qty < @VirtualQty THEN  status_stock.qty - @Qty 
                            ELSE status_stock.qty
                        END
                    , status_stock.stock_status = IF(status_stock.qty - @Qty > 0, 1, 0)
                WHERE 
                    item_stock.product_id = status_stock.product_id
                    AND item_stock.product_id in (select entity_id from `catalog_product_entity` where sku = @SKU);

                UPDATE  `inventory_source_item`
                SET quantity = CASE  
                            WHEN quantity < @VirtualQty THEN  quantity - @Qty 
                            ELSE quantity
                        END
                where sku = @SKU;
            ";

            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("SKU", request.Sku);
            dynamicParams.Add("Qty", request.Quantity);
            dynamicParams.Add("VirtualQty", VirtualQty);

            using (var conn = this.CreateConnection())
            {
                return await conn.ExecuteAsync(sql, dynamicParams).ConfigureAwait(false);
            }
        }

        public async Task<double> GetOrderedQtyByOrderStatus(string sku, List<string>? statuses = null)
        {
            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("sku", sku, DbType.String);

            string sql = @"
                SELECT SUM(main_table.qty_ordered) AS qty_ordered
                FROM sales_order_item AS main_table
                INNER JOIN sales_order AS so ON main_table.order_id = so.entity_id 
                INNER JOIN catalog_product_entity AS p ON main_table.product_id = p.entity_id 
                WHERE (p.sku = @sku)
            ";

            if (statuses != null)
            {
                sql += $" AND (so.status in ({string.Join(',', statuses)}))";
            }
            else
            {
                sql += $" AND (so.status != 'canceled')";
            }
            sql += " GROUP BY main_table.product_id";

            using (var conn = this.CreateConnection())
            {
                return await conn.ExecuteScalarAsync<double>(sql, dynamicParams).ConfigureAwait(false);
            }
        }

        public async Task<double> GetOrderedQtyTransaction(string sku, List<string>? statuses = null)
        {
            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("sku", sku, DbType.String);

            string sql = @"
                SELECT SUM(quantity) FROM inventory_reservation AS ir
                WHERE (ir.sku = @sku)
            ";

            using (var conn = this.CreateConnection())
            {
                return await conn.ExecuteScalarAsync<double>(sql, dynamicParams).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(string sql, bool continueOnCapturedContext = false)
            where T : class
        {
            var conn = this.CreateConnection();

            using (conn)
            {
                return await conn.QueryAsync<T>(sql).ConfigureAwait(continueOnCapturedContext);
            }
        }

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(this.ConnectionString);
        }
    }
}