# Running Tests (Scripts)

This directory contains helper scripts for running **SavaDev.Lab.Processes** tests in a predictable way.

The scripts explicitly build required **test assets** and then run the test suite.
This is important because some tests depend on external worker executables.

Scripts are provided for:

* **Windows (native)**
  (`cmd` and `PowerShell`)
* **Linux via Docker**
  (`cmd` and `PowerShell`, using a temporary container)

All scripts follow the same logical steps.

---

## General prerequisites

Before running any script, make sure:

* You are executing the script **from the project root**
* The working tree does not contain stale `bin/` or `obj/` artifacts
  (especially important when switching between Windows and Linux/Docker builds)

---

## What all test scripts do

Regardless of platform, all scripts perform the following steps:

1. Explicitly restore NuGet packages for:

   * the **test asset project**
   * the **test project**
2. Build the test asset project
3. Run the test suite

The scripts intentionally avoid relying on implicit restore or solution-wide builds.

---

## Why test assets are built explicitly

Some tests rely on **external executables** built from the test asset project
(for example, long-running worker processes).

Because of that, the scripts:

* do **not** rely on implicit build
* do **not** rely on solution-wide build
* always build test assets **before** running tests

This makes test execution deterministic and easier to reason about.

---

## Windows scripts (native)

These scripts run tests directly on Windows, without Docker.

### Prerequisites (Windows)

* **.NET SDK 8.0 or newer** installed

### CMD (Command Prompt)

**Run:**

```cmd
scripts\tests\run-tests-windows.cmd
```

**Notes:**

* Intended for `cmd.exe`
* Uses explicit error handling (`|| goto :error`)
* Stops immediately if any step fails
* Uses default `Debug` configuration

---

### PowerShell

**Run:**

```powershell
.\scripts\tests\run-tests-windows.ps1
```

If script execution is restricted:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

**Notes:**

* Intended for Windows PowerShell or PowerShell Core
* Uses `$ErrorActionPreference = "Stop"`
* Stops immediately on errors
* Uses default `Debug` configuration

---

## Linux scripts (via Docker)

These scripts run tests inside a **temporary Linux Docker container**.
They are useful for validating Linux behavior and catching cross-platform issues.

### Prerequisites (Linux / Docker)

* **Docker** installed and running

### What the Docker scripts do additionally

In addition to the common steps, the Docker scripts:

* start a temporary container using `mcr.microsoft.com/dotnet/sdk:8.0`
* mount the project into the container at `/repo`
* use a container-local NuGet package cache
* disable HTTP/2 to avoid intermittent TLS/network issues
* install and update CA certificates for reliable HTTPS access

The container is removed automatically after execution.

---

### CMD (Command Prompt)

**Run:**

```cmd
scripts\tests\run-tests-linux.cmd
```

**Notes:**

* Intended for `cmd.exe`
* Uses `^` for line continuation
* Uses `%cd%` to mount the current directory

---

### PowerShell

**Run:**

```powershell
.\scripts\tests\run-tests-linux.ps1
```

If script execution is restricted:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

**Notes:**

* Uses PowerShell line continuation (`` ` ``)
* Uses `${PWD}` for the current directory
* Passes a single-line command to `bash` to avoid CRLF issues

---

## Why there is no `dotnet clean`

All scripts intentionally **do not use `dotnet clean`**.

Reasons:

* Docker already provides a clean environment
* `dotnet clean` combined with implicit restore can cause cross-platform issues
* Explicit `dotnet restore` makes behavior deterministic and stable

---

## Configuration and build output

* **Build configuration**: `Debug`
  (default when `-c` / `--configuration` is not specified)

* **Target framework**: determined by each project’s `.csproj`

Test assets are built into:

```
tests/SavaDev.Lab.Processes.TestAssets.LongProcess/bin/Debug/<TFM>/
```

The test suite expects the assets to be present in this location.

---

## Environment variables used (Docker scripts)

| Variable                                                       | Purpose                                                        |
| -------------------------------------------------------------- | -------------------------------------------------------------- |
| `NUGET_PACKAGES`                                               | Uses a container-local NuGet cache to avoid host contamination |
| `DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2SUPPORT=false` | Disables HTTP/2 to avoid intermittent NuGet TLS/network issues |

---

## Expected result

* Test asset project builds successfully
* Test suite runs
* All tests pass
* Script exits with code `0`

If the script exits with a non-zero code, either the build or the tests failed.

---

## Troubleshooting

### NuGet network warnings (Docker)

Occasional NuGet warnings related to TLS or downloads may appear.

If the build and tests succeed, these warnings can usually be ignored.

If failures become frequent:

* ensure Docker has stable network access
* keep CA certificates installation enabled
* avoid reusing stale build artifacts
