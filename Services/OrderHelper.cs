using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BestStoreApi.Models.Dto;

namespace BestStoreApi.Services
{
    public class OrderHelper
    {
        public static Decimal ShippingFee { get; } = 5;

        public static Dictionary<string, string> PaymentMethods { get; } = new()
        {
            {"cash" , "Cash On Delivery"},
            {"card", "Credit Card"},
        };

        public static List<string> PaymentStatusess { get; } = new()
        {
            "Pending", "Accepted", "Cancelled"
        };

        public static List<string> OrderStatusess { get; } = new()
        {
            "Created", "Accepted", "Cancelled", "Shipped", "Delivered", "Returned"
        };
        /*
            retrieve productIdentifier seperated by - like this [1-2-1-4-5-2]

            and return dictionary like this 
            {
                1 : 2,
                2 : 2,
                4 : 1,
                5 : 1,
            }
        */
        public static Dictionary<int, int> GetProductDictionary(string productIdentifier)
        {
            var productDictionary = new Dictionary<int, int>();

            if (productIdentifier.Length > 0)
            {
                string[] productIdArray = productIdentifier.Split('-');

                foreach (var productId in productIdArray)
                {
                    int id = int.Parse(productId);

                    try
                    {
                        if (productDictionary.ContainsKey(id))
                        {
                            productDictionary[id] += 1;
                        }
                        else
                        {
                            productDictionary.Add(id, 1);
                        }
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }
                }
            }



            return productDictionary;
        }

        public static List<BasketDto> GetCartProducts(List<BasketDto> basketDto)
        {
            List<int> ListProductsIds = new List<int>();
            int productId;
            int quantity;

            List<BasketDto> newBasket = new List<BasketDto>();

            foreach (var basket in basketDto)
            {
                productId = (int)basket.ProductId;
                quantity = (int)basket.Quantity;

                if (!ListProductsIds.Contains(productId))
                {
                    ListProductsIds.Add(productId);

                    newBasket.Add(basket);
                }
                else
                {
                    // int newQty = basket.Quantity += quantity;

                    var updateBasket = newBasket.Where(b => b.ProductId == basket.ProductId)
                             //  .Where(b => b.Quantity == basket.Quantity)
                             .First();

                    // return Ok(updateBasket);
                    // if(updateBasket)
                    // {

                    // }
                    updateBasket.Quantity += basket.Quantity;
                    // newBasket.Remove(basket);
                    // basketDto.Remove(basket);
                }
            }

            return newBasket;
        }
    }
}