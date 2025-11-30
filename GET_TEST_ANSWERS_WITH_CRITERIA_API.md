# 📖 API Documentation: Get Test Answers With Criteria Scores

## 🎯 Endpoint
```
GET /api/v1/test-sessions/{testSessionId}/answers-with-criteria
```

## 📄 Mô tả
API này lấy tất cả test answers của một test session và tự động parse JSON trong trường `EssayAnswer` để trả về:
- ✅ Điểm chi tiết từng tiêu chí (criteriaScores)
- ✅ Essay gốc của học sinh (studentAnswer)
- ✅ Thông tin câu hỏi (question)
- ✅ Audio/file đính kèm

---

## 🔑 Authentication
- **Required:** Yes
- **Type:** Bearer Token (JWT)

---

## 📥 Request

### URL Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| testSessionId | int | Yes | ID của test session cần lấy answers |

### Example Request
```http
GET /api/v1/test-sessions/123/answers-with-criteria
Authorization: Bearer {your_jwt_token}
```

---

## 📤 Response

### Success Response (200 OK)

```json
{
  "code": 200,
  "message": "Test answers retrieved successfully.",
  "data": [
    {
      "answerId": 1,
      "score": 24,
      "isCorrect": true,
      "essayAnswer": "{\"criteriaScores\":{\"Pronunciation\":8,\"Fluency\":7,\"Grammar\":9}}",
      "answerAudioFileURL": "https://storage.example.com/audio/answer1.mp3",
      "selectedOptionId": null,
      "criteriaScores": {
        "Pronunciation": 8,
        "Fluency": 7,
        "Grammar": 9
      },
      "studentAnswer": null,
      "question": {
        "questionId": 5,
        "questionContent": "Describe your favorite place and explain why you like it",
        "orderNumber": 1,
        "maxScore": 30
      }
    },
    {
      "answerId": 2,
      "score": 43,
      "isCorrect": true,
      "essayAnswer": "{\"studentAnswer\":\"Climate change is one of the most...\",\"criteriaScores\":{\"Content\":10,\"Organization\":9,\"Language\":8,\"Mechanics\":7,\"Creativity\":9}}",
      "answerAudioFileURL": "",
      "selectedOptionId": null,
      "criteriaScores": {
        "Content": 10,
        "Organization": 9,
        "Language": 8,
        "Mechanics": 7,
        "Creativity": 9
      },
      "studentAnswer": "Climate change is one of the most pressing issues of our time...",
      "question": {
        "questionId": 8,
        "questionContent": "Write an essay about climate change",
        "orderNumber": 2,
        "maxScore": 50
      }
    },
    {
      "answerId": 3,
      "score": 10,
      "isCorrect": true,
      "essayAnswer": "",
      "answerAudioFileURL": "",
      "selectedOptionId": 25,
      "criteriaScores": null,
      "studentAnswer": null,
      "question": {
        "questionId": 12,
        "questionContent": "What is the capital of Vietnam?",
        "orderNumber": 3,
        "maxScore": 10
      }
    }
  ]
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| **answerId** | int | ID của test answer |
| **score** | int | Tổng điểm (sum of criteria scores) |
| **isCorrect** | bool | Câu trả lời đúng/sai |
| **essayAnswer** | string | Raw JSON string trong DB |
| **answerAudioFileURL** | string | Link file audio (nếu có) |
| **selectedOptionId** | int? | ID option được chọn (câu trắc nghiệm) |
| **criteriaScores** | object | **Parsed điểm từng tiêu chí** (key: tên tiêu chí, value: điểm) |
| **studentAnswer** | string? | **Essay gốc của học sinh** (nếu có trong JSON) |
| **question** | object | Thông tin câu hỏi |

#### Question Object
| Field | Type | Description |
|-------|------|-------------|
| questionId | int | ID câu hỏi |
| questionContent | string | Nội dung câu hỏi |
| orderNumber | int | Thứ tự câu hỏi |
| maxScore | int | Điểm tối đa |

---

## 🎯 Use Cases

### 1. Xem chi tiết điểm Speaking Test
```http
GET /api/v1/test-sessions/456/answers-with-criteria
```

**Response:**
```json
{
  "data": [
    {
      "answerId": 10,
      "score": 32,
      "criteriaScores": {
        "Pronunciation": 8,
        "Fluency": 7,
        "Grammar": 9,
        "Vocabulary": 8
      },
      "answerAudioFileURL": "https://storage.../speaking.mp3",
      "question": {
        "questionContent": "Introduce yourself",
        "maxScore": 40
      }
    }
  ]
}
```

### 2. Review Essay với Rubric Scores
```http
GET /api/v1/test-sessions/789/answers-with-criteria
```

**Response:**
```json
{
  "data": [
    {
      "answerId": 20,
      "score": 43,
      "criteriaScores": {
        "Content": 10,
        "Organization": 9,
        "Language Use": 8,
        "Mechanics": 7,
        "Creativity": 9
      },
      "studentAnswer": "In today's digital age, technology has transformed...",
      "question": {
        "questionContent": "Write about the impact of technology"
      }
    }
  ]
}
```

### 3. Mixed Test (Trắc nghiệm + Essay + Speaking)
```http
GET /api/v1/test-sessions/999/answers-with-criteria
```

**Response sẽ bao gồm:**
- Câu trắc nghiệm: `criteriaScores = null`, có `selectedOptionId`
- Câu essay: có `criteriaScores`, có `studentAnswer`
- Câu speaking: có `criteriaScores`, có `answerAudioFileURL`

---

## ❌ Error Responses

### Test Session Not Found (400 Bad Request)
```json
{
  "errorCode": "CREATED_UNSUCC",
  "errorMessage": "Validation failed",
  "errors": [
    "Test session not found."
  ]
}
```

### Unauthorized (401)
```json
{
  "message": "Unauthorized"
}
```

---

## 💡 Frontend Usage Examples

### React/TypeScript
```typescript
interface CriteriaScores {
  [criteriaName: string]: number;
}

interface TestAnswer {
  answerId: number;
  score: number;
  isCorrect: boolean;
  criteriaScores?: CriteriaScores;
  studentAnswer?: string;
  answerAudioFileURL?: string;
  question?: {
    questionId: number;
    questionContent: string;
    orderNumber: number;
    maxScore: number;
  };
}

// Fetch API
const fetchTestAnswers = async (testSessionId: number) => {
  const response = await fetch(
    `http://localhost:5268/api/v1/test-sessions/${testSessionId}/answers-with-criteria`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  const result = await response.json();
  return result.data as TestAnswer[];
};

// Display Criteria Scores
const CriteriaScoresDisplay = ({ criteriaScores }: { criteriaScores?: CriteriaScores }) => {
  if (!criteriaScores) return null;
  
  return (
    <div className="criteria-breakdown">
      <h4>Chi tiết điểm từng tiêu chí:</h4>
      {Object.entries(criteriaScores).map(([criteria, score]) => (
        <div key={criteria} className="criteria-item">
          <span>{criteria}</span>
          <span className="score">{score}</span>
        </div>
      ))}
    </div>
  );
};

// Complete Answer Component
const TestAnswerCard = ({ answer }: { answer: TestAnswer }) => {
  return (
    <div className="answer-card">
      <h3>Question {answer.question?.orderNumber}</h3>
      <p>{answer.question?.questionContent}</p>
      
      {/* Student's Essay */}
      {answer.studentAnswer && (
        <div className="student-essay">
          <h4>Bài làm của học sinh:</h4>
          <p>{answer.studentAnswer}</p>
        </div>
      )}
      
      {/* Audio File */}
      {answer.answerAudioFileURL && (
        <audio controls src={answer.answerAudioFileURL} />
      )}
      
      {/* Criteria Scores Breakdown */}
      <CriteriaScoresDisplay criteriaScores={answer.criteriaScores} />
      
      {/* Total Score */}
      <div className="total-score">
        <strong>Tổng điểm: {answer.score}/{answer.question?.maxScore}</strong>
      </div>
    </div>
  );
};
```

### Vue.js
```vue
<template>
  <div class="test-answers">
    <div v-for="answer in answers" :key="answer.answerId" class="answer-card">
      <h3>{{ answer.question?.questionContent }}</h3>
      
      <!-- Criteria Scores -->
      <div v-if="answer.criteriaScores" class="criteria-scores">
        <h4>Chi tiết điểm:</h4>
        <div v-for="(score, criteria) in answer.criteriaScores" 
             :key="criteria" 
             class="criteria-item">
          <span>{{ criteria }}</span>: 
          <strong>{{ score }}</strong>
        </div>
      </div>
      
      <!-- Student Answer -->
      <div v-if="answer.studentAnswer" class="student-answer">
        <h4>Bài làm:</h4>
        <p>{{ answer.studentAnswer }}</p>
      </div>
      
      <!-- Total -->
      <div class="total">
        Tổng điểm: {{ answer.score }}/{{ answer.question?.maxScore }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const answers = ref([]);

const fetchAnswers = async (testSessionId) => {
  const response = await fetch(
    `/api/v1/test-sessions/${testSessionId}/answers-with-criteria`,
    {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    }
  );
  
  const result = await response.json();
  answers.value = result.data;
};

onMounted(() => {
  fetchAnswers(123);
});
</script>
```

---

## 🧪 cURL Test Examples

### Test 1: Get Answers for Speaking Test
```bash
curl -X GET "http://localhost:5268/api/v1/test-sessions/123/answers-with-criteria" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test 2: With jq for Pretty Print
```bash
curl -X GET "http://localhost:5268/api/v1/test-sessions/123/answers-with-criteria" \
  -H "Authorization: Bearer YOUR_TOKEN" | jq '.data[].criteriaScores'
```

**Output:**
```json
{
  "Pronunciation": 8,
  "Fluency": 7,
  "Grammar": 9
}
```

---

## 🔄 Data Flow

```
┌────────────────────────────┐
│   Frontend Request         │
│   GET /test-sessions/123/  │
│   answers-with-criteria    │
└───────────┬────────────────┘
            ↓
┌────────────────────────────┐
│   Controller               │
│   GetTestAnswersWithCriteria│
└───────────┬────────────────┘
            ↓
┌────────────────────────────┐
│   Handler                  │
│   1. Query TestAnswers     │
│   2. Include Questions     │
└───────────┬────────────────┘
            ↓
┌────────────────────────────────────────────────┐
│   Database: TestAnswer                         │
│   AnswerId: 1                                  │
│   Score: 24                                    │
│   EssayAnswer: {"criteriaScores":{...}}        │
└───────────┬────────────────────────────────────┘
            ↓
┌────────────────────────────┐
│   Parse JSON               │
│   JsonDocument.Parse()     │
│   Extract:                 │
│   - criteriaScores         │
│   - studentAnswer          │
└───────────┬────────────────┘
            ↓
┌────────────────────────────────────┐
│   Response DTO                     │
│   {                                │
│     "answerId": 1,                 │
│     "score": 24,                   │
│     "criteriaScores": {            │
│       "Pronunciation": 8,          │
│       "Fluency": 7,                │
│       "Grammar": 9                 │
│     },                             │
│     "question": {...}              │
│   }                                │
└───────────┬────────────────────────┘
            ↓
┌────────────────────────────┐
│   Frontend Display         │
│   - Show criteria scores   │
│   - Show student essay     │
│   - Play audio if exists   │
└────────────────────────────┘
```

---

## ✅ Benefits

1. **✨ Automatic JSON Parsing**: API tự động parse EssayAnswer JSON, frontend không cần xử lý
2. **📊 Structured Data**: Trả về structured object thay vì raw JSON string
3. **🔍 Easy to Display**: Frontend dễ dàng hiển thị điểm chi tiết
4. **💪 Type Safe**: Response có type rõ ràng cho TypeScript
5. **🎯 Single Request**: Lấy tất cả thông tin cần thiết trong 1 request

---

## 🆚 So sánh với API cũ

| Feature | API Cũ (GetTestAnswersBySessionId) | API Mới (GetTestAnswersWithCriteria) |
|---------|-------------------------------------|--------------------------------------|
| Parse JSON | ❌ Frontend tự parse | ✅ API tự động parse |
| Criteria Scores | ❌ Raw string | ✅ Object structured |
| Student Essay | ❌ Mixed trong JSON | ✅ Field riêng `studentAnswer` |
| Type Safety | ⚠️ string (any) | ✅ Dictionary<string, int> |
| Ease of Use | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

---

## 📝 Notes

- API này **không thay thế** API `GetTestAnswersBySessionId` hiện có
- Dùng API này khi cần **hiển thị chi tiết criteria scores**
- API cũ vẫn dùng được cho các trường hợp đơn giản
- EssayAnswer vẫn được trả về để backward compatibility
