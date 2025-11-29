using System;

namespace Sharpy.Helpers;

static class PrintGradient {
    public static void Run(string text, bool bold , (int R, int G, int B) start, (int R, int G, int B) end) {
        int len = text.Length;

        if (bold) {
            Console.Write("\u001b[1m");
        }

        for (int i = 0; i < len; i++) {
            float t = (float)i / (len - 1);

            int r = (int)(start.R + (end.R - start.R) * t);
            int g = (int)(start.G + (end.G - start.G) * t);
            int b = (int)(start.B + (end.B - start.B) * t);

            Console.Write($"\u001b[38;2;{r};{g};{b}m{text[i]}");
        }

        Console.Write("\u001b[0m\n");
    }
}