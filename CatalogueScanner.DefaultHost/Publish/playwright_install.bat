set PLAYWRIGHT_BROWSERS_PATH=%APPSETTING_PLAYWRIGHT_BROWSERS_PATH%

dotnet tool install --global Microsoft.Playwright.CLI

"%USERPROFILE%\.dotnet\tools\playwright" -p "%~dp0\..\..\..\wwwroot" install chromium
