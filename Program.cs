using injecenv;
using Pastel;
using System.Drawing;

var data = SaveData.Load();
if (data is null)
{
    SaveData.CreateExample();
    Console.WriteLine("Error, no rule provided. Please create " + SaveData.DataFileName.Pastel(Color.Aqua) + ".\n" +
        "An example is provided in " + SaveData.ExampleDataFileName.Pastel(Color.Aqua) + ".");
    Console.ReadLine();
}
else
{
    var injector = new Injector();
    void CurrentDomain_ProcessExit(object? sender, EventArgs e)
    {
        injector.Revert();
    }
    AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    Console.CancelKeyPress += (sender, e) =>
    {
        injector.Completition.Wait();
        injector.Revert();
        AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
    };
    injector.Run(data);
    AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
    injector.Revert();
    Console.ReadLine();
}