using System;
using System.Collections.Generic;
using System.IO;

namespace Sharpy.Commands;

static class Write {
    public static void Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> CheckIfHelp) {
        string[] items = input.Split(' ', 2);
        
        if (!(items.Length == 2)) {
            Log(CmdUsage["write"] , "nml");
            return;
        }

        if (CheckIfHelp("write" , items)) return;
        
        string text = string.Join(' ' , items.Skip(1));

        File.WriteAllText(items[0], $"{text}\n");
    }
}