using System;
using System.ComponentModel.DataAnnotations;
using Patient_mgt.Domain;

namespace Patient_mgt.DTOs
{
    public class UserDTO
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public class CreateUserDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string EmailId { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int Role { get; set; }
    }

    public class GetUserDTO
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}

    public class LoginRequestDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class loginDTO
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
