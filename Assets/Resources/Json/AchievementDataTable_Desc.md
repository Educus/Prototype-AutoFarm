# AchievementDataTable

| 컬럼 | 타입 | 설명 |
|---|---|---|
| ID | ID · Int | 10 1 00 0 (어떤 종류인지 / 세부 종류 / 분류 / 동일하지만 연속 도전과제인 경우 횟수 구분) |
| Name | String |  |
| Type | Enum: Plant, Harvest, Sell, Buy |  |
| Target_Count | Int | 도전과제 달성까지 횟수 |
| Compensation_Type | Enum: UgradeParts, Gold | 어떤 보상인지 |
| Compensaion_Amount | Int | 얼마나 주는지 |
