using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreApi.Models
{
    public class UserListDto
    {
        [Required]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "please provide a last name")]
        public string? LastName { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Required]
        [MinLength(5, ErrorMessage = "minimum length is {0} characters")]
        [MaxLength(10, ErrorMessage = "maximum length is {0} characters")]
        public string? Address { get; set; }
    }
}