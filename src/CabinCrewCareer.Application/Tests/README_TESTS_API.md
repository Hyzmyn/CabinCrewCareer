# Tests API Documentation

## Tổng quan
Module Tests API quản lý các bài kiểm tra cho ứng viên tiếp viên hàng không, bao gồm các bài thi Tiếng Anh và Thực hành.

---

## API Endpoints

### 1. **POST /api/v1/tests** - Tạo bài kiểm tra mới

#### 📋 Mô tả
Examiner tạo một bài kiểm tra mới với các thông tin cần thiết. Hệ thống sẽ tự động generate mã JoinCode ngẫu nhiên 10 ký tự.

#### 🔐 Phân quyền
- **Yêu cầu**: Đăng nhập (Bearer Token)
- **Role**: Chỉ **Examiner** mới được phép tạo test
- **HTTP Status nếu không có quyền**: 403 Forbidden

#### 📥 Request Body

```json
{
  "testName": "English Listening Test - Level A",
  "purpose": "Test listening comprehension for cabin crew candidates",
  "testType": 1,
  "maxScore": 100,
  "audioFileURL": "https://example.com/audio/test-listening-a.mp3"
}
```

#### 📝 Request Parameters

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|-------|
| `testName` | string | ✅ | Tên bài kiểm tra (tối đa 200 ký tự) |
| `purpose` | string | ✅ | Mục đích của bài kiểm tra (tối đa 500 ký tự) |
| `testType` | int | ✅ | Loại bài kiểm tra: 1=English, 2=Practical |
| `maxScore` | int | ✅ | Điểm tối đa (1-1000) |
| `audioFileURL` | string | ⚠️ | Link file audio (BẮT BUỘC nếu testType=1) |

#### ⚙️ Business Logic

**1. Validation AudioFileURL theo TestType:**
- Nếu `testType = 1` (English) → `audioFileURL` là **BẮT BUỘC**
- Nếu `testType = 2` (Practical) → `audioFileURL` có thể **null** hoặc rỗng
- Nếu có `audioFileURL` → phải là URL hợp lệ (http/https)

**2. Generate JoinCode:**
- Tự động tạo mã ngẫu nhiên **10 ký tự** 
- Chỉ gồm chữ in hoa và số: `ABCDEFGHJKLMNPQRSTUVWXYZ23456789`
- Loại bỏ các ký tự dễ nhầm lẫn: I, O, 0, 1, L
- Kiểm tra trùng lặp trong database
- Nếu trùng → tự động generate lại (tối đa 10 lần)

**3. Authorization Check:**
- Lấy thông tin user từ JWT token
- Kiểm tra `RoleId == 5` (Examiner)
- Nếu không phải Examiner → trả về error code `UNAUTHORIZED_EXAMINER_ONLY`

#### 📤 Response (Success - 200 OK)

```json
{
  "data": {
    "testId": 1,
    "testName": "English Listening Test - Level A",
    "purpose": "Test listening comprehension for cabin crew candidates",
    "testType": "English",
    "maxScore": 100,
    "audioFileURL": "https://example.com/audio/test-listening-a.mp3",
    "joinCode": "K7HM9P3QXY",
    "createdAt": "10/11/2025 10:30:00",
    "createdBy": "Nguyễn Văn A"
  },
  "code": 2,
  "message": "Created successfuly"
}
```

#### ❌ Error Responses

**Validation Error (400 Bad Request):**
```json
{
  "errorCode": 3,
  "errorMessage": "Created unsuccessfuly",
  "errors": [
    "Test name is required",
    "Audio file URL is required for English test type"
  ]
}
```

**Unauthorized Examiner (400 Bad Request):**
```json
{
  "errorCode": 17,
  "error": "UNAUTHORIZED_EXAMINER_ONLY",
  "errorMessage": "Unauthorized. Only Examiner can perform this action"
}
```

**User Not Found (400 Bad Request):**
```json
{
  "errorCode": 14,
  "error": "USER_NOT_FOUND",
  "errorMessage": "User not found"
}
```

**Authentication Failed (400 Bad Request):**
```json
{
  "errorCode": 10,
  "error": "FAILED_AUTHENTICATION",
  "errorMessage": "Failed authentication"
}
```

---

### 2. **GET /api/v1/tests** - Lấy danh sách bài kiểm tra

#### 📋 Mô tả
Lấy danh sách các bài kiểm tra với phân trang, tìm kiếm và lọc.

#### 🔐 Phân quyền
- **Yêu cầu**: Đăng nhập (Bearer Token)
- **Role**: Tất cả các role đã đăng nhập

#### 📥 Query Parameters

| Tham số | Kiểu | Mặc định | Mô tả |
|---------|------|----------|-------|
| `searchTerm` | string | null | Tìm kiếm theo tên hoặc mục đích bài test |
| `sortColumn` | string | "testid" | Cột để sắp xếp: name, testtype, maxscore, createdat |
| `sortOrder` | string | "desc" | Thứ tự: asc (tăng dần), desc (giảm dần) |
| `testType` | int | null | Lọc theo loại: 1=English, 2=Practical |
| `page` | int | 1 | Số trang hiện tại |
| `pageSize` | int | 10 | Số bản ghi mỗi trang |

#### 📤 Response (Success - 200 OK)

```json
{
  "data": {
    "items": [
      {
        "testId": 1,
        "testName": "English Listening Test - Level A",
        "purpose": "Test listening comprehension for cabin crew candidates",
        "testType": "English",
        "maxScore": 100,
        "audioFileURL": "https://example.com/audio/test-listening-a.mp3",
        "joinCode": "K7HM9P3QXY",
        "createdAt": "10/11/2025 10:30:00",
        "createdBy": "Nguyễn Văn A"
      },
      {
        "testId": 2,
        "testName": "Practical Skills Assessment",
        "purpose": "Evaluate practical cabin crew skills",
        "testType": "Practical",
        "maxScore": 100,
        "audioFileURL": null,
        "joinCode": "M4N2P8QRST",
        "createdAt": "10/11/2025 09:15:00",
        "createdBy": "Trần Thị B"
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 2,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "code": 0,
  "message": "SUCCESS"
}
```

#### 🔍 Ví dụ sử dụng

**Tìm kiếm bài test có từ "English":**
```
GET /api/v1/tests?searchTerm=English&page=1&pageSize=10
```

**Lọc các bài test Practical và sắp xếp theo tên:**
```
GET /api/v1/tests?testType=2&sortColumn=name&sortOrder=asc
```

**Lấy trang 2 với 20 bản ghi:**
```
GET /api/v1/tests?page=2&pageSize=20
```

---

## 📊 Database Schema

### Test Entity

```csharp
public class Test : AuditableEntity
{
    public int TestId { get; set; }              // Primary Key
    public string TestName { get; set; }         // Tên bài kiểm tra
    public string Purpose { get; set; }          // Mục đích
    public TestType TestType { get; set; }       // Loại: English/Practical
    public int MaxScore { get; set; }            // Điểm tối đa
    public string AudioFileURL { get; set; }     // Link file audio
    public string JoinCode { get; set; }         // Mã tham gia (unique)
}
```

### TestType Enum

```csharp
public enum TestType
{
    English = 1,    // Bài thi Tiếng Anh (yêu cầu audio)
    Practical = 2   // Bài thi Thực hành
}
```

---

## 🔒 Error Codes

| Code | Error | Mô tả |
|------|-------|-------|
| 3 | CREATED_UNSUCC | Tạo bài test thất bại (validation error) |
| 10 | FAILED_AUTHENTICATION | Xác thực thất bại |
| 14 | USER_NOT_FOUND | Không tìm thấy user |
| 17 | UNAUTHORIZED_EXAMINER_ONLY | Chỉ Examiner mới được tạo test |
| 18 | TEST_NOT_FOUND | Không tìm thấy bài test |
| 19 | DUPLICATE_JOIN_CODE | Mã JoinCode bị trùng |

---

## 🧪 Testing với Swagger

1. **Đăng nhập** bằng tài khoản Examiner
2. Copy **Access Token** từ response
3. Click nút **"Authorize"** trên Swagger UI
4. Nhập: `Bearer {token}`
5. Test các API endpoints

---

## 📝 Notes

- **JoinCode** là duy nhất (unique) trong hệ thống
- Chỉ Examiner mới có thể tạo test
- Tất cả user đã đăng nhập có thể xem danh sách tests
- AudioFileURL chỉ bắt buộc với English test type
- Pagination giúp tối ưu hiệu suất khi có nhiều bài test

---

## 🚀 Các bước tiếp theo có thể phát triển

1. ✅ API Create Test (Đã xong)
2. ✅ API Get Tests List (Đã xong)
3. ⏳ API Get Test Detail by ID
4. ⏳ API Update Test
5. ⏳ API Delete Test
6. ⏳ API Add Questions to Test
7. ⏳ API Start Test Session (Candidates)
8. ⏳ API Submit Test Answers
