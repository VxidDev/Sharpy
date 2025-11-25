using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

class Program {
    static bool IsDebug;
    static Dictionary<string , Action> AvailableArgs => new() {
        { "-d" , () => { IsDebug = true; } },
        { "--debug" , () => { IsDebug = true; } }
    };

    static Dictionary<string, string> CmdUsage => new() {
        { "append" , "append:\nUsage: append <filename> <content>" },
        { "create" , "create:\nUsage: create <filename>" },
        { "remove" , "remove:\nUsage: remove [--force] <filename>" },
        { "changedir" , "changedir:\nUsage: changedir <path>"}
    };

    static List<string> Memory = new() {""};
    static int PrevMemoryId = 0;

    static string UserName => Environment.UserName;
    static string UserDomainName => Environment.UserDomainName;

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

    static (int cursorPos , int rcursorPos) __UserInput_rightArrow_(int cursorPos , int rcursorPos) {
        cursorPos++;
        rcursorPos++;

        Console.SetCursorPosition(rcursorPos, Console.CursorTop);

        return ( cursorPos , rcursorPos );
    }

    static (int cursorPos , int rcursorPos) __UserInput_leftArrow_(int cursorPos , int rcursorPos) {
        cursorPos--;
        rcursorPos--;

        Console.SetCursorPosition(rcursorPos, Console.CursorTop);

        return ( cursorPos , rcursorPos );
    }

    static (int cursorPos , int rcursorPos) __UserInput__SetBufferToMemory_(List<char> buffer , int msgLength , int cursorPos , int rcursorPos) {
        string memory = Memory[PrevMemoryId];

        Console.SetCursorPosition(msgLength, Console.CursorTop);
        Console.Write(new string(' ', buffer.Count));
        Console.SetCursorPosition(msgLength, Console.CursorTop);

        buffer.Clear();
        buffer.AddRange(memory);
        Console.Write(memory);

        cursorPos = buffer.Count;
        rcursorPos = msgLength + cursorPos;

        return (cursorPos, rcursorPos);
    }

    static string UserInput() { // Scary function, optimization needed.
        string msg = $"{UserName}@{UserDomainName} {Directory.GetCurrentDirectory().Replace($"/home/{UserName}", "~")} > ";
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
                    Console.Write(new string(buffer.Skip(cursorPos).ToArray()) + " ");
                    Console.SetCursorPosition(rcursorPos, Console.CursorTop);

                    // Console.Write($"r: {rcursorPos} , n: {cursorPos}");
                } else if (key.Key == ConsoleKey.LeftArrow && rcursorPos >= cursorPos && cursorPos > 0) {
                    (cursorPos , rcursorPos) = __UserInput_leftArrow_(cursorPos , rcursorPos);
                } else if (key.Key == ConsoleKey.RightArrow && rcursorPos >= cursorPos && cursorPos >= 0 && cursorPos < buffer.Count) {
                    (cursorPos , rcursorPos) = __UserInput_rightArrow_(cursorPos , rcursorPos);
                } else if (key.Key == ConsoleKey.UpArrow && PrevMemoryId > 0) {
                    (cursorPos , rcursorPos) = __UserInput__SetBufferToMemory_(buffer, msg.Length , cursorPos , rcursorPos);
                    PrevMemoryId--;
                } else if (key.Key == ConsoleKey.DownArrow && PrevMemoryId + 1 != Memory.Count) {
                    PrevMemoryId++;
                    (cursorPos , rcursorPos) = __UserInput__SetBufferToMemory_(buffer, msg.Length , cursorPos , rcursorPos);
                }
                else if (key.KeyChar != '\0' && key.Key != ConsoleKey.Backspace) {
                    buffer.Insert(cursorPos, key.KeyChar);
                    cursorPos++;
                    
                    Console.SetCursorPosition(rcursorPos, Console.CursorTop);
                    Console.Write(new string(buffer.Skip(cursorPos - 1).ToArray()));
                    
                    rcursorPos++;
                    Console.SetCursorPosition(msg.Length + cursorPos, Console.CursorTop);
                }
            }
        }
        Memory.Add(new string(buffer.ToArray()));
        PrevMemoryId = Memory.Count - 1;
        return new string(buffer.ToArray());
    }
    
    static void Echo(string input) {
        Console.WriteLine(input);
    }

    static string CleanUpInput(string input) {
        return string.Join(" ", input.Split()[1..]);
    }

    static void List(string input) {
        if (input == "") {
            input = Directory.GetCurrentDirectory();
        };

        if (!Directory.Exists(input)) {
            Log("Unknown directory/file!", "err");
            return;
        }
        
        try {
            foreach (string item in Directory.EnumerateFileSystemEntries(input)) {
                string output = item.Split("/")[^1];

                if (Directory.Exists(item)) {
                    output = $"\u001b[1;34m{output}\u001b[0m";
                }

                Console.WriteLine(output);
            }
        } catch (UnauthorizedAccessException) {
            Log("Access denied.", "err");
        }
    }

    static void Create(string input) {
        string[] items = input.Split();

        // Console.WriteLine(items.Length);

        if (items.Length == 1) {
            Log(CmdUsage["create"] , "nml");
            return;
        }

        bool isDir = false;
        foreach (string item in items) {
            if (item == "-d") {
                isDir = true;
                break;
            }
        }
        if (!isDir) {
            File.Create(items[^1]);
            return;
        }
        Directory.CreateDirectory(items[^1]);
    }

    static void Remove(string input) {
        string[] items = input.Split();

        if (items.Length == 1) {
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

        File.AppendAllText(items[^2], items[^1]);
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

    static void ParseInput(string input) {
        Dictionary<string, Action> AvailableCommands = new() {
            { "echo" , () => Echo(CleanUpInput(input))},
            { "clear" , Console.Clear},
            { "exit" , () => Environment.Exit(0)},
            { "list" , () => List(CleanUpInput(input))},
            { "create" , () => Create(CleanUpInput(input))},
            { "remove" , () => Remove(CleanUpInput(input))},
            { "append" , () => Append(CleanUpInput(input))},
            { "changedir" , () => Changedir(CleanUpInput(input)) }
        };

        try {
            // Console.WriteLine(input.Split()[0]);
            AvailableCommands[input.Split()[0]]();
        } catch (KeyNotFoundException) {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string program = parts[0];
            string args = string.Join(' ', parts.Skip(1));
            Process process;
            try {
                process = Process.Start(program, args);
            } catch (Exception) {
                process = null;
            }

            if (!(process is null)) {
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
