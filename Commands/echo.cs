using System;
using System.Diagnostics.SymbolStore;
namespace Sharpy.Commands;
static class Echo {
    public static void Run(string input , Func<string , string[] , bool> CheckIfHelp , Dictionary<string , string> Vars , Action<string , string> Log) {
        string[] items = input.Split();

        if (CheckIfHelp("echo", items)) return;

        bool isVar = false;

        if (items.Contains("--var") || items.Contains("-v")) isVar = true;

        if (isVar) {
            if (!Vars.ContainsKey(items[1])) {
                Log("Unknown Variable.", "err");
                return;
            } else {
                Console.WriteLine(Vars[items[1]]);
                return;
            }
        }

        Console.WriteLine(input);
    }
}