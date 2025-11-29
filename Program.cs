using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.FileIO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Net;
using System.Security.Cryptography;
using System.Security.AccessControl;

class Program {
    static bool IsDebug = false;
    static bool IsSudo = Environment.IsPrivilegedProcess;
    static Dictionary<string , Action> AvailableArgs => new() {
        { "-d" , () => { IsDebug = true; } },
        { "--debug" , () => { IsDebug = true; } }
    };

    static Dictionary<string, string> Aliases = [];

    static Dictionary<string, string> CmdUsage => new() {
        { "append" , "append:\nUsage: append <filename> <content>\nAppend string to the file." },
        { "create" , "create:\nUsage: create <filename>\nCreate empty file." },
        { "remove" , "remove:\nUsage: remove [--force] <filename>\nRemove file/directory." },
        { "changedir" , "changedir:\nUsage: changedir <path>\nChange working directory."},
        { "echo" , "echo:\nUsage: echo <string>\nPrint desired string to the console."},
        { "read" , "read:\nUsage: read <path>\nRead the file contents."},
        { "list" , "list:\nUsage: list <path>\nPrint the directory's entries." },
        { "help" , "help:\nUsage: help <cmd>\nGet a tutorial on usage of given command."},
        { "alias" , "alias:\nUsage: alias [--remove<name>/--get <name>/<name>=<command>]\nDefine your own shortcut."},
        { "sdb" , "sdb\nUsage: sdb [--toggle]\nSharpy's debug commands."},
        { "smod" , "smod\nUsage: smod [--toggle]\nEnable sudo mode."},
        { "prompt" , "prompt\nUsage: prompt [--clear/<prompt>]\nChange prompt."},
        { "export" , "export\nUsage: export\nExport your current data(prompt, aliases)."}
    };

    static List<string> Memory = [""];
    static int PrevMemoryId = 0;

    static string UserName => Environment.UserName;
    static string UserDomainName => Environment.UserDomainName;
    static string prompt = "{userName}@{userDomainName} {currDir} {userStat} > ";
    static string exportPath = $"/home/{UserName}/.sharpy";

    static void CreateConfig() {
        Directory.CreateDirectory($"{exportPath}");
        var file = File.Create($"{exportPath}/config.json");
        file.Close();
        Dictionary<string, string> config = new() {
            { "prompt" , prompt }
        };
        File.WriteAllText($"{exportPath}/config.json" , JsonSerializer.Serialize(config));
    }

    static Program() {
        if (!File.Exists($"{exportPath}/config.json")) {
            CreateConfig();
        } else {
            string? content = File.ReadAllText($"{exportPath}/config.json");
            Dictionary<string, object>? data = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            if (data is null) {
                if (IsDebug) {
                    Log("Failed to load data from config." , "err");
                }
                return;
            }
            data.TryGetValue("prompt" , out object? pprompt);
            if (pprompt is null || pprompt.ToString() is null) {
                if (IsDebug) {
                    Log("Failed to get 'prompt' from config.", "err");
                }
                return;
            }
            prompt = pprompt.ToString()!;
        } 
        if (File.Exists($"{exportPath}/aliases.json")) {
            string? aliasesData = File.ReadAllText($"{exportPath}/aliases.json");
            if (aliasesData is null) {
                if (IsDebug) {
                    Log("Failed to load aliases.", "err");
                }
                return;
            }
            Dictionary<string, string>? aliases = JsonSerializer.Deserialize<Dictionary<string , string>>(aliasesData);
            if (aliases is null) {
                if (IsDebug) {
                    Log("Failed to load aliases." , "err");
                }
                return;
            }
            Aliases = aliases;
        } else {
            File.Create($"{exportPath}/aliases.json").Close();
        }
    }

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

    static (int cursorPos , int rcursorPos) UserInput_rightArrow_(int cursorPos , int rcursorPos) {
        cursorPos++;
        rcursorPos++;

        Console.SetCursorPosition(rcursorPos, Console.CursorTop);

        return ( cursorPos , rcursorPos );
    }

    static (int cursorPos , int rcursorPos) UserInput_leftArrow_(int cursorPos , int rcursorPos) {
        cursorPos--;
        rcursorPos--;

        Console.SetCursorPosition(rcursorPos, Console.CursorTop);

        return ( cursorPos , rcursorPos );
    }

    static (int cursorPos , int rcursorPos) UserInput__SetBufferToMemory_(List<char> buffer , int msgLength) {
        string memory = Memory[PrevMemoryId];

        Console.SetCursorPosition(msgLength, Console.CursorTop);
        Console.Write(new string(' ', buffer.Count));
        Console.SetCursorPosition(msgLength, Console.CursorTop);

        buffer.Clear();
        buffer.AddRange(memory);
        Console.Write(memory);

        int cursorPos = buffer.Count;
        int rcursorPos = msgLength + cursorPos;

        return (cursorPos, rcursorPos);
    }

    static (int cursorPos , int rcursorPos) UserInput__AddChar_(List<char> buffer , int cursorPos , int rcursorPos , char key) {
        buffer.Insert(cursorPos, key);
        cursorPos++;
        
        Console.SetCursorPosition(rcursorPos, Console.CursorTop);
        Console.Write(new string([.. buffer.Skip(cursorPos - 1)]));
        
        rcursorPos++;

        return (cursorPos, rcursorPos);
    }

    static string UserInput() { // Scary function, optimization needed.
        string userStat;
        if (IsSudo) {
            userStat = "#";
        } else {
            userStat = "$";
        }
        string msg = prompt
            .Replace("{userName}", UserName)
            .Replace("{userDomainName}", UserDomainName)
            .Replace("{currDir}" , Environment.CurrentDirectory.Replace($"/home/{UserName}" , "~"))
            .Replace("{userStat}" , userStat);
        Console.Write(msg);
        var buffer = new List<char>();
        int rcursorPos = msg.Length;
        int cursorPos = 0;
        while (true) {
            if (Console.KeyAvailable) {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && rcursorPos >= cursorPos && cursorPos > 0) {
                    cursorPos--;
                    rcursorPos--;
                    buffer.RemoveAt(cursorPos);

                    Console.SetCursorPosition(rcursorPos , Console.CursorTop);
                    Console.Write(new string([.. buffer.Skip(cursorPos)]) + " ");
                    Console.SetCursorPosition(rcursorPos, Console.CursorTop);

                    // Console.Write($"r: {rcursorPos} , n: {cursorPos}");
                } else if (key.Key == ConsoleKey.LeftArrow && rcursorPos >= cursorPos && cursorPos > 0) {
                    (cursorPos , rcursorPos) = UserInput_leftArrow_(cursorPos , rcursorPos);
                } else if (key.Key == ConsoleKey.RightArrow && rcursorPos >= cursorPos && cursorPos >= 0 && cursorPos < buffer.Count) {
                    (cursorPos , rcursorPos) = UserInput_rightArrow_(cursorPos , rcursorPos);
                } else if (key.Key == ConsoleKey.UpArrow && PrevMemoryId > 0) {
                    (cursorPos , rcursorPos) = UserInput__SetBufferToMemory_(buffer, msg.Length);
                    PrevMemoryId--;
                } else if (key.Key == ConsoleKey.DownArrow && PrevMemoryId + 1 != Memory.Count) {
                    PrevMemoryId++;
                    (cursorPos , rcursorPos) = UserInput__SetBufferToMemory_(buffer, msg.Length);
                }
                else if (key.KeyChar != '\0' && key.Key != ConsoleKey.Backspace) {
                    (cursorPos, rcursorPos) = UserInput__AddChar_(buffer, cursorPos, rcursorPos, key.KeyChar);
                    Console.SetCursorPosition(msg.Length + cursorPos, Console.CursorTop);
                }
            }
        }
        Memory.Add(new string([.. buffer]));
        PrevMemoryId = Memory.Count - 1;
        return new string([.. buffer]);
    }

    static void PrintGradient(string text, bool bold , (int R, int G, int B) start, (int R, int G, int B) end) {
        int len = text.Length;

        if (bold) {
            Console.Write("\u001b[1m");
        }

        for (int i = 0; i < len; i++) {
            float t = (float)i / (len - 1);

            int r = (int)(start.R + (end.R - start.R) * t);
            int g = (int)(start.G + (end.G - start.G) * t);
            int b = (int)(start.B + (end.B - start.B) * t);

            Console.Write($"\u001b[38;2;{r};{g};{b}m{text[i]}");
        }

        Console.Write("\u001b[0m\n");
    }

    static bool CheckIfHelp(string cmd , string[] items) {
        if (items.Contains("--help")) {
            Log(CmdUsage[cmd], "nml");
            return true;
        }

        return false;
    }

    static void Echo(string input) {
        string[] items = input.Split();

        if (CheckIfHelp("echo" , items)) return;

        Console.WriteLine(input);
    }

    static string CleanUpInput(string input) {
        return string.Join(" ", input.Split()[1..]);
    }

    static void List(string input) {
        string[] items = input.Split();

        if (CheckIfHelp("list" , items)) return;

        if (input == "") {
            input = Directory.GetCurrentDirectory();
        };

        input = input.Replace("~", $"/home/{UserName}/");

        if (!Directory.Exists(input)) {
            Log("Unknown directory/file!", "err");
            return;
        }

        try {
            foreach (string item in Directory.EnumerateFileSystemEntries(input)) {
                string output = item.Split("/")[^1];

                if (Directory.Exists(item)) {
                    PrintGradient(output , true , (153,204,255) , (77,77,255));
                    continue;
                }

                Console.WriteLine($"\u001b[1m{output}\u001b[0m");
            }
        } catch (UnauthorizedAccessException) {
            Log("Access denied.", "err");
        }
    }

    static void Create(string input) {
        string[] items = input.Trim().Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["create"] , "nml");
            return;
        }

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

    static void Remove(string input) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["remove"] , "nml");
            return;
        }

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

        File.AppendAllText(items[^2], $"{items[^1]}\n");
    }

    static void Changedir(string input) {
        string[] items = input.Split(' ' , 2 , StringSplitOptions.RemoveEmptyEntries);

        if (!(items.Length == 1)) {
            Log(CmdUsage["changedir"] , "nml");
            return;
        }

        items[0] = items[0].Replace("~", $"/home/{UserName}/");

        if (!Directory.Exists(items[0])) {
            Log("Directory not found", "err");
            return;
        }

        Directory.SetCurrentDirectory(items[0]);
    }

    static void Read(string input) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["read"], "nml");
            return;
        }

        var file = items.Last().Replace("~", "/home/vxid-dev/");

        if (!File.Exists(file)) {
            Log("File not found.", "err");
            return;
        }

        Console.WriteLine(File.ReadAllText(file));
    }

    static void Help(string input) {
        string[] items = input.Split();

        if (CheckIfHelp("help" , items)) return;

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["help"], "nml");
            return;
        }

        if (!CmdUsage.ContainsKey(items[0])) {
            Log($"Cant find '{items[0]}' in the list of built in commands." , "err");
            return;
        }

        Console.WriteLine(CmdUsage[items[0]]);
    }

    static void Alias(string input) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["alias"], "nml");
            return;
        }

        if (CheckIfHelp("alias" , items)) return;

        if (items.Contains("--remove")) {
            if (items.Length < 2) {
                Log("Invalid arguments.", "err");
                return;
            }

            string rmtarget = items[1];
            
            if (!Aliases.ContainsKey(rmtarget)) {
                Log("Unknown alias.", "err");
                return;
            }

            Aliases.Remove(rmtarget);
            Log($"Alias '{rmtarget}' successfully removed.", "nml");
            return;
        }

        if (items.Contains("--get")) {
            try {
                // Console.WriteLine(list[0]);
                Console.WriteLine(Aliases[items[1]]);
                return;
            } catch (KeyNotFoundException) {
                Log("Unknown alias.", "err");
                return;
            } catch (ArgumentOutOfRangeException) {
                Log("Invalid arguments." , "err");
                return;
            }
        }

        try {
            string[] alias = items[0].Split('=');
            string key = alias[0];
            string value = alias[1].Replace('+', ' ');
            if (key == value) {
                Log("Infinity recursion detected.", "wrn");
            }
            if (!Aliases.TryAdd(key, value)) {
                Log("Alias already exists.", "err");
                return;
            }

            Log($"Alias '{key}' successfully created.", "nml");
        } catch (IndexOutOfRangeException) {
            Log("Invalid arguments.", "err");
            return;
        }
    }

    static void Sdb(string input) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["sdb"], "nml");
            return;
        }

        if (CheckIfHelp("sdb" , items)) return;

        if (items.Contains("--toggle")) {
            IsDebug = !IsDebug;
            Log($"Debug: {IsDebug}" , "nml");
            return;
        }

        if (items.Contains("pAliases")) {
            if (!IsDebug) {
                Log("Debug required.", "err");
                return;
            }
            if (Aliases.Keys.Count == 0) {
                Log(" -~- No aliases -~-", "nml");
            }
            foreach (string key in Aliases.Keys) {
                Console.WriteLine($"{key} = {Aliases[key]}");
            }
            return;
        }

        if (items.Contains("isSudo")) {
            Log($"Official: {Environment.IsPrivilegedProcess} Program: {IsSudo}" , "nml");
        }
    }

    static void Prompt(string input) {
        string[] items = input.Split();

        if (items.Length == 0 && items[0] == "") {
            Log(CmdUsage["prompt"], "nml");
            return;
        }

        if (CheckIfHelp("prompt" , items)) return;

        if (items.Contains("--clear")) {
            prompt = "{userName}@{userDomainName} {currDir} {userStat} > ";
            Log("Successfully cleared out the prompt.", "nml");
            return;
        }

        prompt = items[0].Replace('+', ' ');
    }

    static void Export() {
        // string[] items = input.Split();

        string oldText = File.ReadAllText($"{exportPath}/config.json");
        Dictionary<string, object>? oldData = null;
        int tries = 0;
        while (tries < 3) {
            try {
                tries++;
                oldData = JsonSerializer.Deserialize<Dictionary<string, object>>(oldText!);
                break;
            }
            catch (JsonException) {
                CreateConfig();
                return;
            }
        }

        if (oldData is null) {
            Log("Failed to update config." , "err");
            return;
        }

        oldData["prompt"] = prompt;

        File.WriteAllText($"{exportPath}/config.json", JsonSerializer.Serialize(oldData));

        tries = 0;
        Dictionary<string, string>? oldAliases = null;
        while (tries < 3) {
            try {
                tries++;
                string? text = File.ReadAllText($"{exportPath}/aliases.json");
                if (text is null) continue;
                oldAliases = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
            } catch (JsonException) {
                File.Create($"{exportPath}/aliases.json").Close();
                File.WriteAllText($"{exportPath}/aliases.json", JsonSerializer.Serialize<Dictionary<string, string>>([]));
            } catch (FileNotFoundException) {
                File.Create($"{exportPath}/aliases.json").Close();
                File.WriteAllText($"{exportPath}/aliases.json", JsonSerializer.Serialize<Dictionary<string, string>>([]));
            }
        }

        if (oldAliases is null) {
            if (IsDebug) {
                Log("Failed to export aliases.", "err");
            }
            return;
        }

        oldAliases = Aliases;

        File.WriteAllText($"{exportPath}/aliases.json", JsonSerializer.Serialize(oldAliases));
    }

    static void ParseInput(string input) {
        Dictionary<string, Action> AvailableCommands = new() {
            { "echo" , () => Echo(CleanUpInput(input)) },
            { "clear" , Console.Clear },
            { "exit" , () => Environment.Exit(0) },
            { "list" , () => List(CleanUpInput(input)) },
            { "create" , () => Create(CleanUpInput(input)) },
            { "remove" , () => Remove(CleanUpInput(input)) },
            { "append" , () => Append(CleanUpInput(input)) },
            { "changedir" , () => Changedir(CleanUpInput(input)) },
            { "read" , () => Read(CleanUpInput(input)) },
            { "help" , () => Help(CleanUpInput(input)) },
            { "alias" , () => Alias(CleanUpInput(input)) },
            { "sdb" , () => Sdb(CleanUpInput(input)) },
            { "prompt" , () => Prompt(CleanUpInput(input)) },
            { "export" , Export }
        };

        if (input == "") {
            return;
        }

        try {
            // Console.WriteLine(input.Split()[0]);
            AvailableCommands[input.Split()[0]]();
        } catch (KeyNotFoundException) {
            if (Aliases.ContainsKey(input.Split()[0])) {
                ParseInput(Aliases[input.Split()[0]]);
                return;
            }
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string program = parts[0];
            string args = string.Join(' ', parts.Skip(1));
            Process? process;
            try {
                process = Process.Start(program, args);
            } catch (Exception) {
                process = null;
            }

            if (!(process == null)) {
                process.WaitForExit();
            } else {
                Log("Unknown command!" , "err");
            }
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
