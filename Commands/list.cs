using System;
using System.Threading.Tasks.Dataflow;
using System.IO;

namespace Sharpy.Commands;

static class List {
    public static void Run(string input , Func<string , string[] , bool> CheckIfHelp , string UserName , Action<string , string> Log , Action<string , bool , (int , int , int) , (int , int , int)> PrintGradient) {
        string[] items = input.Split();

        if (input == "") {
            input = Directory.GetCurrentDirectory();
        };

        if (CheckIfHelp("list" , items)) return;

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
}