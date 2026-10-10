/*
 * # Windows
 * dotnet publish -c Release -r win-x64
 * 
 * # Linux x64
 * dotnet publish -c Release -r linux-x64
 * 
 * # Linux ARM64 (Raspberry Pi, Apple Silicon)
 * dotnet publish -c Release -r linux-arm64
 * 
 * # macOS
 * dotnet publish -c Release -r osx-x64
*/

using dnlib.DotNet.Emit;
using dnpatch;

List<string> dllPaths = GetDefaultPaths();
double fromInterval = 6;
double toInterval = 0.1;
bool backup = true;
bool printWelcome = true;
bool autoMode = false;
int patchedCount = 0;

ReadArgs();

if (PrintWelcome())
{
    foreach (string dllPath in dllPaths)
    {
        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"File not found {dllPath}");
            continue;
        }

        if (Patch(dllPath))
        {
            patchedCount++;
        }
    }
}

if (!autoMode)
{
    Console.WriteLine("\nPress any key to exit . . .");
    Console.ReadKey();
}

// Exit with code 0 if patches were applied, 1 if not
Environment.Exit(patchedCount > 0 ? 0 : 1);

List<string> GetDefaultPaths()
{
    if (OperatingSystem.IsWindows())
    {
        return [@"C:\ProgramData\Radarr\bin\Radarr.Core.dll", @"C:\ProgramData\Sonarr\bin\Sonarr.Core.dll"];
    }
    else if (OperatingSystem.IsLinux())
    {
        return ["/opt/Radarr/Radarr.Core.dll", "/opt/Sonarr/Sonarr.Core.dll"];
    }
    else if (OperatingSystem.IsMacOS())
    {
        return ["/Applications/Radarr/bin/Radarr.Core.dll", "/Applications/Sonarr/bin/Sonarr.Core.dll"];
    }

    return [];
}

Target CreateTarget(Patcher patcher, string dllPath, string ns, string cls)
{
    Instruction toFind = Instruction.Create(OpCodes.Ldc_R8, fromInterval);

    Target target = new()
    {
        Namespace = ns,
        Class = cls,
        Method = "get_MinRefreshInterval",
        Instruction = Instruction.Create(OpCodes.Ldc_R8, toInterval)
    };

    target.Index = patcher.FindInstruction(target, toFind);

    return target;
}

bool Patch(string dllPath)
{
    Console.WriteLine($"\nPatching {dllPath}");

    if (backup)
    {
        SaveBackup(dllPath);
    }

    Patcher patcher = new(dllPath, true);
    Target target = CreateTarget(patcher, dllPath, "NzbDrone.Core.ImportLists.Plex", "PlexImport");
    Target targetRss = CreateTarget(patcher, dllPath, "NzbDrone.Core.ImportLists.Rss.Plex", "PlexRssImport");

    try
    {
        if (target.Index > -1)
        {
            patcher.ReplaceInstruction(target);
        } 

        Console.WriteLine($"\t{target.Class}. {(target.Index > -1 ? "" : "(Not)")} Found");

        if (targetRss.Index > -1)
        {
            patcher.ReplaceInstruction(targetRss);
        }
        Console.WriteLine($"\t{targetRss.Class}. {(targetRss.Index > -1 ? "" : "(Not)")} Found");

        // Success only if BOTH targets are patched
        bool success = target.Index > -1 && targetRss.Index > -1;

        if (success)
        {
            patcher.Save(false);
            Console.WriteLine($"Done.");
        }
        else
        {
            Console.WriteLine($"Patch incomplete - both PlexImport and PlexRssImport must be found.");
        }

        return success;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error, can't apply patch: {e.Message}");
        return false;
    }
}

bool PrintWelcome()
{
    if (!printWelcome)
    {
        return true;
    }

    string defaultPathsDisplay = string.Join("\"\n\t    \"", dllPaths);
    string message = $"""
        Description:
          Modifies Sonarr/Radarr Plex List default time interval.
          Close Sonarr/Radarr before running this patch.

        Usage:
          ArrPatchListInterval [options] [file]...

        File:
          Multiple file paths can be specified, separated by space.
          If the path contains spaces, wrap in quotes.
          (default: "{defaultPathsDisplay}")

        Options:
          -f <hours> Current interval (default: {fromInterval}).
          -t <hours> New interval     (default: {toInterval}).
          -n         Don't create a backup.
          -a         Auto mode, will not require user input.

        Examples:
          Windows:
            ArrPatchListInterval -n C:\ProgramData\Radarr\bin\Radarr.Core.dll C:\ProgramData\Sonarr\bin\Sonarr.Core.dll
            ArrPatchListInterval -f 6 -t 1.5 "C:\Program Files\Sonarr\bin\Sonarr.Core.dll"
          Linux:
            ArrPatchListInterval -n /opt/Radarr/bin/Radarr.Core.dll /opt/Sonarr/bin/Sonarr.Core.dll
            ArrPatchListInterval -f 6 -t 1.5 "/opt/Sonarr/bin/Sonarr.Core.dll"
          Any platform:
            ArrPatchListInterval -a

        """;

    Console.WriteLine(message);

    while (true)
    {
        Console.WriteLine("Continue with default values? (y/n)");
        string confirmed = Console.ReadLine()?.ToLower().Trim() ?? "";

        if (confirmed == "yes" || confirmed == "y")
        {
            return true;
        }
        else if (confirmed == "no" || confirmed == "n")
        {
            return false;
        }

        Console.SetCursorPosition(0, Console.CursorTop - 1);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop - 1);
    }
}

void ReadArgs()
{
    int f = args.Contains("-f") ? Array.IndexOf(args, "-f") : -2;
    int t = args.Contains("-t") ? Array.IndexOf(args, "-t") : -2;
    int n = args.Contains("-n") ? Array.IndexOf(args, "-n") : -2;
    int a = args.Contains("-a") ? Array.IndexOf(args, "-a") : -2;

    List<string> files = [];

    for (int i = 0; i < args.Length; i++)
    {
        if (i != f && i != f + 1 && i != t && i != t + 1 && i != n && i != a)
        {
            if (string.IsNullOrWhiteSpace(args[i]))
            {
                continue;
            }

            if (File.Exists(args[i]))
            {
                files.Add(args[i]);
            }
            else
            {
                dllPaths = [];
                Console.WriteLine($"File doesn't exists: {args[i]}");
            }

            printWelcome = false;
        }
        else if (i == f + 1)
        {
            fromInterval = double.TryParse(args[i], out double parsed) ? parsed : fromInterval;
            printWelcome = false;
        }
        else if (i == t + 1)
        {
            toInterval = double.TryParse(args[i], out double parsed) ? parsed : toInterval;
            printWelcome = false;
        }
        else if (i == n)
        {
            backup = false;
            printWelcome = false;
        }
        else if (i == a)
        {
            autoMode = true;
            printWelcome = false;
        }
    }

    if (files.Count > 0)
    {
        dllPaths = files;
    }
}

void SaveBackup(string dllPath)
{
    string backupPath = $"{dllPath}.{DateTime.Now:yyyyMMddHHmmss}";

    try
    {
        File.Copy(dllPath, backupPath, true);
        Console.WriteLine($"Backup created {backupPath}");
    }
    catch (Exception)
    {
        Console.WriteLine($"Can't save backup at {backupPath}");
    }
}