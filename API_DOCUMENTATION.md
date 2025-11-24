# Workflow API Documentation

## Tổng quan

API này cung cấp các endpoints để quản lý hệ thống Signature Workflow - một hệ thống duyệt tài liệu với nhiều cấp độ và kiểm soát loại file.

Base URL: `http://localhost:5000/api`

## Authentication

Hiện tại API chưa có authentication. Trong production cần implement JWT hoặc OAuth2.

---

## FileTypes API

### 1. Lấy tất cả file types
```http
GET /api/filetypes
```

**Response:**
```json
[
  {
    "id": "00000000-0000-0000-0000-0000000000f1",
    "name": "PDF",
    "mime": "application/pdf",
    "extensions": [".pdf"],
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

### 2. Lấy file type theo ID
```http
GET /api/filetypes/{id}
```

### 3. Tạo file type mới
```http
POST /api/filetypes
Content-Type: application/json

{
  "name": "PDF",
  "mime": "application/pdf",
  "extensions": [".pdf"]
}
```

### 4. Xóa file type
```http
DELETE /api/filetypes/{id}
```

---

## Admin API (Permission Management)

### 1. Grant quyền duyệt file type cho user
```http
POST /api/admin/grant-filetype
Content-Type: application/json

{
  "userId": "00000000-0000-0000-0000-000000000001",
  "fileTypeId": "00000000-0000-0000-0000-0000000000f1"
}
```

**Response:**
```json
{
  "id": "guid",
  "userId": "00000000-0000-0000-0000-000000000001",
  "fileTypeId": "00000000-0000-0000-0000-0000000000f1",
  "fileTypeName": "PDF",
  "grantedAt": "2024-01-01T00:00:00Z"
}
```

### 2. Revoke quyền
```http
DELETE /api/admin/revoke-filetype?userId={guid}&fileTypeId={guid}
```

### 3. Lấy tất cả permissions của user
```http
GET /api/admin/user-permissions/{userId}
```

---

## Files API

### 1. Upload file
```http
POST /api/files/upload?uploadedBy={userId}
Content-Type: multipart/form-data

file: [binary data]
```

**Response:**
```json
{
  "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "filename": "invoice.pdf",
  "mimeType": "application/pdf",
  "blobUrl": "/uploads/guid.pdf",
  "fileSize": 102400,
  "uploadedBy": "00000000-0000-0000-0000-000000000001",
  "uploadedAt": "2024-01-01T00:00:00Z"
}
```

### 2. Lấy document theo ID
```http
GET /api/files/{id}
```

---

## Workflow Templates API

### 1. Lấy tất cả templates active
```http
GET /api/workflow/templates
```

### 2. Lấy template theo ID
```http
GET /api/workflow/templates/{id}
```

### 3. Tạo template mới
```http
POST /api/workflow/templates
Content-Type: application/json

{
  "name": "Purchase Approval",
  "description": "Template cho quy trình duyệt hóa đơn mua hàng",
  "createdBy": "00000000-0000-0000-0000-000000000001",
  "levels": [
    {
      "order": 1,
      "approverType": "Department",
      "departmentId": "11111111-1111-1111-1111-111111111111",
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
}
```

**Response:**
```json
{
  "id": "template-guid",
  "name": "Purchase Approval",
  "description": "Template cho quy trình duyệt hóa đơn mua hàng",
  "createdBy": "00000000-0000-0000-0000-000000000001",
  "createdAt": "2024-01-01T00:00:00Z",
  "isActive": true,
  "levels": [...]
}
```

### 4. Deactivate template
```http
DELETE /api/workflow/templates/{id}
```

---

## Workflow Instances API

### 1. Start workflow
```http
POST /api/workflow/{templateId}/start?documentId={documentId}&requestedBy={userId}
```

**Response:**
```json
{
  "id": "instance-guid",
  "templateId": "template-guid",
  "templateName": "Purchase Approval",
  "documentId": "document-guid",
  "documentFilename": "invoice.pdf",
  "currentLevelOrder": 1,
  "status": "InProgress",
  "requestedBy": "user-guid",
  "requestedAt": "2024-01-01T00:00:00Z",
  "closedAt": null,
  "approvalRecords": []
}
```

### 2. Lấy workflow instance
```http
GET /api/workflow/instances/{instanceId}
```

### 3. Lấy workflows chờ duyệt của user
```http
GET /api/workflow/pending-approvals?userId={userId}
```

### 4. Approve workflow
```http
POST /api/workflow/{instanceId}/approve?approverId={userId}
Content-Type: application/json

{
  "signatureBase64": "BASE64_SIGNATURE_DATA",
  "comment": "Looks good, approved"
}
```

**Success Response (200):**
```json
{
  "id": "instance-guid",
  "status": "InProgress",
  "currentLevelOrder": 2,
  "approvalRecords": [
    {
      "id": "record-guid",
      "levelOrder": 1,
      "approverUserId": "user-guid",
      "approved": true,
      "comment": "Looks good, approved",
      "signedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**Error Responses:**
- 400 Bad Request: Workflow không ở trạng thái InProgress
- 403 Forbidden: User không có quyền approve (file type không match, không có permission, hoặc không trong scope)
- 404 Not Found: Instance không tồn tại

### 5. Reject workflow
```http
POST /api/workflow/{instanceId}/reject?approverId={userId}
Content-Type: application/json

{
  "comment": "Needs revision"
}
```

---

## Error Handling

Tất cả các lỗi đều trả về format:
```json
{
  "error": "Mô tả lỗi chi tiết"
}
```

HTTP Status Codes:
- 200: Success
- 201: Created
- 204: No Content (for deletes)
- 400: Bad Request
- 403: Forbidden
- 404: Not Found
- 500: Internal Server Error

---

## Workflow Flow

1. **Setup (Admin)**:
   - Tạo FileTypes: `POST /api/filetypes`
   - Grant permissions cho users: `POST /api/admin/grant-filetype`
   - Tạo Template với levels: `POST /api/workflow/templates`

2. **Request (User)**:
   - Upload file: `POST /api/files/upload`
   - Start workflow: `POST /api/workflow/{templateId}/start`

3. **Approval (Approvers)**:
   - Xem pending approvals: `GET /api/workflow/pending-approvals?userId={id}`
   - Approve: `POST /api/workflow/{instanceId}/approve`
   - hoặc Reject: `POST /api/workflow/{instanceId}/reject`

4. **Tracking**:
   - Check status: `GET /api/workflow/instances/{instanceId}`

---

## Validation Rules

### File Type Validation
- Document mime type hoặc extension phải match với `allowedFileTypes` của level hiện tại
- Nếu `allowedFileTypes` rỗng hoặc null: deny by default

### Permission Validation
- User phải có `UserFileTypePermission` cho file type của document
- FileType được xác định bằng mime type hoặc extension của document

### Approver Scope Validation
- Nếu `approverType == "Users"`: userId phải có trong `userIds` của level
- Nếu `approverType == "Department"`: user phải thuộc department được chỉ định

### Approval Count
- Số approvals tại mỗi level phải >= `requiredApprovals`
- User không thể approve nhiều lần ở cùng 1 level

---

## Migration & Database

### Tạo migration
```bash
cd src
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Seed Data
Database sẽ tự động seed khi chạy lần đầu với:
- 5 FileTypes cơ bản (PDF, Word, Excel, JPEG, PNG)
- 1 Template mẫu "Purchase Approval Workflow"

---

## Testing với cURL

```bash
# 1. Upload file
curl -X POST "http://localhost:5000/api/files/upload?uploadedBy=00000000-0000-0000-0000-000000000001" \
  -F "file=@invoice.pdf"

# 2. Start workflow
curl -X POST "http://localhost:5000/api/workflow/11111111-1111-1111-1111-111111111111/start?documentId=DOCUMENT_ID&requestedBy=USER_ID"

# 3. Approve
curl -X POST "http://localhost:5000/api/workflow/INSTANCE_ID/approve?approverId=USER_ID" \
  -H "Content-Type: application/json" \
  -d '{"comment": "Approved", "signatureBase64": "SIGNATURE"}'
```
