using System;
using System.Collections.Generic;

namespace Sharpy.Commands;

static class Help {
    public static void Run(string input , Func<string , string[] , bool> CheckIfHelp , Action<string , string> Log , Dictionary<string , string> CmdUsage) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["help"], "nml");
            return;
        }

        if (CheckIfHelp("help" , items)) return;

        if (!CmdUsage.ContainsKey(items[0])) {
            Log($"Cant find '{items[0]}' in the list of built in commands." , "err");
            return;
        }

        Console.WriteLine(CmdUsage[items[0]]);
    }
}