# 2D RPG Portfolio — 실무 체크리스트

> **프로젝트:** 2D_RPG_Portfolio  
> **Unity:** 6000.3.13f1  
> **서버:** GameServer `0.0.0.0:7777`  
> **저장소 README:** 루트 `README.md` 참고

---

## ✅ 완료 (MVP — 커밋 `6c22a88` 기준)

- [x] `@Scripts` / `@Scenes` 매니저 구조
- [x] `Managers` + Addressables `ResourceManager`
- [x] Title Preload → 서버 TCP 연결 → GameScene 자동 전환
- [x] EventSystem `DontDestroyOnLoad` (TitleScene)
- [x] proto 변경 시 `GenPackets.bat` (ProtobufCodegen.targets)
- [x] GameServer `0.0.0.0:7777` 리슨
- [x] 루트 README (실행 순서)
- [x] Git 1차 업로드 (클라이언트-서버 연결 MVP)

---

## P0 — 반드시 (포트폴리오·데모 필수)

### 클라이언트 · 네트워크

- [ ] 접속 상태 머신: `Disconnected → Connecting → Connected → LoggedIn → InGame`
- [ ] TCP 연결 후 **`C_S_LOGIN` 전송** + `S_C_LOGIN` 처리
- [ ] 로그인 성공 후 **`C_S_ENTER_GAME`** → GameScene (또는 인게임 진입 패킷)
- [ ] 서버 미실행 / 연결 실패 → **UI 메시지 + 재시도** (로그만 X)
- [ ] `TitleScene` / `UI_TitleScene` **네트워크 이벤트 구독 한곳**으로 정리

### GameScene

- [ ] 진입 시 할 일 명시 (UI, 플레이어 스폰, 카메라 중 1개 이상)
- [ ] **입력 1종** (이동 또는 공격)
- [ ] 씬 이탈 / 앱 종료 시 `Disconnect` 정리

### Addressables

- [ ] **Preload / 씬별 로드 / 언로드** 정책 문서화 (README 또는 Notion)
- [ ] `LoadAllAsync` 실패·0개 케이스 처리
- [ ] 필수 프리팹 없을 때 에러 명확 (`UI_TitleScene` 등)

### 서버

- [ ] 로그인 핸들러 실제 응답 (`Handle_C_S_LOGIN` 등)
- [ ] DummyClient 또는 간단 테스트로 패킷 송수신 확인

### 포트폴리오 문서·데모

- [ ] **아키텍처 다이어그램** 1장 (Managers, Scene, Network, Packet)
- [ ] **시퀀스 다이어그램** 1장 (Title → Preload → Connect → GameScene)
- [ ] **데모 영상** 1~2분 (서버 ON/OFF 포함)

---

## P1 — 강력 추천 (차별화)

### 네트워크

- [ ] IP/포트 **설정 분리** (ScriptableObject, json — `127.0.0.1` 하드코딩 제거)
- [ ] 연결 **타임아웃**
- [ ] 패킷 **1종 이상** 게임 반영 (`S_C_SPAWN`, `S_C_MOVE` 등)
- [ ] `ObjectManager` — objectId → GameObject 매핑

### UI/UX

- [ ] 타이틀 **로딩 진행 UI** (`AssetLoading` 상태 사용)
- [ ] GameScene **`UI_GameScene`**
- [ ] Toast / 팝업 1종

### 데이터

- [ ] `DataManager` — JSON/CSV 1테이블 로드
- [ ] ScriptableObject 1종 실사용

### 코드·저장소

- [ ] `GameManager`에 실제 게임 상태 (PlayerId 등)
- [ ] dead code 정리 (`StartButton`, `WebManager` 주석 등)
- [ ] 의미 있는 **커밋 단위** 유지

---

## P2 — 여유 있을 때

- [ ] Addressables 씬 전환 시 **Release** 정책
- [ ] `Managers.Clear()` / 이벤트 구독 **누수** 점검
- [ ] 서버: 로그인 검증, 세션 끊김 정리
- [ ] 에디터 치트 (IP 변경, 로그인 스킵)
- [ ] CI (Unity + MSBuild)
- [ ] 재접속 / 로비 씬
- [ ] DB (Server readme 참고)

---

## 자가 점검 (면접·README용)

| 질문 | 기대 답 |
|------|---------|
| 서버 안 켜면? | 타이틀 실패 UI / 재시도 |
| proto 수정하면? | VS 빌드 → GenPackets → C#/C++ 갱신 |
| 씬 바꿔도 UI 클릭? | EventSystem DDOL |
| GameScene 들어가면? | 최소 1개 플레이 동작 |
| 메모리 전략? | Preload vs 맵별 로드 문서 |

---

## 추천 작업 순서

1. **P0** — `C_S_LOGIN` / `S_C_LOGIN` + 실패 UI  
2. **P0** — GameScene 스폰 + 이동 1종  
3. **P0** — 다이어그램 + 데모 영상  
4. **P1** — ObjectManager + 패킷 반영 1종  
5. **P1** — Addressables 정책 + DataManager  

---

## Git / 실행 Quick Reference

```
1. Server/Server.sln → GameServer 빌드 (Debug x64)
2. Server/Binary/Debug/GameServer.exe
3. Unity Client → TitleScene Play
4. 연결 성공 → GameScene
```

**클라이언트:** `127.0.0.1:7777`  
**proto 수동:** `Server\Common\Protobuf\bin\GenPackets.bat`
