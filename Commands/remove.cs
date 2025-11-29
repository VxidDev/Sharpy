using System;
using System.IO;
using System.Collections.Generic;

namespace Sharpy.Commands;

static class Remove {
    public static void Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> checkIfHelp) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["remove"] , "nml");
            return;
        }

        if (checkIfHelp("remove" , items)) return;

        bool force = false;
        foreach (string item in items) {
            if (item == "--force") {
                force = true;
            }
        }

        if (force) {
            try {
                if (Directory.Exists(items[^1])) {
                    try {
                        Directory.Delete(items[^1] , true);
                    } catch (DirectoryNotFoundException) {
                    }
                }
                File.Delete(input);
            } catch (UnauthorizedAccessException) {
                Log("Access denied." , "err");
            }
            return;
        }

        if (!File.Exists(items[^1])) {
            Log("File not found.", "err");
            return;
        }

        while (true) {
            Console.Write("Are you sure you want to delete this file? Y/N: ");
            string? userInput = Console.ReadLine();
            
            if (userInput is null) {
                continue;
            }

            if (userInput.Equals("y" , StringComparison.OrdinalIgnoreCase)) {
                File.Delete(items[^1]);
                return;
            } else if (userInput.Equals("n" , StringComparison.OrdinalIgnoreCase)) {
                return;
            } else {
                continue;
            }
        }
    }
}