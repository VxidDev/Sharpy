using System;
using System.Collections.Generic;
using System.IO;

namespace Sharpy.Commands;

static class Append {
    public static void Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> CheckIfHelp) {
        string[] items = input.Split(' ', 2);
        
        if (!(items.Length == 2)) {
            Log(CmdUsage["append"] , "nml");
            return;
        }

        if (CheckIfHelp("run" , items)) return;

        File.AppendAllText(items[^2], $"{items[^1]}\n");
    }
}