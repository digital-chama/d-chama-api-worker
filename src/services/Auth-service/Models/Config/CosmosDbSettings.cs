/*
 * CosmosDbService.cs - Azure Cosmos DB Connection Service
 * 
 * This service handles the configuration and initialization of the Azure Cosmos DB client,
 * database, and container instances for the application. It follows best practices for
 * connection management and implements a repository pattern for data access.
 * 
 * Author: [Your Name]
 * Created: April 11, 2025
 */

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DigiChama.Services.Database
{
    /// <summary>
    /// Service responsible for managing Azure Cosmos DB connections and providing
    /// access to database resources throughout the application.
    /// </summary>
    public class CosmosDbService : ICosmosDbService, IAsyncDisposable
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<CosmosDbService> _logger;
        private readonly string _databaseName;
        private Database _database;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the CosmosDbService class.
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger instance</param>
        public CosmosDbService(IConfiguration configuration, ILogger<CosmosDbService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseName = configuration["CosmosDb:DatabaseName"] ?? "DigiChama";

            // Configure the CosmosClientOptions with retry options and performance settings
            var clientOptions = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct, // Direct mode for better performance
                ServerVersion = ServerVersion.V1,      // Use latest server version
                ApplicationName = "DigiChamaApp",      // Identify application in metrics
                MaxRetryAttemptsOnRateLimitedRequests = 9, // Retry policy for rate limiting
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                EnableContentResponseOnWrite = false   // Optimize for write operations
            };

            // Get credentials from configuration with secure fallbacks
            string endpoint = configuration["CosmosDb:Endpoint"] 
                ?? throw new InvalidOperationException("CosmosDB Endpoint not configured");
            
            // Initialize client with proper credentials
            // For production, use managed identity when possible
            if (!string.IsNullOrEmpty(configuration["CosmosDb:Key"]))
            {
                _logger.LogInformation("Initializing Cosmos DB client with account key");
                _cosmosClient = new CosmosClient(endpoint, configuration["CosmosDb:Key"], clientOptions);
            }
            else if (!string.IsNullOrEmpty(configuration["CosmosDb:ConnectionString"]))
            {
                _logger.LogInformation("Initializing Cosmos DB client with connection string");
                _cosmosClient = new CosmosClient(configuration["CosmosDb:ConnectionString"], clientOptions);
            }
            else if (!string.IsNullOrEmpty(configuration["CosmosDb:ManagedIdentityId"]))
            {
                _logger.LogInformation("Initializing Cosmos DB client with managed identity");
                // Use managed identity for authentication (recommended for production)
                var credential = new Azure.Identity.DefaultAzureCredential(new Azure.Identity.DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = configuration["CosmosDb:ManagedIdentityId"]
                });
                _cosmosClient = new CosmosClient(endpoint, credential, clientOptions);
            }
            else
            {
                throw new InvalidOperationException("No valid authentication method found for Cosmos DB");
            }
        }

        /// <summary>
        /// Initializes the database and containers required by the application.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing Cosmos DB database and containers");
                
                // Create database if it doesn't exist (idempotent operation)
                _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);
                _logger.LogInformation("Database {DatabaseName} initialized", _databaseName);

                // Configure containers with appropriate partition keys and indexing policies
                await InitializeContainersAsync();
                
                _logger.LogInformation("Cosmos DB initialization complete");
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error initializing Cosmos DB: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Initializes the containers needed by the application.
        /// </summary>
        private async Task InitializeContainersAsync()
        {
            // Users container - partition by EntityType for efficient queries
            var usersContainerProperties = new ContainerProperties
            {
                Id = "Users",
                PartitionKeyPath = "/EntityType",
                IndexingPolicy = new IndexingPolicy
                {
                    Automatic = true,
                    IndexingMode = IndexingMode.Consistent,
                    IncludedPaths = { new IncludedPath { Path = "/*" } },
                    ExcludedPaths = { new ExcludedPath { Path = "/ProfilePhotoUrl/?" } } // Don't index large fields
                }
            };

            // Add unique key policy for email
            usersContainerProperties.UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys = { new UniqueKey { Paths = { "/Email" } } }
            };

            await _database.CreateContainerIfNotExistsAsync(usersContainerProperties);
            _logger.LogInformation("Container {ContainerId} initialized", usersContainerProperties.Id);

            // Initialize other containers as needed...
        }

        /// <summary>
        /// Gets a reference to a container by name.
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <returns>Container reference</returns>
        public Container GetContainer(string containerName)
        {
            if (_database == null)
                throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");
                
            return _database.GetContainer(containerName);
        }

        /// <summary>
        /// Disposes of resources used by the Cosmos DB service.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_cosmosClient != null)
            {
                _logger.LogInformation("Disposing Cosmos DB client");
                _cosmosClient.Dispose();
            }
            
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Interface for Cosmos DB service to facilitate dependency injection and testing.
    /// </summary>
    public interface ICosmosDbService
    {
        /// <summary>
        /// Initializes the database and containers.
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Gets a reference to a container by name.
        /// </summary>
        /// <param name="containerName">Name of the container</param>
        /// <returns>Container reference</returns>
        Container GetContainer(string containerName);
    }
}