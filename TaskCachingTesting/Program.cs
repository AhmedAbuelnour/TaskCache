// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


TaskCache.TaskCaching.Current.DefaultInitializer(TimeSpan.FromSeconds(2),3, (e) =>
{
    global::System.Console.WriteLine($"Number Of Tries: {e}");
});

int currentNumberOftries = 0;

await TaskCache.TaskCaching.Current.WrapTaskAsync(new TaskCache.TaskWrapper
{

    WrappedTask = async () =>
    {
        if(currentNumberOftries++ == 2)
        {
            await Task.Delay(1000, new CancellationToken(false));
        }
        else
        {
            await Task.Delay(1000, new CancellationToken(true));
        }
        global::System.Console.WriteLine("Completed!!!");
    }
});

Console.Read();