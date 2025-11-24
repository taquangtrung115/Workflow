# Usage Examples và Test Scenarios

## Mục lục
1. [Setup ban đầu](#setup-ban-đầu)
2. [Scenario 1: Tạo workflow đơn giản](#scenario-1-tạo-workflow-đơn-giản)
3. [Scenario 2: Multi-level approval](#scenario-2-multi-level-approval)
4. [Scenario 3: File type validation](#scenario-3-file-type-validation)
5. [Scenario 4: Permission denied](#scenario-4-permission-denied)
6. [Postman Collection](#postman-collection)

---

## Setup ban đầu

### 1. Tạo sample users (giả sử)
```
Alice: 00000000-0000-0000-0000-000000000001 (Admin)
Bob:   11111111-1111-1111-1111-111111111111 (Approver Level 1)
Carol: 22222222-2222-2222-2222-222222222222 (Approver Level 2)
Dave:  33333333-3333-3333-3333-333333333333 (Approver Level 2)
Eve:   44444444-4444-4444-4444-444444444444 (Requester)
```

### 2. Grant permissions cho approvers
```bash
# Bob có quyền duyệt PDF
curl -X POST "http://localhost:5000/api/admin/grant-filetype" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "11111111-1111-1111-1111-111111111111",
    "fileTypeId": "00000000-0000-0000-0000-0000000000f1"
  }'

# Carol có quyền duyệt PDF và Word
curl -X POST "http://localhost:5000/api/admin/grant-filetype" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "22222222-2222-2222-2222-222222222222",
    "fileTypeId": "00000000-0000-0000-0000-0000000000f1"
  }'

curl -X POST "http://localhost:5000/api/admin/grant-filetype" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "22222222-2222-2222-2222-222222222222",
    "fileTypeId": "00000000-0000-0000-0000-0000000000f2"
  }'

# Dave có quyền duyệt PDF và Word
curl -X POST "http://localhost:5000/api/admin/grant-filetype" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "33333333-3333-3333-3333-333333333333",
    "fileTypeId": "00000000-0000-0000-0000-0000000000f1"
  }'

curl -X POST "http://localhost:5000/api/admin/grant-filetype" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "33333333-3333-3333-3333-333333333333",
    "fileTypeId": "00000000-0000-0000-0000-0000000000f2"
  }'
```

---

## Scenario 1: Tạo workflow đơn giản

### Bước 1: Tạo template với 1 level
```bash
curl -X POST "http://localhost:5000/api/workflow/templates" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Simple Approval",
    "description": "Chỉ cần 1 người duyệt",
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

**Response:**
```json
{
  "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "name": "Simple Approval",
  "description": "Chỉ cần 1 người duyệt",
  "createdBy": "00000000-0000-0000-0000-000000000001",
  "createdAt": "2024-01-01T10:00:00Z",
  "isActive": true,
  "levels": [
    {
      "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "order": 1,
      "approverType": "Users",
      "userIds": ["11111111-1111-1111-1111-111111111111"],
      "requiredApprovals": 1,
      "allowedFileTypes": ["application/pdf", ".pdf"]
    }
  ]
}
```

### Bước 2: Upload file PDF
```bash
curl -X POST "http://localhost:5000/api/files/upload?uploadedBy=44444444-4444-4444-4444-444444444444" \
  -F "file=@invoice.pdf"
```

**Response:**
```json
{
  "id": "dddddddd-dddd-dddd-dddd-dddddddddddd",
  "filename": "invoice.pdf",
  "mimeType": "application/pdf",
  "blobUrl": "/uploads/guid.pdf",
  "fileSize": 102400,
  "uploadedBy": "44444444-4444-4444-4444-444444444444",
  "uploadedAt": "2024-01-01T10:05:00Z"
}
```

### Bước 3: Start workflow
```bash
curl -X POST "http://localhost:5000/api/workflow/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/start?documentId=dddddddd-dddd-dddd-dddd-dddddddddddd&requestedBy=44444444-4444-4444-4444-444444444444"
```

**Response:**
```json
{
  "id": "iiiiiiii-iiii-iiii-iiii-iiiiiiiiiiii",
  "templateId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "templateName": "Simple Approval",
  "documentId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
  "documentFilename": "invoice.pdf",
  "currentLevelOrder": 1,
  "status": "InProgress",
  "requestedBy": "44444444-4444-4444-4444-444444444444",
  "requestedAt": "2024-01-01T10:10:00Z",
  "closedAt": null,
  "approvalRecords": []
}
```

### Bước 4: Bob approve
```bash
curl -X POST "http://localhost:5000/api/workflow/iiiiiiii-iiii-iiii-iiii-iiiiiiiiiiii/approve?approverId=11111111-1111-1111-1111-111111111111" \
  -H "Content-Type: application/json" \
  -d '{
    "comment": "Approved",
    "signatureBase64": "BASE64_SIGNATURE_DATA"
  }'
```

**Response:**
```json
{
  "id": "iiiiiiii-iiii-iiii-iiii-iiiiiiiiiiii",
  "templateId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "templateName": "Simple Approval",
  "documentId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
  "documentFilename": "invoice.pdf",
  "currentLevelOrder": 1,
  "status": "Approved",
  "requestedBy": "44444444-4444-4444-4444-444444444444",
  "requestedAt": "2024-01-01T10:10:00Z",
  "closedAt": "2024-01-01T10:15:00Z",
  "approvalRecords": [
    {
      "id": "rrrrrrrr-rrrr-rrrr-rrrr-rrrrrrrrrrrr",
      "levelOrder": 1,
      "approverUserId": "11111111-1111-1111-1111-111111111111",
      "approved": true,
      "comment": "Approved",
      "signedAt": "2024-01-01T10:15:00Z"
    }
  ]
}
```

✅ **Workflow completed!** Status = "Approved"

---

## Scenario 2: Multi-level approval

### Bước 1: Tạo template 2 levels
```bash
curl -X POST "http://localhost:5000/api/workflow/templates" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Two-Level Approval",
    "description": "Cần 1 approval ở level 1, và 2 approvals ở level 2",
    "createdBy": "00000000-0000-0000-0000-000000000001",
    "levels": [
      {
        "order": 1,
        "approverType": "Users",
        "userIds": ["11111111-1111-1111-1111-111111111111"],
        "requiredApprovals": 1,
        "allowedFileTypes": ["application/pdf", ".pdf"]
      },
      {
        "order": 2,
        "approverType": "Users",
        "userIds": [
          "22222222-2222-2222-2222-222222222222",
          "33333333-3333-3333-3333-333333333333"
        ],
        "requiredApprovals": 2,
        "allowedFileTypes": ["application/pdf", ".pdf", ".docx"]
      }
    ]
  }'
```

### Bước 2-3: Upload file và Start workflow (tương tự Scenario 1)
Template ID: `tttttttt-tttt-tttt-tttt-tttttttttttt`
Instance ID: `mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm`

### Bước 4: Bob approve level 1
```bash
curl -X POST "http://localhost:5000/api/workflow/mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm/approve?approverId=11111111-1111-1111-1111-111111111111" \
  -H "Content-Type: application/json" \
  -d '{"comment": "Level 1 approved"}'
```

**Response:**
```json
{
  "id": "mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm",
  "currentLevelOrder": 2,
  "status": "InProgress",
  ...
}
```

⚠️ Status vẫn là "InProgress", CurrentLevelOrder = 2

### Bước 5: Carol approve level 2 (approval 1/2)
```bash
curl -X POST "http://localhost:5000/api/workflow/mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm/approve?approverId=22222222-2222-2222-2222-222222222222" \
  -H "Content-Type: application/json" \
  -d '{"comment": "Level 2 approval 1"}'
```

**Response:**
```json
{
  "id": "mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm",
  "currentLevelOrder": 2,
  "status": "InProgress",
  "approvalRecords": [
    {"levelOrder": 1, "approverUserId": "11111111...", "approved": true},
    {"levelOrder": 2, "approverUserId": "22222222...", "approved": true}
  ]
}
```

⚠️ Status vẫn "InProgress" vì cần 2 approvals ở level 2

### Bước 6: Dave approve level 2 (approval 2/2)
```bash
curl -X POST "http://localhost:5000/api/workflow/mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm/approve?approverId=33333333-3333-3333-3333-333333333333" \
  -H "Content-Type: application/json" \
  -d '{"comment": "Level 2 approval 2"}'
```

**Response:**
```json
{
  "id": "mmmmmmmm-mmmm-mmmm-mmmm-mmmmmmmmmmmm",
  "currentLevelOrder": 2,
  "status": "Approved",
  "closedAt": "2024-01-01T11:00:00Z",
  "approvalRecords": [
    {"levelOrder": 1, "approverUserId": "11111111...", "approved": true},
    {"levelOrder": 2, "approverUserId": "22222222...", "approved": true},
    {"levelOrder": 2, "approverUserId": "33333333...", "approved": true}
  ]
}
```

✅ **Workflow completed!** Status = "Approved"

---

## Scenario 3: File type validation

### Test Case: Upload Word file nhưng level 1 chỉ cho phép PDF

### Bước 1: Upload Word file
```bash
curl -X POST "http://localhost:5000/api/files/upload?uploadedBy=44444444-4444-4444-4444-444444444444" \
  -F "file=@document.docx"
```

**Response:**
```json
{
  "id": "wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww",
  "filename": "document.docx",
  "mimeType": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
  ...
}
```

### Bước 2: Start workflow (template chỉ cho PDF ở level 1)
```bash
curl -X POST "http://localhost:5000/api/workflow/tttttttt-tttt-tttt-tttt-tttttttttttt/start?documentId=wwwwwwww-wwww-wwww-wwww-wwwwwwwwwwww&requestedBy=44444444-4444-4444-4444-444444444444"
```

**Response:**
```json
{
  "id": "nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn",
  "status": "InProgress",
  "currentLevelOrder": 1,
  ...
}
```

### Bước 3: Bob cố approve (sẽ bị reject)
```bash
curl -X POST "http://localhost:5000/api/workflow/nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn/approve?approverId=11111111-1111-1111-1111-111111111111" \
  -H "Content-Type: application/json" \
  -d '{"comment": "Approved"}'
```

**Error Response (403 Forbidden):**
```json
{
  "error": "File type 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' hoặc extension '.docx' không được phép ở level này"
}
```

❌ **Approve failed!** File type không match với AllowedFileTypes

---

## Scenario 4: Permission denied

### Test Case: User không có permission cho file type

### Bước 1: Upload PDF file
```bash
curl -X POST "http://localhost:5000/api/files/upload?uploadedBy=44444444-4444-4444-4444-444444444444" \
  -F "file=@invoice.pdf"
```

### Bước 2: Start workflow
```bash
curl -X POST "http://localhost:5000/api/workflow/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/start?documentId=DOCUMENT_ID&requestedBy=44444444-4444-4444-4444-444444444444"
```

### Bước 3: Eve (không có permission) cố approve
```bash
curl -X POST "http://localhost:5000/api/workflow/INSTANCE_ID/approve?approverId=44444444-4444-4444-4444-444444444444" \
  -H "Content-Type: application/json" \
  -d '{"comment": "Approved"}'
```

**Error Response (403 Forbidden):**
```json
{
  "error": "User không có quyền duyệt file type 'PDF'"
}
```

❌ **Approve failed!** User không có UserFileTypePermission

---

## Postman Collection

### Import vào Postman:

```json
{
  "info": {
    "name": "Workflow API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "1. Admin - Grant Permission",
      "request": {
        "method": "POST",
        "header": [{"key": "Content-Type", "value": "application/json"}],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"userId\": \"11111111-1111-1111-1111-111111111111\",\n  \"fileTypeId\": \"00000000-0000-0000-0000-0000000000f1\"\n}"
        },
        "url": {
          "raw": "http://localhost:5000/api/admin/grant-filetype",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "admin", "grant-filetype"]
        }
      }
    },
    {
      "name": "2. Create Template",
      "request": {
        "method": "POST",
        "header": [{"key": "Content-Type", "value": "application/json"}],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"name\": \"Simple Approval\",\n  \"description\": \"Test template\",\n  \"createdBy\": \"00000000-0000-0000-0000-000000000001\",\n  \"levels\": [\n    {\n      \"order\": 1,\n      \"approverType\": \"Users\",\n      \"userIds\": [\"11111111-1111-1111-1111-111111111111\"],\n      \"requiredApprovals\": 1,\n      \"allowedFileTypes\": [\"application/pdf\", \".pdf\"]\n    }\n  ]\n}"
        },
        "url": {
          "raw": "http://localhost:5000/api/workflow/templates",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "workflow", "templates"]
        }
      }
    },
    {
      "name": "3. Upload File",
      "request": {
        "method": "POST",
        "body": {
          "mode": "formdata",
          "formdata": [
            {
              "key": "file",
              "type": "file",
              "src": "/path/to/file.pdf"
            }
          ]
        },
        "url": {
          "raw": "http://localhost:5000/api/files/upload?uploadedBy=44444444-4444-4444-4444-444444444444",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "files", "upload"],
          "query": [
            {"key": "uploadedBy", "value": "44444444-4444-4444-4444-444444444444"}
          ]
        }
      }
    },
    {
      "name": "4. Start Workflow",
      "request": {
        "method": "POST",
        "url": {
          "raw": "http://localhost:5000/api/workflow/{{templateId}}/start?documentId={{documentId}}&requestedBy={{userId}}",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "workflow", "{{templateId}}", "start"],
          "query": [
            {"key": "documentId", "value": "{{documentId}}"},
            {"key": "requestedBy", "value": "{{userId}}"}
          ]
        }
      }
    },
    {
      "name": "5. Approve",
      "request": {
        "method": "POST",
        "header": [{"key": "Content-Type", "value": "application/json"}],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"comment\": \"Approved\",\n  \"signatureBase64\": \"BASE64_DATA\"\n}"
        },
        "url": {
          "raw": "http://localhost:5000/api/workflow/{{instanceId}}/approve?approverId={{approverId}}",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "workflow", "{{instanceId}}", "approve"],
          "query": [
            {"key": "approverId", "value": "{{approverId}}"}
          ]
        }
      }
    },
    {
      "name": "6. Get Pending Approvals",
      "request": {
        "method": "GET",
        "url": {
          "raw": "http://localhost:5000/api/workflow/pending-approvals?userId={{userId}}",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "workflow", "pending-approvals"],
          "query": [
            {"key": "userId", "value": "{{userId}}"}
          ]
        }
      }
    }
  ]
}
```

---

## Testing Checklist

### Functional Tests
- [x] Tạo template thành công
- [x] Upload file thành công
- [x] Start workflow thành công
- [x] Approve level 1 -> chuyển sang level 2
- [x] Approve level 2 (2 approvals) -> complete
- [x] Reject workflow -> status = Rejected
- [x] File type validation works
- [x] User permission validation works
- [x] Get pending approvals returns correct list
- [x] Duplicate approval denied (user không approve 2 lần)

### Edge Cases
- [x] Template không có levels -> error
- [x] Document không tồn tại -> error
- [x] Instance đã completed -> không approve được
- [x] AllowedFileTypes rỗng -> deny all
- [x] User không trong approver list -> denied
- [x] Concurrent approvals -> transaction safe

### Performance Tests
- [ ] Upload large files (>10MB)
- [ ] Multiple concurrent approvals
- [ ] Query performance với 1000+ instances

---

## Debugging Tips

### Check workflow status
```bash
curl http://localhost:5000/api/workflow/instances/INSTANCE_ID
```

### Check user permissions
```bash
curl http://localhost:5000/api/admin/user-permissions/USER_ID
```

### Check all file types
```bash
curl http://localhost:5000/api/filetypes
```

### Check pending approvals
```bash
curl "http://localhost:5000/api/workflow/pending-approvals?userId=USER_ID"
```

---

Happy testing! 🚀
