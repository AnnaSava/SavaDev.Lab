// See https://aka.ms/new-console-template for more information

using SavaDev.DemoKit.ConsoleWorker;

await new ConsoleDemoWorker(
    new ConsoleWorkerOptions
    {
        WorkerName = "SavaDev.Lab.Processes.DemoAssets.LongProcess"
    },
    args)
    .RunAsync();
