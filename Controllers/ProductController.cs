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
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        private readonly List<string> listCategories = new List<string>(){
            "Phones", "Computers", "Accessories", "Printers", "Cameras", "Other"
        };

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            return Ok(listCategories);
        }

        [HttpGet]
        public IActionResult GetProduts(string? search, string? category,
            int? minPrice, int? maxPrice,
            string? sort, string? order,
            int? page)
        {
            IQueryable<Product> query = _context.Products;

            // search by name and description
            if (search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }
            // search by category
            if (category != null)
            {
                query = query.Where(p => p.Category == category);
            }
            // search by minPrice and maxPrice
            if (minPrice != null)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            // sort functionality
            if (sort == null) sort = "id";
            if (order == null || order != "asc") order = "desc";

            if (sort.ToLower() == "name")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Name);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Name);
                }
            }
            else if (sort.ToLower() == "brand")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Brand);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Brand);
                }
            }
            else if (sort.ToLower() == "category")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Category);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Category);
                }
            }
            else if (sort.ToLower() == "price")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Price);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Price);
                }
            }
            else if (sort.ToLower() == "date")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.CreatedAt);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAt);
                }
            }
            else
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Id);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
            }

            // pagination functionality
            if (page == null || page < 1) page = 1;

            int pageSize = 5;
            int totalPages = 0;

            decimal count = query.Count();
            totalPages = (int)Math.Ceiling(count / pageSize);

            int skipedRowCount = (int)(page - 1) * pageSize;

            query = query.Skip(skipedRowCount).Take(pageSize);

            var products = query.ToList();

            var response = new
            {
                Products = products,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(response);
            // return Ok(products);
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(r => r.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        // public IActionResult CreateProduct(ProductDto productDto)
        // FromForm to tell swagger to make insert data from form instead of json text
        public IActionResult CreateProduct([FromForm] ProductDto productDto)
        {
            if (!listCategories.Contains(productDto.Category))
            {
                ModelState.AddModelError("Category", "please select a valid category");
                return BadRequest(ModelState);
            }

            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "product image file is required");
                return BadRequest(ModelState);
            }

            //save the image product in the server
            string ImageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            ImageFileName += Path.GetExtension(productDto.ImageFile.FileName);

            string ImageFolderName = webHostEnvironment.WebRootPath + "/images/products/";

            using (var stream = System.IO.File.Create(ImageFolderName + ImageFileName))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // save product into database
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description ?? "",
                ImageFileName = ImageFileName,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromForm] ProductDto productDto)
        {
            var product = _context.Products.FirstOrDefault(r => r.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (!listCategories.Contains(productDto.Category))
            {
                ModelState.AddModelError("Category", "please select a valid category");
                return BadRequest(ModelState);
            }

            string ImageFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                ImageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                ImageFileName += Path.GetExtension(productDto.ImageFile.FileName);

                string ImageFolderName = webHostEnvironment.WebRootPath + "/images/products/";

                using (var stream = System.IO.File.Create(ImageFolderName + ImageFileName))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                // remove old image
                System.IO.File.Delete(ImageFolderName + product.ImageFileName);
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Description = productDto.Description ?? "";
            product.ImageFileName = ImageFileName;

            _context.SaveChanges();

            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {

            var product = _context.Products.Find(id);

            if (product != null)
            {
                return NotFound();
            }

            string ImageFolderName = webHostEnvironment.WebRootPath + "/images/products/";
            System.IO.File.Delete(ImageFolderName + product!.ImageFileName);

            _context.Products.Remove(product!);
            _context.SaveChanges();


            return Ok();
        }

    }
}