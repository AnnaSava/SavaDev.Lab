@echo off
REM ============================================================
REM Runs SavaDev.Lab.Processes tests on Windows.
REM
REM This script:
REM  - restores NuGet packages for test assets and tests
REM  - builds the test asset project
REM  - runs the test suite
REM
REM Notes:
REM  - Uses explicit `dotnet restore` to avoid implicit restore issues
REM  - Assumes Debug configuration (default for dotnet CLI)
REM
REM Requirements:
REM  - .NET SDK 8.0+ installed
REM  - Script must be executed from the repository root
REM ============================================================

setlocal

dotnet restore tests\SavaDev.Lab.Processes.TestAssets.LongProcess || goto :error
dotnet restore tests\SavaDev.Lab.Processes.Tests || goto :error

dotnet build tests\SavaDev.Lab.Processes.TestAssets.LongProcess || goto :error
dotnet test tests\SavaDev.Lab.Processes.Tests || goto :error

echo.
echo Tests completed successfully.
goto :eof

:error
echo.
echo Test run failed.
exit /b 1
