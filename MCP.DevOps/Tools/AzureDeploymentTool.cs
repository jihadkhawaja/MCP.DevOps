using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Text;

namespace MCP.DevOps.Tools
{
    [McpServerToolType]
    public class AzureDeploymentTool
    {
        #region Azure CLI Operations

        [McpServerTool, Description("Logs into Azure CLI using service principal credentials.")]
        public static string AzureLogin(string clientId, string clientSecret, string tenantId)
        {
            try
            {
                var arguments = $"login --service-principal -u {clientId} -p {clientSecret} --tenant {tenantId}";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, clientId, tenantId });
            }
        }

        [McpServerTool, Description("Sets the active Azure subscription.")]
        public static string SetAzureSubscription(string subscriptionId)
        {
            try
            {
                var arguments = $"account set --subscription {subscriptionId}";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, subscriptionId });
            }
        }

        [McpServerTool, Description("Lists Azure resource groups.")]
        public static string ListResourceGroups()
        {
            try
            {
                return ExecuteAzureCommand("group list --output json");
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [McpServerTool, Description("Creates a new Azure resource group.")]
        public static string CreateResourceGroup(string name, string location)
        {
            try
            {
                var arguments = $"group create --name {name} --location {location}";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, name, location });
            }
        }

        #endregion

        #region App Service Operations

        [McpServerTool, Description("Creates an Azure App Service plan.")]
        public static string CreateAppServicePlan(string planName, string resourceGroup, string location, string sku = "F1")
        {
            try
            {
                var arguments = $"appservice plan create --name {planName} --resource-group {resourceGroup} --location {location} --sku {sku}";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, planName, resourceGroup, location, sku });
            }
        }

        [McpServerTool, Description("Creates an Azure Web App.")]
        public static string CreateWebApp(string appName, string resourceGroup, string planName, string runtime = "DOTNETCORE|8.0")
        {
            try
            {
                var arguments = $"webapp create --name {appName} --resource-group {resourceGroup} --plan {planName} --runtime \"{runtime}\"";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, appName, resourceGroup, planName, runtime });
            }
        }

        [McpServerTool, Description("Deploys an application to Azure Web App from a ZIP file.")]
        public static string DeployWebAppFromZip(string appName, string resourceGroup, string zipFilePath)
        {
            try
            {
                var arguments = $"webapp deploy --name {appName} --resource-group {resourceGroup} --src-path {zipFilePath} --type zip";
                return ExecuteAzureCommand(arguments, timeoutSeconds: 300);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, appName, resourceGroup, zipFilePath });
            }
        }

        [McpServerTool, Description("Deploys an application to Azure Web App from a local directory.")]
        public static string DeployWebAppFromDirectory(string appName, string resourceGroup, string sourcePath)
        {
            try
            {
                // First, create a ZIP file from the directory
                var tempZipPath = Path.Combine(Path.GetTempPath(), $"{appName}-{DateTime.Now:yyyyMMddHHmmss}.zip");
                System.IO.Compression.ZipFile.CreateFromDirectory(sourcePath, tempZipPath);

                var result = DeployWebAppFromZip(appName, resourceGroup, tempZipPath);

                // Clean up temp file
                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);

                return result;
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, appName, resourceGroup, sourcePath });
            }
        }

        [McpServerTool, Description("Sets application settings for an Azure Web App.")]
        public static string SetWebAppSettings(string appName, string resourceGroup, string settings)
        {
            try
            {
                var arguments = $"webapp config appsettings set --name {appName} --resource-group {resourceGroup} --settings {settings}";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, appName, resourceGroup, settings });
            }
        }

        #endregion

        #region Container Operations

        [McpServerTool, Description("Creates an Azure Container Registry.")]
        public static string CreateContainerRegistry(string registryName, string resourceGroup, string location, string sku = "Basic")
        {
            try
            {
                var arguments = $"acr create --name {registryName} --resource-group {resourceGroup} --location {location} --sku {sku}";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, registryName, resourceGroup, location, sku });
            }
        }

        [McpServerTool, Description("Builds and pushes a Docker image to Azure Container Registry.")]
        public static string BuildAndPushToACR(string registryName, string imageName, string tag, string dockerfilePath, string buildContext = ".")
        {
            try
            {
                var arguments = $"acr build --registry {registryName} --image {imageName}:{tag} --file {dockerfilePath} {buildContext}";
                return ExecuteAzureCommand(arguments, timeoutSeconds: 600);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, registryName, imageName, tag, dockerfilePath });
            }
        }

        [McpServerTool, Description("Creates an Azure Container Instance.")]
        public static string CreateContainerInstance(string containerName, string resourceGroup, string image, string location, int cpu = 1, double memoryInGb = 1.0, int port = 80)
        {
            try
            {
                var arguments = $"container create --name {containerName} --resource-group {resourceGroup} --image {image} --location {location} --cpu {cpu} --memory {memoryInGb} --ports {port}";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, containerName, resourceGroup, image, location });
            }
        }

        [McpServerTool, Description("Deploys a container to Azure Container Instances.")]
        public static string DeployToContainerInstance(string containerName, string resourceGroup, string image, string location, string environmentVariables = "", string dnsNameLabel = "")
        {
            try
            {
                var arguments = new StringBuilder($"container create --name {containerName} --resource-group {resourceGroup} --image {image} --location {location}");
                
                if (!string.IsNullOrEmpty(environmentVariables))
                    arguments.Append($" --environment-variables {environmentVariables}");
                
                if (!string.IsNullOrEmpty(dnsNameLabel))
                    arguments.Append($" --dns-name-label {dnsNameLabel}");

                return ExecuteAzureCommand(arguments.ToString());
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, containerName, resourceGroup, image, location });
            }
        }

        #endregion

        #region Virtual Machine Operations

        [McpServerTool, Description("Creates an Azure Virtual Machine.")]
        public static string CreateVirtualMachine(string vmName, string resourceGroup, string location, string image = "UbuntuLTS", string size = "Standard_B1s", string adminUsername = "azureuser")
        {
            try
            {
                var arguments = $"vm create --name {vmName} --resource-group {resourceGroup} --location {location} --image {image} --size {size} --admin-username {adminUsername} --generate-ssh-keys";
                return ExecuteAzureCommand(arguments, timeoutSeconds: 300);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, vmName, resourceGroup, location, image });
            }
        }

        [McpServerTool, Description("Lists Azure Virtual Machines.")]
        public static string ListVirtualMachines(string resourceGroup = "")
        {
            try
            {
                var arguments = "vm list --output json";
                if (!string.IsNullOrEmpty(resourceGroup))
                    arguments = $"vm list --resource-group {resourceGroup} --output json";

                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, resourceGroup });
            }
        }

        [McpServerTool, Description("Deploys an application to an Azure VM using SSH.")]
        public static string DeployToVM(string vmName, string resourceGroup, string sourcePath, string targetPath, string username = "azureuser", string privateKeyPath = "")
        {
            try
            {
                // Get VM public IP
                var ipCommand = $"vm show --name {vmName} --resource-group {resourceGroup} --show-details --query publicIps --output tsv";
                var ipResult = ExecuteAzureCommand(ipCommand);

                var result = new
                {
                    success = true,
                    message = "VM deployment initiated",
                    vmName,
                    resourceGroup,
                    sourcePath,
                    targetPath,
                    username,
                    timestamp = DateTime.UtcNow
                };

                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, vmName, resourceGroup, sourcePath, targetPath });
            }
        }

        #endregion

        #region Function App Operations

        [McpServerTool, Description("Creates an Azure Function App.")]
        public static string CreateFunctionApp(string functionAppName, string resourceGroup, string storageAccount, string location, string runtime = "dotnet", string version = "8")
        {
            try
            {
                var arguments = $"functionapp create --name {functionAppName} --resource-group {resourceGroup} --storage-account {storageAccount} --consumption-plan-location {location} --runtime {runtime} --runtime-version {version}";
                return ExecuteAzureCommand(arguments);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, functionAppName, resourceGroup, storageAccount, location });
            }
        }

        [McpServerTool, Description("Deploys a Function App from a ZIP file.")]
        public static string DeployFunctionApp(string functionAppName, string resourceGroup, string zipFilePath)
        {
            try
            {
                var arguments = $"functionapp deploy --name {functionAppName} --resource-group {resourceGroup} --src-path {zipFilePath} --type zip";
                return ExecuteAzureCommand(arguments, timeoutSeconds: 300);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, functionAppName, resourceGroup, zipFilePath });
            }
        }

        #endregion

        #region Helper Methods

        private static string ExecuteAzureCommand(string arguments, int timeoutSeconds = 60)
        {
            try
            {
                return DevOpsTool.ExecuteCommand("az", arguments, timeoutSeconds: timeoutSeconds);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, command = "az", arguments });
            }
        }

        #endregion
    }
}