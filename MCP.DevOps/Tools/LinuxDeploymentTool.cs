using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using Renci.SshNet;

namespace MCP.DevOps.Tools
{
    [McpServerToolType]
    public class LinuxDeploymentTool
    {
        #region SSH Operations

        [McpServerTool, Description("Tests SSH connectivity to a Linux server.")]
        public static string TestSSHConnection(string hostname, string username, string password = "", string privateKeyPath = "", int port = 22)
        {
            try
            {
                using var client = CreateSshClient(hostname, username, password, privateKeyPath, port);
                client.Connect();
                
                var result = client.RunCommand("uname -a");
                client.Disconnect();

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    hostname,
                    username,
                    port,
                    connected = true,
                    systemInfo = result.Result.Trim(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, hostname, username, port, connected = false });
            }
        }

        [McpServerTool, Description("Executes a command on a remote Linux server via SSH.")]
        public static string ExecuteSSHCommand(string hostname, string username, string command, string password = "", string privateKeyPath = "", int port = 22, int timeoutSeconds = 60)
        {
            try
            {
                using var client = CreateSshClient(hostname, username, password, privateKeyPath, port);
                client.Connect();
                
                var sshCommand = client.CreateCommand(command);
                sshCommand.CommandTimeout = TimeSpan.FromSeconds(timeoutSeconds);
                
                var result = sshCommand.Execute();
                client.Disconnect();

                return JsonSerializer.Serialize(new
                {
                    success = sshCommand.ExitStatus == 0,
                    hostname,
                    command,
                    exitStatus = sshCommand.ExitStatus,
                    output = result.Trim(),
                    error = sshCommand.Error.Trim(),
                    executionTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, hostname, command });
            }
        }

        [McpServerTool, Description("Transfers files to a Linux server using SCP.")]
        public static string TransferFiles(string hostname, string username, string localPath, string remotePath, string password = "", string privateKeyPath = "", int port = 22, bool recursive = false)
        {
            try
            {
                using var client = CreateSshClient(hostname, username, password, privateKeyPath, port);
                client.Connect();

                using var scp = new ScpClient(client.ConnectionInfo);
                scp.Connect();

                if (Directory.Exists(localPath))
                {
                    scp.Upload(new DirectoryInfo(localPath), remotePath);
                }
                else if (File.Exists(localPath))
                {
                    scp.Upload(new FileInfo(localPath), remotePath);
                }
                else
                {
                    throw new FileNotFoundException($"Local path not found: {localPath}");
                }

                scp.Disconnect();
                client.Disconnect();

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    hostname,
                    localPath,
                    remotePath,
                    recursive,
                    message = "Files transferred successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, hostname, localPath, remotePath });
            }
        }

        #endregion

        #region Application Deployment

        [McpServerTool, Description("Deploys a .NET application to a Linux server.")]
        public static string DeployDotNetApp(string hostname, string username, string localAppPath, string remoteAppPath, string appName, string password = "", string privateKeyPath = "", int port = 22, bool installDotNet = false)
        {
            try
            {
                using var client = CreateSshClient(hostname, username, password, privateKeyPath, port);
                client.Connect();

                var deploymentSteps = new List<string>();

                // Step 1: Install .NET if requested
                if (installDotNet)
                {
                    deploymentSteps.Add("Installing .NET 8.0");
                    var installCommands = new[]
                    {
                        "curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0",
                        "echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc",
                        "echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.bashrc",
                        "source ~/.bashrc"
                    };

                    foreach (var cmd in installCommands)
                    {
                        var result = client.RunCommand(cmd);
                        if (result.ExitStatus != 0)
                            deploymentSteps.Add($"Warning: {cmd} returned exit code {result.ExitStatus}");
                    }
                }

                // Step 2: Create remote directory
                deploymentSteps.Add($"Creating directory {remoteAppPath}");
                client.RunCommand($"mkdir -p {remoteAppPath}");

                // Step 3: Transfer files
                deploymentSteps.Add("Transferring application files");
                using var scp = new ScpClient(client.ConnectionInfo);
                scp.Connect();
                scp.Upload(new DirectoryInfo(localAppPath), remoteAppPath);
                scp.Disconnect();

                // Step 4: Set executable permissions
                deploymentSteps.Add("Setting executable permissions");
                client.RunCommand($"chmod +x {remoteAppPath}/{appName}");

                // Step 5: Create systemd service (optional)
                var serviceContent = $@"[Unit]
Description={appName}
After=network.target

[Service]
Type=simple
User={username}
WorkingDirectory={remoteAppPath}
ExecStart={remoteAppPath}/{appName}
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target";

                var createServiceCommand = $"echo '{serviceContent}' | sudo tee /etc/systemd/system/{appName}.service";
                var serviceResult = client.RunCommand(createServiceCommand);
                if (serviceResult.ExitStatus == 0)
                {
                    deploymentSteps.Add("Created systemd service");
                    client.RunCommand("sudo systemctl daemon-reload");
                    client.RunCommand($"sudo systemctl enable {appName}");
                }

                client.Disconnect();

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    hostname,
                    appName,
                    remoteAppPath,
                    deploymentSteps,
                    message = "Application deployed successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, hostname, appName, localAppPath, remoteAppPath });
            }
        }

        [McpServerTool, Description("Deploys a Docker container to a Linux server.")]
        public static string DeployDockerApp(string hostname, string username, string imageName, string containerName, string ports = "", string environment = "", string volumes = "", string password = "", string privateKeyPath = "", int port = 22)
        {
            try
            {
                using var client = CreateSshClient(hostname, username, password, privateKeyPath, port);
                client.Connect();

                var deploymentSteps = new List<string>();

                // Step 1: Install Docker if not present
                deploymentSteps.Add("Checking Docker installation");
                var dockerCheck = client.RunCommand("which docker");
                if (dockerCheck.ExitStatus != 0)
                {
                    deploymentSteps.Add("Installing Docker");
                    var dockerInstallCommands = new[]
                    {
                        "sudo apt-get update",
                        "sudo apt-get install -y docker.io",
                        "sudo systemctl start docker",
                        "sudo systemctl enable docker",
                        $"sudo usermod -aG docker {username}"
                    };

                    foreach (var cmd in dockerInstallCommands)
                    {
                        var result = client.RunCommand(cmd);
                        if (result.ExitStatus != 0)
                            deploymentSteps.Add($"Warning: {cmd} returned exit code {result.ExitStatus}");
                    }
                }

                // Step 2: Stop and remove existing container
                deploymentSteps.Add($"Stopping existing container {containerName}");
                client.RunCommand($"docker stop {containerName} || true");
                client.RunCommand($"docker rm {containerName} || true");

                // Step 3: Pull the image
                deploymentSteps.Add($"Pulling image {imageName}");
                var pullResult = client.RunCommand($"docker pull {imageName}");
                if (pullResult.ExitStatus != 0)
                    deploymentSteps.Add($"Warning: Failed to pull image - {pullResult.Error}");

                // Step 4: Run the container
                var runCommand = new StringBuilder($"docker run -d --name {containerName}");
                
                if (!string.IsNullOrEmpty(ports))
                    runCommand.Append($" -p {ports}");
                
                if (!string.IsNullOrEmpty(environment))
                    runCommand.Append($" -e {environment}");
                
                if (!string.IsNullOrEmpty(volumes))
                    runCommand.Append($" -v {volumes}");
                
                runCommand.Append($" {imageName}");

                deploymentSteps.Add("Starting container");
                var runResult = client.RunCommand(runCommand.ToString());
                
                client.Disconnect();

                return JsonSerializer.Serialize(new
                {
                    success = runResult.ExitStatus == 0,
                    hostname,
                    imageName,
                    containerName,
                    deploymentSteps,
                    containerOutput = runResult.Result.Trim(),
                    message = runResult.ExitStatus == 0 ? "Container deployed successfully" : "Container deployment failed",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, hostname, imageName, containerName });
            }
        }

        [McpServerTool, Description("Deploys a web application using Nginx reverse proxy.")]
        public static string DeployWebAppWithNginx(string hostname, string username, string localAppPath, string remoteAppPath, string appName, string domain, int appPort, string password = "", string privateKeyPath = "", int port = 22)
        {
            try
            {
                using var client = CreateSshClient(hostname, username, password, privateKeyPath, port);
                client.Connect();

                var deploymentSteps = new List<string>();

                // Step 1: Deploy the application
                deploymentSteps.Add("Deploying application");
                var deployResult = DeployDotNetApp(hostname, username, localAppPath, remoteAppPath, appName, password, privateKeyPath, port);

                // Step 2: Install Nginx
                deploymentSteps.Add("Installing Nginx");
                client.RunCommand("sudo apt-get update");
                client.RunCommand("sudo apt-get install -y nginx");

                // Step 3: Configure Nginx
                var nginxConfig = $@"server {{
    listen 80;
    server_name {domain};

    location / {{
        proxy_pass http://localhost:{appPort};
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }}
}}";

                var configCommand = $"echo '{nginxConfig}' | sudo tee /etc/nginx/sites-available/{appName}";
                client.RunCommand(configCommand);
                client.RunCommand($"sudo ln -sf /etc/nginx/sites-available/{appName} /etc/nginx/sites-enabled/");
                client.RunCommand("sudo nginx -t");
                client.RunCommand("sudo systemctl reload nginx");

                deploymentSteps.Add("Configured Nginx reverse proxy");

                // Step 4: Start the application
                deploymentSteps.Add("Starting application service");
                client.RunCommand($"sudo systemctl start {appName}");

                client.Disconnect();

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    hostname,
                    appName,
                    domain,
                    appPort,
                    deploymentSteps,
                    message = "Web application deployed with Nginx",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, hostname, appName, domain });
            }
        }

        #endregion

        #region System Management

        [McpServerTool, Description("Gets system information from a Linux server.")]
        public static string GetSystemInfo(string hostname, string username, string password = "", string privateKeyPath = "", int port = 22)
        {
            try
            {
                using var client = CreateSshClient(hostname, username, password, privateKeyPath, port);
                client.Connect();

                var commands = new Dictionary<string, string>
                {
                    ["os"] = "cat /etc/os-release | grep PRETTY_NAME | cut -d'\"' -f2",
                    ["kernel"] = "uname -r",
                    ["uptime"] = "uptime -p",
                    ["memory"] = "free -h | grep Mem | awk '{print $2\" total, \"$3\" used, \"$7\" available\"}'",
                    ["disk"] = "df -h / | tail -1 | awk '{print $2\" total, \"$3\" used, \"$4\" available, \"$5\" used%\"}'",
                    ["cpu"] = "lscpu | grep 'Model name' | cut -d':' -f2 | xargs",
                    ["load"] = "cat /proc/loadavg | awk '{print $1\" (1min) \"$2\" (5min) \"$3\" (15min)\"}'"
                };

                var systemInfo = new Dictionary<string, string>();
                foreach (var cmd in commands)
                {
                    var result = client.RunCommand(cmd.Value);
                    systemInfo[cmd.Key] = result.Result.Trim();
                }

                client.Disconnect();

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    hostname,
                    systemInfo,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, hostname });
            }
        }

        [McpServerTool, Description("Monitors running services on a Linux server.")]
        public static string MonitorServices(string hostname, string username, string serviceName = "", string password = "", string privateKeyPath = "", int port = 22)
        {
            try
            {
                using var client = CreateSshClient(hostname, username, password, privateKeyPath, port);
                client.Connect();

                string command;
                if (string.IsNullOrEmpty(serviceName))
                {
                    command = "systemctl list-units --type=service --state=running --no-pager";
                }
                else
                {
                    command = $"systemctl status {serviceName} --no-pager";
                }

                var result = client.RunCommand(command);
                client.Disconnect();

                return JsonSerializer.Serialize(new
                {
                    success = result.ExitStatus == 0,
                    hostname,
                    serviceName,
                    output = result.Result.Trim(),
                    error = result.Error.Trim(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, hostname, serviceName });
            }
        }

        #endregion

        #region Helper Methods

        private static SshClient CreateSshClient(string hostname, string username, string password, string privateKeyPath, int port)
        {
            if (!string.IsNullOrEmpty(privateKeyPath) && File.Exists(privateKeyPath))
            {
                var keyFile = new PrivateKeyFile(privateKeyPath);
                return new SshClient(hostname, port, username, keyFile);
            }
            else if (!string.IsNullOrEmpty(password))
            {
                return new SshClient(hostname, port, username, password);
            }
            else
            {
                throw new ArgumentException("Either password or privateKeyPath must be provided");
            }
        }

        #endregion
    }
}