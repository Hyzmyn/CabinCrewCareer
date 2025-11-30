# Test API Chấm Điểm Với Nhiều Tiêu Chí

## 🎯 Endpoint
```
PUT http://localhost:5268/api/v1/test-sessions/{testSessionId}/answers/score
```

## 📤 Request Body Mẫu

### Ví dụ 1: Chấm Speaking Test với 3 tiêu chí
```json
PUT /api/v1/test-sessions/123/answers/score
Content-Type: application/json
Authorization: Bearer {your_token}

{
  "answerScores": {
    "1": {
      "criteriaScores": {
        "Pronunciation": 8,
        "Fluency": 7,
        "Grammar": 9,
        "Vocabulary": 8
      },
      "isCorrect": true
    }
  }
}
```

**Tổng điểm Answer 1:** 8 + 7 + 9 + 8 = **32 điểm**

### Ví dụ 2: Chấm Essay với nhiều tiêu chí
```json
PUT /api/v1/test-sessions/123/answers/score
Content-Type: application/json
Authorization: Bearer {your_token}

{
  "answerScores": {
    "2": {
      "criteriaScores": {
        "Content": 10,
        "Organization": 9,
        "Language Use": 8,
        "Mechanics": 7,
        "Creativity": 9
      },
      "isCorrect": true
    }
  }
}
```

**Tổng điểm Answer 2:** 10 + 9 + 8 + 7 + 9 = **43 điểm**

### Ví dụ 3: Chấm nhiều câu cùng lúc với tiêu chí khác nhau
```json
PUT /api/v1/test-sessions/123/answers/score
Content-Type: application/json
Authorization: Bearer {your_token}

{
  "answerScores": {
    "1": {
      "criteriaScores": {
        "Pronunciation": 8,
        "Fluency": 7,
        "Grammar": 9
      },
      "isCorrect": true
    },
    "2": {
      "criteriaScores": {
        "Content": 10,
        "Organization": 8,
        "Language": 9
      },
      "isCorrect": true
    },
    "3": {
      "criteriaScores": {
        "Accuracy": 10,
        "Completeness": 9,
        "Clarity": 8
      },
      "isCorrect": true
    }
  }
}
```

**Tổng điểm:**
- Answer 1: 24 điểm
- Answer 2: 27 điểm  
- Answer 3: 27 điểm
- **TestSession Total: 78 điểm**

---

## 💾 Kết Quả Lưu Trong Database

### Bảng `TestAnswer`

| AnswerId | Score | IsCorrect | EssayAnswer (JSON) |
|----------|-------|-----------|-------------------|
| 1 | 24 | true | `{"criteriaScores":{"Pronunciation":8,"Fluency":7,"Grammar":9}}` |
| 2 | 27 | true | `{"criteriaScores":{"Content":10,"Organization":8,"Language":9}}` |
| 3 | 27 | true | `{"criteriaScores":{"Accuracy":10,"Completeness":9,"Clarity":8}}` |

### Bảng `TestSession`

| TestSessionId | TotalScore |
|---------------|------------|
| 123 | 78 |

---

## 📊 Trường Hợp Đặc Biệt: TestAnswer Đã Có Essay

Nếu học sinh đã submit essay trước đó:

**Trước khi chấm:**
```
TestAnswer.EssayAnswer = "This is my essay about climate change..."
```

**Request chấm điểm:**
```json
{
  "answerScores": {
    "5": {
      "criteriaScores": {
        "Content": 9,
        "Grammar": 8
      },
      "isCorrect": true
    }
  }
}
```

**Sau khi chấm, EssayAnswer sẽ lưu:**
```json
{
  "studentAnswer": "This is my essay about climate change...",
  "criteriaScores": {
    "Content": 9,
    "Grammar": 8
  }
}
```

**Lợi ích:** 
- ✅ Bảo toàn essay gốc của học sinh
- ✅ Lưu chi tiết điểm từng tiêu chí
- ✅ Frontend parse JSON dễ dàng

---

## 🔍 Cách Frontend Đọc Dữ Liệu

### JavaScript/TypeScript
```javascript
// Lấy TestAnswer từ API
const testAnswer = await fetch('/api/v1/test-answers/1').then(r => r.json());

// Parse JSON từ EssayAnswer
const answerData = JSON.parse(testAnswer.essayAnswer);

// Hiển thị điểm từng tiêu chí
console.log('Điểm từng tiêu chí:');
for (const [criteria, score] of Object.entries(answerData.criteriaScores)) {
  console.log(`${criteria}: ${score} điểm`);
}
// Output:
// Pronunciation: 8 điểm
// Fluency: 7 điểm
// Grammar: 9 điểm

// Nếu có essay của học sinh
if (answerData.studentAnswer) {
  console.log('Essay:', answerData.studentAnswer);
}
```

### C# Backend (Khi cần đọc lại)
```csharp
using System.Text.Json;

var testAnswer = await _unitOfWork.TestAnswers.GetByIdAsync(1);

// Parse JSON
var answerData = JsonSerializer.Deserialize<AnswerData>(testAnswer.EssayAnswer);

// Lấy điểm từng tiêu chí
foreach (var criteria in answerData.CriteriaScores)
{
    Console.WriteLine($"{criteria.Key}: {criteria.Value}");
}

public class AnswerData
{
    public string? StudentAnswer { get; set; }
    public Dictionary<string, int> CriteriaScores { get; set; }
}
```

---

## ✅ Validation Rules

1. ✅ TestSession phải tồn tại
2. ✅ AnswerScores không được rỗng
3. ✅ TestAnswer phải tồn tại và chưa được chấm (Score = 0)
4. ✅ CriteriaScores không được rỗng
5. ✅ Tất cả criteria scores phải >= 0
6. ✅ Tổng điểm không vượt quá MaxScore của câu hỏi

---

## 🧪 Test Case

### Test 1: Chấm 1 câu với 4 tiêu chí
```bash
curl -X PUT "http://localhost:5268/api/v1/test-sessions/123/answers/score" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "answerScores": {
      "1": {
        "criteriaScores": {
          "Pronunciation": 8,
          "Fluency": 7,
          "Grammar": 9,
          "Vocabulary": 8
        },
        "isCorrect": true
      }
    }
  }'
```

**Expected Response:**
```json
{
  "code": 200,
  "data": "Test answers scored successfully. Total answers: 1, Total score: 32",
  "message": "SUCCESS"
}
```

**Check Database:**
```sql
SELECT AnswerId, Score, IsCorrect, EssayAnswer 
FROM TestAnswer 
WHERE AnswerId = 1;

-- Result:
-- AnswerId: 1
-- Score: 32
-- IsCorrect: 1
-- EssayAnswer: {"criteriaScores":{"Pronunciation":8,"Fluency":7,"Grammar":9,"Vocabulary":8}}
```

### Test 2: Verify TestSession Total Score
```sql
SELECT TestSessionId, TotalScore 
FROM TestSession 
WHERE TestSessionId = 123;

-- Result:
-- TestSessionId: 123
-- TotalScore: 32
```
