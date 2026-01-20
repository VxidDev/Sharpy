using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Linq;

namespace Sharpy;

class Program {
    static bool IsDebug = false;
    static bool IsSudo => Environment.IsPrivilegedProcess;
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
        { "echo" , "echo:\nUsage: echo [--var/-v] <string>\nPrint desired string to the console."},
        { "read" , "read:\nUsage: read <path>\nRead the file contents."},
        { "list" , "list:\nUsage: list <path>\nPrint the directory's entries." },
        { "help" , "help:\nUsage: help <cmd>\nGet a tutorial on usage of given command."},
        { "alias" , "alias:\nUsage: alias [--remove<name>/--get <name>/<name>=<command>]\nDefine your own shortcut."},
        { "sdb" , "sdb\nUsage: sdb [--toggle/pAliases/pDebug/isSudo/pHistory]\nSharpy's debug commands."},
        { "prompt" , "prompt\nUsage: prompt [--clear/<prompt>]\nChange prompt."},
        { "export" , "export\nUsage: export\nExport your current data(prompt, aliases)."},
        { "history" , "history\nUsage: history [--clear/--pop <amount(int)>]\nManipulate the command history."},
        { "sscript" , "sscript\nUsage: sscript <file>\nExecute your SharpyScript file."},
        { "asv" , "asv\nUsage: asv <varName> <varValue>\nAssign a variable."},
        { "currdir" , "currdir\nUsage: currdir\nPrint current working directory."},
        { "whoami" , "whoami\nUsage: whoami\nPrint current user."},
        { "makedir" , "makedir\nUsage: makedir <directoryName>\nCreate a directory."}
    };

    static List<string> Memory = [""];
    static int PrevMemoryId = 0;

    static string UserName => Environment.UserName;
    static string UserDomainName => Environment.UserDomainName;
    static string prompt = "{userName}@{userDomainName} {currDir} {userStat} > ";
    static string ExportPath => $"/home/{UserName}/.sharpy";

    static Dictionary<string, string> Variables = new() {
        {"$USER" , UserName},
        {"$PROMPT" , prompt},
        {"$HOST" , UserDomainName},
        {"$TIME" , DateTime.Now.ToString("HH:mm:ss")}
    };

    static void CreateConfig() {
        Directory.CreateDirectory($"{ExportPath}");
        var file = File.Create($"{ExportPath}/config.json");
        file.Close();
        Dictionary<string, string> config = new() {
            { "prompt" , prompt }
        };
        File.WriteAllText($"{ExportPath}/config.json" , JsonSerializer.Serialize(config));
    }

    static Program() {
        if (!File.Exists($"{ExportPath}/config.json")) {
            CreateConfig();
        } else {
            string? content = File.ReadAllText($"{ExportPath}/config.json");
            if (content is null || string.IsNullOrWhiteSpace(content)) {
                CreateConfig();
                content = File.ReadAllText($"{ExportPath}/config.json");
            }
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
        if (File.Exists($"{ExportPath}/aliases.json")) {
            string? aliasesData = File.ReadAllText($"{ExportPath}/aliases.json");
            if (aliasesData is null || string.IsNullOrWhiteSpace(aliasesData)) {
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
            File.Create($"{ExportPath}/aliases.json").Close();
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

    static bool CheckIfHelp(string cmd , string[] items) {
        if (items.Contains("--help")) {
            Log(CmdUsage[cmd], "nml");
            return true;
        }

        return false;
    }

    static string CleanUpInput(string input) {
        return string.Join(" ", input.Split()[1..]);
    }

    public static void ParseInput(string input) {
        if (input == "") {
            return;
        }

        Dictionary<string, Action> AvailableCommands = new() {
            { "echo" , () => Sharpy.Commands.Echo.Run(CleanUpInput(input) , CheckIfHelp , Variables , Log) },
            { "clear" , Console.Clear },
            { "exit" , () => Environment.Exit(0) },
            { "list" , () => Sharpy.Commands.List.Run(CleanUpInput(input) , CheckIfHelp , UserName , Log , Sharpy.Helpers.PrintGradient.Run) },
            { "create" , () => Sharpy.Commands.Create.Run(CleanUpInput(input) , Log , CmdUsage , CheckIfHelp) },
            { "remove" , () => Sharpy.Commands.Remove.Run(CleanUpInput(input) , Log , CmdUsage , CheckIfHelp) },
            { "append" , () => Sharpy.Commands.Append.Run(CleanUpInput(input) , Log , CmdUsage , CheckIfHelp) },
            { "changedir" , () => Sharpy.Commands.Changedir.Run(CleanUpInput(input) , Log , CmdUsage , UserName , CheckIfHelp) },
            { "read" , () => Sharpy.Commands.Read.Run(CleanUpInput(input) , Log , CmdUsage , CheckIfHelp) },
            { "help" , () => Sharpy.Commands.Help.Run(CleanUpInput(input) , CheckIfHelp , Log , CmdUsage) },
            { "alias" , () => { Aliases = Sharpy.Shell.Alias.Run(CleanUpInput(input) , Log , CheckIfHelp , CmdUsage , Aliases); } },
            { "sdb" , () => { IsDebug = Sharpy.Shell.Sdb.Run(CleanUpInput(input) , Log , CheckIfHelp , IsDebug , Aliases , IsSudo , CmdUsage , Memory , PrevMemoryId); } },
            { "prompt" , () => { prompt = Sharpy.Shell.Prompt.Run(CleanUpInput(input) , Log , CmdUsage , CheckIfHelp , prompt , Variables); } },
            { "export" , () => Sharpy.Shell.Export.Run(ExportPath , CreateConfig , Log , prompt , IsDebug , Aliases) },
            { "history" , () => { (Memory , PrevMemoryId) = Sharpy.Shell.History.Run(CleanUpInput(input) , Memory , PrevMemoryId , Log , CmdUsage , CheckIfHelp); } },
            { "time" , () => { Sharpy.Commands.Time.Run(CleanUpInput(input) , Log); } },
            { "sscript" , () => { Sharpy.SharpyScript.Interpretator.Run(CleanUpInput(input) , Log , IsDebug , CmdUsage , CheckIfHelp , Variables); }}, 
            { "asv" , () => { Sharpy.SharpyScript.Interpretator.AssignVar(Variables , input , CmdUsage , Log); }},
            { "whoami" , () => { Sharpy.Commands.WhoAmI.Run(UserName); }},
            { "currdir" , () => { Sharpy.Commands.CurrDir.Run(Log); }},
            { "makedir" , () => { Sharpy.Commands.MakeDir.Run(CleanUpInput(input) , CheckIfHelp , Log , CmdUsage); }}
        };

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
            Variables["$TIME"] = DateTime.Now.ToString("HH:mm:ss");

            (string input , PrevMemoryId , Memory) = Sharpy.Helpers.UserInput.Run(prompt , UserName , UserDomainName , IsSudo , PrevMemoryId , Memory);
            ParseInput(input);
        }
    }
}
