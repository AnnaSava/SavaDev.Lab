using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Demo.Launchers;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates a non-standard implementation of
/// <see cref="IProcessLauncher"/> that simulates
/// feline behavior instead of launching real processes.
/// </summary>
/// <remarks>
/// This scenario showcases that the process launcher
/// abstraction is defined by behavior rather than
/// operating system mechanics.
///
/// The underlying launcher emits cat-like output
/// in an infinite loop and relies on cancellation
/// to terminate execution.
///
/// This scenario was inspired by Marta the cat,
/// who actively participated in the design process
/// during an unscheduled keyboard walk.
/// </remarks>
public sealed class MeowProcessScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Meow process (cat-driven launcher)";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var launcher = new MeowProcessLauncher();
        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        Console.WriteLine("Starting meow-driven process...");
        Console.WriteLine("Press Ctrl+C to cancel the cat.");
        Console.WriteLine();
        Console.WriteLine("⚠ Warning: This process was designed by a cat.");
        Console.WriteLine();

        try
        {
            await launcher.RunAsync(
                new ProcessRequest(),
                output,
                ct);

            Console.WriteLine();
            Console.WriteLine("=== Cat process completed ===");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine();
            Console.WriteLine("=== Cat process was cancelled ===");
            Console.WriteLine("The cat has left the keyboard.");
        }
    }
}

