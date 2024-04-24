using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BestStoreApi.Models;
using BestStoreApi.Models.Dto;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public OrderController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize]
        [HttpPost]
        public IActionResult Order(OrderDto orderDto)
        {
            if (!OrderHelper.PaymentMethods.ContainsKey(orderDto.PaymentMethod!))
            {
                ModelState.AddModelError("PaymentMethod", "please select a valid payment method");
                return BadRequest(ModelState);
            }

            int userId = JwtReader.GetUserId(User);
            var user = context.Users.Find(userId);
            if (user == null)
            {
                ModelState.AddModelError("Error", "Please Login in into Your Account");
                return BadRequest(ModelState);
            }

            if (orderDto.CartItems.Count <= 0)
            {
                ModelState.AddModelError("Error", "Please select a product to make a purchase");
                return BadRequest(ModelState);
            }

            Order order = new Order();
            order.UserId = userId;
            order.CreatedAt = DateTime.Now;
            order.ShippingFee = OrderHelper.ShippingFee;
            order.DeliveryAddress = orderDto.DeliveryAddress!;
            order.OrderStatus = OrderHelper.OrderStatusess[0];
            order.PaymentStatus = OrderHelper.PaymentStatusess[0];
            order.PaymentMethod = orderDto.PaymentMethod!;

            var productItems = OrderHelper.GetCartProducts(orderDto.CartItems);

            foreach (var item in productItems)
            {
                var product = context.Products.Find(item.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("Error", "Please select a valid product");
                    return BadRequest(ModelState);
                }

                OrderItem orderItem = new OrderItem();
                orderItem.ProductId = item.ProductId;
                orderItem.Quantity = item.Quantity;
                orderItem.UnitPrice = product.Price;
                // orderItem.TotalUnitPrice = product.Price * item.Quantity;

                order.OrderItems.Add(orderItem);


            }

            if (order.OrderItems.Count < 1)
            {
                ModelState.AddModelError("Order", "Please select a product to make valid order");
                return BadRequest(ModelState);
            }

            context.Orders.Add(order);
            context.SaveChanges();

            order.User!.Password = "";

            return Ok(order);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetOrders(int? page)
        {
            if (page == null || page <= 0)
            {
                page = 1;
            }
            int pageSize = 5;
            int totalPages = 0;

            var userId = JwtReader.GetUserId(User);
            var role = JwtReader.GetUserRole(User);

            IQueryable<Order> query = context.Orders.Include(o => o.User)
                                                    .Include(o => o.OrderItems)
                                                    .ThenInclude(oi => oi.Product);

            if (role != "admin")
            {
                query = query.Where(o => o.UserId == userId);
            }

            query = query.OrderByDescending(o => o.Id);

            decimal count = query.Count();
            totalPages = (int)Math.Ceiling(count / pageSize);

            int skipedRowCount = (int)(page - 1) * pageSize;

            query = query.Skip(skipedRowCount).Take(pageSize);

            var orders = query.Skip(skipedRowCount).Take(pageSize).ToList();

            foreach (var order in orders)
            {
                order.User!.Password = "";
            }

            var response = new
            {
                Orders = orders,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page,
                Count = count
            };

            return Ok(response);

            // return Ok(orders);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrderById(int id)
        {
            var userId = JwtReader.GetUserId(User);
            var role = JwtReader.GetUserRole(User);

            Order? order = null;

            if (role != "admin")
            {
                order = context.Orders.Include(o => o.User)
                                        .Include(o => o.OrderItems)
                                        .ThenInclude(oi => oi.Product)
                                        .FirstOrDefault(o => o.Id == id);

                order!.User!.Password = "";
            }
            else
            {
                order = context.Orders.Include(o => o.OrderItems)
                                        .ThenInclude(oi => oi.Product)
                                        .FirstOrDefault(o => o.Id == id && o.UserId == userId);
            }

            if (order == null)
            {
                return NotFound();
            }



            return Ok(order);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, string? PaymentStatus, string? OrderStatus)
        {
            if (PaymentStatus == null && OrderStatus == null)
            {
                ModelState.AddModelError("Update Order", "Nothing to update");
                return BadRequest(ModelState);
            }

            if (PaymentStatus != null && !OrderHelper.PaymentStatusess.Contains(PaymentStatus!))
            {
                ModelState.AddModelError("PaymentStatus", "please select a valid paymen tStatus");
                return BadRequest(ModelState);
            }

            if (OrderStatus != null && !OrderHelper.OrderStatusess.Contains(OrderStatus!))
            {
                ModelState.AddModelError("OrderStatus", "please select a valid Order Status");
                return BadRequest(ModelState);
            }

            var order = context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (PaymentStatus != null)
            {
                order.PaymentStatus = PaymentStatus;
            }

            if (OrderStatus != null)
            {
                order.OrderStatus = OrderStatus;
            }

            context.SaveChanges();

            return Ok(order);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }
            context.Orders.Remove(order);
            context.SaveChanges();

            return Ok();
        }

    }
}
