using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreApi.Models.Dto
{
    public class CartDto
    {
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
        public decimal SubToral { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalPrice { get; set; }
    }
}