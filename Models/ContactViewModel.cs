using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ContactApplication.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Phone { get; set; }

        [StringLength(10, ErrorMessage = "Zip code cannot exceed 10 characters.")]
        public string? Zip { get; set; }

        [StringLength(30, ErrorMessage = "City cannot exceed 50 characters.")]
        public string? City { get; set; }

        [StringLength(30, ErrorMessage = "State cannot exceed 50 characters.")]
        public string? State { get; set; }

        [StringLength(100, ErrorMessage = "Comments cannot exceed 500 characters.")]
        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string RecaptchaToken { get; set; }
    }

}
