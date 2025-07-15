using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Diagnostics;
using System.Text;

namespace MCP.DevOps.Tools
{
    [McpServerToolType]
    public class DevOpsTool
    {
        #region System Information

        [McpServerTool, Description("Gets system information including OS, runtime, and available tools.")]
        public static string GetSystemInfo()
        {
            try
            {
                var systemInfo = new
                {
                    success = true,
                    os = Environment.OSVersion.ToString(),
                    runtime = Environment.Version.ToString(),
                    machineName = Environment.MachineName,
                    currentDirectory = Environment.CurrentDirectory,
                    availableTools = new[]
                    {
                        "docker",
                        "kubectl",
                        "aws",
                        "az",
                        "terraform",
                        "git",
                        "ssh"
                    }
                };

                return JsonSerializer.Serialize(systemInfo);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        #endregion

        #region Command Execution

        [McpServerTool, Description("Executes a shell command and returns the output. Use with caution.")]
        public static string ExecuteCommand(string command, string arguments = "", string workingDirectory = "", int timeoutSeconds = 30)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? Environment.CurrentDirectory : workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var completed = process.WaitForExit(timeoutSeconds * 1000);
                
                if (!completed)
                {
                    process.Kill();
                    return JsonSerializer.Serialize(new 
                    { 
                        error = "Command timed out", 
                        command, 
                        arguments, 
                        timeoutSeconds 
                    });
                }

                var result = new
                {
                    success = process.ExitCode == 0,
                    exitCode = process.ExitCode,
                    command,
                    arguments,
                    workingDirectory = processStartInfo.WorkingDirectory,
                    output = outputBuilder.ToString().Trim(),
                    error = errorBuilder.ToString().Trim(),
                    executionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = ex.Message, 
                    command, 
                    arguments, 
                    workingDirectory 
                });
            }
        }

        #endregion

        #region Docker Operations

        [McpServerTool, Description("Builds a Docker image from a Dockerfile in the specified directory.")]
        public static string DockerBuild(string dockerfilePath, string imageName, string tag = "latest", string buildContext = ".")
        {
            try
            {
                var arguments = $"build -t {imageName}:{tag} -f {dockerfilePath} {buildContext}";
                return ExecuteCommand("docker", arguments, Path.GetDirectoryName(dockerfilePath) ?? Environment.CurrentDirectory, 300);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, dockerfilePath, imageName, tag });
            }
        }

        [McpServerTool, Description("Pushes a Docker image to a registry.")]
        public static string DockerPush(string imageName, string tag = "latest", string registry = "")
        {
            try
            {
                var fullImageName = string.IsNullOrEmpty(registry) ? 
                    $"{imageName}:{tag}" : 
                    $"{registry}/{imageName}:{tag}";
                
                var arguments = $"push {fullImageName}";
                return ExecuteCommand("docker", arguments, timeoutSeconds: 300);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, imageName, tag, registry });
            }
        }

        [McpServerTool, Description("Lists Docker images on the local system.")]
        public static string DockerListImages()
        {
            try
            {
                return ExecuteCommand("docker", "images --format json");
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [McpServerTool, Description("Runs a Docker container from an image.")]
        public static string DockerRun(string imageName, string tag = "latest", string containerName = "", string ports = "", string environment = "", string volumes = "", bool detached = true)
        {
            try
            {
                var arguments = new StringBuilder("run");
                
                if (detached)
                    arguments.Append(" -d");
                
                if (!string.IsNullOrEmpty(containerName))
                    arguments.Append($" --name {containerName}");
                
                if (!string.IsNullOrEmpty(ports))
                    arguments.Append($" -p {ports}");
                
                if (!string.IsNullOrEmpty(environment))
                    arguments.Append($" -e {environment}");
                
                if (!string.IsNullOrEmpty(volumes))
                    arguments.Append($" -v {volumes}");
                
                arguments.Append($" {imageName}:{tag}");
                
                return ExecuteCommand("docker", arguments.ToString(), timeoutSeconds: 60);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, imageName, tag });
            }
        }

        #endregion

        #region Git Operations

        [McpServerTool, Description("Clones a Git repository to the specified directory.")]
        public static string GitClone(string repositoryUrl, string targetDirectory = "", string branch = "")
        {
            try
            {
                var arguments = new StringBuilder($"clone {repositoryUrl}");
                
                if (!string.IsNullOrEmpty(branch))
                    arguments.Append($" -b {branch}");
                
                if (!string.IsNullOrEmpty(targetDirectory))
                    arguments.Append($" {targetDirectory}");
                
                return ExecuteCommand("git", arguments.ToString(), timeoutSeconds: 120);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, repositoryUrl, targetDirectory, branch });
            }
        }

        [McpServerTool, Description("Gets the current Git status of a repository.")]
        public static string GitStatus(string repositoryPath = ".")
        {
            try
            {
                return ExecuteCommand("git", "status --porcelain", repositoryPath);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, repositoryPath });
            }
        }

        [McpServerTool, Description("Gets the current Git branch and commit information.")]
        public static string GitInfo(string repositoryPath = ".")
        {
            try
            {
                var branchResult = ExecuteCommand("git", "branch --show-current", repositoryPath);
                var commitResult = ExecuteCommand("git", "rev-parse HEAD", repositoryPath);
                var remoteResult = ExecuteCommand("git", "remote get-url origin", repositoryPath);

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    repositoryPath,
                    branch = branchResult,
                    commit = commitResult,
                    remote = remoteResult,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, repositoryPath });
            }
        }

        #endregion

        #region Build Operations

        [McpServerTool, Description("Builds a .NET project or solution.")]
        public static string DotNetBuild(string projectPath, string configuration = "Release", string framework = "", string output = "")
        {
            try
            {
                var arguments = new StringBuilder($"build {projectPath}");
                arguments.Append($" -c {configuration}");
                
                if (!string.IsNullOrEmpty(framework))
                    arguments.Append($" -f {framework}");
                
                if (!string.IsNullOrEmpty(output))
                    arguments.Append($" -o {output}");
                
                return ExecuteCommand("dotnet", arguments.ToString(), Path.GetDirectoryName(projectPath) ?? Environment.CurrentDirectory, 300);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, projectPath, configuration });
            }
        }

        [McpServerTool, Description("Publishes a .NET project for deployment.")]
        public static string DotNetPublish(string projectPath, string configuration = "Release", string framework = "", string output = "", string runtime = "")
        {
            try
            {
                var arguments = new StringBuilder($"publish {projectPath}");
                arguments.Append($" -c {configuration}");
                
                if (!string.IsNullOrEmpty(framework))
                    arguments.Append($" -f {framework}");
                
                if (!string.IsNullOrEmpty(output))
                    arguments.Append($" -o {output}");
                
                if (!string.IsNullOrEmpty(runtime))
                    arguments.Append($" -r {runtime}");
                
                return ExecuteCommand("dotnet", arguments.ToString(), Path.GetDirectoryName(projectPath) ?? Environment.CurrentDirectory, 300);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, projectPath, configuration });
            }
        }

        [McpServerTool, Description("Runs tests for a .NET project.")]
        public static string DotNetTest(string projectPath, string configuration = "Release", string framework = "")
        {
            try
            {
                var arguments = new StringBuilder($"test {projectPath}");
                arguments.Append($" -c {configuration}");
                
                if (!string.IsNullOrEmpty(framework))
                    arguments.Append($" -f {framework}");
                
                arguments.Append(" --logger trx --results-directory TestResults");
                
                return ExecuteCommand("dotnet", arguments.ToString(), Path.GetDirectoryName(projectPath) ?? Environment.CurrentDirectory, 300);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, projectPath, configuration });
            }
        }

        #endregion

        #region File Operations

        [McpServerTool, Description("Creates a deployment configuration file with the specified settings.")]
        public static string CreateDeploymentConfig(string filePath, string appName, string environment, string platform, string settings)
        {
            try
            {
                var config = new
                {
                    appName,
                    environment,
                    platform,
                    settings = JsonSerializer.Deserialize<object>(settings),
                    createdAt = DateTime.UtcNow,
                    version = "1.0"
                };

                var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, configJson);

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    message = "Deployment configuration created successfully",
                    filePath,
                    size = configJson.Length
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, filePath, appName, environment, platform });
            }
        }

        [McpServerTool, Description("Reads and validates a deployment configuration file.")]
        public static string ReadDeploymentConfig(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return JsonSerializer.Serialize(new { error = "Configuration file not found", filePath });

                var content = File.ReadAllText(filePath);
                var config = JsonSerializer.Deserialize<object>(content);

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    filePath,
                    config,
                    size = content.Length
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, filePath });
            }
        }

        #endregion

        #region Health Checks

        [McpServerTool, Description("Performs a health check on a web endpoint.")]
        public static string HealthCheck(string url, int timeoutSeconds = 30, string expectedStatus = "200")
        {
            try
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
                
                var response = httpClient.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                var result = new
                {
                    success = true,
                    url,
                    statusCode = (int)response.StatusCode,
                    statusDescription = response.StatusCode.ToString(),
                    isHealthy = response.IsSuccessStatusCode,
                    responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    contentLength = content.Length,
                    headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
                };

                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new 
                { 
                    success = false,
                    error = ex.Message, 
                    url, 
                    isHealthy = false 
                });
            }
        }

        #endregion

        #region Environment Management

        [McpServerTool, Description("Lists environment variables that match a pattern.")]
        public static string ListEnvironmentVariables(string pattern = "")
        {
            try
            {
                var envVars = Environment.GetEnvironmentVariables()
                    .Cast<System.Collections.DictionaryEntry>()
                    .Where(entry => string.IsNullOrEmpty(pattern) || 
                                  entry.Key.ToString()!.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(entry => entry.Key.ToString()!, entry => entry.Value?.ToString());

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    pattern,
                    count = envVars.Count,
                    variables = envVars
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, pattern });
            }
        }

        [McpServerTool, Description("Gets the value of a specific environment variable.")]
        public static string GetEnvironmentVariable(string variableName)
        {
            try
            {
                var value = Environment.GetEnvironmentVariable(variableName);
                
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    variableName,
                    value,
                    exists = value != null
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, variableName });
            }
        }

        #endregion
    }
}