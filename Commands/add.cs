using System;
using System.Diagnostics.SymbolStore;
using System.Collections.Generic;

namespace Sharpy.Commands;
static class Add {
    public static void Run(string input , Func<string , string[] , bool> CheckIfHelp , Action<string , string> Log , Dictionary<string , string> Vars , Dictionary<string , string> CmdUsage) {
        string[] items = input.Trim().Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["add"] , "nml");
            return;
        }

        if (CheckIfHelp("add" , items)) return;

        if (!Vars.TryGetValue(items[0] , out string? num)) { 
            Log("Variable not found!" , "err");
            return;
        }

        if (!int.TryParse(num, out var parsedNum)) {
            Log("Variable is not a number." , "err");
            return;
        }

        if (!int.TryParse(items[1] , out var number)) {
            Log("Add amount is not a number." , "err");
            return;
        }

        parsedNum += number;

        Vars[items[0]] = parsedNum.ToString();
    }
}