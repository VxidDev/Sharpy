using System;
using System.Runtime.CompilerServices;

namespace Sharpy.Commands;

static class MakeDir {
    public static void Run(string input , Func<string , string[] , bool> CheckIfHelp , Action<string , string> Log , Dictionary<string , string> CmdUsage) {
        string[] items = input.Split();

        if (CheckIfHelp("makedir" , items)) return;

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["makedir"], "nml");
            return;
        }
        
        Directory.CreateDirectory(items[0]);
    }
}