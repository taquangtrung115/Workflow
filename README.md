```markdown
# Signature Workflow — Schema, Flow cấu hình & Bổ sung File

Phiên bản: cập nhật bổ sung flow cấu hình file types, quyền duyệt theo loại file, và các bước triển khai liên quan.

Mục lục
- Tóm tắt
- Flow cấu hình (chi tiết từng bước)
  - A. Thiết lập hệ thống (Admin)
  - B. Cấu hình FileType
  - C. Gán quyền duyệt loại file cho user (UserFileTypePermission)
  - D. Tạo Template & cấu hình Level (AllowedFileTypes)
  - E. Upload file, Start Workflow
  - F. Quy trình duyệt (Approve/Reject) — kiểm tra file-type
- API mẫu (payloads, ví dụ)
- UI Flow / giao diện gợi ý
- Kiểm thử & kịch bản mẫu
- Ghi chú vận hành / policy
- Snippets SQL / Migration / Seed nhanh

---

## Tóm tắt

Hệ thống cho phép bạn:
- Định nghĩa các loại file (FileType) mà hệ thống nhận diện (mime type + extension).
- Gán quyền cho từng user cho phép họ duyệt từng loại file cụ thể.
- Cấu hình từng level trong template để chỉ cho phép duyệt một số loại file (AllowedFileTypes).
- Tại thời điểm approve, hệ thống xác thực:
  - document mime/extension phù hợp với AllowedFileTypes của level,
  - user có quyền duyệt loại file đó (UserFileTypePermission) hoặc role/delegation phù hợp,
  - user nằm trong danh sách approver hoặc thuộc phòng ban được chỉ định.

---

## Flow cấu hình (chi tiết từng bước)

Dưới đây là luồng các bước cần thực hiện để hoàn thiện cấu hình trước khi bắt đầu chạy workflow.

A. Thiết lập hệ thống (Admin)
1. Tạo/đăng nhập tài khoản Admin.
2. Tạo permissions cơ bản (nếu chưa có):
   - `Workflow.Approve` — quyền duyệt.
   - `Template.Manage` — quản lý template.
   - (tùy chọn) `FileType.Manage`, `User.Manage`.
3. Tạo roles và gán permissions:
   - Role `Admin` -> `Template.Manage`, `Workflow.Approve`, `FileType.Manage`
   - Role `Approver` -> `Workflow.Approve`
4. Tạo Departments, Users và gán User -> Department, User -> Role.

B. Cấu hình FileType (Admin)
Mục đích: định nghĩa các loại file mà hệ thống công nhận và dùng để so khớp trong checks.

1. Tạo danh sách FileType:
   - Trường bắt buộc: Id, Name, Mime, ExtensionsJson (mảng extensions, ví dụ [" .pdf", ".docx" ])
   - Ví dụ:
     - PDF: Mime = application/pdf, ExtensionsJson = [" .pdf"]
     - Word: Mime = application/vnd.openxmlformats-officedocument.wordprocessingml.document, ExtensionsJson = [" .docx"]
2. Lưu vào bảng FileTypes.

C. Gán quyền duyệt loại file cho user (UserFileTypePermission)
1. Với mỗi user cần gán quyền, tạo bản ghi UserFileTypePermission:
   - Fields: Id, UserId, FileTypeId
2. Luồng admin:
   - Trang admin hiển thị ma trận Users x FileTypes, admin tick để grant/revoke quyền.
3. Policy:
   - Nếu user không có bản ghi UserFileTypePermission cho fileType tương ứng, họ không thể approve file đó (trừ khi có role global hoặc delegation cho phép).

D. Tạo Template & cấu hình Level
1. Tạo WorkflowTemplate (Name, Description, CreatedBy).
2. Thêm WorkflowLevel(s) với các trường:
   - Order (1..N)
   - ApproverType (Department | Users)
   - DepartmentId (nếu ApproverType == Department)
   - UserIdsJson (nếu ApproverType == Users)
   - RequiredApprovals (số approver cần)
   - AllowedFileTypesJson (mảng string; mỗi entry có thể là mime type "application/pdf" hoặc extension ".pdf")
3. Lưu template.

Chú ý:
- Nếu AllowedFileTypesJson để rỗng: định nghĩa chính sách rõ ràng (Recommend: deny-all by default hoặc allow-all nếu cần backward-compat).
- Thứ tự Order xác định luồng tuần tự; để hỗ trợ N cấp, thêm nhiều level với Order tăng dần.

E. Upload file & Start Workflow
1. Người dùng upload file qua endpoint /api/files/upload:
   - Backend nên lấy `IFormFile.ContentType` làm MimeType và lưu kèm Document.MimeType.
   - Upload thực tế file lên blob/S3, lưu BlobUrl.
2. Người dùng chọn template và gọi StartWorkflow:
   - POST /api/workflow/{templateId}/start?documentId={documentId}
   - Backend tạo WorkflowInstance:
     - TemplateId, DocumentId, CurrentLevelOrder = min(Order), Status = InProgress (hoặc Requested nếu có bước chờ gửi).
     - RequestedBy, RequestedAt.
3. Notification: thông báo cho approver ở level hiện tại (email/queue).

F. Quy trình duyệt (Approve / Reject) — kiểm tra file-type & quyền
1. Khi approver bấm Approve:
   - Server thực hiện authorization + validation:
     a) Lấy instance -> currentLevel (theo CurrentLevelOrder) -> check tồn tại.
     b) Lấy document -> docMime = Document.MimeType, docExt = extension của Document.Filename.
     c) Kiểm tra level.AllowedFileTypesJson:
        - Nếu level định nghĩa allowed list: ensure docMime or docExt trùng 1 trong list.
        - Nếu không trùng -> reject action (HTTP 400/403).
     d) Kiểm tra user có quyền duyệt file type:
        - Lấy FileType record matching docMime or docExt.
        - Kiểm tra UserFileTypePermission (userId, fileTypeId) tồn tại.
        - Nếu không tồn tại: check role-based permission (RolePermission mapping) hoặc delegation.
        - Nếu vẫn không: deny (403).
     e) Kiểm tra user có thuộc scope approver cho level:
        - If ApproverType == Users: userId in UserIdsJson.
        - If ApproverType == Department: user.DepartmentId == level.DepartmentId (hoặc theo policy: include child depts or check DepartmentRole).
     f) Nếu tất cả pass: tiếp tục tạo ApprovalRecord.
2. Tạo ApprovalRecord:
   - InstanceId, LevelOrder, ApproverUserId, Approved = true, Comment, SignatureBlob, SignedAt = now.
   - Lưu transactionally (sử dụng DB transaction).
3. Kiểm tra số Approved tại level:
   - Count ApprovalRecords where InstanceId & LevelOrder & Approved==true.
   - Nếu count >= RequiredApprovals:
     - Nếu next level exists: instance.CurrentLevelOrder = next.Order.
     - Else: instance.Status = Approved; instance.ClosedAt = now.
4. Reject flow:
   - Tạo ApprovalRecord with Approved = false, Comment.
   - Set instance.Status = Rejected; instance.ClosedAt = now.

---

## API mẫu (ví dụ payloads)

1) Tạo FileType
POST /api/filetypes
```json
{
  "name": "PDF",
  "mime": "application/pdf",
  "extensionsJson": [" .pdf"]
}
```

2) Grant user permission to fileType
POST /api/admin/grant-filetype
Body:
```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "fileTypeId": "00000000-0000-0000-0000-0000000000f1"
}
```

3) Create template (minimal)
POST /api/workflow/templates
```json
{
  "name": "Purchase Approval",
  "description": "Template for invoice approvals",
  "createdBy": "00000000-0000-0000-0000-000000000001",
  "levels": [
    {
      "order": 1,
      "approverType": "Department",
      "departmentId": "11111111-1111-1111-1111-111111111111",
      "allowedFileTypesJson": ["application/pdf", ".pdf"],
      "requiredApprovals": 1
    },
    {
      "order": 2,
      "approverType": "Users",
      "userIdsJson": ["22222222-2222-2222-2222-222222222222", "33333333-3333-3333-3333-333333333333"],
      "allowedFileTypesJson": ["application/pdf", ".pdf", ".docx"],
      "requiredApprovals": 2
    }
  ]
}
```

4) Upload file
POST /api/files/upload (form-data)
- file: invoice.pdf
Response:
```json
{
  "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "filename": "invoice.pdf",
  "mimeType": "application/pdf",
  "blobUrl": "https://..."
}
```

5) Start workflow
POST /api/workflow/{templateId}/start?documentId={documentId}

6) Approve
POST /api/workflow/{instanceId}/approve
Body:
```json
{
  "signatureBase64": "BASE64_SIG",
  "comment": "Looks good"
}
```

---

## UI Flow / giao diện gợi ý (bước theo bước)

1. Admin UI:
   - Menu: File Types -> Add/Remove/Edit file types.
   - Menu: User FileType Permissions -> ma trận user x filetype (checkbox).
   - Menu: Roles & Permissions -> quản lý role/permission.
   - Menu: Users -> gán role, gán department.

2. Template Builder:
   - Create Template -> Add Level (repeat)
     - For each Level: choose ApproverType (Department or Users),
     - Chọn department hoặc chọn users (searchable list),
     - RequiredApprovals (numeric),
     - Allowed File Types (multi-select from FileTypes list),
     - Save Level.
   - Save Template.

3. Requester (User) UI:
   - Upload file -> pick template -> Start workflow.

4. Approver UI:
   - Tabbed view: Yêu cầu / Chờ duyệt / Đã duyệt
   - Chờ duyệt: list instances assigned to user or user's department (server may filter on backend)
   - Click instance -> Preview document -> Approve/Reject modal
     - Modal shows: document metadata (filename, mime), current level allowed file types, who already signed, required approvals (progress), approve button which triggers client signing or sends signature (base64) to API.

---

## Kiểm thử & kịch bản mẫu

Kịch bản 1: Người dùng không có FileType permission -> UI ẩn nút Approve / API trả 403.
Kịch bản 2: File mime không match AllowedFileTypes của level -> API trả 400 (hoặc 403) khi approve.
Kịch bản 3: RequiredApprovals > 1 -> nhiều user approve đồng thời => hệ thống phải avoid race (DB transaction).
Kịch bản 4: Delegation active -> user delegatee có thể approve thay delegator.

Các test cases:
- Unit test ApprovalService.ApproveAsync: user allowed, not allowed, repeat approve, concurrent approves.
- Integration test upload -> start -> multi-approve -> completed.

---

## Ghi chú vận hành / policy

- Policy default AllowedFileTypesJson empty: quyết định rõ ràng (recommend deny-by-default).
- FileType registry: admin phải định nghĩa FileType trước khi gán quyền hoặc Sử dụng trong templates.
- Audit: ApprovalRecord lưu SignedAt, ApproverUserId, SignatureBlob; cần lưu thêm AuditLog nếu cần ghi log allow/deny decisions.
- Keys: private keys không nên lưu DB; use HSM/KeyVault or client-side keystore.

---

## Snippets SQL / Migration / Seed nhanh

Migration thêm FileTypes và UserFileTypePermissions:
```sql
CREATE TABLE FileTypes (
  Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
  Name NVARCHAR(200) NULL,
  Mime NVARCHAR(200) NULL,
  ExtensionsJson NVARCHAR(MAX) NULL
);

CREATE TABLE UserFileTypePermissions (
  Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
  UserId UNIQUEIDENTIFIER NOT NULL,
  FileTypeId UNIQUEIDENTIFIER NOT NULL,
  CONSTRAINT FK_UserFileTypePermissions_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
  CONSTRAINT FK_UserFileTypePermissions_FileTypes FOREIGN KEY (FileTypeId) REFERENCES FileTypes(Id)
);

ALTER TABLE Documents ADD MimeType NVARCHAR(200) NULL;
```

Seed ví dụ (tóm tắt):
- Insert permissions, roles, filetypes (PDF, Word), users (alice, bob), grant bob permission to PDF, create a sample template (levels allowing PDF).

---

## Kết luận & bước tiếp theo

Bạn có thể dán phần README này vào repo (file `README.md`) để làm tài liệu hướng dẫn triển khai và cấu hình.  
Nếu muốn, tôi sẽ:
- Tạo file migration EF Core đầy đủ (C#) và file seed code (nếu bạn muốn tôi xuất cả code migration và seed, tôi sẽ gửi tiếp).
- Hoặc tạo một patch/ZIP sẵn sàng để bạn apply trực tiếp vào repo.  

Bạn muốn tôi tiếp tục với: (1) tạo migration EF Core C# + seed method, (2) xuất patch .patch để bạn apply, hay (3) push trực tiếp vào branch `feat/filetype-support` (nếu bạn cấp quyền)? Tôi sẽ làm ngay theo lựa chọn của bạn.
```
