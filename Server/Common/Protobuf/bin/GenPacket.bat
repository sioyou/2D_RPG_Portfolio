@echo off
REM Alias for GenPackets.bat (common typo).
call "%~dp0GenPackets.bat" %*
exit /b %ERRORLEVEL%
