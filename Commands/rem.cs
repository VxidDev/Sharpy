using System;
using System.Diagnostics.SymbolStore;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace Sharpy.Commands;
static class Rem {
    public static void Run(string input , Func<string , string[] , bool> CheckIfHelp , Action<string , string> Log , Dictionary<string , string> Vars , Dictionary<string , string> CmdUsage) {
        string[] items = input.Trim().Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["rem"] , "nml");
            return;
        }

        if (CheckIfHelp("rem" , items)) return;

        if (!Vars.TryGetValue(items[0] , out string? num)) { 
            Log("Variable not found!" , "err");
            return;
        }

        if (!int.TryParse(num, out var parsedNum)) {
            Log("Variable is not a number." , "err");
            return;
        }

        if (!int.TryParse(items[1] , out var number)) {
            Log("Division amount is not a number." , "err");
            return;
        }

        Vars["$REM"] = (parsedNum % number).ToString();
    }
}