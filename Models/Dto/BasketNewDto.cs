using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreApi.Models.Dto
{
    public class BasketNewDto
    {
        public List<BasketItem> CartItems { get; set; } = new List<BasketItem>();

    }
}