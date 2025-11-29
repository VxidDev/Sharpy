using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sharpy.Commands;

static class Read {
    public static void Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> checkIfHelp) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["read"], "nml");
            return;
        }

        if (checkIfHelp("read" , items)) return;

        var file = items.Last().Replace("~", "/home/vxid-dev/");

        if (!File.Exists(file)) {
            Log("File not found.", "err");
            return;
        }

        Console.WriteLine(File.ReadAllText(file));
    }
}