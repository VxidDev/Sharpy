using System;
using System.Diagnostics;
using Sharpy.Helpers;

namespace Sharpy.Commands;

static class Time {
    public static void Run(string input , Action<string , string> Log) {
        long initTime = Stopwatch.GetTimestamp();
        Sharpy.Program.ParseInput(input);
        long endTime = Stopwatch.GetTimestamp();
        Log($"{input} took {(double)(endTime - initTime) / Stopwatch.Frequency}s to finish." , "nml");
    }
}