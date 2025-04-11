/*
 * Azure Cosmos DB Connection Setup
 * 
 * This file handles the configuration and initialization of the Azure Cosmos DB client,
 * database, and container instances for the application.
 */

using Microsoft.Azure.Cosmos; // Main namespace for Azure Cosmos DB SQL API SDK

// Initialize Cosmos Client instance (main entry point for Azure Cosmos DB interactions)
// ---------------------------------------------------------------
// The CosmosClient should be created once per application lifetime (singleton pattern)
// Requires credentials from configuration:
// - Endpoint: URL of your Cosmos DB account (e.g., https://[account-name].documents.azure.com:443/)
// - Key: Primary/secondary authentication key for the Cosmos DB account
var cosmosClient = new CosmosClient(
    configuration["CosmosDb:Endpoint"],  // Retrieved from app configuration (e.g., appsettings.json)
    configuration["CosmosDb:Key"]        // Sensitive credential - should be stored securely
);

// Database and Container Initialization
// ---------------------------------------------------------------
// These are lightweight reference operations - actual connection happens on first data operation

// Get reference to specific database ("DigiChama" in this configuration)
// Note: This doesn't create the database - assumes it already exists
Database database = await cosmosClient.GetDatabase("DigiChama");

// Get reference to specific container ("Users" collection within the database)
// Note: This doesn't create the container - assumes it already exists
Container container = database.GetContainer("Users");

/* Operational Flow:
 *   1. Client initialization establishes connection parameters
 *   2. Database reference obtained through GetDatabase()
 *   3. Container reference obtained through GetContainer()
 *   4. Actual network calls occur on first CRUD operation (lazy initialization)
 *   
 * Best Practices:
 *   - Reuse CosmosClient instance throughout application
 *   - Use async/await for all Cosmos DB operations
 *   - Implement proper error handling (CosmosException)
 *   - Consider using dependency injection for client management
 *   
 * Security Note:
 *   - Never hardcode credentials in source code
 *   - Use Azure Key Vault for production secret management
 *   - Prefer managed identities in Azure-hosted environments
 */