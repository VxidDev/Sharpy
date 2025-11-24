using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

class Program {
    static bool IsDebug;
    static Dictionary<string , Action> AvailableArgs => new() {
        { "-d" , () => { IsDebug = true; } },
        { "--debug" , () => { IsDebug = true; } }
    };

    static Dictionary<string, string> CmdUsage => new() {
        { "append" , "append:\nUsage: append <filename> <content>" },
        { "create" , "create:\nUsage: create <filename> "}
    };

    static void Log(string text , string state) {
        Dictionary<string, string> colors = new() {
            { "nml" , "\u001b[1;37m"},
            { "err" , "\u001b[1;31m" },
            { "wrn" , "\u001b[1;33m" }
        };
        Console.WriteLine($"{colors[state]}[ Sharpy ] {text}\u001b[0m");
    }

    static void ParseArgs(string[] args) {
        foreach (string arg in args) {
            try {
                AvailableArgs[arg]();
            } catch (KeyNotFoundException) {
                Log($"Unknown parameter: {arg}" , "err");
                Environment.Exit(1);
            }
        }
    }

    static string UserInput() {
        Console.Write("> ");
        string? input = Console.ReadLine();
        
        if (input is null) {
            if (IsDebug) {
                Log("User input is null, returning empty string instead..." , "wrn");
            }
            return "";
        }

        return input;
    }
    
    static void Echo(string input) {
        Console.WriteLine(input);
    }

    static string CleanUpInput(string input) {
        return string.Join(" ", input.Split()[1..]);
    }

    static void List(string input) {
        if (input == "") {
            input = Directory.GetCurrentDirectory();
        };

        if (!Directory.Exists(input)) {
            Log("Unknown directory/file!", "err");
            return;
        }

        foreach (string item in Directory.EnumerateFileSystemEntries(input)) {
            string output = item.Split("/")[^1];

            if (Directory.Exists(item)) {
                output = $"\u001b[1;34m{output}\u001b[0m";
            }

            Console.WriteLine(output);
        }
    }

    static void Create(string input) {
        string[] items = input.Split();

        // Console.WriteLine(items.Length);

        if (items.Length == 1) {
            Log(CmdUsage["create"] , "nml");
            return;
        }

        bool isDir = false;
        foreach (string item in items) {
            if (item == "-d") {
                isDir = true;
                break;
            }
        }
        if (!isDir) {
            File.Create(items[^1]);
            return;
        }
        Directory.CreateDirectory(items[^1]);
    }

    static void Remove(string input) {
        string[] items = input.Split();

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
    
    static void Append(string input) {
        string[] items = input.Split(' ', 2);
        
        if (!(items.Length == 2)) {
            Log(CmdUsage["append"] , "nml");
            return;
        }

        File.AppendAllText(items[^2], items[^1]);
    }

    static void ParseInput(string input) {
        Dictionary<string, Action> AvailableCommands = new() {
            { "echo" , () => Echo(CleanUpInput(input))},
            { "clear" , Console.Clear},
            { "exit" , () => Environment.Exit(0)},
            { "list" , () => List(CleanUpInput(input))},
            { "create" , () => Create(CleanUpInput(input))},
            { "remove" , () => Remove(CleanUpInput(input))},
            { "append" , () => Append(CleanUpInput(input))}
        };

        try {
            // Console.WriteLine(input.Split()[0]);
            AvailableCommands[input.Split()[0]]();
        } catch (KeyNotFoundException) {
            Log("Unknown command!" , "err");
        }
    }

    static void Main(string[] args) {
        Console.Clear();
        ParseArgs(args);

        while (true) {
            string input = UserInput();
            ParseInput(input);
        }
    }
}
