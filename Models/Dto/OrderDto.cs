using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreApi.Models.Dto
{
    public class OrderDto
    {
        public List<BasketDto> CartItems { get; set; } = new List<BasketDto>();

        [Required, MinLength(5), MaxLength(100)]
        public string? DeliveryAddress { get; set; }
        [Required, MaxLength(100)]
        public string? PaymentMethod { get; set; }
    }
}