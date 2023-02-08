using System;
using System.Collections.Generic;

namespace Kps.Integration.Api.Models.OrdersKiotViet
{
    public partial class KpsOrderKiotviet
    {
        public uint IdKps { get; set; }
        public Int64? Id { get; set; }
        public string? Code { get; set; }
        public string? SoldByName { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public double? Total { get; set; }
        public double? TotalPayment { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
