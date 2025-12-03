using System;
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
            }
            Sharpy.Program.ParseInput(line);
        }
    }
}
