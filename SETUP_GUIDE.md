# Hướng dẫn Setup và Triển khai

## Prerequisites

### Yêu cầu hệ thống:
- .NET 8.0 SDK hoặc cao hơn
- SQL Server (LocalDB, Express, hoặc Full version)
- Visual Studio 2022 hoặc VS Code (tùy chọn)

### Kiểm tra .NET version:
```bash
dotnet --version
# Nên hiển thị: 8.0.x hoặc cao hơn
```

---

## Bước 1: Clone Repository

```bash
git clone <repository-url>
cd Workflow
```

---

## Bước 2: Cấu hình Database

### Option 1: Sử dụng SQL Server LocalDB (khuyến nghị cho development)

File `src/appsettings.json` đã có connection string mặc định:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WorkflowDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Option 2: Sử dụng SQL Server khác

Sửa connection string trong `src/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=WorkflowDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true"
  }
}
```

---

## Bước 3: Restore Dependencies

```bash
cd src
dotnet restore
```

---

## Bước 4: Tạo Database

### Cách 1: Sử dụng Entity Framework Migrations

```bash
# Tạo migration
dotnet ef migrations add InitialCreate

# Apply migration để tạo database
dotnet ef database update
```

### Cách 2: Database sẽ tự động tạo khi chạy app

Application đã được cấu hình để tự động tạo database và seed data khi khởi động lần đầu.

---

## Bước 5: Chạy Application

```bash
cd src
dotnet run
```

Application sẽ chạy tại:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`

---

## Bước 6: Kiểm tra API

### Mở Swagger UI:
Truy cập: `http://localhost:5000/swagger`

### Hoặc test với cURL:
```bash
# Test health check
curl http://localhost:5000/api/filetypes

# Kết quả sẽ hiển thị danh sách file types đã được seed
```

---

## Seed Data

Khi chạy lần đầu, database sẽ tự động được seed với:

### 1. FileTypes:
- PDF (`application/pdf`, `.pdf`)
- Word Document (`application/vnd.openxmlformats-officedocument.wordprocessingml.document`, `.docx`)
- Excel Spreadsheet (`.xlsx`)
- Image JPEG (`.jpg`, `.jpeg`)
- Image PNG (`.png`)

### 2. Sample Template:
- **Name**: Purchase Approval Workflow
- **Level 1**: Department approval (1 approval required, PDF only)
- **Level 2**: User approval (2 approvals required, PDF & DOCX)

---

## Development Workflow

### 1. Thay đổi Models
Nếu bạn thay đổi models trong `src/Models/`:

```bash
# Tạo migration mới
dotnet ef migrations add YourMigrationName

# Apply changes
dotnet ef database update
```

### 2. Hot Reload
Sử dụng watch mode để tự động reload khi code thay đổi:
```bash
dotnet watch run
```

### 3. Build Production
```bash
dotnet build -c Release
```

---

## Project Structure

```
Workflow/
├── README.md                    # Tài liệu chi tiết về workflow
├── API_DOCUMENTATION.md         # Tài liệu API
├── SETUP_GUIDE.md              # File này
├── DEPLOYMENT_GUIDE.md         # Hướng dẫn deploy
└── src/
    ├── Workflow.csproj         # Project file
    ├── Program.cs              # Entry point
    ├── appsettings.json        # Configuration
    ├── Models/                 # Database models
    │   ├── FileType.cs
    │   ├── UserFileTypePermission.cs
    │   ├── Document.cs
    │   ├── WorkflowTemplate.cs
    │   ├── WorkflowLevel.cs
    │   ├── WorkflowInstance.cs
    │   └── ApprovalRecord.cs
    ├── Data/                   # Database context
    │   ├── WorkflowDbContext.cs
    │   └── DbInitializer.cs
    ├── Services/               # Business logic
    │   ├── FileTypeService.cs
    │   ├── PermissionService.cs
    │   ├── TemplateService.cs
    │   ├── DocumentService.cs
    │   └── WorkflowService.cs
    ├── Controllers/            # API endpoints
    │   ├── FileTypesController.cs
    │   ├── AdminController.cs
    │   ├── WorkflowController.cs
    │   └── FilesController.cs
    └── DTOs/                   # Data transfer objects
        ├── FileTypeDto.cs
        ├── PermissionDto.cs
        ├── WorkflowDto.cs
        └── DocumentDto.cs
```

---

## Troubleshooting

### 1. Database Connection Error
**Lỗi**: Cannot connect to SQL Server

**Giải pháp**:
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string trong `appsettings.json`
- Với LocalDB: `sqllocaldb start mssqllocaldb`

### 2. Migration Error
**Lỗi**: Migrations failed

**Giải pháp**:
```bash
# Xóa database và tạo lại
dotnet ef database drop -f
dotnet ef database update
```

### 3. Port Already in Use
**Lỗi**: Port 5000 or 5001 already in use

**Giải pháp**:
```bash
# Chạy trên port khác
dotnet run --urls "http://localhost:5002"
```

### 4. File Upload Error
**Lỗi**: Cannot save uploaded files

**Giải pháp**:
- Kiểm tra folder `uploads/` tồn tại
- Kiểm tra quyền write: `chmod 755 uploads/`

---

## Next Steps

1. Đọc [API Documentation](API_DOCUMENTATION.md) để hiểu các endpoints
2. Đọc [README.md](README.md) để hiểu flow nghiệp vụ chi tiết
3. Test API với Swagger UI hoặc Postman
4. Tham khảo [Deployment Guide](DEPLOYMENT_GUIDE.md) để deploy lên production

---

## Support

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra logs trong console
2. Kiểm tra database với SQL Server Management Studio
3. Xem lại các bước trong guide này

---

## Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)
