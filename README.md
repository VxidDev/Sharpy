# Sharpy — Interactive Terminal Shell (C#)

### Sharpy is a lightweight, interactive shell written in C#, offering basic filesystem operations, command aliases, debugging utilities, and a custom line-editor with history navigation.

## This README documents:

- Installation / build steps

- CLI arguments

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

### Debug mode toggle (sdb --toggle)

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

- Print a string.

### Usage
```
echo <string>
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
```

## Functions
| Subcommand | Description                                    |
| ---------- | ---------------------------------------------- |
| `--toggle` | Enable/disable debug mode                      |
| `pAliases` | Print all aliases (only when debug mode is ON) |
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





