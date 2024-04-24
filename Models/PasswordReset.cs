using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Irony.Parsing;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Models
{
    [Index("Email", IsUnique = true)]
    public class PasswordReset
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Email { get; set; } = "";
        [MaxLength(100)]
        public string Token { get; set; } = "";
        public DateTime Created_at { get; set; } = DateTime.Now;
    }
}