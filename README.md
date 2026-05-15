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
5. Addressables **Preload** 완료 후 서버에 TCP 연결을 시도하고, 성공 시 **GameScene**으로 전환됩니다.

## 클라이언트

| 항목 | 내용 |
|------|------|
| 스크립트 | `Client/Assets/@Scripts` |
| 씬 | `Client/Assets/@Scenes` (Build Settings: TitleScene → GameScene) |
| 서버 주소 | `127.0.0.1:7777` (`NetworkManager.ConnectGameServer`) |
| 리소스 | Addressables, Preload 라벨 |

### Unity 씬 설정 (필수)

- **TitleScene**: `EventSystem`, `UI_TitleScene` (자식 `StartButton`, `StatusText`)
- **GameScene**: `GameScene` 스크립트가 붙은 오브젝트

## 서버

| 항목 | 내용 |
|------|------|
| 솔루션 | `Server/Server.sln` |
| 실행 파일 | `Server/Binary/Debug/GameServer.exe` |
| 포트 | `7777` |

자세한 protobuf 빌드 안내는 [Server/readme.txt](Server/readme.txt)를 참고하세요.

## Protocol Buffers

- 정의: `Server/Common/Protobuf/bin/*.proto`
- 생성 스크립트: `Server/Common/Protobuf/bin/GenPackets.bat`
- GameServer 빌드 시 `Server/Common/Protobuf/ProtobufCodegen.targets`가 proto 변경을 감지해 자동 실행합니다.

수동 생성:

```bat
cd Server\Common\Protobuf\bin
GenPackets.bat
```

## 프로젝트 구조

```
2D_RPG_Portfolio/
├── Client/          # Unity 클라이언트
├── Server/          # C++ GameServer, ServerCore, DummyClient
└── README.md
```

## 현재 구현 범위 (MVP)

- [x] Managers 프레임워크, Addressables Preload
- [x] 타이틀에서 게임 서버 TCP 연결
- [x] 연결 성공 시 GameScene 전환
- [ ] 로그인 패킷 (`C_S_LOGIN` / `S_C_LOGIN`) 및 인게임 동기화 (예정)
