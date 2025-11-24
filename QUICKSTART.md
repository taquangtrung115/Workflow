# Quick Start Guide - 5 phút để chạy hệ thống

## 🎯 Mục tiêu
Trong 5 phút, bạn sẽ:
1. ✅ Cài đặt và chạy API
2. ✅ Tạo workflow đầu tiên
3. ✅ Approve một document

---

## ⚡ Bước 1: Setup (2 phút)

### Kiểm tra Prerequisites
```bash
# Kiểm tra .NET đã cài chưa
dotnet --version
# Kết quả: 8.0.x hoặc cao hơn

# Nếu chưa có, download tại: https://dotnet.microsoft.com/download
```

### Clone và Restore
```bash
cd /home/runner/work/Workflow/Workflow
cd src
dotnet restore
```

### Chạy App
```bash
dotnet run
```

✅ **Success!** Nếu thấy:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

---

## ⚡ Bước 2: Test API (1 phút)

### Mở Swagger UI
1. Mở browser
2. Truy cập: **http://localhost:5000/swagger**

### Kiểm tra FileTypes đã được seed
```bash
curl http://localhost:5000/api/filetypes
```

Kết quả sẽ show 5 file types (PDF, Word, Excel, JPEG, PNG)

---

## ⚡ Bước 3: Tạo Workflow Đầu Tiên (2 phút)

### 3.1. Grant Permission cho User (Bob)
```bash
curl -X POST "http://localhost:5000/api/admin/grant-filetype" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "11111111-1111-1111-1111-111111111111",
    "fileTypeId": "00000000-0000-0000-0000-0000000000f1"
  }'
```

✅ Bob giờ có quyền duyệt PDF

### 3.2. Tạo Simple Template
```bash
curl -X POST "http://localhost:5000/api/workflow/templates" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My First Workflow",
    "description": "Simple one-level approval",
    "createdBy": "00000000-0000-0000-0000-000000000001",
    "levels": [
      {
        "order": 1,
        "approverType": "Users",
        "userIds": ["11111111-1111-1111-1111-111111111111"],
        "requiredApprovals": 1,
        "allowedFileTypes": ["application/pdf", ".pdf"]
      }
    ]
  }'
```

**Copy templateId từ response!** Ví dụ: `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa`

### 3.3. Upload File (tạo file test)
```bash
# Tạo file PDF test
echo "Test PDF content" > test.pdf

# Upload
curl -X POST "http://localhost:5000/api/files/upload?uploadedBy=44444444-4444-4444-4444-444444444444" \
  -F "file=@test.pdf"
```

**Copy documentId từ response!** Ví dụ: `dddddddd-dddd-dddd-dddd-dddddddddddd`

### 3.4. Start Workflow
```bash
# Thay YOUR_TEMPLATE_ID và YOUR_DOCUMENT_ID
curl -X POST "http://localhost:5000/api/workflow/YOUR_TEMPLATE_ID/start?documentId=YOUR_DOCUMENT_ID&requestedBy=44444444-4444-4444-4444-444444444444"
```

**Copy instanceId từ response!** Ví dụ: `iiiiiiii-iiii-iiii-iiii-iiiiiiiiiiii`

✅ **Workflow started!** Status = "InProgress"

### 3.5. Approve
```bash
# Thay YOUR_INSTANCE_ID
curl -X POST "http://localhost:5000/api/workflow/YOUR_INSTANCE_ID/approve?approverId=11111111-1111-1111-1111-111111111111" \
  -H "Content-Type: application/json" \
  -d '{
    "comment": "Looks good!",
    "signatureBase64": "MySignature123"
  }'
```

✅ **Workflow completed!** Status = "Approved"

---

## 🎉 Xong rồi!

Bạn vừa:
1. ✅ Chạy thành công API
2. ✅ Tạo template với 1 level
3. ✅ Upload file
4. ✅ Start workflow
5. ✅ Approve và complete workflow

---

## 🚀 Next Steps

### Học thêm về hệ thống:
- **Chi tiết API**: Xem [API_DOCUMENTATION.md](API_DOCUMENTATION.md)
- **Test scenarios**: Xem [USAGE_EXAMPLES.md](USAGE_EXAMPLES.md)
- **Deploy production**: Xem [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)

### Thử nghiệm nâng cao:
1. Tạo template với 2 levels
2. Test file type validation (upload .docx nhưng level chỉ cho PDF)
3. Test permission denied (user không có quyền)
4. Multiple approvers cùng level

### Sử dụng Swagger UI:
1. Mở http://localhost:5000/swagger
2. Expand các endpoints
3. Click "Try it out"
4. Thực hiện các bước trên qua UI

---

## 📋 Quick Reference

### Sample GUIDs (để test)
```
Admin:    00000000-0000-0000-0000-000000000001
Bob:      11111111-1111-1111-1111-111111111111
Carol:    22222222-2222-2222-2222-222222222222
Dave:     33333333-3333-3333-3333-333333333333
Eve:      44444444-4444-4444-4444-444444444444

PDF Type: 00000000-0000-0000-0000-0000000000f1
Word Type: 00000000-0000-0000-0000-0000000000f2
```

### Common Commands
```bash
# Chạy app
cd src && dotnet run

# Chạy với hot reload
cd src && dotnet watch run

# Kiểm tra file types
curl http://localhost:5000/api/filetypes

# Kiểm tra templates
curl http://localhost:5000/api/workflow/templates

# Xem pending approvals
curl "http://localhost:5000/api/workflow/pending-approvals?userId=11111111-1111-1111-1111-111111111111"
```

---

## ❗ Troubleshooting

### Port 5000 đang được dùng?
```bash
dotnet run --urls "http://localhost:5002"
```

### Database error?
```bash
# Reset database
cd src
dotnet ef database drop -f
dotnet run
```

### API không response?
- Check console logs
- Kiểm tra SQL Server đang chạy: `sqllocaldb info mssqllocaldb`

---

## 🎓 Hiểu Workflow Flow

```
1. Admin Setup:
   └─ Create FileTypes
   └─ Grant Permissions
   └─ Create Template

2. User Request:
   └─ Upload File
   └─ Start Workflow

3. Approval:
   └─ Level 1: Approve
   └─ Level 2: Approve (if exists)
   └─ Complete!

Status: InProgress → Approved ✅
```

---

## 💡 Pro Tips

1. **Sử dụng Swagger**: Dễ hơn curl để test
2. **Check logs**: Console sẽ show tất cả SQL queries
3. **Database viewer**: Dùng Azure Data Studio hoặc SSMS để xem data
4. **Postman**: Import collection từ USAGE_EXAMPLES.md

---

**Happy Coding!** 🎉

*Nếu gặp vấn đề, xem [SETUP_GUIDE.md](SETUP_GUIDE.md) để biết chi tiết*
