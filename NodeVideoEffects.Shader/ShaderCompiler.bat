@echo off

if "%1"=="" (
    echo Usage: CompileShader.bat input_shader.hlsl
    pause
    exit /b 1
)

set INPUT_SHADER=%1
if not exist "%INPUT_SHADER%" (
    echo Input file "%INPUT_SHADER%" not found.
    pause
    exit /b 1
)
set OUTPUT_SHADER=%~dpn1.cso

"C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\fxc.exe" /T ps_4_1 /E main /Fo %OUTPUT_SHADER% /O3 %INPUT_SHADER%

if %ERRORLEVEL% NEQ 0 (
    echo Shader compilation failed.
) else (
    echo Compilation successful: %OUTPUT_SHADER%
)

pause

