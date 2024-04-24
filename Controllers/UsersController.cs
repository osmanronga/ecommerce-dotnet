using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BestStoreApi.Filters;
using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersListController : ControllerBase
    {

        private static List<UserListDto> listUsers = new List<UserListDto>() {
            new UserListDto()
            {
                FirstName = "Ahmed",
                LastName = "Osman",
                Email = "ahmed@gmail.com",
                Phone = "0987654321",
                Address = "alsalma",
            },
            new UserListDto()
            {
                FirstName = "Ammar",
                LastName = "Ahmed",
                Email = "ammar@gmail.com",
                Phone = "0907654321",
                Address = "aljazera",
            }
        };

        // private readonly IConfiguration configuration;
        // public UsersController(IConfiguration configuration)
        // {
        //     this.configuration = configuration;
        // }

        [HttpGet("info")]
        [DebugFilter]
        public IActionResult GetInfo(int? id, string? name, int? page,
                                            IConfiguration configuration,
                                            [FromServices] TimeService timeService)
        {
            // we need to make response anonymous so that search not working yet
            if (id != null || name != null || page != null)
            {
                var response = new
                {
                    id = id,
                    name = name,
                    page = page,
                    ErrorMessage = "the search not working yet",
                };
                return Ok(response);
            }

            List<string> listInfo = new List<string>()
            {
                "AppName = " + configuration["AppName"],
                "Log = " + configuration["Logging:LogLevel:Default"],
            };
            listInfo.Add("Date = " + timeService.GetDate());
            listInfo.Add("Time = " + timeService.GetTime());
            return Ok(listInfo);
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(listUsers);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetUser(int id)
        {
            if (id >= 0 && id < listUsers.Count)
            {
                return Ok(listUsers[id]);
            }
            return NotFound();
        }

        [HttpGet("{name}")]
        public IActionResult GetUser(string name)
        {
            var user = listUsers.FirstOrDefault(u => u.FirstName!.Contains(name) || u.LastName!.Contains(name));
            if (user != null)
            {
                return Ok(user);
            }
            return NotFound();
        }

        [HttpPost]

        public IActionResult CreateUser(UserListDto user)
        {
            if (user.Email!.Equals("user@example.com"))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address");
                return BadRequest(ModelState);
            }

            listUsers.Add(user);

            return Ok(user);
        }

        [HttpPut("{id}")]

        public IActionResult UpdateUser(int id, UserListDto user)
        {
            if (user.Email!.Equals("user@example.com"))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address");
                return BadRequest(ModelState);
            }

            if (id >= 0 && id < listUsers.Count)
            {
                listUsers[id] = user;
                // return listUsers;
            }
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            if (id >= 0 && id < listUsers.Count)
            {
                listUsers.RemoveAt(id);
            }

            return NoContent();
        }


    }
}