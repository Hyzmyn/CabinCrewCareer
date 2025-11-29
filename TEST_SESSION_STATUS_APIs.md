# Test Session Status APIs - Documentation

## Overview
Tạo 3 API mới để get test sessions với trạng thái pass/fail/ongoing dựa trên RoundActivity:

1. **GET /api/v1/test-sessions/by-round** - Admin/Examiner xem tất cả test sessions theo round
2. **GET /api/v1/test-sessions/my-english** - Candidate xem English test sessions (Listening + Speaking) của mình
3. **GET /api/v1/test-sessions/my-practical** - Cabin Crew xem Practical test sessions của mình

---

## API 1: Get Test Sessions By Round (Admin/Examiner)

### Endpoint
```
GET /api/v1/test-sessions/by-round
```

### Purpose
Admin hoặc Examiner xem tất cả test sessions trong một round với trạng thái RoundActivity của từng user.

### Query Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| roundId | int | ✅ Yes | Round ID để lấy test sessions |
| testType | int? | ❌ No | Filter theo test type (1: Listening, 2: Speaking, 3: Practical) |
| searchTerm | string? | ❌ No | Search theo user name hoặc email |
| sortColumn | string? | ❌ No | Sort column (testSessionId, username, totalScore, startTime, endTime, createdAt) |
| sortOrder | string? | ❌ No | "asc" hoặc "desc" (default: desc by createdAt) |
| page | int | ❌ No | Page number (default: 1) |
| pageSize | int | ❌ No | Items per page (default: 10) |

### Request Example
```http
GET /api/v1/test-sessions/by-round?roundId=12&testType=1&page=1&pageSize=20
Authorization: Bearer <token>
```

### Response Example
```json
{
  "succeeded": true,
  "code": 0,
  "message": "Test sessions for round 12 retrieved successfully",
  "data": {
    "items": [
      {
        "testSessionId": 501,
        "userId": 301,
        "userFullName": "Nguyen Van A",
        "userEmail": "nguyenvana@example.com",
        "imgURL": "https://example.com/avatar.jpg",
        "testId": 6,
        "testName": "Listening Test - Campaign 2024",
        "testType": "EnglishListening",
        "startTime": "2024-11-20T09:00:00",
        "endTime": "2024-11-20T10:30:00",
        "totalScore": 110,
        "maxScore": 120,
        "status": true,
        "createdAt": "2024-11-20T09:00:00",
        "totalAnswers": 40,
        "activityStatus": "Passed",
        "roundId": 12,
        "roundType": "EnglishListeningTest",
        "activityId": 201
      },
      {
        "testSessionId": 502,
        "userId": 302,
        "userFullName": "Tran Thi B",
        "userEmail": "tranthib@example.com",
        "imgURL": null,
        "testId": 6,
        "testName": "Listening Test - Campaign 2024",
        "testType": "EnglishListening",
        "startTime": "2024-11-20T09:00:00",
        "endTime": "2024-11-20T10:25:00",
        "totalScore": 70,
        "maxScore": 120,
        "status": true,
        "createdAt": "2024-11-20T09:00:00",
        "totalAnswers": 40,
        "activityStatus": "Failed",
        "roundId": 12,
        "roundType": "EnglishListeningTest",
        "activityId": 202
      },
      {
        "testSessionId": 503,
        "userId": 303,
        "userFullName": "Le Van C",
        "userEmail": "levanc@example.com",
        "imgURL": "https://example.com/avatar2.jpg",
        "testId": 6,
        "testName": "Listening Test - Campaign 2024",
        "testType": "EnglishListening",
        "startTime": "2024-11-20T09:00:00",
        "endTime": "2024-11-20T10:20:00",
        "totalScore": 95,
        "maxScore": 120,
        "status": false,
        "createdAt": "2024-11-20T09:00:00",
        "totalAnswers": 35,
        "activityStatus": "Ongoing",
        "roundId": 12,
        "roundType": "EnglishListeningTest",
        "activityId": 203
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalRecords": 3,
    "totalPages": 1
  }
}
```

### Activity Status Logic
- **"Ongoing"**: User đang trong round này (chưa được đánh giá pass/fail)
- **"Passed"**: User đã pass round này (TotalScore >= MaxScore * 0.8)
- **"Failed"**: User đã fail round này (TotalScore < MaxScore * 0.8)

### Use Cases
1. Admin xem tất cả candidates đã hoàn thành Listening test trong round 12
2. Examiner lọc theo testType để xem riêng Speaking hoặc Practical tests
3. Kiểm tra ai đã pass/fail để quyết định chuyển vòng tiếp theo

---

## API 2: Get My English Test Sessions (Candidate)

### Endpoint
```
GET /api/v1/test-sessions/my-english
```

### Purpose
Candidate xem các English test sessions (cả Listening và Speaking) của mình với trạng thái pass/fail.

### Authentication
- Sử dụng JWT token để identify user
- UserId được lấy tự động từ token, không cần truyền parameter

### Query Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| roundId | int? | ❌ No | Filter theo round ID |
| searchTerm | string? | ❌ No | Search theo test name |
| sortColumn | string? | ❌ No | Sort column (testSessionId, testtype, totalScore, startTime, endTime, createdAt) |
| sortOrder | string? | ❌ No | "asc" hoặc "desc" |
| page | int | ❌ No | Page number (default: 1) |
| pageSize | int | ❌ No | Items per page (default: 10) |

### Request Example
```http
GET /api/v1/test-sessions/my-english?page=1&pageSize=10
Authorization: Bearer <candidate_token>
```

### Response Example
```json
{
  "succeeded": true,
  "code": 0,
  "message": "My English test sessions retrieved successfully",
  "data": {
    "items": [
      {
        "testSessionId": 601,
        "userId": 401,
        "userFullName": "Pham Van D",
        "userEmail": "phamvand@example.com",
        "imgURL": "https://example.com/avatar3.jpg",
        "testId": 6,
        "testName": "Listening Test - Campaign 2024",
        "testType": "EnglishListening",
        "startTime": "2024-11-21T10:00:00",
        "endTime": "2024-11-21T11:30:00",
        "totalScore": 110,
        "maxScore": 120,
        "status": true,
        "createdAt": "2024-11-21T10:00:00",
        "totalAnswers": 40,
        "activityStatus": "Passed",
        "roundId": 12,
        "roundType": "EnglishListeningTest",
        "activityId": 301
      },
      {
        "testSessionId": 701,
        "userId": 401,
        "userFullName": "Pham Van D",
        "userEmail": "phamvand@example.com",
        "imgURL": "https://example.com/avatar3.jpg",
        "testId": 7,
        "testName": "Speaking Test - Campaign 2024",
        "testType": "EnglishSpeaking",
        "startTime": "2024-11-22T14:00:00",
        "endTime": "2024-11-22T15:00:00",
        "totalScore": 85,
        "maxScore": 100,
        "status": true,
        "createdAt": "2024-11-22T14:00:00",
        "totalAnswers": 0,
        "activityStatus": "Passed",
        "roundId": 13,
        "roundType": "EnglishSpeakingTest",
        "activityId": 302
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalRecords": 2,
    "totalPages": 1
  }
}
```

### Use Cases
1. Candidate kiểm tra kết quả Listening test của mình
2. Candidate xem điểm Speaking test
3. Candidate biết được mình đã pass cả 2 test hay chưa để tiến vào vòng Interview

### Important Notes
- Trả về CẢ Listening VÀ Speaking test sessions
- Candidate chỉ thấy test sessions của chính mình
- Activity status phụ thuộc vào việc examiner đã chấm điểm và update RoundActivity chưa

---

## API 3: Get My Practical Test Sessions (Cabin Crew)

### Endpoint
```
GET /api/v1/test-sessions/my-practical
```

### Purpose
Cabin crew applicant xem Practical test sessions của mình với trạng thái pass/fail.

### Authentication
- Sử dụng JWT token để identify user
- UserId được lấy tự động từ token

### Query Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| roundId | int? | ❌ No | Filter theo round ID |
| searchTerm | string? | ❌ No | Search theo test name |
| sortColumn | string? | ❌ No | Sort column (testSessionId, totalScore, startTime, endTime, createdAt) |
| sortOrder | string? | ❌ No | "asc" hoặc "desc" |
| page | int | ❌ No | Page number (default: 1) |
| pageSize | int | ❌ No | Items per page (default: 10) |

### Request Example
```http
GET /api/v1/test-sessions/my-practical?page=1&pageSize=10
Authorization: Bearer <cabincrew_token>
```

### Response Example
```json
{
  "succeeded": true,
  "code": 0,
  "message": "My Practical test sessions retrieved successfully",
  "data": {
    "items": [
      {
        "testSessionId": 801,
        "userId": 501,
        "userFullName": "Hoang Thi E",
        "userEmail": "hoangthie@example.com",
        "imgURL": "https://example.com/avatar4.jpg",
        "testId": 10,
        "testName": "Practical Exam - Safety Procedures",
        "testType": "Practical",
        "startTime": "2024-11-23T08:00:00",
        "endTime": "2024-11-23T10:00:00",
        "totalScore": 92,
        "maxScore": 100,
        "status": true,
        "createdAt": "2024-11-23T08:00:00",
        "totalAnswers": 50,
        "activityStatus": "Passed",
        "roundId": 15,
        "roundType": "PracticalTest",
        "activityId": 401
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1
  }
}
```

### Use Cases
1. Cabin crew applicant kiểm tra kết quả Practical test
2. Applicant biết được mình đã pass hay fail
3. Nếu pass (>= 80%), applicant sẽ được chuyển sang vòng Interview ngay lập tức

### Important Notes
- Practical test chỉ cần **1 round** để pass (khác với English cần cả Listening + Speaking)
- Activity status "Passed" nghĩa là sẵn sàng vào vòng Interview
- Practical test thường có nhiều câu hỏi multiple choice về quy trình, an toàn, dịch vụ

---

## Database Schema

### TestSession Table
```sql
TestSessionId   INT PRIMARY KEY
UserId          INT
TestId          INT
StartTime       DATETIME
EndTime         DATETIME
TotalScore      INT
Status          BIT  -- Finalized (true) or not (false)
CreatedAt       DATETIME
```

### RoundActivity Table
```sql
ActivityId  INT PRIMARY KEY
RoundId     INT
UserId      INT
Status      INT  -- 1: Ongoing, 2: Passed, 3: Failed
```

### Test Table
```sql
TestId      INT PRIMARY KEY
TestName    NVARCHAR(255)
TestType    INT  -- 1: EnglishListening, 2: EnglishSpeaking, 3: Practical
MaxScore    INT
```

### Round Table
```sql
RoundId         INT PRIMARY KEY
RoundType       INT  -- 4: EnglishListeningTest, 5: EnglishSpeakingTest, 6: PracticalTest, 7: Interview
TestId          INT
CampaignRoundId INT
```

---

## Activity Status Logic

### Determination Flow
```
1. Get TestSession by TestId and UserId
2. Find Round that uses this TestId
3. Find RoundActivity by RoundId and UserId
4. Return RoundActivity.Status:
   - ActivityStatus.Ongoing (1) → "Ongoing"
   - ActivityStatus.Passed (2)  → "Passed"
   - ActivityStatus.Failed (3)  → "Failed"
```

### Status Meanings

#### "Ongoing" (Default)
- User đã tạo test session nhưng chưa được đánh giá
- Hoặc test session chưa finalized (Status = false)
- Hoặc chưa ai chạy API move-to-interview/update status

#### "Passed"
- TotalScore >= MaxScore * 0.8 (ngưỡng 80%)
- Đã được đánh giá bởi API move-to-interview
- For English: CẢ Listening VÀ Speaking phải Passed
- For Practical: Chỉ cần 1 round Passed

#### "Failed"
- TotalScore < MaxScore * 0.8
- User không đủ điều kiện chuyển sang round tiếp theo
- Cần thực hiện lại test hoặc bị loại khỏi campaign

---

## Workflow Integration

### Candidate Journey (English Tests)
```
1. Candidate thi Listening Test
   └─> POST /api/v1/test-sessions/submit-multiple-choice
   
2. Examiner chấm điểm (auto-graded for multiple choice)
   
3. Admin chạy move-to-interview cho Listening round
   └─> POST /api/v1/round-activities/move-to-interview
       Body: { "roundId": 12 }
   
4. Candidate check kết quả Listening
   └─> GET /api/v1/test-sessions/my-english
       Response: activityStatus = "Passed" (đợi Speaking)
   
5. Candidate thi Speaking Test
   └─> POST /api/v1/test-sessions/submit-speaking
   
6. Examiner chấm Speaking test
   └─> POST /api/v1/test-sessions/{id}/score-speaking
   
7. Admin chạy move-to-interview cho Speaking round
   └─> POST /api/v1/round-activities/move-to-interview
       Body: { "roundId": 13 }
   
8. Candidate check kết quả final
   └─> GET /api/v1/test-sessions/my-english
       Response: Cả 2 tests đều "Passed" → Interview RoundActivity được tạo
```

### Cabin Crew Journey (Practical Test)
```
1. Cabin crew thi Practical Test
   └─> POST /api/v1/test-sessions/submit-multiple-choice
   
2. Auto-graded hoặc manual grading
   
3. Admin chạy move-to-interview cho Practical round
   └─> POST /api/v1/round-activities/move-to-interview
       Body: { "roundId": 15 }
   
4. Cabin crew check kết quả
   └─> GET /api/v1/test-sessions/my-practical
       Response: activityStatus = "Passed" → Interview RoundActivity tạo ngay lập tức
```

---

## Error Handling

### Validation Errors
```json
{
  "succeeded": false,
  "code": 400,
  "message": "Round with ID 999 not found"
}
```

### Authentication Errors
```json
{
  "succeeded": false,
  "code": 401,
  "message": "Unauthorized - Invalid or expired token"
}
```

### Test Type Mismatch
```json
{
  "succeeded": false,
  "code": 400,
  "message": "Round 12 does not match the specified test type"
}
```

---

## Testing Scenarios

### Scenario 1: Admin xem Listening test results
```http
GET /api/v1/test-sessions/by-round?roundId=12&testType=1
```
Expected: Tất cả test sessions của Listening test trong round 12

### Scenario 2: Candidate xem English test progress
```http
GET /api/v1/test-sessions/my-english
```
Expected: Cả Listening và Speaking sessions với status

### Scenario 3: Cabin crew xem Practical result
```http
GET /api/v1/test-sessions/my-practical
```
Expected: Practical test sessions với activityStatus

### Scenario 4: Filter by round
```http
GET /api/v1/test-sessions/my-english?roundId=12
```
Expected: Chỉ English tests của round 12

### Scenario 5: Search users (Admin)
```http
GET /api/v1/test-sessions/by-round?roundId=12&searchTerm=nguyen
```
Expected: Test sessions của users có "nguyen" trong tên

---

## Summary

### Files Created
1. `TestSessionWithStatusResponse.cs` - DTO với ActivityStatus field
2. `GetTestSessionsByRoundQuery.cs` + Handler - Admin API
3. `GetMyEnglishTestSessionsQuery.cs` + Handler - Candidate API
4. `GetMyPracticalTestSessionsWithStatusQuery.cs` + Handler - Cabin Crew API
5. Controller endpoints trong `TestSessionsController.cs`

### Key Features
✅ Activity status based on RoundActivity (Ongoing/Passed/Failed)
✅ Filter by RoundId and TestType
✅ Pagination và sorting
✅ Search functionality
✅ JWT authentication với CurrentUserService
✅ Comprehensive error handling
✅ Detailed XML documentation

### Next Steps
1. Test các API với Postman/Swagger
2. Verify activity status logic với database thực
3. Kiểm tra performance với large datasets
4. Add caching nếu cần
5. Consider notification system khi status thay đổi
