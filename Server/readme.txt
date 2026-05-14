protobuf는 왠만해선 여기에 있는 .exe랑 .lib .dll로 적용

xxxserver.exe 실행파일 쪽에 protobuf관련 .dll 같은 경로에 넣어줘야됨

.proto는 변경되어서 인식을 못함
-> .vcxproj
   <ItemGroup>
    <UpToDateCheckInput Include="..\Common\Protobuf\bin\Enum.proto" />
    <UpToDateCheckInput Include="..\Common\Protobuf\bin\Protocol.proto" />
    <UpToDateCheckInput Include="..\Common\Protobuf\bin\Struct.proto" />
  </ItemGroup>
추가 (UpToDateCheckInput->변경된 내용이 있으면 빌드 돌리게 해주는것) 

cmd에서 phyton 치고 안나오면 환경변수 설정해줘야됨
phyton에 패키지 관리 -> jinja2, pyinstaller 추가

SQL 
odbc connection 관련해서 검색