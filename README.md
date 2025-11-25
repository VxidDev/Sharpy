# Sharpy

**Sharpy** is a fast and lightweight C# shell for running commands and managing files in an interactive CLI environment.

## Features

- Execute built-in commands like `list`, `append`, `remove`, and more
- Navigate and manipulate directories
- Create, view( Not done ), and edit files quickly
- Simple and intuitive command syntax
- Cross-platform support via .NET
- Easily manipulate your command using left/right arrow or access previous commands using up/down arrows

## Installation

1. Clone the repository:
```bash
git clone https://github.com/VxidDev/Sharpy.git
dotnet new console -n sharpy
mv Sharpy/Program.cs sharpy/Program.cs
cd sharpy
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o .
chmod +x sharpy 
sudo mv sharpy /usr/local/bin/sharpy
```

and done!

## Known Issues:
- double up/down arrow needed to manage through "memory"
