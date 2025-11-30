# 📊 Cấu Trúc Lưu Trữ Điểm Tiêu Chí Trong Database

## 🗄️ Bảng TestAnswer - Trường EssayAnswer

### Cấu trúc JSON được lưu:

```
┌─────────────────────────────────────────────────────────────────┐
│                     TestAnswer Table                             │
├──────────┬───────┬───────────┬──────────────────────────────────┤
│ AnswerId │ Score │ IsCorrect │ EssayAnswer (nvarchar(max))      │
├──────────┼───────┼───────────┼──────────────────────────────────┤
│    1     │  24   │   true    │ {"criteriaScores":{...}}         │
└──────────┴───────┴───────────┴──────────────────────────────────┘
```

---

## 📝 Ví Dụ Cụ Thể

### Case 1: Speaking Test - Chấm với 4 tiêu chí

**Request Body:**
```json
{
  "answerScores": {
    "101": {
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

**Database After Scoring:**

```sql
-- Bảng: TestAnswer
-- AnswerId: 101

AnswerId: 101
Score: 32  -- (8 + 7 + 9 + 8)
IsCorrect: 1
EssayAnswer: {"criteriaScores":{"Pronunciation":8,"Fluency":7,"Grammar":9,"Vocabulary":8}}
```

**Parsed JSON (Frontend):**
```json
{
  "criteriaScores": {
    "Pronunciation": 8,
    "Fluency": 7,
    "Grammar": 9,
    "Vocabulary": 8
  }
}
```

---

### Case 2: Essay Question - Chấm với 5 tiêu chí

**Request Body:**
```json
{
  "answerScores": {
    "102": {
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

**Database After Scoring:**

```sql
-- Bảng: TestAnswer
-- AnswerId: 102

AnswerId: 102
Score: 43  -- (10 + 9 + 8 + 7 + 9)
IsCorrect: 1
EssayAnswer: {"criteriaScores":{"Content":10,"Organization":9,"Language Use":8,"Mechanics":7,"Creativity":9}}
```

**Parsed JSON (Frontend):**
```json
{
  "criteriaScores": {
    "Content": 10,
    "Organization": 9,
    "Language Use": 8,
    "Mechanics": 7,
    "Creativity": 9
  }
}
```

---

### Case 3: Essay Đã Có Nội Dung - Chấm và bảo toàn essay

**Trạng thái TRƯỚC khi chấm:**
```sql
AnswerId: 103
Score: 0
IsCorrect: 0
EssayAnswer: "Climate change is one of the most pressing issues of our time. Global temperatures have risen significantly over the past century..."
```

**Request Body:**
```json
{
  "answerScores": {
    "103": {
      "criteriaScores": {
        "Content": 9,
        "Organization": 8,
        "Grammar": 8,
        "Vocabulary": 7
      },
      "isCorrect": true
    }
  }
}
```

**Database AFTER Scoring:**

```sql
AnswerId: 103
Score: 32  -- (9 + 8 + 8 + 7)
IsCorrect: 1
EssayAnswer: {"studentAnswer":"Climate change is one of the most pressing issues of our time. Global temperatures have risen significantly over the past century...","criteriaScores":{"Content":9,"Organization":8,"Grammar":8,"Vocabulary":7}}
```

**Parsed JSON (Frontend):**
```json
{
  "studentAnswer": "Climate change is one of the most pressing issues of our time. Global temperatures have risen significantly over the past century...",
  "criteriaScores": {
    "Content": 9,
    "Organization": 8,
    "Grammar": 8,
    "Vocabulary": 7
  }
}
```

**➡️ Lợi ích:**
- Essay gốc của học sinh được bảo toàn trong `studentAnswer`
- Điểm chi tiết từng tiêu chí lưu trong `criteriaScores`
- Frontend có thể hiển thị cả essay và điểm chi tiết

---

### Case 4: Batch Scoring - Chấm nhiều câu cùng lúc

**Request Body:**
```json
{
  "answerScores": {
    "201": {
      "criteriaScores": {
        "Pronunciation": 8,
        "Fluency": 7
      },
      "isCorrect": true
    },
    "202": {
      "criteriaScores": {
        "Content": 10,
        "Grammar": 9
      },
      "isCorrect": true
    },
    "203": {
      "criteriaScores": {
        "Accuracy": 10,
        "Speed": 8,
        "Technique": 9
      },
      "isCorrect": true
    }
  }
}
```

**Database After Scoring:**

```sql
-- TestAnswer #201
AnswerId: 201
Score: 15  -- (8 + 7)
EssayAnswer: {"criteriaScores":{"Pronunciation":8,"Fluency":7}}

-- TestAnswer #202
AnswerId: 202
Score: 19  -- (10 + 9)
EssayAnswer: {"criteriaScores":{"Content":10,"Grammar":9}}

-- TestAnswer #203
AnswerId: 203
Score: 27  -- (10 + 8 + 9)
EssayAnswer: {"criteriaScores":{"Accuracy":10,"Speed":8,"Technique":9}}

-- TestSession
TestSessionId: 123
TotalScore: 61  -- (15 + 19 + 27)
```

---

## 🔍 Query Patterns

### 1. Lấy tất cả điểm chi tiết
```sql
SELECT 
    AnswerId,
    Score AS TotalScore,
    EssayAnswer AS CriteriaScoresJSON
FROM TestAnswer
WHERE TestSessionId = 123;
```

### 2. Parse JSON trong SQL Server (nếu cần)
```sql
SELECT 
    AnswerId,
    Score,
    JSON_VALUE(EssayAnswer, '$.criteriaScores.Pronunciation') AS PronunciationScore,
    JSON_VALUE(EssayAnswer, '$.criteriaScores.Fluency') AS FluencyScore,
    JSON_VALUE(EssayAnswer, '$.criteriaScores.Grammar') AS GrammarScore
FROM TestAnswer
WHERE AnswerId = 101;
```

### 3. Lấy essay gốc của học sinh
```sql
SELECT 
    AnswerId,
    JSON_VALUE(EssayAnswer, '$.studentAnswer') AS OriginalEssay,
    JSON_QUERY(EssayAnswer, '$.criteriaScores') AS ScoreBreakdown
FROM TestAnswer
WHERE AnswerId = 103;
```

---

## 📊 Visual Representation

```
Input Request:
┌──────────────────────────────────┐
│   PUT /test-sessions/123/score   │
│                                   │
│   {                               │
│     "answerScores": {             │
│       "1": {                      │
│         "criteriaScores": {       │
│           "Pronunciation": 8,     │
│           "Fluency": 7,           │
│           "Grammar": 9            │
│         }                         │
│       }                           │
│     }                             │
│   }                               │
└──────────────────────────────────┘
            ↓
┌──────────────────────────────────┐
│    Processing in Handler          │
│                                   │
│  1. Calculate: 8+7+9 = 24         │
│  2. Serialize to JSON             │
│  3. Save to EssayAnswer           │
└──────────────────────────────────┘
            ↓
┌──────────────────────────────────────────────────────────────┐
│                    Database: TestAnswer                       │
├────────────┬─────────┬────────────┬──────────────────────────┤
│ AnswerId   │ Score   │ IsCorrect  │ EssayAnswer              │
├────────────┼─────────┼────────────┼──────────────────────────┤
│     1      │   24    │    true    │ {"criteriaScores":{...}} │
└────────────┴─────────┴────────────┴──────────────────────────┘
            ↓
┌──────────────────────────────────┐
│    Frontend Parse JSON            │
│                                   │
│  const data = JSON.parse(...)     │
│                                   │
│  Display:                         │
│  ✓ Pronunciation: 8/10            │
│  ✓ Fluency: 7/10                  │
│  ✓ Grammar: 9/10                  │
│  ────────────────                 │
│  Total: 24/30                     │
└──────────────────────────────────┘
```

---

## 🎯 Kết Luận

**Cách lưu trữ:**
- ✅ Trường `EssayAnswer` (nvarchar(max)) lưu JSON string
- ✅ Không cần bảng riêng cho criteria scores
- ✅ Flexible: Số lượng và tên tiêu chí động
- ✅ Dễ query, parse, và hiển thị

**Format JSON trong EssayAnswer:**
```json
// Chỉ có criteria scores
{"criteriaScores":{"Criteria1":8,"Criteria2":9}}

// Có cả essay gốc + criteria scores
{"studentAnswer":"...","criteriaScores":{"Criteria1":8,"Criteria2":9}}
```
