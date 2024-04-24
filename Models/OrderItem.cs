using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        [Precision(16, 2)]
        public decimal UnitPrice { get; set; }

        // navigation properties
        // public Order Order { get; set; } = new();

        // public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}