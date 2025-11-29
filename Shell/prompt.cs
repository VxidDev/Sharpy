using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharpy.Shell;

static class Prompt {
    public static string Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> CheckIfHelp , string prompt) {
        string[] items = input.Split();

        if (items.Length == 0 && items[0] == "") {
            Log(CmdUsage["prompt"], "nml");
            return prompt;
        }

        if (CheckIfHelp("prompt" , items)) return prompt;

        if (items.Contains("--clear")) {
            prompt = "{userName}@{userDomainName} {currDir} {userStat} > ";
            Log("Successfully cleared out the prompt.", "nml");
            return prompt;
        }

        prompt = items[0].Replace('+', ' ');
        return prompt;
    }
}