# SavaDev.Lab.Processes.Demo

This project is a thin console application that showcases the
`SavaDev.Lab.Processes` library through interactive demo scenarios.
It uses `SavaDev.DemoKit.ConsoleEngine` to present a menu and execute
scenarios in-process.

---

## What it demonstrates

The demo app runs process-launching scenarios to show:

* Real-time output streaming with observers
* Cancellation handling for running processes
* Environment variable injection into child processes
* Non-zero exit code and failure flow handling
* Large output processing and buffering options
* Multiple/chained/parallel process execution
* Shell command launch (`IShellProcessLauncher`)
* Dotnet command launch (`DotNetProcessLauncher`)
* Custom launcher and custom observer integration

---

## How to run

From the repository root:

```bash
dotnet run --project lib/SavaDev.Lab/Processes/demo/SavaDev.Lab.Processes.Demo
```

Then select a scenario from the menu.

---

## Notes

* Most scenarios launch external OS processes via `ProcessLauncher`.
* Output handling is scenario-driven: streaming-only, buffered, or mixed.
* The app itself is intentionally thin: it wires scenarios and starts
  the demo engine.
