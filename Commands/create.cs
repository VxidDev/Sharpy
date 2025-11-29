using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sharpy.Commands;

static class Create {
    public static void Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> CheckIfHelp) {
        string[] items = input.Trim().Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["create"] , "nml");
            return;
        }

        if (CheckIfHelp("create" , items)) return;

        bool isDir = false;

        if (items.Contains("-d")) isDir = true;

        if (!isDir) {
            try {
                var file = File.Create(items.Last());
                file.Close();
            } catch (ArgumentException) {
                Log("Invalid arguments.", "err");
            }
            return;
        }

        Directory.CreateDirectory(items.Last());
    }
}