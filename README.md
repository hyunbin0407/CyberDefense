# CyberDefense - 타워 디펜스 게임

## 개발 일지

| 일차 | 날짜 | 작업 내용 |
|------|------|-----------|
| 1일차 | 2026-07-29 | 개발 환경 구성 (Unity 프로젝트 세팅) |
| 2일차 | 2026-07-30 | 적 공격 및 사망 시스템 구현 |
| 3일차 | 2026-07-31 | 크레딧/서버 체력 증감 및 HUD 실시간 표시 구현 |
| 4일차 | 2026-08-01 | 방화벽 타워 건설(클릭 배치) 기능 구현 및 버그 수정 |
| 5일차 | 2026-08-02 | 승리/패배 결과 UI(GameOverUI) 구현 |
| 6일차 | 2026-08-03 | 안티바이러스 타워 추가 및 설치 버그 수정 |
| 7일차 | 2026-08-04 | IDS/IPS·허니팟 타워, 제로데이·DDoS봇 적 추가 및 UI 버그 수정 |
| 8일차 | 2026-08-05 | 웨이브 카운트다운 UI 구현 및 HUD 클릭 차단 버그 수정 |
| 9일차 | 2026-08-09 | 타워 공격/피격/사망 이펙트 연출 추가 |
| 10일차 | 2026-08-10 | 프로시저럴 효과음(타워 발사/건설, 적 사망) 구현 |
| 11일차 | 2026-08-12 | 경로 셀 자동 계산 및 런타임 그리드 시각화 구현 |
| 12일차 | 2026-08-13 | 타워 클릭 선택 및 업그레이드 시스템 구현 |
| 13일차 | 2026-08-14 | 메인 메뉴/상점/인벤토리/맵·난이도 선택 흐름 구현 |
| 14일차 | 2026-08-15 | 메인 메뉴 UI 잘림/겹침 버그 수정, 상점 코인 표시 추가 |
| 15일차 | 2026-08-16 | 게임 종료 후 뒤로가기 버튼 추가, 방화벽 타워 아트 적용 |

---


사이버 보안을 테마로 한 Unity 2D 타워 디펜스 게임입니다.
방화벽, 침입 탐지 시스템 등의 타워로 웜, 랜섬웨어 등의 악성코드(적)가 서버에 도달하는 것을 막는 것이 목표입니다.

---

## 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs         # 게임 전체 상태 관리
│   │   └── GridManager.cs         # 타워 배치용 격자 관리
│   ├── Data/
│   │   ├── TowerData.cs           # 타워 스탯 ScriptableObject
│   │   └── EnemyData.cs           # 적 스탯 ScriptableObject
│   ├── Economy/
│   │   └── CurrencyManager.cs     # 재화(사이버 크레딧) 관리
│   ├── Enemies/
│   │   └── EnemyController.cs     # 적 이동/피격/사망 처리
│   ├── Towers/
│   │   └── TowerController.cs     # 타워 감지/공격/업그레이드 처리
│   ├── UI/
│   │   └── TowerPlacementController.cs  # 터치/클릭으로 타워 배치
│   └── Waves/
│       └── WaveSpawner.cs         # 웨이브별 적 스폰 관리
├── Firewall_Data.asset            # 방화벽 타워 데이터
├── Worm_Data.asset                # 웜 적 데이터
├── Firewall_Tower.prefab          # 방화벽 타워 프리팹
└── Worm_Enemy.prefab              # 웜 적 프리팹
```

---

## 스크립트 설명

### GameManager.cs
`Core/GameManager.cs`

게임 전체 상태를 관리하는 싱글톤입니다.

**게임 상태 (GameState)**
| 상태 | 설명 |
|------|------|
| `Prepare` | 웨이브 시작 전 타워 배치 단계 |
| `Playing` | 웨이브 진행 중 |
| `Paused` | 일시정지 (`Time.timeScale = 0`) |
| `Victory` | 모든 웨이브 클리어 |
| `Defeat` | 서버 체력 0 → 패배 |

**주요 메서드**
```csharp
GameManager.Instance.DamageServer(int damage)   // 서버 체력 감소 (적이 도달 시 호출)
GameManager.Instance.SetState(GameState state)  // 상태 전환
GameManager.Instance.TogglePause()              // 일시정지/재개
GameManager.Instance.RestartLevel()             // 씬 재시작
```

---

### GridManager.cs
`Core/GridManager.cs`

타워 배치를 위한 격자(그리드)를 관리하는 싱글톤입니다.

**Inspector 설정**
- `width` / `height`: 그리드 크기 (기본 12 × 8)
- `cellSize`: 셀 하나의 크기 (기본 1)
- `originPosition`: 그리드 시작 좌표
- `pathCells`: 적 이동 경로 셀 목록 (타워 배치 불가)

**주요 메서드**
```csharp
GridManager.Instance.WorldToCell(Vector3 pos)       // 월드 좌표 → 셀 좌표
GridManager.Instance.CellToWorld(Vector2Int cell)   // 셀 좌표 → 월드 좌표 (셀 중앙)
GridManager.Instance.CanPlaceTower(Vector2Int cell) // 타워 배치 가능 여부
GridManager.Instance.OccupyCell(Vector2Int cell)    // 셀 점유 표시
GridManager.Instance.FreeCell(Vector2Int cell)      // 셀 점유 해제
```

> **Gizmo**: Scene 뷰에서 파란색 = 그리드 전체, 노란색 = 경로 셀로 표시됩니다.

---

### TowerData.cs
`Data/TowerData.cs`

타워 종류별 스탯을 정의하는 ScriptableObject입니다.
`Project 창 우클릭 → Create → CyberDefense → Tower Data` 로 생성합니다.

**타워 종류 (TowerType)**
| 타입 | 이름 | 특징 |
|------|------|------|
| `Firewall` | 방화벽 | 기본 단일 공격 |
| `IDSIPS` | 침입 탐지/차단 | 은신(제로데이) 탐지 가능 |
| `Antivirus` | 안티바이러스 | 광역 공격 |
| `Honeypot` | 허니팟 | 적 유인/지연 |
| `WAF` | 웹 방화벽 | 고위협 대상 고데미지 |

**주요 필드**
```csharp
float damage            // 기본 데미지
float attackRange       // 공격 사거리
float attackCooldown    // 공격 간격 (초)
bool isAreaAttack       // true면 범위 공격
float areaRadius        // 범위 공격 반경
bool canDetectStealth   // 은신 적 감지 여부
int buildCost           // 건설 비용
int upgradeCost         // 업그레이드 비용 (레벨 × upgradeCost)
int maxLevel            // 최대 레벨 (1~5)
float upgradeDamageMultiplier // 레벨업마다 곱해지는 데미지 배율
```

---

### EnemyData.cs
`Data/EnemyData.cs`

적 종류별 스탯을 정의하는 ScriptableObject입니다.
`Project 창 우클릭 → Create → CyberDefense → Enemy Data` 로 생성합니다.

**적 종류 (EnemyType)**
| 타입 | 이름 | 특징 |
|------|------|------|
| `Worm` | 웜 | 빠르고 약함, 다수 등장 |
| `Ransomware` | 랜섬웨어 | 체력 높고 느림 |
| `ZeroDay` | 제로데이 | 은신(탐지 타워 필요) |
| `DDoSBot` | DDoS 봇 | 매우 빠름, 낮은 체력 |

**주요 필드**
```csharp
float maxHealth         // 최대 체력
float moveSpeed         // 이동 속도
int damageToServer      // 서버 도달 시 입히는 피해
int rewardOnKill        // 처치 시 지급 재화
bool isStealth          // 은신 여부 (canDetectStealth 타워만 공격 가능)
float damageResistance  // 데미지 감소율 (0~0.9)
```

---

### CurrencyManager.cs
`Economy/CurrencyManager.cs`

게임 내 재화(사이버 크레딧)를 관리하는 싱글톤입니다.

**주요 메서드**
```csharp
CurrencyManager.Instance.CanAfford(int amount)   // 잔액 충분 여부 확인
CurrencyManager.Instance.TrySpend(int amount)    // 재화 지불 (부족하면 false 반환)
CurrencyManager.Instance.AddCurrency(int amount) // 재화 획득 (적 처치 보상 등)
```

**이벤트**
```csharp
CurrencyManager.Instance.OnCurrencyChanged += (newAmount) => { /* UI 갱신 등 */ };
```

---

### EnemyController.cs
`Enemies/EnemyController.cs`

적 한 마리의 이동, 피격, 사망을 처리합니다.

**동작 흐름**
1. `WaveSpawner`가 적을 스폰할 때 `Initialize(data, path)` 호출
2. `FixedUpdate`에서 `rb.MovePosition()`으로 웨이포인트를 순서대로 이동
3. 마지막 웨이포인트 도달 시 `ReachServer()` → 서버 데미지 후 사망
4. `TakeDamage()` 호출 시 저항력 계산 후 체력 감소 → 0 이하면 `Die()`

**주요 프로퍼티**
```csharp
EnemyData Data          // 적 스탯 데이터
float CurrentHealth     // 현재 체력
bool IsDead             // 사망 여부
bool IsStealth          // 은신 여부
int WaypointIndex       // 현재 목표 웨이포인트 인덱스 (경로 진행도)
```

**이벤트**
```csharp
enemy.OnDeath += (enemy) => { /* 처치 보상 처리 등 */ };
enemy.OnReachedServer += (enemy) => { /* 서버 피해 처리 등 */ };
```

> **물리 설정**: 반드시 `Rigidbody2D (Kinematic, Use Full Kinematic Contacts: ON, Gravity Scale: 0)` 이 있어야 타워의 트리거 감지가 정상 동작합니다.

---

### TowerController.cs
`Towers/TowerController.cs`

타워의 감지, 공격, 업그레이드를 처리합니다.

**동작 흐름**
1. `Awake()`에서 `CircleCollider2D (trigger)` 자동 생성
2. `Initialize(data, cell)` 호출 시 트리거 반경을 `attackRange`로 설정
3. 적이 사거리에 들어오면 `OnTriggerEnter2D` → `enemiesInRange` 리스트에 추가
4. 적이 사거리에서 나가면 `OnTriggerExit2D` → 리스트에서 제거
5. `Update`에서 매 프레임 최적 타겟 선택 → `attackCooldown` 경과 시 공격

**타겟 우선순위**: 서버에 가장 가까운 적(경로를 가장 많이 진행한 적) 우선

**Inspector 필드**
```
TowerData data      // 타워 스탯 (씬에 직접 배치 시 할당)
Transform firePoint // 발사 위치 (선택)
LineRenderer attackLine // 공격 라인 연출 (선택)
```

**주요 메서드**
```csharp
tower.Initialize(TowerData data, Vector2Int cell) // 타워 초기화 (배치 시 호출)
tower.TryUpgrade()     // 업그레이드 시도 (재화는 외부에서 차감)
tower.GetUpgradeCost() // 현재 레벨 업그레이드 비용 반환
```

---

### TowerPlacementController.cs
`UI/TowerPlacementController.cs`

터치(모바일) 또는 마우스 클릭으로 타워를 배치합니다.

**사용법**
1. UI 버튼에서 `SelectTowerToBuild(TowerData data)` 호출 → 배치할 타워 선택
2. 화면 터치/클릭 → 해당 그리드 셀에 타워 건설
3. `CancelPlacement()` 호출 → 배치 취소

**배치 조건**: 그리드 범위 내 + 경로 셀 아님 + 미점유 + 재화 충분

**Preview 색상**
- 초록색: 배치 가능
- 빨간색: 배치 불가

---

### WaveSpawner.cs
`Waves/WaveSpawner.cs`

웨이브 정의에 따라 순서대로 적을 스폰합니다.

**Inspector 설정**
- `waves`: 웨이브 목록. 각 웨이브에 적 종류/수/스폰 간격 설정
- `pathWaypoints`: 적 이동 경로 웨이포인트 목록
- `spawnPoint`: 적 스폰 위치

**웨이브 흐름**
```
웨이브 시작 전 대기 (delayBeforeWave 초)
→ GameState: Prepare → Playing
→ 적 스폰 (종류별로 spawnInterval 간격)
→ 모든 적 처치/도달 대기
→ 다음 웨이브 반복
→ 전부 완료 시 GameManager.NotifyAllWavesCleared()
```

**웨이브 데이터 구조**
```csharp
// WaveDefinition (웨이브 1개)
waveName         // 웨이브 이름
delayBeforeWave  // 이전 웨이브 종료 후 대기 시간 (초)
enemies          // 스폰할 적 목록 (EnemySpawnEntry)

// EnemySpawnEntry (적 종류 1개)
enemyData        // 어떤 적인지
count            // 몇 마리 스폰할지
spawnInterval    // 각 적 사이 스폰 간격 (초)
```

---

## 주요 데이터 흐름

```
[UI 버튼] → TowerPlacementController.SelectTowerToBuild()
         → 화면 터치 → TryPlaceTower()
         → TowerController.Initialize()
         → CircleCollider2D(trigger) 사거리 설정

[WaveSpawner] → SpawnEnemy()
             → EnemyController.Initialize()
             → FixedUpdate: rb.MovePosition() 으로 경로 이동

[적이 사거리 진입] → TowerController.OnTriggerEnter2D()
                 → enemiesInRange 리스트 추가
                 → Update: attackCooldown 경과 시 Attack()
                 → EnemyController.TakeDamage()
                 → 체력 0 → Die() → CurrencyManager.AddCurrency()

[적이 서버 도달] → EnemyController.ReachServer()
               → GameManager.DamageServer()
               → 서버 체력 0 → GameState.Defeat
```

---

## 새 타워/적 추가 방법

### 새 타워 추가
1. `Project → Create → CyberDefense → Tower Data` 로 데이터 에셋 생성
2. 타워 프리팹 생성 후 `TowerController` 컴포넌트 추가
3. `TowerType` 열거형에 새 타입 추가 (선택)
4. UI 버튼에서 `SelectTowerToBuild(새TowerData)` 연결

### 새 적 추가
1. `Project → Create → CyberDefense → Enemy Data` 로 데이터 에셋 생성
2. 적 프리팹에 `EnemyController`, `CircleCollider2D`, `Rigidbody2D (Kinematic, Use Full Kinematic Contacts: ON)` 추가
3. `WaveSpawner`의 waves 목록에 새 적 등록
