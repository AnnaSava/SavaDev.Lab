# SavaDev.Lab.Processes

**SavaDev.Lab.Processes** is a small **library** for launching external processes with explicit, observable, and cancellation-aware execution.

It is intended to be embedded into tooling or infrastructure code where you need to run external commands in a predictable way, without hiding execution details.

---

## Purpose

This library exists to provide a minimal, well-documented abstraction over `System.Diagnostics.Process` that:

* runs external processes asynchronously
* streams standard output and error line-by-line
* optionally captures output in memory
* supports cooperative cancellation
* allows pluggable output observers

The library contains **no orchestration** and **no domain-specific logic**.

---

## Basic usage

Create a launcher, define a request, and run:

```csharp
var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());

var request = new ProcessRequest
{
    FileName = "dotnet",
    Arguments = "--info"
};

var result = await launcher.RunAsync(request);
```

Add real-time output observation:

```csharp
var observer = new ConsoleContextualObserver();
var output = new ProcessOutputHandling(observer);

var result = await launcher.RunAsync(request, output);
```

Stream output only (no buffering in result):

```csharp
var observer = new ConsoleBasicObserver();
var output = new ProcessOutputHandling(observer, captureOutput: false);

await launcher.RunAsync(request, output);
```

---

## Execution model

When `RunAsync` is called, the launcher:

1. Starts the process with redirected stdout/stderr
2. Streams output line-by-line to observers
3. Optionally captures output in memory
4. Waits for completion
5. Returns a `ProcessResult` with the exit code and captured output

---

## Core types

### `ProcessRequest`

Describes what to run:

* executable name
* arguments
* working directory
* environment variables
* unique request identifier

### `ProcessOutputHandling`

Defines how output is handled:

* whether output is captured in memory
* which observers receive output events

### `ProcessResult`

Represents the execution outcome:

* exit code
* captured standard output and error (if enabled)

Success evaluation is provided via the `ProcessResultExtensions.IsSuccess()` extension method
in the `SavaDev.Lab.Processes.Extensions` namespace and follows the `ExitCode == 0` convention.

---

## Specialized launchers

### `IDotNetProcessLauncher`

Convenience wrapper for executing `dotnet` commands:

```csharp
var dotnet = new DotNetProcessLauncher(launcher);
var result = await dotnet.RunAsync("build");
```

### `IShellProcessLauncher`

Executes a command using the default OS shell:

```csharp
var shell = new ShellProcessLauncher(launcher);
var result = await shell.RunAsync("echo hello");
```

---

## Cancellation behavior

* If cancellation is requested while the process is running, the process is terminated as a best-effort operation.
* If cancellation is requested after the process has completed, the result is still returned.

---

## Intended usage

This library is intended to be used as:

* a low-level process execution building block
* a reliable component inside CLI tooling or infrastructure code

It is **not** intended to implement orchestration, retries, or business workflows.

---

## Design principles

* Explicit behavior over hidden policies
* Predictable and observable execution
* Minimal dependencies
* Small surface area

---

## Summary

`SavaDev.Lab.Processes` is a focused library for predictable process execution with first-class output observation and cancellation support.

It is intentionally low-level and aims to be a stable foundation for higher-level tooling.
