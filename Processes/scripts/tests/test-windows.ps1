<#
============================================================
Runs SavaDev.Lab.Processes tests on Windows.

This script:
 - restores NuGet packages for test assets and tests
 - builds the test asset project
 - runs the test suite

Notes:
 - Uses explicit `dotnet restore`
 - Assumes Debug configuration (default for dotnet CLI)

Requirements:
 - .NET SDK 8.0+ installed
 - Script must be executed from the repository root
============================================================
#>

$ErrorActionPreference = "Stop"

dotnet restore tests/SavaDev.Lab.Processes.TestAssets.LongProcess
dotnet restore tests/SavaDev.Lab.Processes.Tests

dotnet build tests/SavaDev.Lab.Processes.TestAssets.LongProcess
dotnet test tests/SavaDev.Lab.Processes.Tests

Write-Host ""
Write-Host "Tests completed successfully."
