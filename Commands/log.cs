using System;
using System.Collections.Generic;
using System.IO;

namespace Sharpy.Commands;

static class Logger {
    public static void Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> CheckIfHelp) {
        string[] items = input.Split(' ', 2);
        
        if (!(items.Length == 2)) {
            Log(CmdUsage["log"] , "nml");
            return;
        }

        if (CheckIfHelp("log" , items)) return;
        
        string text = string.Join(' ' , items.Skip(1));

        Log(text, items[0]);
    }
}