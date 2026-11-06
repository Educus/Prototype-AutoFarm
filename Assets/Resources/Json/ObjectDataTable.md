# ObjectDataTable

| 컬럼 | 타입 | 설명 |
|---|---|---|
| ID | ID · Int |  |
| Name | String |  |
| Type | Enum: PC, NPC, Cattle |  |
| MaxEnergy | Int |  |
| MainInv | Int | 메인 인벤토리(수확한 아이템 저장소) 슬롯 수 |
| SubInv | Int | 서브 인벤토리(씨앗과 같은 생산 재료) 슬롯 수 |
| Speed | Int | 이동 속도 |
| WorkDuration | Int | 작업 속도 |
| DailyYield | Int | 하루 최대 수확량(가축 한정) |
| YieldCoolDown | Int | 한 번 수확 이후, 다음 수확까지의 쿨 타임 |
| YieldMaterial | Int | 어떤 상품으로 생산하는 지(재료 아이템 ID값으로) |
| YieldItem | Int | 어떤 상품을 생산하는 지(상품 아이템 ID값으로) |
