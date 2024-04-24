using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreApi.Models.Dto
{
    public class ContactDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";
        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = "";
        [MaxLength(12)]
        public string Phone { get; set; } = "";
        // public string Subject { get; set; } = "";
        public int SubjectId { get; set; }

        [Required, MinLength(10), MaxLength(300)]
        public string Message { get; set; } = "";
    }
}