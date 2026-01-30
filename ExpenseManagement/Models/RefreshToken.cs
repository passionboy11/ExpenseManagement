using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Models
{
    public class RefreshToken
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = null!; // required non-nullable

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        public bool Enabled { get; set; } = true;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;
    }
}
