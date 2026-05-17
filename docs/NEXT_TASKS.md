# 다음 작업 목록

> **기준일:** 2026-05-15  
> **프로젝트:** 2D_RPG_Portfolio  
> **관련 문서:** [PORTFOLIO_PHASES_NOTION.md](./PORTFOLIO_PHASES_NOTION.md), 루트 [README.md](../README.md)

---

## 오늘까지 완료한 것 (요약)

### 서버
- [x] DB 제거, 인메모리 `PlayerManager` / `MonsterManager` / `Zone` / `ZoneManager`
- [x] `CreatureStat` 공통 스탯, `ObjectIdGenerator`
- [x] 로그인 (`C_S_LOGIN` / `S_C_LOGIN`)
- [x] 입장 (`C_S_ENTER_GAME` → 기존 유저에게 `S_C_SPAWN`, 본인에게 `S_C_ENTER_GAME` + 전체 스폰 목록)
- [x] 퇴장 (`C_S_LEAVE_GAME` → `S_C_DESPAWN` 브로드캐스트 + `S_C_LEAVE_GAME`)
- [x] 존 기본 몬스터 3마리 스폰 (Slime×2, Goblin×1)
- [x] `Zone::Broadcast` + 공유 `SendBuffer` 브로드캐스트
- [x] disconnect 시 `Logout` → `LeaveGame` + 세션 정리

### 클라이언트
- [x] `@Scripts` / `@Scenes` 매니저 구조, Addressables Preload
- [x] Title: Preload → TCP 연결 → `UI_LoginPopup` 로그인 → GameScene
- [x] `TitleScene` / `UI_TitleScene` 네트워크 이벤트 역할 분리
- [x] GameScene 진입 시 `C_S_ENTER_GAME`, `ObjectManager` 스폰/디스폰
- [x] 앱 종료·씬 이탈 시 `LeaveGameAndDisconnect`
- [x] `PlayerObject` / `MonsterObject` / `BaseObject`
- [x] 윈도우 빌드 창 모드 1280×720
- [x] `OrthographicCameraScaler` (해상도 변경 시 캐릭터 화면 크기 유지)
- [x] 카메라 MSAA on

### 기타
- [x] proto `S_C_DESPAWN` 추가, `GenPackets.bat` 연동
- [x] `.gitignore`에 `.cursor/` 추가

---

## P0 — 다음 세션 최우선 (게임플레이 + 데모)

### 1. 이동 동기화 (가장 먼저)
- [ ] proto `C_S_MOVE` / `S_C_MOVE` 필드 확정 (objectId, posX, posY, state 등 — `Struct.proto`와 맞출 것)
- [ ] 서버 `Handle_C_S_MOVE`: Zone 내 플레이어 위치 갱신 + `S_C_MOVE` 브로드캐스트
- [ ] 클라 `UI_Joystick` / 입력 → `C_S_MOVE` 전송
- [ ] 클라 `S_C_MOVEHandler`: 다른 유저·몬스터(필요 시) 위치 보간/즉시 반영
- [ ] **내 캐릭터**는 로컬 이동 + 서버 보정(간단히 서버 권위 또는 클라 예측 중 하나 선택)

### 2. 카메라 & 조작감
- [ ] `Main Camera`가 `Managers.Game.MyPlayer` 추적 (2D follow, 부드러운 lerp)
- [ ] 조이스틱·이동 속도 튜닝 (월드 단위 상수 1곳에서 관리)

### 3. 크리쳐 비주얼 (플레이스홀더 → 실제 에셋)
- [ ] Player / Monster **실제 스프라이트** Import (Pixels Per Unit 통일, 예: 16 또는 32)
- [ ] 프리팹 `Transform.localScale` → `(1,1,1)`, 크기는 PPU로 조절
- [ ] Addressables Preload 라벨·키(`Player`, `Monster`) 유지 확인
- [ ] (의도 유지) 프리팹 없으면 `Debug.LogError` — placeholder 자동 생성 없음

### 4. 연결/로그인 UX
- [ ] 연결 실패·로그인 실패 시 **Toast 또는 StatusText** + 재시도 버튼
- [ ] 로그인 실패 후 `UI_LoginPopup` 버튼 다시 활성화
- [ ] (선택) 접속 상태 enum: `Disconnected → Connecting → Connected → LoggedIn → InGame`

### 5. 멀티 데모 확인
- [ ] GameServer 2클라: A 입장 → B 입장 → 서로 보임
- [ ] A 퇴장/종료 → B 화면에서 A `S_C_DESPAWN`
- [ ] A 재접속(타이틀 로그인) 정상
- [ ] 창 크기 변경 시 캐릭터 크기 일정 (`OrthographicCameraScaler` 동작 확인)

---

## P1 — 강력 추천 (포트폴리오 차별화)

### 네트워크·설정
- [ ] 서버 IP/포트 **ScriptableObject** 또는 json 분리 (`127.0.0.1` 하드코딩 제거)
- [ ] TCP 연결 **타임아웃** + 실패 콜백 UI 연동
- [ ] 인게임 **「타이틀로」** 버튼: `SendLeaveGame` → TitleScene (연결 유지 vs 끊기 정책 결정)

### 전투·채팅 (스텁 → 최소 구현)
- [ ] `C_S_ATTACK` / `S_C_ATTACK` 서버·클라 1라운드 (데미지 숫자만이라도)
- [ ] `C_S_CHAT` / `S_C_CHAT` — 채팅 로그 UI 1줄

### UI
- [ ] `UI_GameScene` — FPS, HP 바, 조이스틱 레이아웃 정리
- [ ] `UI_Toast` 또는 공통 메시지 팝업 1종
- [ ] Preload 진행률 표시 (`LoadAllAsync` 콜백 활용)

### 데이터
- [ ] `DataManager` — 몬스터 테이블 JSON/CSV 1개 (서버 하드코딩 → 테이블 로드로 전환 검토)
- [ ] ScriptableObject 1종 (예: `CreatureData` 클라 표시용)

### 서버
- [ ] 몬스터 스폰 데이터를 `ZoneManager::InitDefaultZone` 밖으로 분리
- [ ] `DummyClient` 또는 간단 콘솔 테스트로 MOVE 패킷 단독 검증

---

## P2 — 여유 있을 때

- [ ] Addressables 씬 전환 시 **Release** 정책 문서화·구현
- [ ] `Managers.Clear()` / 이벤트 구독 해제 전수 점검
- [ ] 에디터 치트 (IP 변경, 로그인 스킵, 오브젝트 ID 표시)
- [ ] CI (Unity batch + MSBuild GameServer)
- [ ] 픽셀 아트면 `com.unity.2d.pixel-perfect` 검토 (현재는 `OrthographicCameraScaler`로 충분할 수 있음)
- [ ] Zone 다중 맵 / 포탈 (Room 개념 도입)
- [ ] DB 재도입 (포트폴리오 범위 밖이면 생략)

---

## 포트폴리오 산출물 (README·면접용)

- [ ] **아키텍처 다이어그램** 1장  
  Managers, Scene, Network, Packet, Zone/Player/Monster 관계
- [ ] **시퀀스 다이어그램** 1장  
  Title → Preload → Connect → Login → EnterGame → Spawn/Despawn
- [ ] **데모 영상** 1~2분 (서버 ON/OFF, 2인 접속, 이동, 퇴장)
- [ ] README 갱신 (로그인 팝업, 퇴장, 창 모드, 크리쳐 프리팹 경로)
- [ ] 의미 있는 **Git 커밋** (`.cursor/` 제외, 오늘 작업분 묶기)

---

## 추천 작업 순서 (다음 날)

| 순서 | 작업 | 예상 효과 |
|------|------|-----------|
| 1 | **이동 패킷 + 조이스틱** | “게임”으로 보이는 최소 단위 |
| 2 | **카메라 Follow + 실제 스프라이트** | 시연 품질 상승 |
| 3 | **연결 실패/재시도 UI** | 안정적인 데모 |
| 4 | **2클라 멀티 테스트** | 네트워크 구조 증명 |
| 5 | **다이어그램 + 데모 영상 + 커밋** | 포트폴리오 마감 |

---

## 참고 — 아직 스텁인 것

| 위치 | 내용 |
|------|------|
| `ClientPacketHandler.cpp` | `Handle_C_S_MOVE`, `Handle_C_S_ATTACK`, `Handle_C_S_CHAT` 빈 구현 |
| `PacketHandler.cs` | `S_C_MOVE`, `S_C_ATTACK`, `S_C_DIE` 로그만 |
| `PlayerObject.cs` | 이동·애니 로직 없음 |
| `UI_GameScene.cs` | FPS·레이아웃 미완 |

---

## Git 커밋 전 체크

- [ ] Unity에서 TitleScene / GameScene Play 테스트
- [ ] `Server/Binary/Debug/GameServer.exe` 빌드·실행
- [ ] `GenPackets.bat` 후 C#/C++ MsgId 일치 확인
- [ ] 커밋 제외: `.cursor/`, `Library/`, `Temp/`, `UserSettings/` (`.gitignore` 확인)

---

*이 문서는 세션 종료 시점 기준입니다. 완료 항목은 PR/커밋 후 `[x]`로 갱신하세요.*
