![Built with Godot](https://img.shields.io/badge/Built%20with-Godot%20Engine-478CBF.svg?style=for-the-badge&logo=Godot-Engine&logoColor=white) and ![C#](https://img.shields.io/badge/C%23-512BD4.svg?style=for-the-badge&logo=csharp&logoColor=white)
# GalaTime: Chronicles of the Past
The game was created and directed by: Nihhiu  
Made by: GalaTime Team
# Getting Started
To run the game we should follow the instructions below:
## Prerequisites
Install the following
- [Godot Engine - .NET 4.2.1 or later](https://godotengine.org/)
- [.NET 6.0 or later](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Git](https://git-scm.com/) ~~(obvisously)~~
## Installation
1. Clone the repository
    ```bash
    git clone https://github.com/Ardub92/Galatime/Galatime.git
    ```
2. Restore NuGet packages
    ```bash
    cd GalaTime-CotP
    dotnet restore
    ```
3. Open the project with Godot
4. Run the project by using `F5` or by pressing "play" in the top left corner
## Notes
- Please, **DO NOT** use `StringBuilder.AppendLine()` in your code, because Godot [has a bug with it](https://github.com/godotengine/godot/issues/74351) that breaks line breaks when project is exported. Instead use `StringBuilder.Append("\n")`. If still having trouble try to switch line endings to `CRLF`, `LF` or `CR` in your editor in any files you are editing (For example `.tres`, `.txt`, `.json`, `.csv`, etc).