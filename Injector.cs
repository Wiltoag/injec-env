using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace injecenv
{
    internal class Injector
    {
        private bool reverted;

        public Injector()
        {
            Drive = new DriveInfo(Environment.CurrentDirectory);
            ToRevert = new();
            ToDelete = new();
            PathsToRemove = new();
            reverted = true;
            PathEnvironnementVariable = "Path";
            Completition = Task.CompletedTask;
        }

        public Task Completition { get; private set; }
        private DriveInfo Drive { get; }
        private string PathEnvironnementVariable { get; }
        private List<string> PathsToRemove { get; }
        private List<(string Key, string NewValue)> ToDelete { get; }
        private Dictionary<string, (string Old, string New)> ToRevert { get; }

        public void Revert()
        {
            if (!reverted)
            {
                reverted = true;
                Console.WriteLine("Reverting actions...".Pastel(Color.Orange));
                foreach (var todel in ToDelete)
                {
                    var currentValue = Environment.GetEnvironmentVariable(todel.Key, EnvironmentVariableTarget.User);
                    if (currentValue != todel.NewValue)
                    {
                        // The variable has changed by an external tool or something, we don't revert anything
                        Console.WriteLine("Variable " + todel.Key.Pastel(Color.LightYellow) + " has been changed during the runtime, no action is done");
                        continue;
                    }
                    Environment.SetEnvironmentVariable(todel.Key, null, EnvironmentVariableTarget.User);
                    Console.WriteLine("Variable " + todel.Key.Pastel(Color.LightYellow) + " deleted".Pastel(Color.Red));
                }
                ToDelete.Clear();
                foreach (var torev in ToRevert)
                {
                    var currentValue = Environment.GetEnvironmentVariable(torev.Key, EnvironmentVariableTarget.User);
                    if (currentValue != torev.Value.New)
                    {
                        // The variable has changed by an external tool or something, we don't revert anything
                        Console.WriteLine("Variable " + torev.Key.Pastel(Color.LightYellow) + " has been changed during the runtime, no action is done");
                        continue;
                    }
                    Environment.SetEnvironmentVariable(torev.Key, torev.Value.Old, EnvironmentVariableTarget.User);
                    Console.WriteLine("Variable " + torev.Key.Pastel(Color.LightYellow) + " reverted to " + torev.Value.Old.Pastel(Color.LightGreen));
                }
                ToRevert.Clear();
                if (PathsToRemove.Any())
                {
                    var result = Environment.GetEnvironmentVariable(PathEnvironnementVariable, EnvironmentVariableTarget.User) ?? "";
                    foreach (var pathToRemove in PathsToRemove)
                    {
                        result = result.Replace(pathToRemove + Path.PathSeparator, "");
                    }
                    Console.WriteLine("Removed from  " + PathEnvironnementVariable.Pastel(Color.LightYellow) + " :");
                    Console.WriteLine(string.Join("\n", PathsToRemove.Select(a => "    " + a.Pastel(Color.Red))));
                    Environment.SetEnvironmentVariable(PathEnvironnementVariable, result, EnvironmentVariableTarget.User);
                    PathsToRemove.Clear();
                }
                Console.WriteLine("Reverts done".Pastel(Color.LightGreen));
            }
        }

        public void Run(SaveData rules)
        {
            var taskSource = new TaskCompletionSource();
            Completition = taskSource.Task;
            reverted = false;
            Console.WriteLine($"Started relative to {Drive.Name.PastelBg(Color.DarkGreen)} {Drive.VolumeLabel}".Pastel(Color.LightGreen));
            foreach (var setVariable in rules.Variables)
            {
                string value = GetValue(setVariable.Value);
                var initialValue = Environment.GetEnvironmentVariable(setVariable.Key, EnvironmentVariableTarget.User);
                if (initialValue is null)
                {
                    ToDelete.Add((setVariable.Key, value));
                    Console.WriteLine("Variable " + setVariable.Key.Pastel(Color.LightYellow) + " set to " + value.Pastel(Color.LightGreen));
                    Environment.SetEnvironmentVariable(setVariable.Key, value, EnvironmentVariableTarget.User);
                }
                else
                {
                    ToRevert.Add(setVariable.Key, (initialValue, value));
                    Console.WriteLine("Variable " + setVariable.Key.Pastel(Color.LightYellow) + " changed from " +
                        initialValue.Pastel(Color.Red) + " to " + value.Pastel(Color.LightGreen));
                    Environment.SetEnvironmentVariable(setVariable.Key, value, EnvironmentVariableTarget.User);
                }
            }
            if (rules.Paths.Any())
            {
                var result = "";
                foreach (var pathToAdd in rules.Paths)
                {
                    var value = GetValue(pathToAdd);
                    PathsToRemove.Add(value);
                    result += value + Path.PathSeparator;
                }
                result += Environment.GetEnvironmentVariable(PathEnvironnementVariable, EnvironmentVariableTarget.User) ?? "";
                Console.WriteLine("Added to  " + PathEnvironnementVariable.Pastel(Color.LightYellow) + " :");
                Console.WriteLine(string.Join("\n", PathsToRemove.Select(a => "    " + a.Pastel(Color.LightGreen))));
                Environment.SetEnvironmentVariable(PathEnvironnementVariable, result, EnvironmentVariableTarget.User);
            }
            Console.WriteLine($"Injections done".Pastel(Color.LightGreen));
            taskSource.SetResult();
            var clock = new System.Timers.Timer(500)
            {
                AutoReset = true
            };
            void Clock_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
            {
                if (!DriveInfo.GetDrives().Any(drive => drive.Name == Drive.Name))
                {
                    clock.Stop();
                    Console.WriteLine($" {Drive.Name} HAS BEEN REMOVED ".Pastel(Color.White).PastelBg(Color.Red));
                    Revert();
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
            clock.Elapsed += Clock_Elapsed;
            clock.Start();
            do
            {
                if (reverted)
                    break;
                Console.WriteLine("Write " + "exit".Pastel(Color.Red) + " To exit the program, reverting the actions.\n" +
                "SIGTERM and SIGINT also revert it, but to be safe, " + " DO NOT CLOSE THE WINDOW ".Pastel(Color.White).PastelBg(Color.Red) + ".\n" +
                "Ctrl+C should be fine.");
                Console.Write(">");
            }
            while (Console.ReadLine() != "exit");
            clock.Stop();
        }

        private string GetValue(string keyValue)
        {
            string value;
            if (keyValue.StartsWith(":"))
            {
                value = keyValue.Substring(1);
            }
            else
            {
                keyValue = keyValue.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (keyValue.StartsWith(Path.DirectorySeparatorChar) &&
                    !keyValue.StartsWith($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}"))
                {
                    value = Drive.Name.Substring(0, Drive.Name.Length - 1) + keyValue;
                }
                else if (!Path.IsPathRooted(keyValue))
                {
                    value = Path.GetFullPath(keyValue);
                }
                else
                {
                    value = keyValue;
                }
            }
            return value;
        }
    }
}
