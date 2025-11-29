using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharpy.Shell;

static class Alias {
    public static Dictionary<string , string> Run(string input , Action<string , string> Log , Func<string , string[] , bool> CheckIfHelp , Dictionary<string , string> CmdUsage  , Dictionary<string , string> Aliases) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["alias"], "nml");
            return Aliases;
        }

        if (CheckIfHelp("alias" , items)) return Aliases;

        if (items.Contains("--remove")) {
            if (items.Length < 2) {
                Log("Invalid arguments.", "err");
                return Aliases;
            }

            string rmtarget = items[1];
            
            if (!Aliases.ContainsKey(rmtarget)) {
                Log("Unknown alias.", "err");
                return Aliases;
            }

            Aliases.Remove(rmtarget);
            Log($"Alias '{rmtarget}' successfully removed.", "nml");
            return Aliases;
        }

        if (items.Contains("--get")) {
            try {
                // Console.WriteLine(list[0]);
                Console.WriteLine(Aliases[items[1]]);
                return Aliases;
            } catch (KeyNotFoundException) {
                Log("Unknown alias.", "err");
                return Aliases;
            } catch (ArgumentOutOfRangeException) {
                Log("Invalid arguments." , "err");
                return Aliases;
            }
        }

        try {
            string[] alias = items[0].Split('=');
            string key = alias[0];
            string value = alias[1].Replace('+', ' ');
            if (key == value) {
                Log("Infinity recursion detected.", "wrn");
            }
            if (CmdUsage.ContainsKey(key)) {
                Log("Command with this name already exists.", "err");
                return Aliases;
            }
            if (!Aliases.TryAdd(key, value)) {
                Log("Alias already exists.", "err");
                return Aliases;
            }

            Log($"Alias '{key}' successfully created.", "nml");
        } catch (IndexOutOfRangeException) {
            Log("Invalid arguments.", "err");
            return Aliases;
        }

        return Aliases;
    }
}