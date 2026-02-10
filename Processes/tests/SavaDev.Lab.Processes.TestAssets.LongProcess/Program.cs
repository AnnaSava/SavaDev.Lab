/// <summary>
/// Entry point for the test asset worker process.
/// </summary>
/// <remarks>
/// This application acts as a controllable external process
/// used by automated tests for the
/// <c>SavaDev.Lab.Processes</c> library.
///
/// The process is intentionally simple and long-running:
/// it periodically writes messages to standard output and
/// runs until a cancellation signal is received.
///
/// The primary purpose of this executable is to support
/// integration and behavioral tests that require:
/// <list type="bullet">
/// <item>a real external process</item>
/// <item>continuous output streaming</item>
/// <item>deterministic cancellation behavior</item>
/// </list>
///
/// This project is not a demo scenario and is not intended
/// for direct user interaction.
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Starts the test asset worker process.
    /// </summary>
    /// <param name="args">
    /// Command-line arguments. Currently ignored.
    /// </param>
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            // Prevent immediate process termination and
            // translate Ctrl+C into cooperative cancellation.
            e.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine("Test asset worker started.");
        Console.WriteLine("Press Ctrl+C to request cancellation.");
        Console.WriteLine();

        try
        {
            await RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine();
            Console.WriteLine("Test asset worker cancelled.");
        }
    }

    /// <summary>
    /// Runs the main worker loop.
    /// </summary>
    /// <param name="ct">
    /// A cancellation token used to stop the worker loop.
    /// </param>
    private static async Task RunAsync(CancellationToken ct)
    {
        var counter = 0;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            counter++;

            Console.WriteLine($"Tick {counter}");

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                ct);
        }
    }
}
