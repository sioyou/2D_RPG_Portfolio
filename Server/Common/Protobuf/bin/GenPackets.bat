@echo off
pushd %~dp0

echo [Step 1] Generating Proto Files...
protoc.exe -I=./ --cpp_out=./ ./Enum.proto ./Struct.proto ./Protocol.proto
if errorlevel 1 (
    echo [ERROR] protoc C++ generation failed.
    popd
    exit /b 1
)

protoc.exe -I=./ --csharp_out=./ ./Enum.proto ./Struct.proto ./Protocol.proto
if errorlevel 1 (
    echo [ERROR] protoc C# generation failed.
    popd
    exit /b 1
)

echo [Step 2] Generating Packet Handlers...
PacketGenerator.exe --path=./Protocol.proto --output=ClientPacketHandler --recv=C_ --send=S_ --lang=cpp --enum=./Enum.proto --startId=1010
if errorlevel 1 (
    echo [ERROR] ClientPacketHandler generation failed.
    popd
    exit /b 1
)

PacketGenerator.exe --path=./Protocol.proto --output=ServerPacketHandler --recv=S_ --send=C_ --lang=csharp --enum=./Enum.proto --startId=1010
if errorlevel 1 (
    echo [ERROR] ServerPacketHandler generation failed.
    popd
    exit /b 1
)

echo [Step 3] Normalizing line endings (CRLF)...
powershell -NoProfile -Command "Get-ChildItem -File -Include *.cs,*.h,*.cc,*.pb.cc,*.pb.h | ForEach-Object { $content = Get-Content -Raw -LiteralPath $_.FullName; $content = $content -replace \"`r`n\", \"`n\" -replace \"`n\", \"`r`n\"; [System.IO.File]::WriteAllText($_.FullName, $content, [System.Text.UTF8Encoding]::new($false)) }"

set "SERVER_PATH=..\..\..\GameServer\"
set "DUMMYCLIENT_PATH=..\..\..\DummyClient\"
set "UNITY_PATH=..\..\..\..\Client\Assets\@Scripts\Packet\Generated"

echo [Step 4] Copying server (C++) files to %SERVER_PATH%
if not exist "%SERVER_PATH%" (
    echo [ERROR] Server path not found: %SERVER_PATH%
    popd
    exit /b 1
)

del /Q /F "%SERVER_PATH%*.pb.h" 2>nul
del /Q /F "%SERVER_PATH%*.pb.cc" 2>nul
del /Q /F "%SERVER_PATH%ClientPacketHandler.h" 2>nul

xcopy /Y /Q *.pb.h "%SERVER_PATH%" >nul
xcopy /Y /Q *.pb.cc "%SERVER_PATH%" >nul
xcopy /Y /Q ClientPacketHandler.h "%SERVER_PATH%" >nul

echo [Step 4b] Copying server (C++) files to %DUMMYCLIENT_PATH%
if not exist "%DUMMYCLIENT_PATH%" (
    echo [ERROR] DummyClient path not found: %DUMMYCLIENT_PATH%
    popd
    exit /b 1
)

del /Q /F "%DUMMYCLIENT_PATH%*.pb.h" 2>nul
del /Q /F "%DUMMYCLIENT_PATH%*.pb.cc" 2>nul

xcopy /Y /Q *.pb.h "%DUMMYCLIENT_PATH%" >nul
xcopy /Y /Q *.pb.cc "%DUMMYCLIENT_PATH%" >nul

echo [Step 5] Copying client (C#) files to %UNITY_PATH%
if not exist "%UNITY_PATH%" (
    mkdir "%UNITY_PATH%"
)

del /Q /F "%UNITY_PATH%\Enum.cs" 2>nul
del /Q /F "%UNITY_PATH%\enum.cs" 2>nul
del /Q /F "%UNITY_PATH%\Struct.cs" 2>nul
del /Q /F "%UNITY_PATH%\Protocol.cs" 2>nul
del /Q /F "%UNITY_PATH%\ServerPacketHandler.cs" 2>nul

xcopy /Y /Q Enum.cs "%UNITY_PATH%\" >nul
xcopy /Y /Q Struct.cs "%UNITY_PATH%\" >nul
xcopy /Y /Q Protocol.cs "%UNITY_PATH%\" >nul
xcopy /Y /Q ServerPacketHandler.cs "%UNITY_PATH%\" >nul

echo [Step 6] Cleaning temporary files in bin...
del /Q /F *.pb.h *.pb.cc ClientPacketHandler.h Enum.cs Struct.cs Protocol.cs ServerPacketHandler.cs 2>nul

popd
echo [Complete] Packet generation finished successfully.
exit /b 0
