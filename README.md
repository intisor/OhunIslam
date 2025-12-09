# Ohun Islam - Voice of Islam Broadcasting Platform

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Microservices](#microservices)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Database Schema](#database-schema)
- [Messaging Infrastructure](#messaging-infrastructure)
- [Development](#development)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)

## Overview

**Ohun Islam** (Voice of Islam) is a comprehensive web-based platform designed to collate, manage, and stream all recordings of the Voice of Islam broadcast aired on Bond FM 92.9 by the Lagos State Ahmadiyya Jama'at. Built with a modern microservices architecture using .NET 8, the platform provides a centralized repository for managing Islamic broadcast content with features for uploading, categorizing, searching, streaming, and downloading audio recordings.

### Key Features

- **Media Management**: Complete CRUD operations for audio recordings with metadata (title, description, lecturer, date)
- **Live Radio Streaming**: Real-time streaming integration with Bond FM 92.9
- **Search & Filter**: Advanced search capabilities by date, topic, speaker, and other metadata
- **Audio Streaming & Download**: Stream recordings online or download in MP3 format
- **Event-Driven Architecture**: Asynchronous messaging using RabbitMQ and MassTransit
- **Scalable Microservices**: Independently deployable services with API Gateway
- **Database Persistence**: SQL Server database with Entity Framework Core
- **RESTful APIs**: Well-documented REST APIs with Swagger/OpenAPI support

## Architecture

Ohun Islam follows a **microservices architecture** pattern, ensuring scalability, fault tolerance, and ease of maintenance. The system consists of the following components:

```
         ┌─────────────────┐
         │   API Gateway   │  (YARP Reverse Proxy)
         │   (YARPGateway) │
         └────────┬────────┘
                  │
             ┌────┴────┐
             │         │
         ┌───▼────┐ ┌──▼──────┐
         │ Radio  │ │ WebAPI  │
         │Service │ │ Service │
         └───┬────┘ └────┬────┘
             │           │
             └─────┬─────┘
                   │
             ┌─────▼─────┐      ┌──────────────┐
             │ RabbitMQ  │      │  SQL Server  │
             │ (Message  │      │  (Database)  │
             │   Bus)    │      └──────▲───────┘
             └───────────┘             │
                                  (WebAPI only)

Note: Both Radio and WebAPI services reference the Shared Library 
for common models (RadioStreamingStatus, StreamStatsUpdate, etc.)
```

### Architecture Principles

1. **Separation of Concerns**: Each microservice handles a specific domain
2. **Event-Driven Communication**: Services communicate asynchronously via RabbitMQ
3. **API Gateway Pattern**: Single entry point for external clients
4. **Shared Kernel**: Common models and utilities in shared library
5. **Database per Service**: WebAPI manages its own SQL Server database
6. **Containerization Ready**: Docker support for easy deployment

## Technology Stack

### Backend Framework
- **.NET 8.0**: Latest LTS version of .NET
- **ASP.NET Core**: Web API framework

### Data Access
- **Entity Framework Core 8.0.11**: ORM for database operations
- **Microsoft.Data.SqlClient 6.0.1**: SQL Server data provider
- **SQL Server**: Primary database

### Messaging & Event Bus
- **RabbitMQ 3-management-alpine**: Message broker
- **MassTransit 8.2.5**: Distributed application framework
- **MassTransit.RabbitMQ 8.2.5**: RabbitMQ integration
- **RabbitMQ.Client 6.8.1**: RabbitMQ .NET client

### API Gateway
- **YARP (Yet Another Reverse Proxy) 2.3.0**: High-performance reverse proxy

### API Documentation
- **Swashbuckle.AspNetCore 6.4.0**: Swagger/OpenAPI implementation
- **Microsoft.AspNetCore.OpenApi 8.0.8**: OpenAPI support

### Other Dependencies
- **Newtonsoft.Json 13.0.3**: JSON serialization
- **Microsoft.Extensions.Logging 8.0.1**: Logging abstraction

### Infrastructure
- **Docker & Docker Compose**: Containerization and orchestration
- **Git**: Version control

## Microservices

### 1. OhunIslam.WebAPI

**Purpose**: Core media management service handling recording CRUD operations, database persistence, and media streaming.

**Responsibilities**:
- Media file upload and storage management
- CRUD operations for media items (recordings)
- Audio file streaming and downloads
- Database management (MediaItems, ConsumedMessages, StreamStats)
- Consuming messages from Radio service via RabbitMQ
- Persisting radio streaming events and statistics

**Key Components**:
- `MediaController`: REST API for media operations
- `MediaContext`: EF Core DbContext for database operations
- `MassTSConsumer`: Consumes RadioStreamingStatus and StreamStatsUpdate messages
- `RadioMessageSubscriber`: Background service for message processing
- `AddRadioEventProcessor`: Processes radio events

**Endpoints**:
- `GET /api/media` - List all media items
- `GET /api/media/{id}` - Get specific media item
- `POST /api/media` - Upload new media item
- `PUT /api/media/{id}` - Update media item
- `DELETE /api/media/{id}` - Delete media item
- `GET /api/media/stream/{id}` - Stream audio file

**Database Tables**:
- `MediaItems`: Audio recording metadata
- `ConsumedMessages`: Logged streaming status messages
- `StreamStats`: Aggregated streaming statistics

### 2. OhunIslam.Radio

**Purpose**: Handles live radio streaming from Bond FM 92.9 and publishes streaming events.

**Responsibilities**:
- Streaming integration with Bond FM (https://go.webgateready.com/bondfm)
- Publishing streaming status to RabbitMQ
- Publishing stream statistics updates
- Monitoring stream health and availability

**Key Components**:
- `RadioController`: REST API for radio operations
- `MassTransitService`: Publishes streaming events to message bus
- Stream monitoring and error handling

**Endpoints**:
- `GET /api/radio/play` - Initiate/verify radio stream

**Events Published**:
- `RadioStreamingStatus`: Stream start/stop/error events
- `StreamStatsUpdate`: Daily stream count statistics

### 3. YARPGateway

**Purpose**: API Gateway providing a single entry point for all services.

**Responsibilities**:
- Request routing to appropriate microservices
- Load balancing
- Request/response transformation (future)
- Authentication/authorization gateway (future)

**Technology**: YARP (Yet Another Reverse Proxy) 2.3.0

### 4. OhunIslam.Shared

**Purpose**: Shared library containing common models, interfaces, and utilities.

**Components**:
- `RadioStreamingStatus`: Model for radio stream status events
- `StreamStatsUpdate`: Model for statistics updates
- `StreamStatus` enum: Started, Stopped, Error, Playing
- Custom logger implementations

## Prerequisites

### Required Software

1. **.NET 8.0 SDK or later**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **SQL Server** (Local or Azure)
   - SQL Server 2019 or later recommended
   - SQL Server Express (free) is sufficient
   - Azure SQL Database also supported

3. **Docker & Docker Compose** (for RabbitMQ)
   - Docker Desktop: https://www.docker.com/products/docker-desktop
   - Required for local RabbitMQ instance

4. **Git**
   - For version control and cloning the repository

### Optional Tools

- **Visual Studio 2022** or **JetBrains Rider**: Full-featured IDE
- **Visual Studio Code**: Lightweight editor with C# extension
- **Postman** or **Insomnia**: API testing
- **SQL Server Management Studio (SSMS)**: Database management

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/intisor/OhunIslam.git
cd OhunIslam
```

### 2. Start RabbitMQ

Using Docker Compose:

```bash
docker-compose up -d
```

This starts RabbitMQ with management interface:
- RabbitMQ Port: 5672
- Management UI: http://localhost:15672
- Default credentials: `guest` / `guest`

Verify RabbitMQ is running:
```bash
docker ps
```

### 3. Configure Database Connection

Update the connection string in `OhunIslam.WebAPI/appsettings.json`:

**For Development (Windows Integrated Security):**
```json
{
  "ConnectionStrings": {
    "ConnectionString": "Server=YOUR_SERVER;Database=OhunIslam;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;"
  }
}
```

**For SQL Server Authentication:**
```json
"ConnectionString": "Server=YOUR_SERVER;Database=OhunIslam;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False;"
```

**For Production (with encryption enabled):**
```json
"ConnectionString": "Server=YOUR_SERVER;Database=OhunIslam;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;"
```

> **⚠️ Security Note**: The development examples disable encryption for local development convenience. For production deployments, always enable encryption (`Encrypt=True`) and use proper certificate validation (`TrustServerCertificate=False`).

### 4. Apply Database Migrations

Navigate to the WebAPI project and apply migrations:

```bash
cd OhunIslam.WebAPI
dotnet ef database update
```

This creates the database and all required tables.

### 5. Build the Solution

From the root directory:

```bash
dotnet build OhunIslam.sln
```

### 6. Run the Services

You can run services individually or all together.

#### Option A: Run All Services (Multiple Terminals)

**Terminal 1 - WebAPI Service:**
```bash
cd OhunIslam.WebAPI
dotnet run
```
Default URL: https://localhost:7001

**Terminal 2 - Radio Service:**
```bash
cd OhunIslam.Radio
dotnet run
```
Default URL: https://localhost:7002

**Terminal 3 - API Gateway (Optional):**
```bash
cd YARPGateway
dotnet run
```
Default URL: https://localhost:7000

#### Option B: Run in Visual Studio

1. Open `OhunIslam.sln` in Visual Studio
2. Set multiple startup projects:
   - Right-click solution → Properties
   - Select "Multiple startup projects"
   - Set OhunIslam.WebAPI and OhunIslam.Radio to "Start"
3. Press F5 to debug

### 7. Access the Applications

- **WebAPI Swagger UI**: https://localhost:7001/swagger
- **Radio Service Swagger UI**: https://localhost:7002/swagger
- **RabbitMQ Management**: http://localhost:15672

### 8. Test the System

#### Upload a Media File:

```bash
curl -X POST "https://localhost:7001/api/media" \
  -H "Content-Type: multipart/form-data" \
  -F "MediaTitle=Test Recording" \
  -F "MediaDescription=Test Description" \
  -F "MediaLecturer=Test Speaker" \
  -F "MediaFile=@/path/to/audio.mp3"
```

#### Stream Bond FM Radio:

```bash
curl -X GET "https://localhost:7002/api/radio/play"
```

This initiates a stream verification and publishes events to RabbitMQ, which are consumed by the WebAPI service and stored in the database.

## Configuration

### appsettings.json Structure

#### OhunIslam.WebAPI/appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "ConnectionString": "Server=SERVER;Database=OhunIslam;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;"
  },
  "AllowedHosts": "*",
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

#### OhunIslam.Radio/appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment-Specific Configuration

Use `appsettings.Development.json` for development overrides and `appsettings.Production.json` for production settings.

### RabbitMQ Configuration

RabbitMQ settings are configured in `docker-compose.yml`:

```yaml
services:
  rabbitmq:
    container_name: "rabbitmq"
    image: rabbitmq:3-management-alpine
    hostname: localhost
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - "5672:5672"   # AMQP port
      - "15672:15672" # Management UI
    networks:
      - ohunislam-network
    restart: always
```

> **⚠️ Security Warning**: The default credentials (guest/guest) are only suitable for local development. For production deployments, you **must** change these credentials and configure proper authentication. Update the environment variables and corresponding service configurations accordingly.

## API Documentation

### Media Management API

**Base URL**: `https://localhost:7001/api/media`

#### Get All Media Items
```http
GET /api/media
```

**Response**:
```json
[
  {
    "mediaId": 1,
    "mediaTitle": "Friday Sermon",
    "mediaDescription": "Weekly sermon",
    "mediaLecturer": "Imam Abdullah",
    "mediaPath": "/path/to/audio.mp3",
    "dateIssued": "2024-12-09T00:00:00"
  }
]
```

#### Get Media Item by ID
```http
GET /api/media/{id}
```

#### Upload New Media
```http
POST /api/media
Content-Type: multipart/form-data

{
  "MediaTitle": "string",
  "MediaDescription": "string",
  "MediaLecturer": "string",
  "MediaFile": file
}
```

#### Update Media Item
```http
PUT /api/media/{id}
Content-Type: application/json

{
  "mediaId": 1,
  "mediaTitle": "Updated Title",
  "mediaDescription": "Updated Description",
  "mediaLecturer": "Updated Speaker",
  "mediaPath": "/path/to/audio.mp3",
  "dateIssued": "2024-12-09T00:00:00"
}
```

#### Delete Media Item
```http
DELETE /api/media/{id}
```

#### Stream Audio
```http
GET /api/media/stream/{id}
```

**Response**: Audio stream (audio/mpeg)

### Radio Streaming API

**Base URL**: `https://localhost:7002/api/radio`

#### Verify/Start Radio Stream
```http
GET /api/radio/play
```

**Response**:
```json
{
  "mediaTitle": "Bond FM Radio",
  "streamUrl": "https://go.webgateready.com/bondfm",
  "status": "Started",
  "contentType": "audio/mpeg",
  "message": "Stream is available. Use the streamUrl in an audio player to listen."
}
```

## Database Schema

### MediaItems Table

| Column | Type | Description |
|--------|------|-------------|
| MediaId | int (PK) | Unique identifier |
| MediaTitle | nvarchar | Title of the recording |
| MediaDescription | nvarchar | Description of content |
| MediaLecturer | nvarchar | Speaker/lecturer name |
| MediaPath | nvarchar | File path to audio file |
| DateIssued | datetime2 | Publication date |

### ConsumedMessages Table

| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier (PK) | Unique message ID |
| MessageContent | nvarchar | Serialized message JSON |
| MediaTitle | nvarchar | Radio stream title |
| StreamStartTime | datetime2 | When stream started |
| StreamStatus | int (enum) | Status code (Started/Stopped/Error/Playing) |
| StreamDuration | time | Duration of stream |
| ReceivedAt | datetime2 | Message receipt timestamp |

### StreamStats Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Unique identifier |
| TotalStreamsToday | int | Count of streams for the day |
| UpdateTime | datetime2 | Last update timestamp |
| MessageContent | nvarchar | Serialized stats JSON |
| ReceivedAt | datetime2 | Record creation time |

## Messaging Infrastructure

### RabbitMQ Architecture

The system uses RabbitMQ for asynchronous, event-driven communication between microservices.

#### Message Flow

```
Radio Service → RabbitMQ → WebAPI Service
    (Publish)   (Exchange)    (Consume)
```

#### Exchanges and Queues

1. **radio_exchange** (Direct Exchange)
   - Used for streaming status messages
   - Type: Direct
   - Durable: Yes

2. **radio_streaming_queue**
   - Receives RadioStreamingStatus messages
   - Receives StreamStatsUpdate messages
   - Durable: Yes
   - Auto-delete: No
   - Consumed by WebAPI service

#### WebAPI Consumer Configuration

The WebAPI service consumes messages from `radio_streaming_queue` using the following configuration:

- **Endpoint Name**: `queue_stats` (MassTransit consumer endpoint)
- **Prefetch Count**: 1 (processes one message at a time)
- **Retry Policy**: 5 attempts with 10-second intervals between retries

#### Message Types

**RadioStreamingStatus**:
```csharp
{
    string MediaTitle;
    DateTime StreamStartTime;
    StreamStatus StreamStatus; // Started, Stopped, Error, Playing
    TimeSpan StreamDuration;
}
```

**StreamStatsUpdate**:
```csharp
{
    int TotalStreamsToday;
    DateTime UpdateTime;
}
```

### MassTransit Configuration

MassTransit provides a higher-level abstraction over RabbitMQ with built-in features:

- **Automatic retry**: 5 retries with exponential backoff
- **Message serialization**: JSON serialization by default
- **Consumer management**: Automatic message acknowledgment
- **Error handling**: Dead-letter queues for failed messages
- **Request/response**: Support for request-response patterns

## Development

### Project Structure

```
OhunIslam/
├── OhunIslam.sln                    # Solution file
├── docker-compose.yml               # Docker services configuration
├── README.md                        # This file
├── SRS.txt                         # Software Requirements Specification
├── OhunIslam.WebAPI/               # Media management service
│   ├── Controllers/                # API controllers
│   ├── Model/                      # Domain models
│   ├── Infrastructure/             # Database context
│   ├── Services/                   # Business logic
│   ├── EventProcessing/            # Event processors
│   ├── Migrations/                 # EF Core migrations
│   └── Program.cs                  # Application entry point
├── OhunIslam.Radio/                # Radio streaming service
│   ├── Controllers/                # API controllers
│   ├── Services/                   # Business logic
│   ├── EventProcessing/            # Event processors
│   └── Program.cs                  # Application entry point
├── OhunIslam.Shared/               # Shared library
│   ├── Models/                     # Shared models
│   └── Logger/                     # Logging utilities
└── YARPGateway/                    # API Gateway
    ├── Program.cs                  # Gateway configuration
    └── appsettings.json            # Routing configuration
```

### Adding Database Migrations

When you modify database models:

```bash
cd OhunIslam.WebAPI
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Running Tests

Currently, the project doesn't have automated tests. To add tests:

```bash
# Create test project
dotnet new xunit -n OhunIslam.Tests
dotnet sln add OhunIslam.Tests/OhunIslam.Tests.csproj

# Run tests
dotnet test
```

### Code Style

- Follow Microsoft C# coding conventions
- Use nullable reference types (`#nullable enable`)
- Use async/await for I/O operations
- Implement proper logging with ILogger
- Handle exceptions appropriately

### Logging

The application uses the built-in .NET logging infrastructure:

- **Console output**: Structured logging to console (enabled by default)
- **File logging**: Custom file logger implementation exists in the codebase but is currently disabled

Log files referenced in the codebase (when enabled):
- `WebAPILogs.txt` - WebAPI service logs
- `RadioLogs.txt` - Radio service logs

To enable file logging, you would need to implement and register a file logger provider in `Program.cs` of each service.

**Log Levels** (configured in `appsettings.json`):
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

## Deployment

### Local Deployment

Follow the [Getting Started](#getting-started) guide for local development.

### Docker Deployment (Future)

The project includes commented Docker configuration in `docker-compose.yml`. To enable:

1. Create Dockerfile for each service
2. Uncomment the `ohunislam-api` service in docker-compose.yml
3. Configure environment variables
4. Run: `docker-compose up -d`

### Azure Deployment

#### Azure Resources Required:

1. **Azure App Service** (for WebAPI and Radio services)
2. **Azure SQL Database** (for data persistence)
3. **Azure Service Bus** or **RabbitMQ on Azure VM** (for messaging)
4. **Azure Blob Storage** (for audio file storage - recommended)

#### Steps:

1. Provision Azure resources
2. Update connection strings in Azure App Settings
3. Configure CI/CD pipeline (GitHub Actions or Azure DevOps)
4. Deploy services to Azure App Service
5. Configure custom domains and SSL certificates

### Production Considerations

- **Security**: 
  - Use Azure Key Vault for secrets
  - Enable HTTPS only
  - Implement authentication/authorization (Azure AD, JWT)
  - Configure CORS policies
  
- **Performance**:
  - Enable response caching
  - Implement CDN for audio files
  - Configure connection pooling
  - Use Azure Application Insights for monitoring

- **Scalability**:
  - Enable auto-scaling for App Services
  - Use Azure SQL DTU-based or vCore pricing
  - Implement read replicas for database
  - Consider Azure API Management for API Gateway

- **Reliability**:
  - Configure health checks
  - Set up alerts and monitoring
  - Implement circuit breaker pattern
  - Enable Application Insights

## Contributing

We welcome contributions to Ohun Islam! Here's how you can help:

### Getting Started

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes
4. Commit your changes: `git commit -m 'Add amazing feature'`
5. Push to the branch: `git push origin feature/amazing-feature`
6. Open a Pull Request

### Contribution Guidelines

- Write clear, descriptive commit messages
- Follow existing code style and conventions
- Add tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting PR
- Keep PRs focused and atomic

### Areas for Contribution

- Frontend development (web interface)
- User authentication and authorization
- Advanced search features
- Audio file transcription
- Mobile applications
- Internationalization (i18n)
- Performance optimizations
- Additional test coverage

## License

This project is developed for the Lagos State Ahmadiyya Jama'at. All rights reserved.

For licensing inquiries, please contact the project maintainers.

## Contact & Support

**Project Maintainer**: Intisor  
**Organization**: Lagos State Ahmadiyya Jama'at  
**Repository**: https://github.com/intisor/OhunIslam

### Getting Help

- **Issues**: Report bugs or request features via [GitHub Issues](https://github.com/intisor/OhunIslam/issues)
- **Discussions**: General questions and discussions in [GitHub Discussions](https://github.com/intisor/OhunIslam/discussions)

### Acknowledgments

- Lagos State Ahmadiyya Jama'at
- Bond FM 92.9
- .NET Community
- MassTransit Team
- RabbitMQ Community

---

**Built with ❤️ for the Voice of Islam by the Ahmadiyya Muslim Community**
