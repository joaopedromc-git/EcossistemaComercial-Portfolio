@echo off
setlocal EnableExtensions

set "ORIGEM=C:\CAMINHO_DA_NOVA_VERSAO"
set "DESTINO=C:\NotificadorDemo"
set "EXE=NotificadorDemo.exe"

echo ========================================
echo Atualizacao - Notificador Demo
echo ========================================
echo.

if not exist "%ORIGEM%" (
    echo ERRO: A pasta de atualizacao nao foi encontrada.
    echo Caminho configurado:
    echo %ORIGEM%
    echo.
    pause
    exit /b 1
)

if not exist "%DESTINO%" (
    mkdir "%DESTINO%"
)

taskkill /IM "%EXE%" /F >nul 2>&1

timeout /t 2 /nobreak >nul

robocopy "%ORIGEM%" "%DESTINO%" /E /R:2 /W:2

if %ERRORLEVEL% GEQ 8 (
    echo.
    echo ERRO: Falha durante a copia dos arquivos.
    pause
    exit /b 1
)

echo.
echo Atualizacao concluida.
pause
exit /b 0
