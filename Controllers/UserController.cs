using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BestStoreApi.Models.Dto;
using BestStoreApi.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public UserController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetUsers(int? page)
        {
            if (page == null || page <= 0)
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            decimal count = context.Users.Count();
            totalPages = (int)Math.Ceiling(count / pageSize);

            int skipedRowCount = (int)(page - 1) * pageSize;

            var users = context.Users
                               .OrderByDescending(u => u.Id)
                               .Skip(skipedRowCount)
                               .Take(pageSize)
                               .ToList();

            List<UserProfileDto> usersProfiles = new List<UserProfileDto>();

            foreach (var user in users)
            {
                var userProfileDto = new UserProfileDto()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                usersProfiles.Add(userProfileDto);
            }

            var response = new
            {
                Users = usersProfiles,
                Count = count,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(response);
            // return Ok(usersProfiles);
        }

        [HttpGet("id")]
        public IActionResult GetUserById(int id)
        {
            var user = context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            var userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Ok(userProfileDto);
        }
    }
}