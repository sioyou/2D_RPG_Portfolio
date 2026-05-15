protobuf는 왠만해선 여기에 있는 .exe랑 .lib .dll로 적용

xxxserver.exe 실행파일 쪽에 protobuf관련 .dll 같은 경로에 넣어줘야됨

.proto 변경 시 자동 코드 생성
-> Common/Protobuf/ProtobufCodegen.targets (GameServer, DummyClient에서 import)
   .proto / GenPackets.bat 변경 시에만 GenPackets.bat 실행 후 빌드
   수동 실행: Common\Protobuf\bin\GenPackets.bat

GameServer는 0.0.0.0:7777 에 바인드 (모든 NIC에서 접속 가능) 

cmd에서 phyton 치고 안나오면 환경변수 설정해줘야됨
phyton에 패키지 관리 -> jinja2, pyinstaller 추가

SQL 
odbc connection 관련해서 검색