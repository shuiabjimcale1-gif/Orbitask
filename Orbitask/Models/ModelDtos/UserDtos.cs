using System.ComponentModel.DataAnnotations;

namespace Orbitask.Models.ModelDtos
{
    // ============================================
    // USER DTOs - ALL IN ONE FILE
    // ============================================

    /// <summary>
    /// DTO returned on registration/login with JWT token
    /// </summary>
    public class NewUserDto
    {
        public string DisplayName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for user information (no token)
    /// Used for displaying user details in UI
    /// </summary>
    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// DTO for user search results (for invite functionality)
    /// </summary>
    public class UserSearchResultDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// DTO for updating current user's profile
    /// </summary>
    public class UpdateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string DisplayName { get; set; } = string.Empty;

        [Url]
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// Request DTO for batch user retrieval
    /// </summary>
    public class BatchUserRequest
    {
        public List<string> UserIds { get; set; } = new();
    }

    /// <summary>
    /// Login Dto
    /// </summary>
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
    /// <summary>
    /// Register DTO
    /// </summary>
    public class RegisterDto
    {
        [Required]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}