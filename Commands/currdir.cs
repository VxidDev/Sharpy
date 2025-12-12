using System;

namespace Sharpy.Commands;

static class CurrDir {
    public static void Run(Action<string , string> Log) {
        Log(Environment.CurrentDirectory , "nml");
    }
}