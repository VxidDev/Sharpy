using System;
namespace Sharpy.Commands;
static class Echo {
    public static void Run(string input , Func<string , string[] , bool> CheckIfHelp) {
        string[] items = input.Split();

        if (CheckIfHelp("echo", items)) return;

        Console.WriteLine(input);
    }
}