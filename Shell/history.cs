using System;
using System.Reflection.Metadata.Ecma335;
using Sharpy.Helpers;

namespace Sharpy.Shell;

static class History {
    public static (List<string> , int) Run(string input , List<string> Memory , int PrevMemoryId , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> CheckIfHelp) {
        string[] items = input.Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["history"] , "nml");
            return (Memory , PrevMemoryId);
        }

        if (CheckIfHelp("history" , items)) return (Memory , PrevMemoryId);
        
        if (items.Contains("--clear")) {
            Memory = [""];
            PrevMemoryId = 0;
            return (Memory , PrevMemoryId);
        }

        if (items.Contains("--pop")) {
            if (int.TryParse(items[1] , out int amount)) {
                int removed = 0;
                for (int i = amount; i >= 0; i--) {
                    if (!(Memory.Count > 0)) {
                        Log($"Removed only {removed} items. Reason: empty" , "err");
                        return (Memory , PrevMemoryId);
                    }
                    Memory.RemoveAt(Memory.Count - 1);
                    PrevMemoryId--;
                    removed++;
                }

                return (Memory , PrevMemoryId);
            } else {
                Log("Invalid arguments passed.", "err");
            }
        }

        return (Memory , PrevMemoryId);
    }
}