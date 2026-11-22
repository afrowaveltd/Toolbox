@echo off
rem Convenience launcher for Windows Command Prompt.
rem All storage checks and actual work are performed by run.ps1.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0run.ps1" %*
exit /b %ERRORLEVEL%
