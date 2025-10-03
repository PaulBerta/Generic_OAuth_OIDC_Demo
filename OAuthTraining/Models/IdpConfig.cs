using System.ComponentModel.DataAnnotations;

namespace OAuthTraining.Models
{
    /// <summary>
    /// Represents the identity provider connection details captured from the customer and stored in
    /// the local database.  These values are later mapped onto <see cref="OpenIdConnectOptions"/> so
    /// the middleware can negotiate the sign-in flow.
    /// </summary>
    public class IdpConfig
    {
        [Key]
        public int Id { get; set; }
        [Required]
        // Fully qualified URI for the customer's identity provider.
        public string Authority { get; set; } = string.Empty;
        [Required]
        // Optional tenant identifier that some providers (such as Entra ID) require.
        public string TenantId { get; set; } = string.Empty;
        [Required]
        // The application's client identifier as issued by the identity provider.
        public string ClientId { get; set; } = string.Empty;
        [Required]
        // Encrypted client secret; it is transparently decrypted before configuring the middleware.
        public string ClientSecret { get; set; } = string.Empty;

    }
}

