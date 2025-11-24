# Hướng dẫn Deployment

## Tổng quan

Hướng dẫn này giúp bạn deploy Workflow API lên các môi trường production khác nhau.

---

## Option 1: Deploy lên Azure App Service

### Bước 1: Chuẩn bị Azure Resources

```bash
# Login Azure
az login

# Tạo Resource Group
az group create --name WorkflowRG --location southeastasia

# Tạo SQL Database
az sql server create \
  --name workflow-sql-server \
  --resource-group WorkflowRG \
  --location southeastasia \
  --admin-user sqladmin \
  --admin-password YourStrongPassword123!

az sql db create \
  --resource-group WorkflowRG \
  --server workflow-sql-server \
  --name WorkflowDb \
  --service-objective S0

# Tạo App Service Plan
az appservice plan create \
  --name WorkflowPlan \
  --resource-group WorkflowRG \
  --sku B1 \
  --is-linux

# Tạo Web App
az webapp create \
  --resource-group WorkflowRG \
  --plan WorkflowPlan \
  --name workflow-api-app \
  --runtime "DOTNET|8.0"
```

### Bước 2: Cấu hình Connection String

```bash
# Get SQL connection string
az sql db show-connection-string \
  --client ado.net \
  --server workflow-sql-server \
  --name WorkflowDb

# Set connection string in App Service
az webapp config connection-string set \
  --resource-group WorkflowRG \
  --name workflow-api-app \
  --settings DefaultConnection="Server=tcp:workflow-sql-server.database.windows.net,1433;Database=WorkflowDb;User ID=sqladmin;Password=YourStrongPassword123!;Encrypt=True;TrustServerCertificate=False;" \
  --connection-string-type SQLAzure
```

### Bước 3: Deploy Code

```bash
# Publish app
cd src
dotnet publish -c Release -o ./publish

# Zip files
cd publish
zip -r ../app.zip .

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group WorkflowRG \
  --name workflow-api-app \
  --src ../app.zip
```

### Bước 4: Cấu hình Firewall

```bash
# Allow Azure services
az sql server firewall-rule create \
  --resource-group WorkflowRG \
  --server workflow-sql-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Allow your IP (for management)
az sql server firewall-rule create \
  --resource-group WorkflowRG \
  --server workflow-sql-server \
  --name AllowMyIP \
  --start-ip-address YOUR_IP \
  --end-ip-address YOUR_IP
```

### Bước 5: Run Migrations

```bash
# Option 1: Local connection với Azure SQL
dotnet ef database update --connection "YOUR_CONNECTION_STRING"

# Option 2: SSH vào App Service
az webapp ssh --resource-group WorkflowRG --name workflow-api-app
# Sau đó chạy: dotnet ef database update
```

---

## Option 2: Deploy lên IIS (Windows Server)

### Bước 1: Cài đặt Prerequisites

1. **Cài đặt IIS**:
   - Server Manager > Add Roles and Features
   - Chọn Web Server (IIS)

2. **Cài đặt .NET Hosting Bundle**:
   ```
   Tải về: https://dotnet.microsoft.com/download/dotnet/8.0
   Chọn: .NET 8.0 Hosting Bundle
   ```

### Bước 2: Publish Application

```bash
cd src
dotnet publish -c Release -o C:\inetpub\wwwroot\workflow-api
```

### Bước 3: Cấu hình IIS

1. Mở IIS Manager
2. Add Website:
   - Site name: WorkflowAPI
   - Physical path: `C:\inetpub\wwwroot\workflow-api`
   - Binding: http, port 80 (hoặc 443 cho HTTPS)

3. Application Pool Settings:
   - .NET CLR version: No Managed Code
   - Managed pipeline mode: Integrated

### Bước 4: Cấu hình Connection String

Chỉnh sửa `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=WorkflowDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true"
  }
}
```

### Bước 5: Run Migrations

```bash
cd C:\inetpub\wwwroot\workflow-api
dotnet ef database update
```

---

## Option 3: Deploy với Docker

### Bước 1: Tạo Dockerfile

Tạo file `Dockerfile` trong thư mục gốc:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Workflow.csproj", "src/"]
RUN dotnet restore "src/Workflow.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "Workflow.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Workflow.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Workflow.dll"]
```

### Bước 2: Tạo docker-compose.yml

```yaml
version: '3.8'

services:
  workflow-api:
    build: .
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sql-server;Database=WorkflowDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - sql-server

  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql

volumes:
  sql-data:
```

### Bước 3: Build và Run

```bash
# Build images
docker-compose build

# Run containers
docker-compose up -d

# Check logs
docker-compose logs -f workflow-api

# Run migrations
docker-compose exec workflow-api dotnet ef database update
```

---

## Option 4: Deploy lên AWS EC2

### Bước 1: Launch EC2 Instance

1. Chọn Amazon Linux 2 hoặc Ubuntu
2. Instance type: t2.small hoặc lớn hơn
3. Cấu hình Security Group:
   - Port 22 (SSH)
   - Port 80 (HTTP)
   - Port 443 (HTTPS)
   - Port 5000 (API)

### Bước 2: Cài đặt .NET

```bash
# SSH vào EC2
ssh -i your-key.pem ec2-user@your-ec2-ip

# Cài đặt .NET 8
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Add to PATH
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
```

### Bước 3: Setup Database

Option 1: Sử dụng RDS
```bash
# Tạo RDS SQL Server instance từ AWS Console
# Copy endpoint và configure connection string
```

Option 2: Install SQL Server trên EC2
```bash
# Install Docker
sudo yum install docker -y
sudo service docker start

# Run SQL Server container
sudo docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sql-server \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

### Bước 4: Deploy Application

```bash
# Upload code
scp -i your-key.pem -r ./src ec2-user@your-ec2-ip:~/workflow

# Build and run
cd ~/workflow/src
dotnet restore
dotnet publish -c Release -o ./publish
cd publish
dotnet Workflow.dll
```

### Bước 5: Setup as Service (Systemd)

Tạo file `/etc/systemd/system/workflow.service`:

```ini
[Unit]
Description=Workflow API
After=network.target

[Service]
WorkingDirectory=/home/ec2-user/workflow/src/publish
ExecStart=/home/ec2-user/.dotnet/dotnet /home/ec2-user/workflow/src/publish/Workflow.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=workflow-api
User=ec2-user
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Start service:
```bash
sudo systemctl enable workflow
sudo systemctl start workflow
sudo systemctl status workflow
```

---

## Production Checklist

### Security
- [ ] Thay đổi tất cả default passwords
- [ ] Enable HTTPS với SSL certificate
- [ ] Implement authentication (JWT, OAuth2)
- [ ] Configure CORS cho specific domains
- [ ] Enable rate limiting
- [ ] Implement logging và monitoring

### Database
- [ ] Backup strategy
- [ ] Connection pooling
- [ ] Enable SQL Server encryption
- [ ] Regular maintenance plan

### Application
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Remove Swagger in production (hoặc protect với authentication)
- [ ] Configure proper logging (Application Insights, Serilog)
- [ ] Set up health checks
- [ ] Configure file upload limits

### Infrastructure
- [ ] Setup load balancer (nếu cần)
- [ ] Configure auto-scaling
- [ ] Setup monitoring (CloudWatch, Azure Monitor)
- [ ] Configure alerts
- [ ] Disaster recovery plan

---

## Environment Variables

Trong production, sử dụng environment variables thay vì hardcode trong appsettings.json:

```bash
export ConnectionStrings__DefaultConnection="Your_Connection_String"
export ASPNETCORE_ENVIRONMENT="Production"
export Logging__LogLevel__Default="Warning"
```

Hoặc trong `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{ConnectionString}#"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## Monitoring & Logging

### Application Insights (Azure)

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

Trong `Program.cs`:
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Serilog

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

Cấu hình trong `Program.cs`:
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/workflow-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

---

## Rollback Strategy

### Azure
```bash
# List deployment slots
az webapp deployment slot list --name workflow-api-app --resource-group WorkflowRG

# Swap slots
az webapp deployment slot swap --name workflow-api-app --resource-group WorkflowRG --slot staging
```

### Docker
```bash
# Revert to previous image
docker-compose down
docker-compose up -d --scale workflow-api=0
docker tag workflow-api:previous workflow-api:latest
docker-compose up -d
```

---

## Performance Tuning

### Database
- Enable connection pooling
- Add proper indexes
- Use async/await consistently
- Implement caching (Redis)

### API
- Enable response compression
- Use CDN for static files
- Implement pagination
- Add request throttling

---

## Support & Troubleshooting

### Check logs
```bash
# Azure
az webapp log tail --name workflow-api-app --resource-group WorkflowRG

# Docker
docker-compose logs -f workflow-api

# IIS
Check: C:\inetpub\logs\LogFiles\
```

### Common Issues
1. Database connection timeout → Check firewall rules
2. 502 Bad Gateway → Check application pool/container health
3. High memory usage → Check for memory leaks, enable garbage collection logs
4. Slow response → Enable query logging, check indexes

---

## Continuous Deployment

### GitHub Actions

Tạo `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
    
    - name: Build
      run: dotnet build src/Workflow.csproj -c Release
    
    - name: Publish
      run: dotnet publish src/Workflow.csproj -c Release -o ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: workflow-api-app
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

---

Chúc bạn deploy thành công! 🚀
