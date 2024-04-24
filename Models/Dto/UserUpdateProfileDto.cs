using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreApi.Models.Dto
{
    public class UserUpdateProfileDto
    {
        public string FirstName { get; set; } = "";
        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";
        [Required, Phone, MaxLength(12)]
        public string Phone { get; set; } = "";
        [Required, MaxLength(100)]
        public string Address { get; set; } = "";
    }
}