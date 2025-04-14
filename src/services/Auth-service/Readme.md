# Authentication Microservice for Digichame

## Overview
This microservice handles user authentication for the Digichame application, providing secure registration, login, and verification processes for users. It's built using a clean architecture approach with .NET and Azure Cosmos DB, integrating with external notification services for email and SMS verification.

## Architecture

The microservice follows a layered architecture pattern:

```
AuthMicroservice/
├── src/
│   ├── Auth.API                      # API endpoints and configuration
│   ├── Auth.Core                     # Business logic and domain models
│   ├── Auth.Infrastructure           # External dependencies and implementations
│   └── Auth.Common                   # Shared utilities
└── tests/                            # Test projects
```

### Key Components

#### Auth.API
Contains the API controllers, middleware, and application configuration:
- `AuthController`: Handles registration, login, verification, and other authentication endpoints
- `Startup.cs`: Configures service dependencies and middleware pipeline

#### Auth.Core
Contains the business logic and domain models:
- `User.cs`: Primary entity model containing authentication-related properties
- `AuthService.cs`: Core authentication business logic
- `TokenService.cs`: JWT token generation and validation
- `OtpService.cs`: OTP generation and validation logic

#### Auth.Infrastructure
Implements interfaces defined in the core layer:
- `UserRepository.cs`: Cosmos DB data operations for user management
- `EventPublisher.cs`: Azure Service Bus integration for publishing auth events
- `NotificationService.cs`: Handles sending notifications via email or SMS

## Data Model

### User Entity
The central model for authentication with the following key fields:
- `Id`: Unique identifier
- `FullName`: User's full name
- `Email/Phone`: Contact method based on user preference
- `AuthProvider`: Email, Phone, or OAuth provider
- `PasswordHash`: Securely stored password hash
- `IsVerified`: Verification status
- `VerificationCode`: OTP for account verification
- `InitialRole`: Default "not allocated" role
- `Location`: User's location information
- `ProfilePictureUrl`: Optional profile image

## Authentication Flow

1. **Registration**:
   - User provides full name, email/phone, and password
   - System creates unverified user account
   - OTP sent to email or phone
   - Event published: `UserCreatedEvent`

2. **Verification**:
   - User submits OTP
   - System validates OTP and marks account as verified
   - JWT token issued for authenticated session
   - Event published: `UserVerifiedEvent`

3. **Login**:
   - User provides email/phone and password
   - System validates credentials
   - JWT token issued for authenticated session

4. **OAuth Flow**:
   - User authorizes via Google OAuth
   - System retrieves user information from Google
   - Account created or existing account retrieved
   - JWT token issued for authenticated session

5. **Role Management**:
   - When user creates a chama group: role changes to "admin"
   - When user joins a group: role changes to "member"
   - Event published: `UserRoleChangedEvent`

## Messaging System

The microservice uses Azure Service Bus for event-driven communication with other microservices. This ensures loose coupling and reliable message delivery.

### Event Publishing

When significant authentication events occur, the service publishes events to Azure Service Bus topics:

1. `EventPublisher.cs` serializes event data to messages
2. Messages are sent to predefined topics:
   - `user-created`: When a new user registers
   - `user-verified`: When a user completes verification
   - `user-role-changed`: When a user's role changes

### Subscription Pattern

Other microservices (e.g., Group Management) subscribe to these topics to react to authentication events:

1. Group Management service subscribes to `user-role-changed`
2. When a user creates a group and becomes an admin, the Group Management service receives the event
3. Group Management updates its database accordingly

This pattern maintains separation of concerns while ensuring data consistency across microservices.

## External Services Integration

### Email Service
- Uses the selected .NET email library to send verification emails and notifications
- Configurable templates for different notification types

### SMS Service
- Integrates with Africa's Talking API for SMS delivery
- Sends verification codes and notifications to users who register with phone numbers

## Configuration

Key configuration settings required for deployment:
- Cosmos DB connection string
- JWT secret and token lifetime
- Azure Service Bus connection string
- Email service credentials
- Africa's Talking API credentials

## Development Setup

1. Install .NET 6.0+ SDK
2. Configure user secrets or local environment variables
3. Run database migrations
4. Start the service: `dotnet run --project src/Auth.API`

## Testing

The solution includes:
- Unit tests for core business logic
- Integration tests for API endpoints
- Mock services for external dependencies

Run tests with: `dotnet test`