using System.ComponentModel.DataAnnotations;

namespace OAuthTraining.Models
{
    public class IdpConfig
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Authority { get; set; } = string.Empty;
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [Required]
        public string ClientSecret { get; set; } = string.Empty;
        public string? TenantId { get; set; }
    }
}

