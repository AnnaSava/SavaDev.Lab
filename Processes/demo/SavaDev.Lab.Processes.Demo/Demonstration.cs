using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Demo.Scenarios;

namespace SavaDev.Lab.Processes.Demo;

internal class Demonstration
{
    /// <summary>
    /// Runs the interactive demo menu and executes
    /// the selected scenarios.
    /// </summary>
    public static async Task RunAsync()
    {
        var scenarios = BuildScenarios();

        var options = new ConsoleDemoOptions
        {
            Title = "SavaDev.Lab.Processes - Demo Scenarios"
        };

        var engine = new ConsoleDemoEngine(options);
        await engine.RunAsync(scenarios);
    }

    /// <summary>
    /// Builds the list of available demo scenarios.
    /// </summary>
    /// <returns>
    /// A read-only list of scenarios displayed in the menu.
    /// </returns>
    private static IReadOnlyList<IConsoleDemoScenario> BuildScenarios()
        => [
            new RealTimeOutputScenario(),
            new CancellationScenario(),
            new EnvironmentVariablesScenario(),
            new CustomObserverScenario(),
            new FailingProcessScenario(),
            new LargeOutputScenario(),
            new StreamingOnlyScenario(),
            new BufferedObserverScenario(),
            new TimingObserverScenario(),
            new MultipleObserversScenario(),
            new ChainedProcessesScenario(),
            new ChainedProcessesWithSharedObserverScenario(),
            new RetryProcessScenario(),
            new ParallelProcessesScenario(),
            new ShellCommandScenario(),
            new DotNetInfoScenario(),
            new MeowProcessScenario(),
        ];
}

