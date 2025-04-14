
/*
 * User.cs - Entity Model
 * 
 * This file defines the User entity model for the DigiChama application.
 * The User class represents individuals who can authenticate and interact with the system.
 * It supports multiple authentication methods (email/phone + password or social logins),
 * role-based authorization, and profile management.
 * 
 * Database: Azure Cosmos DB
 * Container: Users
 * 
 * Author: Collins Munene Developer
 * Created: April 11, 2025
 */
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DigiChama.Models.Entities.User
{
    /// <summary>
    /// Represents a user in the DigiChama system with authentication details, profile information,
    /// and role assignments. Designed to work with Azure Cosmos DB as the data store.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user. In Cosmos DB, this becomes the document id.
        /// </summary>
        [JsonProperty(propertyName = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Full name of the user as provided during registration.
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Email address of the user. May be used for authentication and communications.
        /// Must be unique across the system if used for authentication.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Phone number of the user. May be used for authentication and communications
        /// in regions where SMS is preferred. Must be unique if used for authentication.
        /// </summary>    
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Geographic location of the user.
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Hashed password for users who register with email/phone + password.
        /// Never store passwords in plain text.
        /// </summary>
        public string PasswordHash { get; set; }
        /// <summary>
        /// Random salt used during password hashing to prevent rainbow table attacks.
        /// Unique per user and generated during password creation/change.
        /// </summary>
        public string PasswordSalt { get; set; }
        /// <summary>
        /// URL to the user's profile photo.
        /// </summary>
        public string ProfilePhotoUrl { get; set; }
        /// <summary>
        /// Flag indicating if the email address has been verified.
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;
        /// <summary>
        /// Flag indicating if the phone number has been verified.
        /// </summary>
        public bool IsPhoneVerified { get; set; } = false;
        /// <summary>
        /// Flag indicating if two-factor authentication is enabled for this user.
        /// </summary>
        public bool IsTwoFactorEnabled { get; set; } = false;
        /// <summary>
        /// Secret key used for TOTP (Time-based One-Time Password) generation
        /// if 2FA is enabled with an authenticator app.
        /// </summary>
        public string TwoFactorSecret { get; set; }
        /// <summary>
        /// Collection of external login providers associated with this account.
        /// Allows for social media logins like Google.
        /// </summary>
        public List<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();
        // <summary>
        /// Collection of roles assigned to this user.
        /// All users start with the "Member" role by default.
        /// Users become "Admin" when they create a chama.
        /// </summary>
        public List<string> Roles { get; set; } = new List<string> { "Member" };
        /// <summary>
        /// Type discriminator for Cosmos DB partitioning strategy.
        /// Helps with efficient partition key design.
        /// </summary>
        public string EntityType { get; set; } = "User";
        /// <summary>
        /// Timestamp when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Timestamp when the user account was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Timestamp of the user's last login.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
        /// <summary>
        /// Collection of refresh tokens issued to this user for maintaining sessions.
        /// </summary>
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        /// <summary>
        /// IDs of chamas where this user is an admin.
        /// </summary>
        public List<string> AdminOfChamaIds { get; set; } = new List<string>();
        /// <summary>
        /// IDs of chamas where this user is a member.
        /// </summary>
        public List<string> MemberOfChamaIds { get; set; } = new List<string>();
        /// <summary>
        /// API keys for service-to-service communication.
        /// Each key has permissions, expiration, and usage tracking.
        /// </summary>
        public List<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
        /// <summary>
        /// Represents an external authentication provider linked to a user account.
        /// </summary>
        /// 
        public class ExternalLogin
        {
            /// <summary>
            /// The name of the provider (e.g., "Google").
            /// </summary>
            public string provider { get; set; }
            /// <summary>
            /// The unique identifier provided by the external service.
            /// </summary>
            public string ProviderKey { get; set; }
            /// <summary>
            /// Timestamp when this external login was added to the user account.
            /// </summary>
            public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
        }
        /// <summary>
        /// Represents a refresh token for maintaining user sessions.
        /// </summary>

        public class RefreshToken
        {
            /// <summary>
            /// Unique identifier for the refresh token.
            /// </summary>
            /// 
            public string Token { get; set; } = Guid.NewGuid().ToString();

            /// <summary>
            /// Timestamp when the token was created.
            /// </summary>
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            /// <summary>
            /// Timestamp when the token expires.
            /// </summary>
            public DateTime ExpiresAt { get; set; }
            /// <summary>
            /// Device information from which this token was issued.
            /// </summary>
            public string Device { get; set; }

            /// <summary>
            /// IP address from which this token was issued.
            /// </summary>
            public string IpAddress { get; set; }
            /// <summary>
            /// Flag indicating if this token has been revoked.
            /// </summary>
            public bool IsRevoked { get; set; } = false;

            /// <summary>
            /// Timestamp when the token was revoked, if applicable.
            /// </summary>
            public DateTime? RevokedAt { get; set; }
        }
        /// <summary>
        /// Represents an API key for service-to-service communication.
        /// </summary>
        /// 
        public class ApiKey
        {
            /// <summary>
            /// Unique identifier for the API key.
            /// </summary>
            public string Id { get; set; } = Guid.NewGuid().ToString;
            /// <summary>
            /// The hashed API key value. Never store API keys in plain text.
            /// </summary>
            public string KeyHash { get; set; }
            /// <summary>
            /// Human-readable name for the API key.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Description of what this API key is used for.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Timestamp when the API key was created.
            /// </summary>
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            /// <summary>
            /// Timestamp when the API key expires.
            /// </summary>
            public DateTime ExpiresAt { get; set; }
            /// <summary>
            /// List of permissions granted to this API key.
            /// </summary>
            public List<string> Permissions { get; set; } = new List<string>();

            /// <summary>
            /// Count of how many times this API key has been used.
            /// </summary>
            /// 
            public int UsageCount { get; set; } = 0;
            /// <summary>
            /// Timestamp of last usage of this API key.
            /// </summary>
            public DateTime? LastUsedAt { get; set; }
            /// <summary>
            /// Flag indicating if this API key has been revoked.
            /// </summary>
            public bool IsRevoked { get; set; } = false;
            /// <summary>
            /// Timestamp when the API key was revoked, if applicable.
            /// </summary>
            public DateTime? RevokedAt { get; set; }


        }

    }
}