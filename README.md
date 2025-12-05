# Sharpy — Interactive Terminal Shell (C#)

### Sharpy is a lightweight, interactive shell written in C#, offering basic filesystem operations, command aliases, debugging utilities, and a custom line-editor with history navigation. Sharpy also has its own scripting language(limited)!

## This README documents:

- Installation / build steps

- CLI arguments

- Scripting Language

- Built-in commands with examples

- Alias system

- Debug tools

- Notes and limitations

## ✨ Features

### Custom prompt:
username@domain ~/current/path >

### Command history (↑ / ↓)

### Cursor movement inside input (← / →)

### Aliases with add/remove/get

### Basic shell-like commands (list, remove, changedir, create, etc.)

### External command fallback (ls, nano, python3, etc.)

### Debug mode tool (sdb)

## 📦 Installation
```
git clone https://github.com/VxidDev/Sharpy.git
cd Sharpy
make build
```
## 🛠 Command Line Arguments
| Argument  | Description       |
| --------- | ----------------- |
| `-d`      | Enable debug mode |
| `--debug` | Enable debug mode |

## Scripting Language(SharpyScript)

Language is pretty limited for now , but you can use it by create a .ss file and entering sharpy's commands and running it in shell via ```sscript <file>```

### Example:
HelloWorld.ss
```
echo Creating a Hello World in C#...

create test.cs
append test.cs using System;
append test.cs static class Program {
append test.cs static void Main() {
append test.cs Console.WriteLine("Hello World!");
append test.cs }
append test.cs }

echo Done!
```
This create a test.cs with this content:
```
using System;
static class Program {
static void Main() {
Console.WriteLine("Hello World!");
}
}
```
theres also ```asv``` which means ASsignVariable
```
asv var 5
echo -v var
```
output:
```
5
```
theres also ```if```
```
if <logic <var> <var> <alias/command>
```

Example:
```
asv arg1 5
asv arg2 5

if equals arg1 arg2 echo hello world
```

Current logics available:
- equals

## 📚 Built-in Commands

Each command supports --help to print usage.

### Example:
```
list --help
```
You can also check command usage by doing:
```
help list
```
1. echo

- Print a string or variable.

### Usage
```
echo [-v/--var] <string>
```

2. list
 
- List directory contents. Printed directories use a nice blue gradient.

### Usage
```
list <directory>
```
If no path is provided, it lists the current directory.

3. create

- Create a file or directory.

## Usage
```
create <filename>
create -d <dirname>      # create directory
```
4. remove

- Delete files or directories.

## Usage
```
remove <filename>
remove --force <path>
```
## Behaviour
| Mode      | Description                                              |
| --------- | -------------------------------------------------------- |
| Normal    | Asks for confirmation before deleting files              |
| `--force` | Deletes files and directories recursively without asking |

5. append

- Append text to a file.
## Usage
```
append <file> <string>
```
6. changedir

- Change working directory.

Supports home shortcut (~ → /home/<user>)

## Usage
```
changedir <path>
```
7. read

- Read and print file contents.

## Usage
```
read <file>
```
8. help

- Print usage guide for any command.
## Usage
```
help <cmd>
```

9. alias

- Create, delete, or display aliases.
## Usage
```
alias <name>=<command>
alias --remove <name>
alias --get <name>
```
Spaces inside the command are written using + (converted back to spaces).
## Examples
Create alias:
```
alias ll=list+~/
```
Use alias:
```
ll
```
Get alias value:
```
alias --get ll
```
Remove alias:
```
alias --remove ll
```
10. sdb — Sharpy Debug Tools

- Debug utilities for development.
## Usage
```
sdb
sdb --toggle
sdb pAliases
sdb pHistory
sdb isSudo
sdb pDebug
```
## Functions
| Subcommand | Description                                    |
| ---------- | ---------------------------------------------- |
| `--toggle` | Enable/disable debug mode                      |
| `pAliases` | Print all aliases (only when debug mode is ON) |
| `isSudo`   | Print current sudo status                      |
| `pDebug`   | Print current debug status                     |
| `pHistory` | Print current history entries to terminal.     |

11. prompt

Edit existing prompt.

## Usage
```
prompt [--clear] <prompt>
```

## Example
```
prompt |{userStat}|+{userName}+{currDir}++>>
```
This turns prompt from this:
```
vxid-dev@VxidDev ~/Coding/sharpy $ >
```
To this:
```
|$| vxid-dev ~/Coding/sharpy  >>
```
Careful, you have to replace whitespace with '+'.

12. export

Export aliases and current prompt.

## Usage
```
export
```
...Pretty self explanatory right?

All data is stored within ```/home/<username>/.sharpy``` folder.

13. history

Manipulate the command history.

## Usage
```
history [--clear/--pop <amount(int)>]
```
## Example:
```
vxid-dev@VxidDev ~/Coding/C# - Basics/sharpy $ > sdb pHistory
[ID: 0] 
[ID: 1] a
[ID: 2] b
[ID: 3] c
[ID: 4] sdb pHistory
PrevMemoryID: 4
vxid-dev@VxidDev ~/Coding/C# - Basics/sharpy $ > history --pop 5
vxid-dev@VxidDev ~/Coding/C# - Basics/sharpy $ > sdb pHistory
[ID: 0] sdb pHistory
PrevMemoryID: 0
```
## Functions

| Subcommand | Description                                                    |
| ---------- | ---------------------------------------------------------------|
| `--clear`  | Remove all history entries.                                    |
| `--pop <int>`    | Remove desired amount of entries from the end of entries.|


## ⌨️ Line Editor Features

Sharpy uses a custom input system, instead of generic Console.ReadLine() we use:
| Key         | Action                         |
| ----------- | ------------------------------ |
| `←`         | Move cursor left               |
| `→`         | Move cursor right              |
| `↑`         | Previous command in history    |
| `↓`         | Next command                   |
| `Backspace` | Delete character before cursor |
| `Enter`     | Confirm input                  |

## 📦 Internal Architecture Overview

- Memory list — command history

- Aliases dictionary — string → string mappings

- Gradient printer — for directory listing

- Recursive input parser — supports alias recursion

- External process launcher

## ⚠ Notes / Limitations

- remove with no --force works only for files, not directories

- Alias commands cannot include actual spaces; must use +

- Broken arguments may trigger ArgumentOutOfRangeException from user input split

- No quoting support ("text with spaces") yet

- No piping (|) or redirection (>, <)

# Known issues
- Weird behaviour with next/previous command(either 1 click or 2 clicks needed for change).

# Bug Submits
To submit a bug either open an issue or mail me on ```stas050595@gmail.com```.
I will be really grateful if anyone help me debug the app because I am pretty bad code tester:(

## License
of course, MIT.





