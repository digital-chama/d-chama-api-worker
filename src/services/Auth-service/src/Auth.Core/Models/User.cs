// Description: 
// This file defines the `User` class, which models a user entity in the authentication system. 
// It includes properties for managing user identity, authentication, and profile information. 
// For example, in a real-life scenario, this class could represent a user of a financial group 
// management application (e.g., a "chama" app). Users can register with their email or phone, 
// authenticate using passwords or OAuth providers, and manage their profiles. The class also 
// supports advanced features like account verification via OTP, password hashing for security, 
// and account locking after multiple failed login attempts. Additionally, it integrates with 
// Cosmos DB for efficient data storage and retrieval, making it suitable for scalable cloud-based 
// applications.
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Auth.Common.Enums;

namespace Auth.Core.Models
{
  /// <summary>
  /// Represents a user in the authentication system.
  /// This is the primary entity for user authentication and identity management.
  /// </summary>
  public class User
  {
    /// <summary>
    /// Unique identifier for the user.
    /// Used as the partition key in Cosmos DB for efficient queries.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// User's full name.
    /// </summary>
    [Required]
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;
    /// <summary>
    /// User's email address.
    /// Optional if phone is provided as the primary contact method.
    /// </summary>
    [Required]
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    /// <summary>
    /// User's phone number.
    /// Optional if email is provided as the primary contact method.
    /// Format: E.164 international format (+[country code][number])
    /// </summary>
    /// 
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    /// <summary>
    /// The authentication provider used by the user.
    /// Determines whether user authenticated via email, phone, or OAuth provider.
    /// </summary>
    [Required]
    [JsonPropertyName("authProvider")]

    public AuthProvider AuthProvider { get; set; }
    /// <summary>
    /// Hashed password using BCrypt or Argon2id algorithm.
    /// Null for OAuth-authenticated users.
    /// 
    /// Algorithm: Argon2id with following parameters:
    /// - Memory: 65536 KB
    /// - Iterations: 3
    /// - Parallelism: 1
    /// - Hash length: 32 bytes
    /// - Salt length: 16 bytes
    /// </summary>
    /// 
    [JsonPropertyName("passwordHash")]
    public string? passwordHash { get; set; }
    /// <summary>
    /// Random salt used for password hashing.
    /// Generated using cryptographically strong random number generator.
    /// </summary>
    [JsonPropertyName("passwordSalt")]
    public string? PasswordSalt { get; set; }
    /// <summary>
    /// OAuth provider's unique identifier for the user.
    /// Only populated for users who authenticate via OAuth.
    /// </summary>
    [JsonPropertyName("oauthId")]
    public string? OAuthId { get; set; }
    /// <summary>
    /// Indicates whether the user's account has been verified.
    /// Account verification is done via OTP sent to email or phone.
    /// </summary>
    [Required]
    [JsonPropertyName("isVerified")]
    public bool IsVerified { get; set; }
    /// <summary>
    /// One-time password for account verification or password reset.
    /// 
    /// Algorithm: HMAC-based OTP (HOTP) with:
    /// - 6-digit numeric code
    /// - SHA-256 hash function
    /// - User-specific secret key
    /// </summary>
    [JsonPropertyName("verificationCode")]
    public string? VerificationCode { get; set; }
    /// <summary>
    /// Timestamp when the verification code expires.
    /// Typically set to 10 minutes after generation.
    /// </summary>
    [JsonPropertyName("verificationExpiry")]
    public DateTime? verificationExpiry { get; set; }
    /// <summary>
    /// Number of failed verification attempts.
    /// Used for rate limiting to prevent brute force attacks.
    /// Resets after successful verification or time-based expiry.
    /// </summary>

    [JsonPropertyName("failedVerificationAttempts")]
    public int FailedVerificationAttempts { get; set; } = 0;
    /// <summary>
    /// User's location information.
    /// </summary>
    [Required]
    [JsonPropertyName("location")]
    public string? Location { get; set; }
    /// <summary>
    /// URL to user's profile picture.
    /// Optional during registration.
    /// </summary>
    [JsonPropertyName("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }
    /// <summary>
    /// Initial role assigned to user.
    /// Default is "not allocated".
    /// Changes to "admin" when user creates a chama group.
    /// Changes to "member" when user joins a chama group.
    /// 
    /// Note: Group-specific roles are managed through a separate membership mechanism.
    /// </summary>
    [Required]
    [JsonPropertyName("initialRole")]
    public UserRole InitialRole { get; set; } = UserRole.Notallocated;
    /// <summary>
    /// Flag indicating if the account has been locked due to security concerns.
    /// Accounts can be locked after multiple failed login attempts.
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Timestamp when the account lock expires.
    /// Null if account is not locked.
    /// </summary>
    [JsonPropertyName("lockExpiry")]
    public DateTime? LockExpiry { get; set; }

    /// <summary>
    /// Number of consecutive failed login attempts.
    /// Resets after successful login.
    /// Used to trigger account lockout after threshold (typically 5 attempts).
    /// </summary>
    [JsonPropertyName("failedLoginAttempts")]
    public int FailedLoginAttempts { get; set; } = 0;
    /// <summary>
    /// Last IP address used for successful login.
    /// Used for security monitoring and notifications of suspicious activity.
    /// </summary>
    [JsonPropertyName("lastLoginIp")]
    public string? LastLoginIp { get; set; }
    /// <summary>
    /// Timestamp of last successful login.
    /// </summary>
    [JsonPropertyName("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }
    /// <summary>
    /// User's preferred language for notifications and UI.
    /// ISO 639-1 language code.
    /// </summary>
    [JsonPropertyName("preferredLanguage")]
    public string PreferredLanguage { get; set; } = "en";
    /// <summary>
    /// Version of the terms of service the user has accepted.
    /// Used to prompt for re-acceptance when terms are updated.
    /// </summary>
    [JsonPropertyName("acceptedTermsVersion")]
    public string? AcceptedTermsVersion { get; set; }
    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    [Required]
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Timestamp when the user was last updated.
    /// </summary>
    [Required]
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Type discriminator for Cosmos DB.
    /// Used for polymorphic queries across different entity types.
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; } = "User";
    /// <summary>
    /// ETag for optimistic concurrency control.
    /// Cosmos DB uses this to prevent conflicting updates.
    /// </summary>
    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }



  }
}