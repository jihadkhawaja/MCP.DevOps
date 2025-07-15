# Changelog

All notable changes to MCP.DevOps will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-12-15

### Added
- Initial release of MCP.DevOps server
- AWS deployment support:
  - EC2 instance management and deployment
  - ECS service deployment
  - Lambda function deployment
  - S3 storage operations
- Azure deployment support:
  - App Service deployment
  - Container Instance deployment
  - Virtual Machine management
  - Function App deployment
  - Azure Container Registry integration
- Linux server deployment support:
  - SSH-based deployment
  - Systemd service management
  - Nginx reverse proxy configuration
  - Docker container deployment
- Core DevOps operations:
  - Git repository management
  - .NET build and publish automation
  - Docker image management
  - Command execution with timeout
  - Environment variable management
- Health monitoring and system information tools
- Comprehensive documentation and examples
- Docker support for containerized deployment
- Model Context Protocol (MCP) server implementation

### Features
- Multi-platform deployment automation
- Cloud provider native SDK integration
- Secure SSH operations for Linux deployments
- Structured JSON responses for all operations
- Error handling and timeout management
- Environment configuration management
- Build and test automation
- Container orchestration support

### Dependencies
- .NET 8.0 runtime
- AWS SDK for .NET
- Azure SDK for .NET
- SSH.NET for secure shell operations
- Docker.DotNet for container management
- ModelContextProtocol for MCP server functionality