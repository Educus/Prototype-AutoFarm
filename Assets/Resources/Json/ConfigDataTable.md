# ConfigDataTable

| 컬럼 | 타입 | 설명 |
|---|---|---|
| UsedBatteryPerWork | Int | 행동 마다 사용되는 베터리 용량 |
| RealTimePerDay | Int | 게임 내 하루의 실제 시간(초 단위) |
| DaysPerYear | Int | 게임 기준 1년 |
| DaysPerMonth | Int | 게임 기준 1달 |
| RocketSlotAmount | Int | 로켓 용량 |
| RocketDeliveryTime | Int | 로켓 이동 시간(초 단위) |
| RocketLoadingTime | Int | 로켓 상하차 시간(초 단위) |
| BadEndGraceDays | Int | 게임 오버 유예 기간(게임 내 일 단위) |
| GraceInterest | Float | 유예 기간 이자 비율 |
| MarketOpenHour | Int | 장 시작 시간(게임 내 시간 단위) |
| MarketCloseHour | Int | 장 마감 시간(게임 내 시간 단위) |
| EventProb | Float | 이벤트 발생 확률 |
| RumorProb | Float | 소문 발생 확률 |
| RumorStdMul | Float | 소문 적용 시 표준편차 변동 비율 |
| FollowUpDecay | Float | 후속 이벤트 발생 확률(0.7^연속 일수) |
| FollowUpOverMeanMul | Float | 목표 미달 시 반전될 확률 |
| EmgProb | Float | 긴급 이벤트 발생 확률 |
| EmgMinDays | Int | 긴급 이벤트 최소 진행 기간 |
| EmgMaxDays | Int | 긴급 이벤트 최대 진행 기간 |
| EmgCoolDownDays | Int | 긴급 이벤트 재 발생 쿨 타임 |
| EmgAfterLockReachDays | Int | 긴급 이벤트 목표 달성 종료 시,  평균 회귀 유예 최소 기간 |
| EmgAfterLockExpireDays | Int | 긴급 이벤트 기간 만료 종료 시, 평균 회귀 유예 최소 기간 |
| EmgLockDecay | Float | 유예 최소 기간 이후 유예 유지 확률 (0.7 ^ 연속 일수) |
| EmgVirtualLineMul | Float | 긴급 이벤트 가상 선 강도 (기존 지지/저항 선의 0.3배) |
| EmgVirtualLineKeepDays | Int | 가상선 유지 기간 |
| RegressionRangeMin | Float | 평균회귀 목표 예외 범위(0.5~2배를 벗어난 경우엔 원가를 목표로) |
| RegressionRangeMax | Float | 평균회귀 목표 예외 범위(0.5~2배를 벗어난 경우엔 원가를 목표로) |
| LineSampleDays | Int | 지지/저항선 표본 |
| LineSettleDays | Int | 지지/저항선 형성 주기 |
| LineBinSigma | Float | 28일 표본 중, 비슷한 가격대 끼리 묶는 배수 |
| LineMinSamples | Int | 선이 형성되기 위한 최소한의 표본 개수 |
| LineBlockDecay | Float | 지지/저항선이 한 번 부딪힐 때 줄어드는 강도 비율 |
| LineQuietPower | Float | 이벤트가 없는 날의 고정 돌파 강도 |
| TaxPeriodDays | Int | 세금 지출 주기 |
| EndingGoalMoney | Int | 굿 엔딩 기준 금액 |
| StartingPotatoSeed | Int | 시작 지원 아이템 : 씨 감자 |
| StartingPotato | Int | 시작 지원 아이템 : 감자 |
| StartingNPC | Int | 시작 지원 아이템 : 기본 NPC 로봇 |
| StartingMoney | Int | 시작 지원 아이템 : 정착 지원금 |
| ExpireWarningDays | Int | 유통기한 경고 일수 |
| TimeScaleStep | Float | 시간 배수 증가 배율 (0.5씩 증가) |
| TimeScaleMax | Float | 최대 시간 배수 증가 수치 |
| UITimeStepMinutes | Int | UI 시간 표시 최소 단위(10분단위) |
| FieldMaxExpand | Int | 각 방향 별 필드 확장 최대 횟수 |
