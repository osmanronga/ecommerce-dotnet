using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreApi.Models.Dto
{
    public class CartItemDto
    {
        [Required]
        public Product Product { get; set; } = new Product();
        [Required]
        public int Quantity { get; set; }
    }
}