# 2D RPG Portfolio

Unity 클라이언트와 Windows C++ 게임 서버로 구성된 2D RPG 포트폴리오 프로젝트입니다.

## 요구 사항

- **Unity** `6000.3.13f1` (Client/ProjectSettings 기준)
- **Visual Studio 2022** (v143, x64)
- **Windows** (서버 빌드/실행)

## 실행 순서

1. `Server/Server.sln`을 Visual Studio에서 열고 **GameServer**를 **Debug | x64**로 빌드합니다.
2. 빌드 결과물 실행: `Server/Binary/Debug/GameServer.exe`  
   - 기본 리슨 주소: `0.0.0.0:7777` (모든 NIC)
3. Unity Hub에서 `Client` 폴더를 프로젝트로 엽니다.
4. **TitleScene**을 실행(Play)합니다.
5. Addressables **Preload** 완료 후 서버에 TCP 연결 → **로그인** (`UI_LoginPopup`) 성공 시 **GameScene**으로 전환됩니다.

## 클라이언트

| 항목 | 내용 |
|------|------|
| 스크립트 | `Client/Assets/@Scripts` |
| 씬 | `Client/Assets/@Scenes` (Build Settings: TitleScene → GameScene) |
| 서버 주소 | `127.0.0.1:7777` (`NetworkManager.ConnectGameServer`) |
| 리소스 | Addressables, Preload 라벨 |

### Unity 씬 설정 (필수)

- **TitleScene**: `EventSystem`, `UI_TitleScene` (연결 후 `UI_LoginPopup` 표시)
- **GameScene**: `GameScene` 스크립트가 붙은 오브젝트

## 서버

| 항목 | 내용 |
|------|------|
| 솔루션 | `Server/Server.sln` |
| 실행 파일 | `Server/Binary/Debug/GameServer.exe` |
| 포트 | `7777` |

자세한 protobuf 빌드 안내는 [Server/readme.txt](Server/readme.txt)를 참고하세요.

## Protocol Buffers

- **Git에 포함:** `Server/Common/Protobuf/bin/*.proto`, `GenPackets.bat` (패킷 스키마 원본)
- **Git에 미포함:** `*.pb.cc` / `*.pb.h` / `ClientPacketHandler.h` / Unity `Packet/Generated/*.cs` (생성물)
- **로컬 도구:** `bin` 폴더에 `protoc.exe`, `PacketGenerator.exe` 필요 ([Server/readme.txt](Server/readme.txt) 참고)
- GameServer·DummyClient 빌드 시 `ProtobufCodegen.targets`가 proto 변경을 감지해 `GenPackets.bat`을 자동 실행합니다.

클론 후 Unity만 열 때는 먼저 아래를 실행하세요.

```bat
cd Server\Common\Protobuf\bin
GenPackets.bat
```

수동 생성도 동일합니다.

## 프로젝트 구조

```
2D_RPG_Portfolio/
├── Client/          # Unity 클라이언트
├── Server/          # C++ GameServer, ServerCore, DummyClient
└── README.md
```

## 현재 구현 범위 (MVP)

### 클라이언트

- [x] Managers 프레임워크, Addressables Preload
- [x] TitleScene: Preload → TCP 연결 → `UI_LoginPopup` 로그인 → GameScene 전환
- [x] GameScene 진입 시 `C_S_ENTER_GAME`, 퇴장·앱 종료 시 `LeaveGameAndDisconnect`
- [x] `ObjectManager` — 플레이어·몬스터 스폰/디스폰 (`PlayerObject`, `MonsterObject`, `MyPlayerObject`)
- [x] 조이스틱 이동, `C_S_MOVE` 주기 전송 및 `S_C_MOVE` 반영 (서버 위치 보정 포함)
- [x] 공격 입력 → `C_S_ATTACK`, `S_C_ATTACK` / `S_C_DIE` 애니메이션·HP 반영
- [x] 카메라 Follow (`CameraController`), 월드 HP 바 (`UI_WorldHpBar`)
- [x] 맵 로드·타일맵 빌드 (`MapManager`, `MapTilemapBuilder`, `MapCollisions.json`)
- [x] 클라·서버 공통 이동 검증 상수 (`MoveValidation`)

### 서버

- [x] IOCP 기반 GameServer (`0.0.0.0:7777`), Protobuf 패킷 처리
- [x] 인메모리 `PlayerManager` / `CreatureManager` / `RoomManager`
- [x] JSON 데이터 테이블 (`Monsters`, `Players`, `Rooms`, `RoomSpawns`, `MapCollisions`)
- [x] Room + Zone AOI — 시야 내 스폰·브로드캐스트
- [x] 맵 충돌 검증 (`MapCollision`), 이동·공격 서버 권위 처리
- [x] 몬스터 스폰 (룸별 `RoomSpawns.json`), 피격·사망 처리

### 네트워크 패킷

- [x] `C_S_LOGIN` / `S_C_LOGIN`
- [x] `C_S_ENTER_GAME` / `S_C_ENTER_GAME` (스폰 목록 포함)
- [x] `C_S_LEAVE_GAME` / `S_C_LEAVE_GAME`, `S_C_SPAWN` / `S_C_DESPAWN`
- [x] `C_S_MOVE` / `S_C_MOVE`
- [x] `C_S_ATTACK` / `S_C_ATTACK`, `S_C_DIE`
- [ ] `C_S_CHAT` / `S_C_CHAT` (핸들러 스텁, UI 미구현)

### 예정

- [ ] 연결·로그인 실패 재시도 UI
- [ ] 몬스터 AI·리스폰
- [ ] 채팅 UI, 추가 Room/맵
