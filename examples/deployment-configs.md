# Example deployment configurations

## AWS EC2 Deployment
```json
{
  "appName": "MyWebApp",
  "environment": "production",
  "platform": "aws-ec2",
  "settings": {
    "region": "us-east-1",
    "instanceType": "t3.micro",
    "keyName": "my-ec2-key",
    "securityGroup": "sg-12345678",
    "targetPath": "/home/ec2-user/myapp",
    "runtime": "dotnet8",
    "port": 5000
  }
}
```

## Azure App Service Deployment
```json
{
  "appName": "MyWebApp",
  "environment": "production", 
  "platform": "azure-appservice",
  "settings": {
    "resourceGroup": "my-resource-group",
    "location": "eastus",
    "runtime": "DOTNETCORE|8.0",
    "sku": "B1",
    "appSettings": {
      "ASPNETCORE_ENVIRONMENT": "Production"
    }
  }
}
```

## Linux Server Deployment
```json
{
  "appName": "MyWebApp",
  "environment": "production",
  "platform": "linux-server",
  "settings": {
    "hostname": "192.168.1.100",
    "username": "ubuntu",
    "targetPath": "/var/www/myapp",
    "domain": "myapp.example.com",
    "port": 5000,
    "useNginx": true,
    "installDotNet": true
  }
}
```

## Docker Container Deployment
```json
{
  "appName": "MyWebApp",
  "environment": "production",
  "platform": "docker",
  "settings": {
    "imageName": "myapp",
    "tag": "latest",
    "registry": "myregistry.azurecr.io",
    "ports": "80:5000",
    "environment": "ASPNETCORE_ENVIRONMENT=Production",
    "volumes": "/data:/app/data"
  }
}
```