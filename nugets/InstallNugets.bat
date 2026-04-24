@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul

set "DIR=C:\vucapp\nugets"
set "TEMP_PS=get_exact.ps1"
set "LIST=exact_packages.txt"

if not exist "%DIR%" mkdir "%DIR%"

del *.nupkg 2>nul
md obj 2>nul
copy "C:\vucapp\systemfiles\useapp\AppGenerated\obj\project.assets.json" "C:\vucapp\nugets\obj\project.assets.json" >nul
copy "C:\vucapp\systemfiles\useapp\AppGenerated\AppGenerated.csproj" "AppGenerated.csproj" >nul

echo =========================================================
echo 1. РАСЧЕТ ЗАВИСИМОСТЕЙ (ВКЛЮЧАЕМ ИНТЕРНЕТ)
echo =========================================================
if exist "nuget.config" (
    ren nuget.config nuget.config.bak
    echo [i] Строгий nuget.config временно переименован.
)

:: Заставляем dotnet просчитать ВСЕ пакеты и утилиты для всех трех архитектур
echo Вычисляем точные версии для x64, x86 и ARM64...
call "C:\Program Files\dotnet\dotnet.exe" restore -r win-x64 /p:PublishReadyToRun=true /p:PublishTrimmed=true >nul
call "C:\Program Files\dotnet\dotnet.exe" restore -r win-x86 /p:PublishReadyToRun=true /p:PublishTrimmed=true >nul
call "C:\Program Files\dotnet\dotnet.exe" restore -r win-arm64 /p:PublishReadyToRun=true /p:PublishTrimmed=true >nul

if not exist "obj\project.assets.json" (
    echo [!] Ошибка: project.assets.json не был создан. Проверьте интернет.
    if exist "nuget.config.bak" ren nuget.config.bak nuget.config
    pause & exit /b
)

echo =========================================================
echo 2. ИЗВЛЕЧЕНИЕ 100%% ВСЕХ ВЕРСИЙ (БИБЛИОТЕКИ + РАНТАЙМЫ)
echo =========================================================
if exist "%TEMP_PS%" del "%TEMP_PS%"

echo $j = Get-Content 'obj\project.assets.json' -Raw ^| ConvertFrom-Json > "%TEMP_PS%"
echo $res = @() >> "%TEMP_PS%"
echo $runtimeVer = $null >> "%TEMP_PS%"

:: 1. Читаем блок библиотек
echo foreach($lib in $j.libraries.PSObject.Properties) { >> "%TEMP_PS%"
echo   if ($lib.Value.type -eq 'package') { >> "%TEMP_PS%"
echo     $res += "$($lib.Name.Split('/')[0].ToLower())|$($lib.Name.Split('/')[1])" >> "%TEMP_PS%"
echo   } >> "%TEMP_PS%"
echo } >> "%TEMP_PS%"

:: 2. Читаем блок рантаймов и "ловим" версию .NET
echo foreach($fw in $j.project.frameworks.PSObject.Properties.Value) { >> "%TEMP_PS%"
echo   if ($fw.downloadDependencies) { >> "%TEMP_PS%"
echo     foreach($d in $fw.downloadDependencies) { >> "%TEMP_PS%"
echo       $v = $d.version.Trim('[]'); if($v.Contains(',')){$v=$v.Split(',')[0].Trim()} >> "%TEMP_PS%"
echo       $res += "$($d.name.ToLower())|$v" >> "%TEMP_PS%"
echo       if ($d.name -match 'Microsoft.NETCore.App.Runtime') { $runtimeVer = $v } >> "%TEMP_PS%"
echo     } >> "%TEMP_PS%"
echo   } >> "%TEMP_PS%"
echo   if ($fw.dependencies) { >> "%TEMP_PS%"
echo     foreach($dep in $fw.dependencies.PSObject.Properties) { >> "%TEMP_PS%"
echo       $v = $dep.Value.version.Replace('[','').Replace(']','').Replace('(','').Replace(')',''); if($v.Contains(',')){$v=$v.Split(',')[0].Trim()} >> "%TEMP_PS%"
echo       $res += "$($dep.Name.ToLower())|$v" >> "%TEMP_PS%"
echo     } >> "%TEMP_PS%"
echo   } >> "%TEMP_PS%"
echo } >> "%TEMP_PS%"

:: 3. ПРИНУДИТЕЛЬНО добавляем ILLink (компилятор требует его при PublishTrimmed=true)
echo if ($runtimeVer) { >> "%TEMP_PS%"
echo     $res += "microsoft.net.illink.tasks|$runtimeVer" >> "%TEMP_PS%"
echo } >> "%TEMP_PS%"

echo [System.IO.File]::WriteAllLines('%LIST%', ($res ^| Select-Object -Unique)) >> "%TEMP_PS%"

powershell -NoProfile -ExecutionPolicy Bypass -File "%TEMP_PS%"

echo =========================================================
echo 3. ЗАГРУЗКА ПАКЕТОВ В ОФЛАЙН ПАПКУ
echo =========================================================
for /f "usebackq tokens=1,2 delims=|" %%A in ("%LIST%") do (
    set "NAME=%%A"
    set "VER=%%B"
    set "FNAME=!NAME!.!VER!.nupkg"
    
    if not exist "%DIR%\!FNAME!" (
        echo.
        echo СКАЧИВАНИЕ: !FNAME!
        
        :: 1. Основной NuGet.org
        curl -L -f -o "%DIR%\!FNAME!" "https://api.nuget.org/v3-flatcontainer/!NAME!/!VER!/!FNAME!" --progress-bar
        
        :: 2. Сервисный фид Microsoft
        if !ERRORLEVEL! NEQ 0 (
            echo [!] Пробую Microsoft Dev Feed...
            curl -L -f -o "%DIR%\!FNAME!" "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flatcontainer/!NAME!/!VER!/!FNAME!" --progress-bar
        )
    ) else (
        echo [OK] !FNAME! уже есть в кэше.
    )
)

:: ВОЗВРАЩАЕМ КОНФИГ ОБРАТНО (СНОВА ОФЛАЙН)
if exist "nuget.config.bak" (
    ren nuget.config.bak nuget.config
    echo.
    echo [i] Строгий nuget.config возвращен на место.
)

:: Чистка мусора
del "%TEMP_PS%"
del "%LIST%"
rd "C:\vucapp\nugets\obj" /s /q
del "C:\vucapp\nugets\AppGenerated.csproj"

echo.
echo =========================================================
echo ГОТОВО! 
echo Теперь папка %DIR% содержит ПОЛНЫЙ список пакетов.
timeout 3 >nul
exit
