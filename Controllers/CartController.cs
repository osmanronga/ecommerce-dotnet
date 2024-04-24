using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BestStoreApi.Models.Dto;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public CartController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public IActionResult Cart(List<BasketDto> basketDto)
        {
            CartDto cartDto = new CartDto();
            cartDto.CartItems = new List<CartItemDto>();
            cartDto.SubToral = 0;
            cartDto.ShippingFee = OrderHelper.ShippingFee;
            cartDto.TotalPrice = 0;

            List<BasketDto> newBasket = OrderHelper.GetCartProducts(basketDto);

            foreach (var item in newBasket)
            {
                int productId = item.ProductId;
                var product = context.Products.Find(productId);
                if (product == null)
                {
                    continue;
                }

                CartItemDto cartItemDto = new CartItemDto();
                cartItemDto.Product = product;
                cartItemDto.Quantity = item.Quantity;

                cartDto.CartItems.Add(cartItemDto);
                cartDto.SubToral += product.Price * item.Quantity;

            }

            cartDto.TotalPrice = cartDto.SubToral + cartDto.ShippingFee;

            return Ok(cartDto);
        }


        [HttpGet("PaymentMethods")]
        public IActionResult PaymentMethods()
        {
            return Ok(OrderHelper.PaymentMethods);
        }

        [HttpPost("NewCart")]
        public IActionResult NewCart(List<BasketNewDto> basketNewDto)
        {
            List<int> ListProductsIds = new List<int>();
            int productId;
            int quantity;

            var countBasket = basketNewDto.Count();

            List<BasketItem> newBasket = new List<BasketItem>();

            var basket = new BasketItem();

            // foreach (var bas in basketNewDto)

            for (int i = 0; i < countBasket; i++)
            {

                productId = basketNewDto[i].CartItems[i].ProductId;
                quantity = basketNewDto[i].CartItems[i].Quantity;

                basket.ProductId = productId;
                basket.Quantity = quantity;


                if (!ListProductsIds.Contains(productId))
                {
                    ListProductsIds.Add(productId);

                    newBasket.Add(basket);
                }
                else
                {
                    // int newQty = basket.Quantity += quantity;

                    var updateBasket = newBasket.Where(b => b.ProductId == productId)
                             //  .Where(b => b.Quantity == basket.Quantity)
                             .First();

                    // return Ok(updateBasket);
                    // if(updateBasket)
                    // {

                    // }
                    updateBasket.Quantity += quantity;
                    // newBasket.Remove(basket);
                    // basketDto.Remove(basket);
                }

                // foreach (var b in bas.CartItems)
                // {
                //     int productId= b.ProductId;
                // }
            }
            return Ok(ListProductsIds);
        }

        // ~CartController()
        // {
        //     GC.Collect();
        // }
    }
}