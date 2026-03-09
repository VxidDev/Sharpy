using System.Security.Cryptography;

namespace Sharpy.Commands;

static class Repeat {
    public static void Run(string input , Action<string , string> Log , Dictionary<string , string> CmdUsage , Func<string , string[] , bool> CheckIfHelp) {
        string[] items = input.Trim().Split();

        if (items.Length == 1 && items[0] == "") {
            Log(CmdUsage["repeat"] , "nml");
            return;
        }

        if (CheckIfHelp("repeat" , items)) return;

        if (!int.TryParse(items[0], out var times)) {
            Log("Invalid amount of repeats passed!", "err");
            return;
        }

        while (times-- > 0) {
            var exec = string.Join(" ", items.Skip(1));
            Sharpy.Program.ParseInput(exec);
        }
    }
}