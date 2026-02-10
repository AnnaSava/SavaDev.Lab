<#
============================================================
Runs SavaDev.Lab.Processes tests inside a Linux Docker container.

This script:
 - mounts the repository into a Linux .NET SDK container
 - configures a temporary NuGet package cache inside the container
 - disables HTTP/2 for dotnet restore to avoid intermittent TLS/network issues
 - installs and updates CA certificates for reliable NuGet HTTPS access
 - explicitly restores required projects
 - builds the test asset project
 - runs the test suite

Notes:
 - The script relies on explicit `dotnet restore` instead of `dotnet clean`
   to avoid cross-platform build cache issues (Windows ↔ Linux).
 - The repository should not contain stale `bin/` or `obj/` artifacts.

Requirements:
 - Docker must be installed and running
 - Script must be executed from the repository root
============================================================
#>

$cmd = "set -e && " +
       "apt-get update && " +
       "apt-get install -y --no-install-recommends ca-certificates && " +
       "update-ca-certificates && " +
       "dotnet restore tests/SavaDev.Lab.Processes.TestAssets.LongProcess && " +
       "dotnet restore tests/SavaDev.Lab.Processes.Tests && " +
       "dotnet build tests/SavaDev.Lab.Processes.TestAssets.LongProcess && " +
       "dotnet test tests/SavaDev.Lab.Processes.Tests"

docker run --rm -it `
  -e NUGET_PACKAGES=/tmp/nuget-packages `
  -e DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2SUPPORT=false `
  -v "${PWD}:/repo" `
  -w /repo `
  mcr.microsoft.com/dotnet/sdk:8.0 `
  bash -c $cmd
