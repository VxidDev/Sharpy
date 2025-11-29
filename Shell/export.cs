using System;
using System.Data;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

namespace Sharpy.Shell;

static class Export {
    public static void Run(string exportPath , Action CreateConfig , Action<string , string> Log , string prompt , bool IsDebug , Dictionary<string , string> Aliases) {
        // string[] items = input.Split();

        string oldText = File.ReadAllText($"{exportPath}/config.json");
        Dictionary<string, object>? oldData = null;
        int tries = 0;
        while (tries < 3) {
            try {
                tries++;
                oldData = JsonSerializer.Deserialize<Dictionary<string, object>>(oldText!);
                break;
            }
            catch (JsonException) {
                CreateConfig();
                return;
            }
        }

        if (oldData is null) {
            Log("Failed to update config." , "err");
            return;
        }

        oldData["prompt"] = prompt;

        File.WriteAllText($"{exportPath}/config.json", JsonSerializer.Serialize(oldData));

        tries = 0;
        Dictionary<string, string>? oldAliases = null;
        while (tries < 3) {
            try {
                tries++;
                string? text = File.ReadAllText($"{exportPath}/aliases.json");
                if (text is null) continue;
                oldAliases = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
            } catch (JsonException) {
                File.Create($"{exportPath}/aliases.json").Close();
                File.WriteAllText($"{exportPath}/aliases.json", JsonSerializer.Serialize<Dictionary<string, string>>([]));
            } catch (FileNotFoundException) {
                File.Create($"{exportPath}/aliases.json").Close();
                File.WriteAllText($"{exportPath}/aliases.json", JsonSerializer.Serialize<Dictionary<string, string>>([]));
            }
        }

        if (oldAliases is null) {
            if (IsDebug) {
                Log("Failed to export aliases.", "err");
            }
            return;
        }

        oldAliases = Aliases;

        File.WriteAllText($"{exportPath}/aliases.json", JsonSerializer.Serialize(oldAliases));
    }
}