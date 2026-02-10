@echo off
REM ============================================================
REM Runs SavaDev.Lab.Processes tests inside a Linux Docker container.
REM
REM This script:
REM  - mounts the repository into a .NET SDK (Linux) container
REM  - configures a temporary NuGet package cache inside the container
REM  - disables HTTP/2 for dotnet restore to avoid intermittent TLS issues
REM  - installs and updates CA certificates for reliable NuGet HTTPS access
REM  - explicitly restores required projects
REM  - builds the test asset project
REM  - runs the test suite
REM
REM Notes:
REM  - The script relies on explicit `dotnet restore` instead of `dotnet clean`
REM    to avoid cross-platform build cache issues (Windows ↔ Linux).
REM  - The repository must not contain stale `bin/` or `obj/` artifacts.
REM
REM Requirements:
REM  - Docker must be installed and running
REM  - Script must be executed from the repository root
REM ============================================================

docker run --rm -it ^
  -e NUGET_PACKAGES=/tmp/nuget-packages ^
  -e DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2SUPPORT=false ^
  -v "%cd%":/repo ^
  -w /repo ^
  mcr.microsoft.com/dotnet/sdk:8.0 ^
  bash -c "set -e && apt-get update && apt-get install -y --no-install-recommends ca-certificates && update-ca-certificates && dotnet restore tests/SavaDev.Lab.Processes.TestAssets.LongProcess && dotnet restore tests/SavaDev.Lab.Processes.Tests && dotnet build tests/SavaDev.Lab.Processes.TestAssets.LongProcess && dotnet test tests/SavaDev.Lab.Processes.Tests"