//auth related constatnts



namespace Auth.Common.Enums
{

    /// <summary>
    /// Defines the authentication provider or method used by the user.
    /// </summary>

    public enum AuthProvider
    {
        /// <summary>
        /// User authenticates with email and password
        /// </summary>
        Email = 1,
        /// <summary>
        /// User authenticates with phone number and password
        /// </summary>
        Phone = 2,
        /// <summary>
        /// User authenticates via Google OAuth
        /// </summary>
        Google = 3

        // Add other OAuth providers as needed
        // Microsoft = 4,
        // Facebook = 5,
        // etc.
    }

}