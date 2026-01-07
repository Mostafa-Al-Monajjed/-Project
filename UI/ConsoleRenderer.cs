using System;
using Spectre.Console;

namespace StoreManagement.UI
{
    public static class ConsoleRenderer
    {
        public static void ShowError(string message)
        {
            AnsiConsole.Write(new Panel($"[red]{message}[/]")
                .Header("Error")
                .BorderColor(Color.Red));
        }

        public static void ShowSuccess(string message)
        {
            AnsiConsole.Write(new Panel($"[green]{message}[/]")
                .Header("Success")
                .BorderColor(Color.Green));
        }

        public static void ShowWarning(string message)
        {
            AnsiConsole.Write(new Panel($"[yellow]{message}[/]")
                .Header("Warning")
                .BorderColor(Color.Yellow));
        }

        public static void ShowInfo(string message)
        {
            AnsiConsole.Write(new Panel($"[blue]{message}[/]")
                .Header("Information")
                .BorderColor(Color.Blue));
        }

        public static void PrintHeader(string title)
        {
            AnsiConsole.Write(new Rule($"[blue]{title}[/]").LeftJustified());
        }

        public static void Pause()
        {
            AnsiConsole.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }
}
