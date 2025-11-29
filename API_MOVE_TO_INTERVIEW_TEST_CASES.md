# API Move To Interview Round - Test Examples

## Endpoint
```
POST /api/v1/round-activities/move-to-interview
```

---

## Test Case 1: Practical Test - Mixed Results

### Initial Database State
```sql
-- Round table
RoundId | RoundType        | TestId | CampaignRoundId
--------|------------------|--------|----------------
10      | PracticalTest(6) | 5      | 3

-- Test table
TestId | TestName          | TestType    | MaxScore
-------|-------------------|-------------|----------
5      | Practical Exam    | Practical(3)| 100

-- RoundActivity table (BEFORE)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
101        | 10      | 201    | Ongoing(1)
102        | 10      | 202    | Ongoing(1)
103        | 10      | 203    | Ongoing(1)

-- TestSession table
SessionId | TestId | UserId | TotalScore | Status (Finalized) | CreatedAt
----------|--------|--------|------------|--------------------|--------------------
501       | 5      | 201    | 85         | true               | 2024-11-20 09:00:00
502       | 5      | 202    | 92         | true               | 2024-11-20 09:15:00
503       | 5      | 203    | 65         | true               | 2024-11-20 09:30:00

-- Interview Round (exists in campaign)
RoundId | RoundType     | TestId | CampaignRoundId
--------|---------------|--------|----------------
11      | Interview(7)  | NULL   | 3

-- User table (for reference)
UserId | FullName       | Email
-------|----------------|--------------------
201    | Nguyen Van A   | nguyenvana@email.com
202    | Tran Thi B     | tranthib@email.com
203    | Le Van C       | levanc@email.com
```

### Request
```http
POST /api/v1/round-activities/move-to-interview
Content-Type: application/json
Authorization: Bearer <admin_token>

{
  "roundId": 10
}
```

### Response
```json
{
  "succeeded": true,
  "code": 0,
  "message": "Processed 3 activities. 2 passed, 1 failed.",
  "data": {
    "roundId": 10,
    "roundType": "PracticalTest",
    "totalProcessed": 3,
    "passedCount": 2,
    "failedCount": 1,
    "results": [
      {
        "activityId": 101,
        "userId": 201,
        "userName": "Nguyen Van A",
        "userEmail": "nguyenvana@email.com",
        "status": "Passed",
        "detail": "PASSED - Moved to Interview (Score: 85/100, Threshold: 80)",
        "testScores": [
          {
            "testSessionId": 501,
            "totalScore": 85,
            "maxScore": 100,
            "percentage": 85.0,
            "passed": true
          }
        ]
      },
      {
        "activityId": 102,
        "userId": 202,
        "userName": "Tran Thi B",
        "userEmail": "tranthib@email.com",
        "status": "Passed",
        "detail": "PASSED - Moved to Interview (Score: 92/100, Threshold: 80)",
        "testScores": [
          {
            "testSessionId": 502,
            "totalScore": 92,
            "maxScore": 100,
            "percentage": 92.0,
            "passed": true
          }
        ]
      },
      {
        "activityId": 103,
        "userId": 203,
        "userName": "Le Van C",
        "userEmail": "levanc@email.com",
        "status": "Failed",
        "detail": "FAILED - Below 80% threshold (Score: 65/100, Threshold: 80)",
        "testScores": [
          {
            "testSessionId": 503,
            "totalScore": 65,
            "maxScore": 100,
            "percentage": 65.0,
            "passed": false
          }
        ]
      }
    ]
  }
}
```

### Database Changes (AFTER API Call)
```sql
-- RoundActivity table (UPDATED)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
101        | 10      | 201    | Passed(2)    ✅ CHANGED: Ongoing → Passed
102        | 10      | 202    | Passed(2)    ✅ CHANGED: Ongoing → Passed
103        | 10      | 203    | Failed(3)    ✅ CHANGED: Ongoing → Failed
104        | 11      | 201    | Ongoing(1)   🆕 CREATED (Interview for User 201)
105        | 11      | 202    | Ongoing(1)   🆕 CREATED (Interview for User 202)

-- Summary of Changes:
-- ✅ 2 RoundActivities updated to Passed
-- ✅ 1 RoundActivity updated to Failed
-- 🆕 2 new Interview RoundActivities created
```

---

## Test Case 2: English Listening Test (First English Round)

### Initial Database State
```sql
-- Rounds
RoundId | RoundType                | TestId | CampaignRoundId
--------|--------------------------|--------|----------------
12      | EnglishListeningTest(4)  | 6      | 4
13      | EnglishSpeakingTest(5)   | 7      | 4
14      | Interview(7)             | NULL   | 4

-- Tests
TestId | TestName            | TestType           | MaxScore
-------|---------------------|--------------------|---------
6      | Listening Test      | EnglishListening(1)| 120
7      | Speaking Test       | EnglishSpeaking(2) | 100

-- RoundActivity (Listening Round - BEFORE)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
201        | 12      | 301    | Ongoing(1)
202        | 12      | 302    | Ongoing(1)
203        | 12      | 303    | Ongoing(1)

-- RoundActivity (Speaking Round - Not started yet)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
204        | 13      | 301    | Ongoing(1)
205        | 13      | 302    | Ongoing(1)
206        | 13      | 303    | Ongoing(1)

-- TestSession (Listening)
SessionId | TestId | UserId | TotalScore | Status | CreatedAt
----------|--------|--------|------------|--------|--------------------
601       | 6      | 301    | 110        | true   | 2024-11-21 10:00:00
602       | 6      | 302    | 102        | true   | 2024-11-21 10:05:00
603       | 6      | 303    | 85         | true   | 2024-11-21 10:10:00

-- Threshold Calculation: 120 * 0.8 = 96
-- User 301: 110 >= 96 ✅ PASS
-- User 302: 102 >= 96 ✅ PASS
-- User 303: 85 < 96 ❌ FAIL

-- User table
UserId | FullName       | Email
-------|----------------|--------------------
301    | Pham Van D     | phamvand@email.com
302    | Hoang Thi E    | hoangthie@email.com
303    | Vu Van F       | vuvanf@email.com
```

### Request
```http
POST /api/v1/round-activities/move-to-interview
Content-Type: application/json
Authorization: Bearer <admin_token>

{
  "roundId": 12
}
```

### Response
```json
{
  "succeeded": true,
  "code": 0,
  "message": "Processed 3 activities. 2 passed, 1 failed.",
  "data": {
    "roundId": 12,
    "roundType": "EnglishListeningTest",
    "totalProcessed": 3,
    "passedCount": 2,
    "failedCount": 1,
    "results": [
      {
        "activityId": 201,
        "userId": 301,
        "userName": "Pham Van D",
        "userEmail": "phamvand@email.com",
        "status": "Passed",
        "detail": "PASSED - Waiting for other English test (Speaking) to complete (Score: 110/120, Threshold: 96)",
        "testScores": [
          {
            "testSessionId": 601,
            "totalScore": 110,
            "maxScore": 120,
            "percentage": 91.67,
            "passed": true
          }
        ]
      },
      {
        "activityId": 202,
        "userId": 302,
        "userName": "Hoang Thi E",
        "userEmail": "hoangthie@email.com",
        "status": "Passed",
        "detail": "PASSED - Waiting for other English test (Speaking) to complete (Score: 102/120, Threshold: 96)",
        "testScores": [
          {
            "testSessionId": 602,
            "totalScore": 102,
            "maxScore": 120,
            "percentage": 85.0,
            "passed": true
          }
        ]
      },
      {
        "activityId": 203,
        "userId": 303,
        "userName": "Vu Van F",
        "userEmail": "vuvanf@email.com",
        "status": "Failed",
        "detail": "FAILED - Below 80% threshold (Score: 85/120, Threshold: 96)",
        "testScores": [
          {
            "testSessionId": 603,
            "totalScore": 85,
            "maxScore": 120,
            "percentage": 70.83,
            "passed": false
          }
        ]
      }
    ]
  }
}
```

### Database Changes (AFTER API Call)
```sql
-- RoundActivity (Listening Round - UPDATED)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
201        | 12      | 301    | Passed(2)    ✅ CHANGED: Ongoing → Passed (waiting for Speaking)
202        | 12      | 302    | Passed(2)    ✅ CHANGED: Ongoing → Passed (waiting for Speaking)
203        | 12      | 303    | Failed(3)    ✅ CHANGED: Ongoing → Failed

-- RoundActivity (Speaking Round - NO CHANGE)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
204        | 13      | 301    | Ongoing(1)   ⏳ NO CHANGE (waiting for test)
205        | 13      | 302    | Ongoing(1)   ⏳ NO CHANGE (waiting for test)
206        | 13      | 303    | Ongoing(1)   ⏳ NO CHANGE (will likely fail anyway)

-- NO Interview RoundActivities created yet! ❌
-- Reason: Waiting for Speaking test to be completed and evaluated

-- Summary:
-- ✅ 2 users passed Listening, waiting for Speaking
-- ❌ 1 user failed Listening (eliminated from campaign)
-- ⏳ No Interview activities yet (need Speaking results first)
```

---

## Test Case 3: English Speaking Test (Both Tests Completed)

### Initial Database State (After Listening Test in Test Case 2)
```sql
-- RoundActivity (Listening - Already Passed)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
201        | 12      | 301    | Passed(2)
202        | 12      | 302    | Passed(2)
203        | 12      | 303    | Failed(3)

-- RoundActivity (Speaking - BEFORE this API call)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
204        | 13      | 301    | Ongoing(1)
205        | 13      | 302    | Ongoing(1)
206        | 13      | 303    | Ongoing(1)

-- TestSession (Speaking - NEW)
SessionId | TestId | UserId | TotalScore | Status | CreatedAt
----------|--------|--------|------------|--------|--------------------
701       | 7      | 301    | 85         | true   | 2024-11-22 14:00:00
702       | 7      | 302    | 82         | true   | 2024-11-22 14:10:00
703       | 7      | 303    | 70         | true   | 2024-11-22 14:20:00

-- Threshold Calculation: 100 * 0.8 = 80
-- User 301: 85 >= 80 ✅ PASS + Listening PASSED → Interview ✅
-- User 302: 82 >= 80 ✅ PASS + Listening PASSED → Interview ✅
-- User 303: 70 < 80 ❌ FAIL + Listening FAILED → No Interview ❌
```

### Request
```http
POST /api/v1/round-activities/move-to-interview
Content-Type: application/json
Authorization: Bearer <admin_token>

{
  "roundId": 13
}
```

### Response
```json
{
  "succeeded": true,
  "code": 0,
  "message": "Processed 3 activities. 2 passed, 1 failed.",
  "data": {
    "roundId": 13,
    "roundType": "EnglishSpeakingTest",
    "totalProcessed": 3,
    "passedCount": 2,
    "failedCount": 1,
    "results": [
      {
        "activityId": 204,
        "userId": 301,
        "userName": "Pham Van D",
        "userEmail": "phamvand@email.com",
        "status": "Passed",
        "detail": "PASSED - Both English tests passed, moved to Interview (Score: 85/100, Threshold: 80)",
        "testScores": [
          {
            "testSessionId": 701,
            "totalScore": 85,
            "maxScore": 100,
            "percentage": 85.0,
            "passed": true
          }
        ]
      },
      {
        "activityId": 205,
        "userId": 302,
        "userName": "Hoang Thi E",
        "userEmail": "hoangthie@email.com",
        "status": "Passed",
        "detail": "PASSED - Both English tests passed, moved to Interview (Score: 82/100, Threshold: 80)",
        "testScores": [
          {
            "testSessionId": 702,
            "totalScore": 82,
            "maxScore": 100,
            "percentage": 82.0,
            "passed": true
          }
        ]
      },
      {
        "activityId": 206,
        "userId": 303,
        "userName": "Vu Van F",
        "userEmail": "vuvanf@email.com",
        "status": "Failed",
        "detail": "FAILED - Below 80% threshold (Score: 70/100, Threshold: 80)",
        "testScores": [
          {
            "testSessionId": 703,
            "totalScore": 70,
            "maxScore": 100,
            "percentage": 70.0,
            "passed": false
          }
        ]
      }
    ]
  }
}
```

### Database Changes (AFTER API Call)
```sql
-- RoundActivity (Listening - NO CHANGE)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
201        | 12      | 301    | Passed(2)    (unchanged)
202        | 12      | 302    | Passed(2)    (unchanged)
203        | 12      | 303    | Failed(3)    (unchanged)

-- RoundActivity (Speaking - UPDATED)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
204        | 13      | 301    | Passed(2)    ✅ CHANGED: Ongoing → Passed
205        | 13      | 302    | Passed(2)    ✅ CHANGED: Ongoing → Passed
206        | 13      | 303    | Failed(3)    ✅ CHANGED: Ongoing → Failed

-- RoundActivity (Interview - CREATED) 🆕
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
207        | 14      | 301    | Ongoing(1)   🆕 CREATED (Both English tests passed!)
208        | 14      | 302    | Ongoing(1)   🆕 CREATED (Both English tests passed!)

-- Summary:
-- ✅ User 301: Listening ✅ + Speaking ✅ → Interview Created 🎉
-- ✅ User 302: Listening ✅ + Speaking ✅ → Interview Created 🎉
-- ❌ User 303: Listening ❌ + Speaking ❌ → Eliminated from campaign
```

---

## Test Case 4: Speaking Test When Listening Failed

### Initial Database State
```sql
-- RoundActivity (Listening - FAILED)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
301        | 12      | 401    | Failed(3)

-- RoundActivity (Speaking - BEFORE)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
302        | 13      | 401    | Ongoing(1)

-- TestSession (Listening - Failed)
SessionId | TestId | UserId | TotalScore | Status | CreatedAt
----------|--------|--------|------------|--------|--------------------
801       | 6      | 401    | 70         | true   | 2024-11-21 10:00:00

-- TestSession (Speaking - Passed)
SessionId | TestId | UserId | TotalScore | Status | CreatedAt
----------|--------|--------|------------|--------|--------------------
802       | 7      | 401    | 90         | true   | 2024-11-22 14:00:00

-- User table
UserId | FullName       | Email
-------|----------------|--------------------
401    | Nguyen Van G   | nguyenvang@email.com
```

### Request
```http
POST /api/v1/round-activities/move-to-interview
Content-Type: application/json
Authorization: Bearer <admin_token>

{
  "roundId": 13
}
```

### Response
```json
{
  "succeeded": true,
  "code": 0,
  "message": "Processed 1 activities. 1 passed, 0 failed.",
  "data": {
    "roundId": 13,
    "roundType": "EnglishSpeakingTest",
    "totalProcessed": 1,
    "passedCount": 1,
    "failedCount": 0,
    "results": [
      {
        "activityId": 302,
        "userId": 401,
        "userName": "Nguyen Van G",
        "userEmail": "nguyenvang@email.com",
        "status": "Passed",
        "detail": "PASSED - Waiting for other English test (Listening) to complete (Score: 90/100, Threshold: 80)",
        "testScores": [
          {
            "testSessionId": 802,
            "totalScore": 90,
            "maxScore": 100,
            "percentage": 90.0,
            "passed": true
          }
        ]
      }
    ]
  }
}
```

### Database Changes (AFTER API Call)
```sql
-- RoundActivity (Listening - NO CHANGE)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
301        | 12      | 401    | Failed(3)    (unchanged - already failed)

-- RoundActivity (Speaking - UPDATED)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
302        | 13      | 401    | Passed(2)    ✅ CHANGED: Ongoing → Passed

-- NO Interview RoundActivity created! ❌
-- Reason: Listening test status is Failed, not Passed

-- Summary:
-- ✅ User passed Speaking (90/100)
-- ❌ But Listening was Failed (70/120)
-- ❌ Cannot move to Interview (requires BOTH tests to pass)
-- 💡 Detail message says "Waiting" but actually Listening already Failed
```

---

## Test Case 5: Multiple Test Sessions Per User

### Initial Database State
```sql
-- Practical Round
RoundId | RoundType        | TestId | CampaignRoundId
--------|------------------|--------|----------------
15      | PracticalTest(6) | 8      | 5

-- Test
TestId | TestName          | TestType    | MaxScore
-------|-------------------|-------------|----------
8      | Practical Final   | Practical(3)| 150

-- RoundActivity (BEFORE)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
401        | 15      | 501    | Ongoing(1)

-- TestSession (Multiple sessions - user retook test)
SessionId | TestId | UserId | TotalScore | Status | CreatedAt
----------|--------|--------|------------|--------|--------------------
901       | 8      | 501    | 90         | true   | 2024-11-23 08:00:00 (1st attempt)
902       | 8      | 501    | 110        | true   | 2024-11-23 10:00:00 (2nd attempt)
903       | 8      | 501    | 135        | true   | 2024-11-23 14:00:00 (3rd attempt)

-- Threshold: 150 * 0.8 = 120
-- Session 901: 90 < 120 ❌
-- Session 902: 110 < 120 ❌
-- Session 903: 135 >= 120 ✅
-- Logic: ALL sessions must pass (>= 80%)
```

### Request
```http
POST /api/v1/round-activities/move-to-interview
Content-Type: application/json
Authorization: Bearer <admin_token>

{
  "roundId": 15
}
```

### Response
```json
{
  "succeeded": true,
  "code": 0,
  "message": "Processed 1 activities. 0 passed, 1 failed.",
  "data": {
    "roundId": 15,
    "roundType": "PracticalTest",
    "totalProcessed": 1,
    "passedCount": 0,
    "failedCount": 1,
    "results": [
      {
        "activityId": 401,
        "userId": 501,
        "userName": "Le Thi H",
        "userEmail": "lethih@email.com",
        "status": "Failed",
        "detail": "FAILED - Not all test sessions passed threshold (2 out of 3 sessions below 80%)",
        "testScores": [
          {
            "testSessionId": 901,
            "totalScore": 90,
            "maxScore": 150,
            "percentage": 60.0,
            "passed": false
          },
          {
            "testSessionId": 902,
            "totalScore": 110,
            "maxScore": 150,
            "percentage": 73.33,
            "passed": false
          },
          {
            "testSessionId": 903,
            "totalScore": 135,
            "maxScore": 150,
            "percentage": 90.0,
            "passed": true
          }
        ]
      }
    ]
  }
}
```

### Database Changes (AFTER API Call)
```sql
-- RoundActivity (UPDATED)
ActivityId | RoundId | UserId | Status
-----------|---------|--------|-------------
401        | 15      | 501    | Failed(3)    ✅ CHANGED: Ongoing → Failed

-- NO Interview RoundActivity created! ❌
-- Reason: Not ALL test sessions passed the 80% threshold

-- Summary:
-- ❌ User failed because 2 out of 3 sessions were below threshold
-- 💡 Logic requires ALL sessions to pass, not just the latest one
```

---

## Error Cases

### Error Case 1: Round Not Found
```http
POST /api/v1/round-activities/move-to-interview
Content-Type: application/json
Authorization: Bearer <admin_token>

{
  "roundId": 999
}
```

**Response:**
```json
{
  "succeeded": false,
  "code": 400,
  "message": "Round with ID 999 not found"
}
```

### Error Case 2: Invalid Round Type (Not a Test Round)
```http
POST /api/v1/round-activities/move-to-interview
Content-Type: application/json

{
  "roundId": 1
}
```

**Response:**
```json
{
  "succeeded": false,
  "code": 400,
  "message": "Round 1 is not a test round. Must be EnglishListeningTest, EnglishSpeakingTest, or PracticalTest"
}
```

### Error Case 3: Test Sessions Not Finalized
```sql
-- TestSession with Status = false (not scored yet)
SessionId | TestId | UserId | TotalScore | Status | CreatedAt
----------|--------|--------|------------|--------|--------------------
1001      | 5      | 601    | 0          | false  | 2024-11-24 09:00:00
```

**Response:**
```json
{
  "succeeded": true,
  "code": 0,
  "message": "Processed 1 activities. 0 passed, 1 failed.",
  "data": {
    "results": [
      {
        "userId": 601,
        "status": "Failed",
        "detail": "FAILED - Not all test sessions are finalized/scored"
      }
    ]
  }
}
```

---

## Summary of Logic

### Pass Criteria
```
1. All test sessions must have Status = true (finalized)
2. All test sessions must have TotalScore >= MaxScore * 0.8

For Practical:
  IF all sessions pass THEN
    Status = Passed
    Create Interview RoundActivity
  END IF

For English (Listening or Speaking):
  IF all sessions pass THEN
    Status = Passed
    IF other English test also has Status = Passed THEN
      Create Interview RoundActivity
    ELSE
      Wait for other test
    END IF
  END IF
```

### Key Points
✅ Batch processing - xử lý tất cả users trong round cùng lúc
✅ Practical test - chỉ cần 1 round pass
✅ English tests - cần CẢ Listening VÀ Speaking pass
✅ Multiple sessions - TẤT CẢ phải >= 80%
✅ Transaction safety - rollback nếu có lỗi
