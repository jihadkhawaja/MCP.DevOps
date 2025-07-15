# MCP.DevOps

A comprehensive **Model Context Protocol (MCP) Server** for automating DevOps operations and deploying applications to various cloud providers and servers. This server enables AI assistants and other MCP clients to perform sophisticated deployment automation, infrastructure management, and application lifecycle operations.

## Features

### Multi-Platform Deployment Support
- **AWS Deployment**: EC2 instances, ECS containers, Lambda functions, S3 storage
- **Azure Deployment**: App Service, Container Instances, Virtual Machines, Function Apps
- **Linux Server Deployment**: SSH-based deployment, systemd services, Nginx configuration

### Development & Build Operations
- **Git Operations**: Clone repositories, check status, get commit information
- **Build Automation**: .NET build, publish, and test operations
- **Docker Operations**: Build images, push to registries, run containers
- **Command Execution**: Execute shell commands with timeout and error handling

### Infrastructure Management
- **Cloud Resources**: Create and manage cloud infrastructure
- **Container Orchestration**: Deploy and manage containerized applications
- **Service Management**: Monitor and control system services
- **Health Monitoring**: Perform health checks and system monitoring

### Configuration & Environment Management
- **Environment Variables**: Manage and retrieve environment settings
- **Deployment Configurations**: Create and validate deployment configurations
- **Secrets Management**: Handle sensitive configuration data securely

## Technology Stack

- **.NET 8.0**: Built on the latest .NET platform
- **C# 12.0**: Using modern C# language features
- **Model Context Protocol**: Implements MCP server specification
- **AWS SDK**: Native AWS service integration
- **Azure SDK**: Native Azure service integration
- **SSH.NET**: Secure Shell operations for Linux deployments
- **Docker.DotNet**: Docker container management

## Installation

### Prerequisites
- .NET 8.0 SDK or later
- Compatible MCP client (Claude Desktop, VS Code with MCP extension, etc.)
- Cloud CLI tools (optional but recommended):
  - AWS CLI for AWS deployments
  - Azure CLI for Azure deployments
  - Docker for container operations

### Building from Source
```bash
git clone https://github.com/jihadkhawaja/MCP.DevOps.git
cd MCP.DevOps
dotnet build
```

### Running the Server
```bash
dotnet run --project MCP.DevOps
```

### Docker Deployment
```bash
docker build -t mcp-devops .
docker run -it mcp-devops
```

## Usage

### MCP Client Configuration

#### Claude Desktop
Add to your Claude Desktop configuration:
```json
{
  "mcpServers": {
    "MCP.DevOps": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/MCP.DevOps.csproj"]
    }
  }
}
```

#### VS Code with MCP Extension
Configure in your MCP settings:
```json
"servers": {
  "MCP.DevOps": {
    "type": "stdio",
    "command": "dotnet",
    "args": [
      "run",
      "--project",
      "path/to/MCP.DevOps.csproj"
    ]
  }
}
```

## Available Tools

### System & Command Operations
| Tool | Description | Parameters |
|------|-------------|------------|
| `GetSystemInfo` | Get system information and available tools | - |
| `ExecuteCommand` | Execute shell commands with timeout | `command`, `arguments?`, `workingDirectory?`, `timeoutSeconds?` |
| `ListEnvironmentVariables` | List environment variables by pattern | `pattern?` |
| `GetEnvironmentVariable` | Get specific environment variable value | `variableName` |

### Git Operations
| Tool | Description | Parameters |
|------|-------------|------------|
| `GitClone` | Clone a repository | `repositoryUrl`, `targetDirectory?`, `branch?` |
| `GitStatus` | Get repository status | `repositoryPath?` |
| `GitInfo` | Get branch and commit information | `repositoryPath?` |

### Build & Test Operations
| Tool | Description | Parameters |
|------|-------------|------------|
| `DotNetBuild` | Build .NET project | `projectPath`, `configuration?`, `framework?`, `output?` |
| `DotNetPublish` | Publish .NET project | `projectPath`, `configuration?`, `framework?`, `output?`, `runtime?` |
| `DotNetTest` | Run .NET tests | `projectPath`, `configuration?`, `framework?` |

### Docker Operations
| Tool | Description | Parameters |
|------|-------------|------------|
| `DockerBuild` | Build Docker image | `dockerfilePath`, `imageName`, `tag?`, `buildContext?` |
| `DockerPush` | Push image to registry | `imageName`, `tag?`, `registry?` |
| `DockerRun` | Run container | `imageName`, `tag?`, `containerName?`, `ports?`, `environment?`, `volumes?`, `detached?` |
| `DockerListImages` | List local images | - |

### AWS Deployment Tools
| Tool | Description | Parameters |
|------|-------------|------------|
| `ConfigureAWS` | Configure AWS credentials | `accessKeyId`, `secretAccessKey`, `region?`, `outputFormat?` |
| `ListEC2Instances` | List EC2 instances | `region?`, `state?` |
| `LaunchEC2Instance` | Launch new EC2 instance | `imageId`, `instanceType`, `keyName`, `securityGroupId`, `subnetId?`, `minCount?`, `maxCount?` |
| `DeployToEC2` | Deploy to EC2 instance | `instanceId`, `privateKeyPath`, `sourcePath`, `targetPath`, `username?`, `deploymentScript?` |
| `DeployToECS` | Deploy to ECS service | `clusterName`, `serviceName`, `taskDefinition`, `desiredCount?`, `region?` |
| `DeployLambda` | Deploy Lambda function | `functionName`, `zipFilePath`, `runtime?`, `handler?`, `roleArn?`, `region?` |
| `CreateLambdaFunction` | Create Lambda function | `functionName`, `zipFilePath`, `roleArn`, `runtime?`, `handler?`, `region?`, `timeout?`, `memorySize?` |
| `UploadToS3` | Upload files to S3 | `localPath`, `bucketName`, `s3Key?`, `region?`, `recursive?` |
| `SyncToS3` | Sync directory with S3 | `localPath`, `bucketName`, `s3Prefix?`, `region?`, `delete?` |

### Azure Deployment Tools
| Tool | Description | Parameters |
|------|-------------|------------|
| `AzureLogin` | Login to Azure CLI | `clientId`, `clientSecret`, `tenantId` |
| `SetAzureSubscription` | Set active subscription | `subscriptionId` |
| `ListResourceGroups` | List resource groups | - |
| `CreateResourceGroup` | Create resource group | `name`, `location` |
| `CreateAppServicePlan` | Create App Service plan | `planName`, `resourceGroup`, `location`, `sku?` |
| `CreateWebApp` | Create Web App | `appName`, `resourceGroup`, `planName`, `runtime?` |
| `DeployWebAppFromZip` | Deploy Web App from ZIP | `appName`, `resourceGroup`, `zipFilePath` |
| `DeployWebAppFromDirectory` | Deploy Web App from directory | `appName`, `resourceGroup`, `sourcePath` |
| `SetWebAppSettings` | Set Web App settings | `appName`, `resourceGroup`, `settings` |
| `CreateContainerRegistry` | Create Container Registry | `registryName`, `resourceGroup`, `location`, `sku?` |
| `BuildAndPushToACR` | Build and push to ACR | `registryName`, `imageName`, `tag`, `dockerfilePath`, `buildContext?` |
| `CreateContainerInstance` | Create Container Instance | `containerName`, `resourceGroup`, `image`, `location`, `cpu?`, `memoryInGb?`, `port?` |
| `DeployToContainerInstance` | Deploy to Container Instance | `containerName`, `resourceGroup`, `image`, `location`, `environmentVariables?`, `dnsNameLabel?` |
| `CreateVirtualMachine` | Create Virtual Machine | `vmName`, `resourceGroup`, `location`, `image?`, `size?`, `adminUsername?` |
| `ListVirtualMachines` | List Virtual Machines | `resourceGroup?` |
| `DeployToVM` | Deploy to VM | `vmName`, `resourceGroup`, `sourcePath`, `targetPath`, `username?`, `privateKeyPath?` |
| `CreateFunctionApp` | Create Function App | `functionAppName`, `resourceGroup`, `storageAccount`, `location`, `runtime?`, `version?` |
| `DeployFunctionApp` | Deploy Function App | `functionAppName`, `resourceGroup`, `zipFilePath` |

### Linux Server Deployment Tools
| Tool | Description | Parameters |
|------|-------------|------------|
| `TestSSHConnection` | Test SSH connectivity | `hostname`, `username`, `password?`, `privateKeyPath?`, `port?` |
| `ExecuteSSHCommand` | Execute SSH command | `hostname`, `username`, `command`, `password?`, `privateKeyPath?`, `port?`, `timeoutSeconds?` |
| `TransferFiles` | Transfer files via SCP | `hostname`, `username`, `localPath`, `remotePath`, `password?`, `privateKeyPath?`, `port?`, `recursive?` |
| `DeployDotNetApp` | Deploy .NET app to Linux | `hostname`, `username`, `localAppPath`, `remoteAppPath`, `appName`, `password?`, `privateKeyPath?`, `port?`, `installDotNet?` |
| `DeployDockerApp` | Deploy Docker app to Linux | `hostname`, `username`, `imageName`, `containerName`, `ports?`, `environment?`, `volumes?`, `password?`, `privateKeyPath?`, `port?` |
| `DeployWebAppWithNginx` | Deploy web app with Nginx | `hostname`, `username`, `localAppPath`, `remoteAppPath`, `appName`, `domain`, `appPort`, `password?`, `privateKeyPath?`, `port?` |
| `GetSystemInfo` | Get Linux system information | `hostname`, `username`, `password?`, `privateKeyPath?`, `port?` |
| `MonitorServices` | Monitor system services | `hostname`, `username`, `serviceName?`, `password?`, `privateKeyPath?`, `port?` |

### Configuration & Health Tools
| Tool | Description | Parameters |
|------|-------------|------------|
| `CreateDeploymentConfig` | Create deployment configuration | `filePath`, `appName`, `environment`, `platform`, `settings` |
| `ReadDeploymentConfig` | Read deployment configuration | `filePath` |
| `HealthCheck` | Perform web endpoint health check | `url`, `timeoutSeconds?`, `expectedStatus?` |

## Example Workflows

### Deploy .NET App to AWS EC2
```json
{
  "workflow": "Deploy to AWS EC2",
  "steps": [
    {
      "tool": "ConfigureAWS",
      "params": {
        "accessKeyId": "your-access-key",
        "secretAccessKey": "your-secret-key",
        "region": "us-east-1"
      }
    },
    {
      "tool": "DotNetPublish",
      "params": {
        "projectPath": "./MyApp.csproj",
        "configuration": "Release",
        "output": "./publish"
      }
    },
    {
      "tool": "DeployToEC2",
      "params": {
        "instanceId": "i-1234567890abcdef0",
        "privateKeyPath": "./my-key.pem",
        "sourcePath": "./publish",
        "targetPath": "/home/ec2-user/myapp"
      }
    }
  ]
}
```

### Deploy Container to Azure
```json
{
  "workflow": "Deploy to Azure Container Instance",
  "steps": [
    {
      "tool": "AzureLogin",
      "params": {
        "clientId": "your-client-id",
        "clientSecret": "your-client-secret",
        "tenantId": "your-tenant-id"
      }
    },
    {
      "tool": "DockerBuild",
      "params": {
        "dockerfilePath": "./Dockerfile",
        "imageName": "myapp",
        "tag": "latest"
      }
    },
    {
      "tool": "DeployToContainerInstance",
      "params": {
        "containerName": "myapp-container",
        "resourceGroup": "my-resource-group",
        "image": "myregistry.azurecr.io/myapp:latest",
        "location": "eastus"
      }
    }
  ]
}
```

### Deploy to Linux Server with Nginx
```json
{
  "workflow": "Deploy to Linux Server",
  "steps": [
    {
      "tool": "DotNetPublish",
      "params": {
        "projectPath": "./WebApp.csproj",
        "configuration": "Release",
        "output": "./publish"
      }
    },
    {
      "tool": "DeployWebAppWithNginx",
      "params": {
        "hostname": "192.168.1.100",
        "username": "ubuntu",
        "privateKeyPath": "./server-key.pem",
        "localAppPath": "./publish",
        "remoteAppPath": "/var/www/myapp",
        "appName": "myapp",
        "domain": "myapp.example.com",
        "appPort": 5000
      }
    }
  ]
}
```

## Security Considerations

- **Credential Management**: Store sensitive credentials as environment variables or use secure credential stores
- **SSH Keys**: Use SSH key authentication instead of passwords when possible
- **Network Security**: Ensure proper firewall and security group configurations
- **Access Control**: Use least-privilege principles for cloud service accounts
- **Secrets**: Never commit secrets or private keys to version control

## Error Handling

All tools return structured JSON responses with error information:

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {...},
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Error Response
```json
{
  "error": "Detailed error message",
  "context": {...},
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Dependencies

- `Microsoft.Extensions.Hosting` (9.0.7) - Application hosting framework
- `ModelContextProtocol` (0.3.0-preview.2) - MCP server implementation
- `AWSSDK.Core` (3.7.400.63) - AWS SDK core components
- `AWSSDK.EC2` (3.7.423.1) - AWS EC2 service
- `AWSSDK.ECS` (3.7.407.9) - AWS ECS service
- `AWSSDK.Lambda` (3.7.408.2) - AWS Lambda service
- `Azure.Identity` (1.13.1) - Azure authentication
- `Azure.ResourceManager` (1.14.0-beta.1) - Azure resource management
- `Azure.ResourceManager.AppService` (1.2.0) - Azure App Service
- `Azure.ResourceManager.ContainerInstance` (1.2.1) - Azure Container Instances
- `SSH.NET` (2024.2.0) - SSH operations
- `Docker.DotNet` (3.125.15) - Docker container management

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.

## Support

For questions, issues, or contributions, please visit the [GitHub repository](https://github.com/jihadkhawaja/MCP.DevOps) or open an issue.

---

**Note**: This MCP server provides powerful DevOps automation capabilities. Ensure you understand the security implications and have proper access controls in place when deploying to production environments.