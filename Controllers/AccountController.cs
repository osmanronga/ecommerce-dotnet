using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BestStoreApi.Models;
using BestStoreApi.Models.Dto;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BestStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration configuration;
        private readonly EmailSender emailSender;

        public AccountController(ApplicationDbContext context, IConfiguration configuration, EmailSender emailSender)
        {
            this.context = context;
            this.configuration = configuration;
            this.emailSender = emailSender;
        }

        [HttpPost("Register")]
        public IActionResult Register(UserDto userDto)
        {
            // check if email is already registered
            var userEmail = context.Users.Count(r => r.Email == userDto.Email);
            if (userEmail > 0)
            {
                ModelState.AddModelError("Email", "Email is registered already");
                return BadRequest(ModelState);
            }

            // encrypt the password
            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

            // save the user in the database table
            User user = new User()
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone,
                Address = userDto.Address,
                Password = encryptedPassword,
                Role = "client",
                CreatedAt = DateTime.Now
            };
            context.Users.Add(user);
            context.SaveChanges();

            var jwt = CreateJWToken(user);

            UserProfileDto userProfileDto = new UserProfileDto()
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

            var response = new
            {
                Token = jwt,
                User = userProfileDto
            };

            return Ok(response);
        }

        [HttpPost("Login")]
        public IActionResult Login(string email, string password)
        {
            var user = context.Users.FirstOrDefault(r => r.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("Error", "Email or password  not valid");
                return BadRequest(ModelState);
            }

            // check password
            var passwordHasher = new PasswordHasher<User>();
            var checkPasswordValidation = passwordHasher.VerifyHashedPassword(new User(), user!.Password, password);

            if (checkPasswordValidation == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Error", "Password has not been validated");
                return BadRequest(ModelState);
            }

            var jwt = CreateJWToken(user);

            UserProfileDto userProfileDto = new UserProfileDto()
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

            var response = new
            {
                Token = jwt,
                User = userProfileDto
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet("AuthorizeAuthenticatedUser")]
        public IActionResult AuthorizeAuthenticatedUser()
        {
            return Ok("Authorized, Authenticated User");
        }

        [Authorize(Roles = "admin")]
        [HttpGet("AuthorizeAdmins")]
        public IActionResult AuthorizeAdmins()
        {
            return Ok("Authorized, Authenticated Admin");
        }

        [Authorize(Roles = "admin, seller")]
        [HttpGet("AuthorizeAdminsSellers")]
        public IActionResult AuthorizeAdminsSellers()
        {
            return Ok("Authorized, Authenticated Admins & Sellers");
        }

        [Authorize]
        [HttpGet("GetTokenClaims")]
        public IActionResult GetTokenClaims()
        {
            var identity = User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                Dictionary<string, string> claims = new Dictionary<string, string>();

                foreach (Claim claim in identity.Claims)
                {
                    claims.Add(claim.Type, claim.Value);
                }

                return Ok(claims);
            }

            return Ok();
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = context.Users.FirstOrDefault(r => r.Email == email);
            if (user == null)
            {
                return NotFound();
            }


            var oldPassword = context.PasswordResets.FirstOrDefault(r => r.Email == email);
            if (oldPassword != null)
            {
                context.Remove(oldPassword);
            }

            var token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();

            PasswordReset passwordReset = new PasswordReset()
            {
                Email = email,
                Token = token,
                Created_at = DateTime.Now
            };

            context.PasswordResets.Add(passwordReset);
            context.SaveChanges();

            // send the password token reset email to the user
            string emailSubject = "Reset Password";
            string userName = user.FirstName + " " + user.LastName;
            string emailMessage = "dear : " + userName + " \n\n" +
                    "we received the reset password request from \n" +
                    "please copy the following token and copy in the field of token in form update Password" +
                    token + "\n \n" +
                    "Best Regards";

            // await emailSender.SendEmailAsync(user.Email, emailSubject, emailMessage);
            // emailSender.SendEmail(emailSubject, user.Email, username, emailMessage).Wait();

            return Ok();
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(string token, string password)
        {
            var pwdReset = context.PasswordResets.FirstOrDefault(pwd => pwd.Token == token);
            if (pwdReset == null)
            {
                ModelState.AddModelError("Token", "wrong or expired token");
                return BadRequest(ModelState);
            }

            var user = context.Users.FirstOrDefault(u => u.Email == pwdReset.Email);
            if (user == null)
            {
                ModelState.AddModelError("Token", "wrong or expired token");
                return BadRequest(ModelState);
            }

            // encrypt the password
            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), password);

            user.Password = encryptedPassword;

            context.PasswordResets.Remove(pwdReset);

            context.SaveChanges();

            return Ok("Your password has been changed");
        }

        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            int userId = JwtReader.GetUserId(User);

            var user = context.Users.Find(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            UserProfileDto userProfileDto = new UserProfileDto()
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

        [HttpPut("UpdateUserProfile")]
        public ActionResult UpdateUserProfile(UserUpdateProfileDto userUpdateProfileDto)
        {
            int userId = JwtReader.GetUserId(User);
            var user = context.Users.Find(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            user.FirstName = userUpdateProfileDto.FirstName;
            user.LastName = userUpdateProfileDto.LastName;
            user.Email = userUpdateProfileDto.Email;
            user.Phone = userUpdateProfileDto.Phone;
            user.Address = userUpdateProfileDto.Address;
            context.SaveChanges();

            UserProfileDto userProfileDto = new UserProfileDto()
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

        [Authorize]
        [HttpPut("UpdatePassword")]
        public IActionResult UpdatePassword([Required, MinLength(5), MaxLength(20)] string password)
        {
            int userId = JwtReader.GetUserId(User);
            var user = context.Users.Find(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), password);

            user.Password = encryptedPassword;

            context.SaveChanges();

            return Ok("your password has been Updated");
        }

        // moved this function to the JwtReader service and changed signature to GetUserId(ClaimsPrincipal user)

        // private int GetUserId()
        // {
        //     var identity = User.Identity as ClaimsIdentity;

        //     if (identity == null)
        //     {
        //         return 0;
        //     }

        //     var claim = identity.Claims.FirstOrDefault(c => c.Type.ToLower() == "id");
        //     if (claim == null)
        //     {
        //         return 0;
        //     }
        //     int userId;
        //     try
        //     {
        //         userId = int.Parse(claim.Value);
        //     }
        //     catch (System.Exception)
        //     {
        //         return 0;
        //     }

        //     return userId;
        // }

        // [HttpGet("TestToken")]
        // public IActionResult TestToken()
        // {
        //     User user = new User() { Id = 123, Role = "Administrator" };
        //     var jwt = CreateJWToken(user);
        //     var response = new { JWToken = jwt };
        //     return Ok(response);
        // }

        private string CreateJWToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id", ""+user.Id),
                new Claim("role", ""+user.Role),

            };

            string keyToken = configuration["JwtSettings:key"]!;

            // create object of key from secret key string 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyToken));

            // select the algoritm that make the signature of requests of json
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            // to create our token

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            // make jwt

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}