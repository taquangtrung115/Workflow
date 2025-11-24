# Hệ thống Workflow Duyệt Tài Liệu - Hoàn Chỉnh

## 🎯 Tổng Quan

Đây là một hệ thống workflow hoàn chỉnh cho phép quản lý quy trình duyệt tài liệu với nhiều cấp độ (multi-level approval), kiểm soát loại file, và quản lý quyền truy cập chi tiết.

### ✨ Tính Năng Chính

- ✅ **Multi-Level Approval**: Hỗ trợ nhiều cấp duyệt tuần tự
- ✅ **File Type Control**: Kiểm soát loại file được phép ở từng level
- ✅ **Permission Management**: Quản lý quyền duyệt file type cho từng user
- ✅ **Flexible Approvers**: Hỗ trợ duyệt theo department hoặc danh sách users
- ✅ **Audit Trail**: Ghi lại đầy đủ lịch sử approval với chữ ký số
- ✅ **Transaction Safety**: Đảm bảo tính toàn vẹn dữ liệu khi approve đồng thời
- ✅ **RESTful API**: API đầy đủ với Swagger documentation
- 🆕 **Highly Extensible**: Strategy Pattern và Validation Pipeline cho khả năng mở rộng cao
- 🆕 **Pluggable Architecture**: Dễ dàng thêm approver types và validation rules mới

---

## 📋 Yêu Cầu Hệ Thống

- **.NET 8.0 SDK** hoặc cao hơn
- **SQL Server** (LocalDB, Express, hoặc Full version)
- **Visual Studio 2022** hoặc **VS Code** (tùy chọn)

---

## 🚀 Quick Start

### Bước 1: Clone và Restore
```bash
cd /path/to/Workflow
cd src
dotnet restore
```

### Bước 2: Tạo Database
```bash
# Cách 1: Tự động tạo khi chạy app (khuyến nghị)
dotnet run

# Cách 2: Sử dụng EF migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Bước 3: Chạy Application
```bash
dotnet run
```

Application sẽ chạy tại:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger UI**: http://localhost:5000/swagger

---

## 📁 Cấu Trúc Project

```
Workflow/
├── README.md                    # Tài liệu gốc (tiếng Anh)
├── README_VN.md                # Tài liệu này (tiếng Việt)
├── API_DOCUMENTATION.md        # Chi tiết tất cả API endpoints
├── SETUP_GUIDE.md             # Hướng dẫn cài đặt chi tiết
├── DEPLOYMENT_GUIDE.md        # Hướng dẫn deploy production
├── USAGE_EXAMPLES.md          # Ví dụ sử dụng và test scenarios
├── SQL_SCRIPTS.sql            # SQL scripts đầy đủ
└── src/
    ├── Models/                # 7 database models
    │   ├── FileType.cs
    │   ├── UserFileTypePermission.cs
    │   ├── Document.cs
    │   ├── WorkflowTemplate.cs
    │   ├── WorkflowLevel.cs
    │   ├── WorkflowInstance.cs
    │   └── ApprovalRecord.cs
    ├── Data/                  # Database context
    │   ├── WorkflowDbContext.cs
    │   └── DbInitializer.cs
    ├── Services/              # Business logic (5 services)
    │   ├── FileTypeService.cs
    │   ├── PermissionService.cs
    │   ├── TemplateService.cs
    │   ├── DocumentService.cs
    │   └── WorkflowService.cs
    ├── Controllers/           # API endpoints (4 controllers)
    │   ├── FileTypesController.cs
    │   ├── AdminController.cs
    │   ├── WorkflowController.cs
    │   └── FilesController.cs
    ├── DTOs/                 # Data transfer objects
    └── Program.cs            # Entry point
```

---

## 🔄 Quy Trình Sử Dụng

### 1️⃣ Setup (Admin)

#### a) Tạo File Types
```bash
POST /api/filetypes
{
  "name": "PDF",
  "mime": "application/pdf",
  "extensions": [".pdf"]
}
```

#### b) Grant Quyền cho Users
```bash
POST /api/admin/grant-filetype
{
  "userId": "user-guid",
  "fileTypeId": "filetype-guid"
}
```

#### c) Tạo Workflow Template
```bash
POST /api/workflow/templates
{
  "name": "Purchase Approval",
  "description": "Quy trình duyệt hóa đơn",
  "createdBy": "admin-guid",
  "levels": [
    {
      "order": 1,
      "approverType": "Users",
      "userIds": ["user1-guid"],
      "requiredApprovals": 1,
      "allowedFileTypes": ["application/pdf", ".pdf"]
    },
    {
      "order": 2,
      "approverType": "Users",
      "userIds": ["user2-guid", "user3-guid"],
      "requiredApprovals": 2,
      "allowedFileTypes": ["application/pdf", ".pdf", ".docx"]
    }
  ]
}
```

### 2️⃣ Request (User)

#### a) Upload File
```bash
POST /api/files/upload?uploadedBy=user-guid
Content-Type: multipart/form-data

file: invoice.pdf
```

#### b) Start Workflow
```bash
POST /api/workflow/{templateId}/start?documentId={docId}&requestedBy={userId}
```

### 3️⃣ Approval (Approvers)

#### a) Xem Danh Sách Chờ Duyệt
```bash
GET /api/workflow/pending-approvals?userId={userId}
```

#### b) Approve
```bash
POST /api/workflow/{instanceId}/approve?approverId={userId}
{
  "comment": "Approved",
  "signatureBase64": "BASE64_SIGNATURE"
}
```

#### c) Reject
```bash
POST /api/workflow/{instanceId}/reject?approverId={userId}
{
  "comment": "Needs revision"
}
```

### 4️⃣ Tracking

#### Kiểm Tra Status
```bash
GET /api/workflow/instances/{instanceId}
```

---

## 🔐 Validation & Security

### 1. File Type Validation
- Document mime type hoặc extension **PHẢI** match với `allowedFileTypes` của level hiện tại
- Nếu `allowedFileTypes` rỗng: **deny by default**

### 2. Permission Validation
- User **PHẢI** có `UserFileTypePermission` cho file type của document
- FileType được xác định bằng mime type hoặc extension

### 3. Approver Scope Validation
- `approverType = "Users"`: userId phải có trong `userIds` của level
- `approverType = "Department"`: user phải thuộc department được chỉ định

### 4. Approval Count
- Số approvals tại mỗi level phải **>= requiredApprovals**
- User **KHÔNG** thể approve nhiều lần ở cùng 1 level

---

## 📊 Database Schema

### Tables
1. **FileTypes**: Định nghĩa các loại file
2. **UserFileTypePermissions**: Quyền duyệt file của user
3. **Documents**: Thông tin files đã upload
4. **WorkflowTemplates**: Templates định nghĩa quy trình
5. **WorkflowLevels**: Các level trong template
6. **WorkflowInstances**: Instances đang chạy
7. **ApprovalRecords**: Lịch sử approve/reject

### Relationships
```
WorkflowTemplate --< WorkflowLevel
WorkflowTemplate --< WorkflowInstance
WorkflowInstance --< ApprovalRecord
Document --< WorkflowInstance
FileType --< UserFileTypePermission
```

---

## 🧪 Testing

### Test với cURL
Xem file `USAGE_EXAMPLES.md` cho 4 test scenarios đầy đủ:
1. ✅ Simple single-level approval
2. ✅ Multi-level approval với 2 approvers
3. ❌ File type validation failure
4. ❌ Permission denied

### Test với Swagger
1. Mở http://localhost:5000/swagger
2. Expand các endpoints
3. Click "Try it out"
4. Fill parameters và execute

### Test với Postman
Import collection từ `USAGE_EXAMPLES.md`

---

## 📝 Sample Data

Khi chạy lần đầu, hệ thống tự động seed:

### File Types
- PDF (application/pdf)
- Word Document (.docx)
- Excel Spreadsheet (.xlsx)
- Image JPEG (.jpg, .jpeg)
- Image PNG (.png)

### Sample Template
- **Name**: Purchase Approval Workflow
- **Level 1**: 1 approval required, PDF only
- **Level 2**: 2 approvals required, PDF & DOCX

---

## 🛠️ Development

### Hot Reload
```bash
dotnet watch run
```

### Add Migration
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Build Release
```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

---

## 🚢 Deployment

Xem `DEPLOYMENT_GUIDE.md` cho hướng dẫn chi tiết deploy lên:
- ☁️ Azure App Service
- 🪟 IIS (Windows Server)
- 🐳 Docker
- 🌐 AWS EC2

---

## 📖 Documentation Links

| Tài liệu | Mô tả |
|----------|-------|
| [API_DOCUMENTATION.md](API_DOCUMENTATION.md) | Chi tiết tất cả API endpoints, request/response |
| [SETUP_GUIDE.md](SETUP_GUIDE.md) | Hướng dẫn cài đặt từng bước |
| [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) | Deploy lên production environments |
| [USAGE_EXAMPLES.md](USAGE_EXAMPLES.md) | Test scenarios với cURL examples |
| [SQL_SCRIPTS.sql](SQL_SCRIPTS.sql) | SQL scripts và useful queries |
| 🆕 [**EXTENSIBILITY_GUIDE.md**](EXTENSIBILITY_GUIDE.md) | **Hướng dẫn mở rộng hệ thống - Thêm strategies & validators** |
| 🆕 [**IMPROVEMENTS.md**](IMPROVEMENTS.md) | **Tóm tắt các cải tiến về khả năng mở rộng** |

---

## 🎓 Workflow Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    ADMIN SETUP PHASE                        │
├─────────────────────────────────────────────────────────────┤
│ 1. Create FileTypes (PDF, Word, Excel, etc.)               │
│ 2. Grant UserFileTypePermissions                            │
│ 3. Create WorkflowTemplate with Levels                      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   REQUEST PHASE (User)                      │
├─────────────────────────────────────────────────────────────┤
│ 1. Upload Document                                          │
│ 2. Select Template                                          │
│ 3. Start Workflow → Creates WorkflowInstance               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                 APPROVAL PHASE (Approvers)                  │
├─────────────────────────────────────────────────────────────┤
│ LEVEL 1:                                                    │
│   ├─ Validate: File type allowed?                          │
│   ├─ Validate: User has permission?                        │
│   ├─ Validate: User in approver list?                      │
│   └─ Approve → Create ApprovalRecord                       │
│                                                             │
│ Check: Approvals >= RequiredApprovals?                     │
│   ├─ YES → Move to Level 2                                 │
│   └─ NO  → Wait for more approvals                         │
│                                                             │
│ LEVEL 2:                                                    │
│   ├─ (Same validations)                                    │
│   ├─ Approve (User 1) → ApprovalRecord                     │
│   └─ Approve (User 2) → ApprovalRecord                     │
│                                                             │
│ Check: Approvals >= RequiredApprovals?                     │
│   ├─ YES & No more levels → Status = "Approved" ✅        │
│   └─ NO → Wait for more approvals                          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    COMPLETED PHASE                          │
├─────────────────────────────────────────────────────────────┤
│ Status: Approved / Rejected                                 │
│ ClosedAt: Timestamp                                         │
│ Full audit trail in ApprovalRecords                         │
└─────────────────────────────────────────────────────────────┘
```

---

## ❓ Troubleshooting

### Database Connection Error
```bash
# Kiểm tra SQL Server đang chạy
# Với LocalDB:
sqllocaldb start mssqllocaldb
```

### Port Already in Use
```bash
# Chạy trên port khác
dotnet run --urls "http://localhost:5002"
```

### Migration Error
```bash
# Xóa database và tạo lại
dotnet ef database drop -f
dotnet ef database update
```

---

## 🤝 Contributing

1. Fork repository
2. Create feature branch: `git checkout -b feature/AmazingFeature`
3. Commit changes: `git commit -m 'Add AmazingFeature'`
4. Push to branch: `git push origin feature/AmazingFeature`
5. Open Pull Request

---

## 📄 License

This project is provided as-is for educational and commercial use.

---

## 💡 Tips & Best Practices

### 1. Security
- ✅ Implement authentication (JWT/OAuth2) trước khi deploy production
- ✅ Validate all user inputs
- ✅ Use HTTPS trong production
- ✅ Không hardcode passwords trong code

### 2. Performance
- ✅ Enable database indexes (đã có trong DbContext)
- ✅ Use async/await consistently (đã implement)
- ✅ Implement caching cho FileTypes và Templates
- ✅ Add pagination cho list endpoints

### 3. Monitoring
- ✅ Setup logging (Serilog, Application Insights)
- ✅ Monitor database performance
- ✅ Track approval times
- ✅ Alert on failed approvals

### 4. Backup
- ✅ Regular database backups
- ✅ Archive old workflows (>1 year)
- ✅ Backup uploaded files

---

## 📞 Support

Nếu gặp vấn đề:
1. Check console logs
2. Check database với SQL Server Management Studio
3. Xem lại documentation
4. Create GitHub issue với error details

---

## 🎉 Features Coming Soon

- [ ] Email notifications cho approvers
- [ ] Workflow analytics dashboard
- [ ] Bulk approval
- [ ] Workflow delegation
- [ ] Mobile app support
- [ ] Signature pad integration
- [ ] Advanced reporting

---

**Chúc bạn sử dụng thành công!** 🚀

---

*Generated with ❤️ for the Vietnamese developer community*
