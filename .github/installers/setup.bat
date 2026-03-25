@echo off
setlocal enabledelayedexpansion

:: Nintenlord UPS Patcher Installer (No Admin Required)
echo.
echo Nintenlord UPS Patcher Setup
echo ============================
echo.

:: Default install location (per-user, no admin needed)
set "DEFAULT_INSTALL=%LOCALAPPDATA%\Programs\NUPS"
set "INSTALL_DIR=%DEFAULT_INSTALL%"

:: Ask for install location
echo Default installation folder: %DEFAULT_INSTALL%
echo.
set /p "CUSTOM_DIR=Press Enter to accept, or type a different path: "
if not "%CUSTOM_DIR%"=="" set "INSTALL_DIR=%CUSTOM_DIR%"

:: Create install directory
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

:: Copy files
echo.
echo Installing to: %INSTALL_DIR%
xcopy /E /I /Y /Q ".\*" "%INSTALL_DIR%" >nul
if errorlevel 1 (
    echo ERROR: Failed to copy files
    pause
    exit /b 1
)

:: Create Start Menu shortcut
set "SHORTCUT_DIR=%APPDATA%\Microsoft\Windows\Start Menu\Programs"
set "SHORTCUT_PATH=%SHORTCUT_DIR%\Nintenlord UPS Patcher.lnk"

echo Creating Start Menu shortcut...
powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%SHORTCUT_PATH%'); $s.TargetPath = '%INSTALL_DIR%\Nintenlord UPS patcher.Avalonia.exe'; $s.WorkingDirectory = '%INSTALL_DIR%'; $s.Save()"

:: Optional desktop shortcut
echo.
choice /C YN /M "Create desktop shortcut?"
if errorlevel 2 goto :skip_desktop
if errorlevel 1 (
    set "DESKTOP_SHORTCUT=%USERPROFILE%\Desktop\Nintenlord UPS Patcher.lnk"
    powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('!DESKTOP_SHORTCUT!'); $s.TargetPath = '%INSTALL_DIR%\Nintenlord UPS patcher.Avalonia.exe'; $s.WorkingDirectory = '%INSTALL_DIR%'; $s.Save()"
    echo Desktop shortcut created.
)

:skip_desktop
echo.
echo Installation complete!
echo.
echo Nintenlord UPS Patcher has been installed to:
echo %INSTALL_DIR%
echo.
choice /C YN /M "Launch Nintenlord UPS Patcher now?"
if errorlevel 2 goto :end
if errorlevel 1 start "" "%INSTALL_DIR%\Nintenlord UPS patcher.Avalonia.exe"

:end
timeout /t 3
exit /b 0
