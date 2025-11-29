using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharpy.Helpers;

static class UserInput {
    static (int cursorPos , int rcursorPos) UserInput_rightArrow_(int cursorPos , int rcursorPos) {
        cursorPos++;
        rcursorPos++;

        Console.SetCursorPosition(rcursorPos, Console.CursorTop);

        return ( cursorPos , rcursorPos );
    }

    static (int cursorPos , int rcursorPos) UserInput_leftArrow_(int cursorPos , int rcursorPos) {
        cursorPos--;
        rcursorPos--;

        Console.SetCursorPosition(rcursorPos, Console.CursorTop);

        return ( cursorPos , rcursorPos );
    }

    static (int cursorPos , int rcursorPos) UserInput__SetBufferToMemory_(List<char> buffer , int msgLength , List<string> Memory , int PrevMemoryId) {
        string memory = Memory[PrevMemoryId];

        Console.SetCursorPosition(msgLength, Console.CursorTop);
        Console.Write(new string(' ', buffer.Count));
        Console.SetCursorPosition(msgLength, Console.CursorTop);

        buffer.Clear();
        buffer.AddRange(memory);
        Console.Write(memory);

        int cursorPos = buffer.Count;
        int rcursorPos = msgLength + cursorPos;

        return (cursorPos, rcursorPos);
    }

    static (int cursorPos , int rcursorPos) UserInput__AddChar_(List<char> buffer , int cursorPos , int rcursorPos , char key) {
        buffer.Insert(cursorPos, key);
        cursorPos++;
        
        Console.SetCursorPosition(rcursorPos, Console.CursorTop);
        Console.Write(new string([.. buffer.Skip(cursorPos - 1)]));
        
        rcursorPos++;

        return (cursorPos, rcursorPos);
    }
    public static (string , int , List<string>) Run(string prompt , string UserName , string UserDomainName , bool IsSudo , int PrevMemoryId , List<string> Memory) { // Scary function, optimization needed.
        string userStat;
        if (IsSudo) {
            userStat = "#";
        } else {
            userStat = "$";
        }
        string msg = prompt
            .Replace("{userName}", UserName)
            .Replace("{userDomainName}", UserDomainName)
            .Replace("{currDir}" , Environment.CurrentDirectory.Replace($"/home/{UserName}" , "~"))
            .Replace("{userStat}" , userStat);
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
                    Console.Write(new string([.. buffer.Skip(cursorPos)]) + " ");
                    Console.SetCursorPosition(rcursorPos, Console.CursorTop);

                    // Console.Write($"r: {rcursorPos} , n: {cursorPos}");
                } else if (key.Key == ConsoleKey.LeftArrow && rcursorPos >= cursorPos && cursorPos > 0) {
                    (cursorPos , rcursorPos) = UserInput_leftArrow_(cursorPos , rcursorPos);
                } else if (key.Key == ConsoleKey.RightArrow && rcursorPos >= cursorPos && cursorPos >= 0 && cursorPos < buffer.Count) {
                    (cursorPos , rcursorPos) = UserInput_rightArrow_(cursorPos , rcursorPos);
                } else if (key.Key == ConsoleKey.UpArrow && PrevMemoryId > 0) {
                    (cursorPos , rcursorPos) = UserInput__SetBufferToMemory_(buffer, msg.Length , Memory , PrevMemoryId);
                    PrevMemoryId--;
                } else if (key.Key == ConsoleKey.DownArrow && PrevMemoryId + 1 != Memory.Count) {
                    PrevMemoryId++;
                    (cursorPos , rcursorPos) = UserInput__SetBufferToMemory_(buffer, msg.Length , Memory , PrevMemoryId);
                }
                else if (key.KeyChar != '\0' && key.Key != ConsoleKey.Backspace) {
                    (cursorPos, rcursorPos) = UserInput__AddChar_(buffer, cursorPos, rcursorPos, key.KeyChar);
                    Console.SetCursorPosition(msg.Length + cursorPos, Console.CursorTop);
                }
            }
        }
        Memory.Add(new string([.. buffer]));
        PrevMemoryId = Memory.Count - 1;
        return (new string([.. buffer]) , PrevMemoryId , Memory);
    }
}