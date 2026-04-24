cd C:\vucapp\dev\project
dotnet publish AppGenerated.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false /p:PublishReadyToRun=true /p:DebugType=none /p:DebugSymbols=false /p:AssemblyName=vucapp -o "C:\vucapp\dev"
pause

echo Этот файл служит для компиляции приложения, winui 3 с localhost внутри
