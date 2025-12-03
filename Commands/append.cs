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

        if (CheckIfHelp("append" , items)) return;
        
        string text = string.Join(' ' , items.Skip(1));

        File.AppendAllText(items[0], $"{text}\n");
    }
}