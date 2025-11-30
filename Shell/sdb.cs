using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;

namespace Sharpy.Shell;

static class Sdb {
    public static bool Run(string input , Action<string , string> Log , Func<string , string[] , bool> CheckIfHelp , bool IsDebug , Dictionary<string , string> Aliases , bool IsSudo , Dictionary<string , string> CmdUsage , List<string> Memory , int PrevMemoryId) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["sdb"], "nml");
            return IsDebug;
        }

        if (CheckIfHelp("sdb" , items)) return IsDebug;

        if (items.Contains("--toggle")) {
            IsDebug = !IsDebug;
            Log($"Debug: {IsDebug}" , "nml");
            return IsDebug;
        }

        if (items.Contains("pDebug")) {
            Log($"{IsDebug}" , "nml");
            return IsDebug;
        }

        if (items.Contains("pAliases")) {
            if (!IsDebug) {
                Log("Debug required.", "err");
                return IsDebug;
            }
            if (Aliases.Keys.Count == 0) {
                Log(" -~- No aliases -~-", "nml");
            }
            foreach (string key in Aliases.Keys) {
                Console.WriteLine($"{key} = {Aliases[key]}");
            }
            return IsDebug;
        }

        if (items.Contains("isSudo")) {
            Log($"Official: {Environment.IsPrivilegedProcess} | Program: {IsSudo}" , "nml");
            return IsDebug;
        }
        if (items.Contains("pHistory")) {
            for (int i = 0; i < Memory.Count; i++) {
                Console.WriteLine($"[ID: {i}] {Memory[i]}");
            }
            Console.WriteLine($"PrevMemoryID: {PrevMemoryId}");
            return IsDebug;
        }

        return IsDebug;
    }
}
