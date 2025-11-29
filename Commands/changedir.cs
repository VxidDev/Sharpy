using System;
using System.IO;
using System.Collections.Generic;

namespace Sharpy.Commands;

static class Changedir {
    public static void Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , string UserName , Func<string , string[] , bool> CheckIfHelp) {
        string[] items = input.Split(' ' , 2 , StringSplitOptions.RemoveEmptyEntries);

        if (!(items.Length == 1)) {
            Log(CmdUsage["changedir"] , "nml");
            return;
        }

        if (CheckIfHelp("run" , items)) return;

        items[0] = items[0].Replace("~", $"/home/{UserName}/");

        if (!Directory.Exists(items[0])) {
            Log("Directory not found", "err");
            return;
        }

        Directory.SetCurrentDirectory(items[0]);
    }
}