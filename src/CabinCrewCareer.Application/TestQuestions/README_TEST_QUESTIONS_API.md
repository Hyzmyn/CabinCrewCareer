# Test Questions API Documentation

## Tổng quan
Module Test Questions API quản lý câu hỏi cho các bài kiểm tra, bao gồm cả các tùy chọn trả lời (options) cho từng câu hỏi.

---

## API Endpoints

### 1. **POST /api/v1/test-questions** - Tạo câu hỏi cho bài test

#### 📋 Mô tả
Examiner tạo câu hỏi mới cho một bài test cụ thể. Logic tạo câu hỏi phụ thuộc vào loại test (TestType).

#### 🔐 Phân quyền
- **Yêu cầu**: Đăng nhập (Bearer Token)
- **Role**: Chỉ **Examiner** mới được phép tạo câu hỏi
- **HTTP Status nếu không có quyền**: 403 Forbidden

#### 📥 Request Body

**Với TestType = EnglishListening hoặc Practical (CẦN options):**
```json
{
  "testId": 10,
  "questionContent": "What is the primary responsibility of a flight attendant during an emergency?",
  "score": 10,
  "orderNumber": 1,
  "options": [
    {
      "optionContent": "Serving food and beverages",
      "isCorrect": false
    },
    {
      "optionContent": "Ensuring passenger safety",
      "isCorrect": true
    },
    {
      "optionContent": "Collecting tickets",
      "isCorrect": false
    },
    {
      "optionContent": "Cleaning the cabin",
      "isCorrect": false
    }
  ]
}
```

**Với TestType = EnglishSpeaking (KHÔNG cần options):**
```json
{
  "testId": 12,
  "questionContent": "Describe a time when you had to handle a difficult customer. How did you resolve the situation?",
  "score": 20,
  "orderNumber": 1,
  "options": null
}
```

#### 📝 Request Parameters

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|----------|-------|
| `testId` | int | ✅ | ID của bài test (phải > 0) |
| `questionContent` | string | ✅ | Nội dung câu hỏi (tối đa 1000 ký tự) |
| `score` | int | ✅ | Điểm số cho câu hỏi (1-100) |
| `orderNumber` | int | ✅ | Thứ tự hiển thị câu hỏi (phải > 0) |
| `options` | array | ⚠️ | Các lựa chọn trả lời (xem logic bên dưới) |

#### ⚙️ Business Logic theo TestType

**1. EnglishListening (TestType = 1) - Trắc nghiệm:**
- ✅ **BẮT BUỘC** phải có `options`
- ✅ Tối thiểu 2 options, tối đa 6 options
- ✅ **Đúng 1 option** phải có `isCorrect = true`
- ✅ Mỗi option content là bắt buộc (tối đa 500 ký tự)
- ❌ Nếu không có options → Trả về lỗi validation

**2. EnglishSpeaking (TestType = 2) - Tự luận:**
- ❌ **KHÔNG được** có `options` (hoặc `options = null`)
- ✅ Chỉ có câu hỏi, không có đáp án trắc nghiệm
- ✅ Thí sinh sẽ trả lời bằng văn bản hoặc audio
- ❌ Nếu có options → Trả về lỗi validation

**3. Practical (TestType = 3) - Trắc nghiệm:**
- ✅ **BẮT BUỘC** phải có `options`
- ✅ Tối thiểu 2 options, tối đa 6 options
- ✅ **Đúng 1 option** phải có `isCorrect = true`
- ✅ Mỗi option content là bắt buộc (tối đa 500 ký tự)
- ❌ Nếu không có options → Trả về lỗi validation

#### ✅ Validation Rules

**TestQuestion:**
- TestId phải > 0 và bài test phải tồn tại
- QuestionContent: Bắt buộc, tối đa 1000 ký tự
- Score: 1-100 điểm
- OrderNumber: Phải > 0

**Options (nếu có):**
- Số lượng: 2-6 options
- Đúng 1 option có `isCorrect = true`
- OptionContent: Bắt buộc, tối đa 500 ký tự

#### 📤 Response (Success - 200 OK)

**Với EnglishListening/Practical (có options):**
```json
{
  "data": {
    "questionId": 15,
    "questionContent": "What is the primary responsibility of a flight attendant during an emergency?",
    "score": 10,
    "orderNumber": 1,
    "testId": 10,
    "testName": "English Listening Comprehension Test",
    "testType": "EnglishListening",
    "options": [
      {
        "optionId": 45,
        "optionContent": "Serving food and beverages",
        "isCorrect": false
      },
      {
        "optionId": 46,
        "optionContent": "Ensuring passenger safety",
        "isCorrect": true
      },
      {
        "optionId": 47,
        "optionContent": "Collecting tickets",
        "isCorrect": false
      },
      {
        "optionId": 48,
        "optionContent": "Cleaning the cabin",
        "isCorrect": false
      }
    ]
  },
  "code": 2,
  "message": "Created successfuly"
}
```

**Với EnglishSpeaking (không có options):**
```json
{
  "data": {
    "questionId": 16,
    "questionContent": "Describe a time when you had to handle a difficult customer. How did you resolve the situation?",
    "score": 20,
    "orderNumber": 1,
    "testId": 12,
    "testName": "English Speaking Assessment",
    "testType": "EnglishSpeaking",
    "options": []
  },
  "code": 2,
  "message": "Created successfuly"
}
```

#### ❌ Error Responses

**Test Not Found (400 Bad Request):**
```json
{
  "errorCode": 18,
  "error": "TEST_NOT_FOUND",
  "errorMessage": "Test not found"
}
```

**Missing Options for EnglishListening/Practical (400 Bad Request):**
```json
{
  "errorCode": 3,
  "errorMessage": "Created unsuccessfuly",
  "errors": [
    "Options are required for EnglishListening and Practical test types"
  ]
}
```

**Options Provided for EnglishSpeaking (400 Bad Request):**
```json
{
  "errorCode": 3,
  "errorMessage": "Created unsuccessfuly",
  "errors": [
    "EnglishSpeaking test type should not have options"
  ]
}
```

**Invalid Correct Answer Count (400 Bad Request):**
```json
{
  "errorCode": 3,
  "errorMessage": "Created unsuccessfuly",
  "errors": [
    "Exactly one option must be marked as correct"
  ]
}
```

**Validation Errors (400 Bad Request):**
```json
{
  "errorCode": 3,
  "errorMessage": "Created unsuccessfuly",
  "errors": [
    "Question content is required",
    "Score must be greater than 0",
    "At least 2 options are required for multiple choice questions"
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

#### 🔍 Ví dụ sử dụng

**Tạo câu hỏi trắc nghiệm cho Listening Test:**
```bash
POST /api/v1/test-questions
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "testId": 10,
  "questionContent": "Listen to the announcement. What gate should passengers go to?",
  "score": 5,
  "orderNumber": 1,
  "options": [
    {
      "optionContent": "Gate A12",
      "isCorrect": false
    },
    {
      "optionContent": "Gate B15",
      "isCorrect": true
    },
    {
      "optionContent": "Gate C20",
      "isCorrect": false
    }
  ]
}
```

**Tạo câu hỏi tự luận cho Speaking Test:**
```bash
POST /api/v1/test-questions
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "testId": 12,
  "questionContent": "Tell me about your experience working in customer service.",
  "score": 15,
  "orderNumber": 2,
  "options": null
}
```

---

## 📊 Database Schema

### TestQuestion Entity

```csharp
public class TestQuestion
{
    public int QuestionId { get; set; }          // Primary Key
    public string QuestionContent { get; set; }  // Nội dung câu hỏi
    public int Score { get; set; }               // Điểm số
    public int OrderNumber { get; set; }         // Thứ tự hiển thị
    public int? TestId { get; set; }             // Foreign Key to Test
    
    // Navigation Properties
    public virtual Test Test { get; set; }
    public virtual ICollection<TestOption> TestOptions { get; set; }
}
```

### TestOption Entity

```csharp
public class TestOption
{
    public int OptionId { get; set; }            // Primary Key
    public string OptionContent { get; set; }    // Nội dung lựa chọn
    public bool IsCorrect { get; set; }          // Đáp án đúng?
    public int? QuestionId { get; set; }         // Foreign Key to TestQuestion
    
    // Navigation Properties
    public virtual TestQuestion TestQuestion { get; set; }
}
```

---

## 🎯 Use Cases

### Use Case 1: Tạo bài test Listening với 10 câu hỏi trắc nghiệm
```
1. Examiner tạo Test (TestType = EnglishListening)
2. Examiner tạo 10 câu hỏi với POST /api/v1/test-questions
3. Mỗi câu hỏi có 4 options, 1 đáp án đúng
4. OrderNumber: 1, 2, 3, ..., 10
5. Score: 10 điểm/câu (tổng 100 điểm)
```

### Use Case 2: Tạo bài test Speaking với 5 câu hỏi tự luận
```
1. Examiner tạo Test (TestType = EnglishSpeaking)
2. Examiner tạo 5 câu hỏi với POST /api/v1/test-questions
3. Mỗi câu hỏi KHÔNG có options
4. OrderNumber: 1, 2, 3, 4, 5
5. Score: 20 điểm/câu (tổng 100 điểm)
```

### Use Case 3: Tạo bài test Practical với câu hỏi hỗn hợp
```
1. Examiner tạo Test (TestType = Practical)
2. Examiner tạo câu hỏi về quy trình an toàn
3. Mỗi câu có 3-4 options về các bước thực hiện
4. 1 đáp án đúng đánh dấu quy trình chính xác
```

---

## 🔒 Error Codes

| Code | Error | Mô tả |
|------|-------|-------|
| 3 | CREATED_UNSUCC | Tạo câu hỏi thất bại (validation error) |
| 10 | FAILED_AUTHENTICATION | Xác thực thất bại |
| 14 | USER_NOT_FOUND | Không tìm thấy user |
| 17 | UNAUTHORIZED_EXAMINER_ONLY | Chỉ Examiner mới được tạo câu hỏi |
| 18 | TEST_NOT_FOUND | Không tìm thấy bài test |

---

## 💡 Best Practices

1. **OrderNumber**: Đặt số thứ tự tuần tự (1, 2, 3...) để dễ quản lý
2. **Score Distribution**: Phân bố điểm hợp lý (ví dụ: 10 câu x 10 điểm = 100)
3. **Option Count**: Nên có 4 options cho câu hỏi trắc nghiệm chuẩn
4. **Correct Answer**: Luôn đảm bảo có đúng 1 đáp án đúng
5. **Content Length**: Câu hỏi ngắn gọn, rõ ràng (< 200 ký tự tốt nhất)
6. **TestType Logic**: Kiểm tra TestType trước khi tạo câu hỏi

---

## 🚀 Roadmap

- ✅ **API Create Test Question** (Đã xong)
- ⏳ API Get Questions by Test ID
- ⏳ API Update Test Question
- ⏳ API Delete Test Question
- ⏳ API Reorder Questions
- ⏳ API Bulk Create Questions (Import from file)

---

## 🧪 Testing với Swagger

1. Đăng nhập với tài khoản **Examiner**
2. Copy **Access Token** từ response
3. Click **"Authorize"** → Nhập `Bearer {token}`
4. Tạo Test trước (POST /api/v1/tests)
5. Lấy TestId từ response
6. Tạo câu hỏi (POST /api/v1/test-questions) với TestId

---

**Lưu ý quan trọng:**
- EnglishListening & Practical: **PHẢI có options**
- EnglishSpeaking: **KHÔNG được có options**
- Chỉ Examiner mới được tạo câu hỏi
- Test phải tồn tại trước khi tạo câu hỏi
