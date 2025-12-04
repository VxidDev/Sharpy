using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace Sharpy.SharpyScript;

static class Interpretator {
    public static bool AssignVar(Dictionary<string , string> Vars , string line) {
        string[] items = line.Split();

        if (items.Length != 3) {
            return false;
        }

        Vars[items[1]] = items[2];
        return true;
    }

    static bool Equals(string x , string y)  {
        return x == y;
    }

    public static (bool , string) If(Dictionary<string , string> Vars , string line , bool IsDebug) {
        string[] items = line.Split();

        if (items.Length < 5) { // 1: if , 2: <logic> , 3: <arg1> , 4: <arg2> , 5: <alias/command>
            return (false , "arg count");
        }

        if (!Vars.TryGetValue(items[2] , out string? arg1) || !Vars.TryGetValue(items[3] , out string? arg2)) return (false , "unknown vars");

        if (arg1 is null || arg2 is null) return (false , "Null args");

        Dictionary<string, Func<bool>> Logics = new() {
            { "equals" , () => Equals(arg1 , arg2) }
        };

        if (Logics.TryGetValue(items[1] , out var func)) {
            if (func() is true) {
                string command = string.Join(' ' , items.Skip(4));
                if (IsDebug) Console.WriteLine($"Command: {command}");
                Sharpy.Program.ParseInput(command);
                return (true , "success");
            }
        }

        return (true , "statement is false");
    }

    public static void Run(string file , Action<string , string> Log , bool isDebug , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> CheckIfHelp , Dictionary<string , string> Vars) {
        if (file.Split().Length == 1 && file.Split()[0] == "") {
            Log(CmdUsage["sscript"] , "nml");
            return;
        }

        if (CheckIfHelp("sscript" , file.Split())) return;

        if (!File.Exists(file)) {
            Log("File not found." , "err");
            return;
        }

        if (file.Split('.')[1] != "ss") {
            Log("File must have '.ss' extension.", "err");
            return;
        }

        if (isDebug) {
            Log("Loading code...", "nml");
        }

        string[] lines = File.ReadAllLines(file);
        int currLine = 0;

        foreach (string line in lines) {
            currLine++;
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

            if (line.StartsWith("asv")) {
                if (!AssignVar(Vars , line)) {
                    Console.WriteLine($"SScript: Syntax error @ line: {currLine}\nasv <varName> <varValue>");
                    return;
                }
            } else if (line.StartsWith("if")) {
                (bool state, string msg) = If(Vars, line , isDebug);
                if (!state) {
                    Console.WriteLine($"SScript: Syntax error @ line: {currLine}\nIF: {msg}");
                    return;
                }
                continue;
            }

            Sharpy.Program.ParseInput(line);
        }
    }
}