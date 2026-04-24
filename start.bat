@echo off
title VUCAPPcheck

:: Настройки
set "PHP_PORT=6024"
set "PHP_EXE=%~dp0systemfiles\library\php\php.exe"
set "EXT_DIR=%~dp0systemfiles\library\php\ext"

:: 1. Переходим в рабочую папку
cd /D "%~dp0systemfiles"

:: 2. Запускаем PHP-сервер
:: Используем /B, чтобы PHP не создавал лишних окон, если это не требуется
echo Starting PHP server on port %PHP_PORT%...
start "" /B "%PHP_EXE%" -S 0.0.0.0:%PHP_PORT% -c "%~dp0systemfiles\library\php\php.ini" -d extension_dir="%EXT_DIR%" -d display_errors=On -d error_reporting=E_ALL

:: 3. Запускаем приложение
echo Starting VUCAPP...
start "" "vucapp.exe"

:: 4. Проверка процесса vucapp.exe
:: Цикл проверяет наличие процесса в памяти каждые 2 секунды
:MonitorLoop
timeout /t 2 /nobreak >nul
tasklist /FI "IMAGENAME eq vucapp.exe" 2>nul | find /i "vucapp.exe" >nul
if not errorlevel 1 goto MonitorLoop

:: 5. Завершение работы
echo.
echo VUCAPP closed.
echo Stopping PHP server on port %PHP_PORT%...

:: Ищем PID (ID процесса), который слушает наш порт, и убиваем именно его
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :%PHP_PORT% ^| findstr LISTENING') do (
    taskkill /F /PID %%a /T >nul 2>&1
)

echo PHP server stopped.
exit