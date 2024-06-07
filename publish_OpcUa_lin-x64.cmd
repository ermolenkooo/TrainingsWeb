@echo off
set out_folder=.\artifacts\linux-x64
set build_options=-c Release -r linux-x64 --no-self-contained --nologo -v q
set project_path=C:\Users\ermolenko\Documents\programs\web\React\React.Server\React.Server.csproj
pushd "%~dp0\.."

echo Build OpcUa
call :publish 

:publish
set out_name=%2
if "%out_name%"=="" set out_name=%~n1
echo Publish `%project_path%` into `%out_folder%\%out_name%\`
dotnet publish %project_path% --output %out_folder%\%out_name%\ %build_options%
