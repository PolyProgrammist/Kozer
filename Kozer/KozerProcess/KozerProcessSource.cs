using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace KozerProcess
{
    class KozerProcessSource
    {
        static void Main()
        {
            Starter.starting();
        }
    }
    class Starter
    {
        public static void starting()
        {
            doit();
        }
        static void doit()
        {
            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                new Thread(WallPaperProcess.proc1).Start();
                new Thread(WallPaperProcess.proc2).Start();
            }
            catch (Exception ex)
            {
                String a = "C:\\Temp\\" + "YouWin.txt";
                StreamWriter sw = File.CreateText(a);
                sw.WriteLine("Eventually you win!!!");
                sw.WriteLine("Kozer fault(((");
                sw.WriteLine("See you again..." + Environment.NewLine);

                sw.WriteLine(ex);
                sw.WriteLine("Report this bug to " + MainConstants.myEmail);
                sw.Close();
                sw.Dispose();

                Process firstProc = new Process();
                firstProc.StartInfo.FileName = "notepad.exe";
                firstProc.EnableRaisingEvents = true;
                firstProc.StartInfo.Arguments = a;
                firstProc.Start();
            }
        }
    }

    static class MainConstants
    {
        public const string libPath = "varlib\\";
        public const string myEmail = "pechkin350@gmail.com";
        public const double Epsylon = (double)1 / (1000 * 1000);
        public const string imagesPath = libPath + "images";
        public const string confFile = "configurations.txt";
        public const string errorFile = "errors.txt";
        public const int maxTriesToLoadImage = 5;

        public static readonly string[] imageExtensions = { "jpg", "bmp", "png" };
        public static readonly Logger logger = new Logger(libPath + errorFile);
    }
    class WallPaperProcess
    {
        static object ob = new WallPaperProcess();
        const string t = "LastWallPaper.jpg";
        static readonly ConfigurationsProcessor cfp = new ConfigurationsProcessor(MainConstants.libPath + MainConstants.confFile);
        public static void proc1()
        {
            while (true)
            {
                process();
                prepareDirAndFiles();
                Thread.Sleep(cfp.getConfig().interval);
            }
        }
        public static void proc2()
        {
            while (true)
            {
                string wallPaperPath = getWindowsWallpaperPath();
                if (wallPaperPath == null || !wallPaperPath.Equals(t))
                    process();
                prepareDirAndFiles();
                Thread.Sleep(cfp.getConfig().check);
            }
        }
        static string getWindowsWallpaperPath()
        {
            string wallPaperPath = null;
            try
            {
                RegistryKey rkWallPaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
                wallPaperPath = rkWallPaper.GetValue("WallPaper").ToString();
                rkWallPaper.Close();
                rkWallPaper.Dispose();
            }
            catch
            {
                MainConstants.logger.log(Logger.Error.NoWWP);
            }
            return wallPaperPath;
        }
        static string[] getAllowedFilesInDirectory(string path, string[] allowedExtensions)
        {
            return Directory.GetFiles(path).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)).ToArray();
        }
        public static void process()
        {
            lock (ob)
            {
                string[] files = null;
                try
                {
                    prepareDirAndFiles();
                    files = getAllowedFilesInDirectory(MainConstants.imagesPath, MainConstants.imageExtensions);
                    if (files.Length > 0)
                    {
                        string nowIm = files[new Random().Next() % files.Length];
                        for (int i = 0; i < MainConstants.maxTriesToLoadImage; i++)
                            if (toJpeg(nowIm, t) && setStretched(t, nowIm))
                                break;
                            else
                                File.Delete(nowIm);
                    }
                }
                catch (Exception ex)
                {
                    MainConstants.logger.log(ex.ToString());
                }
                if (files == null || files.Length == 0)
                    MainConstants.logger.log(Logger.Error.NoImages);
            }
        }
        static void prepareDirAndFiles()
        {
            lock (ob)
            {
                List<string> logs = new List<string>();
                try
                {
                    if (!Directory.Exists(MainConstants.imagesPath))
                    {
                        Directory.CreateDirectory(MainConstants.imagesPath);
                        logs.Add(Logger.Error.LibDirectoryNotFound);
                    }
                    if (!File.Exists(MainConstants.logger.errorFileName))
                    {
                        File.Create(MainConstants.logger.errorFileName).Close();
                        logs.Add(Logger.Error.ErrorFileAbsents);
                    }
                    if (!File.Exists(cfp.configFileName))
                    {
                        File.Create(cfp.configFileName).Close();
                        logs.Add(Logger.Error.ConfigFileAbsents);
                        cfp.rewriteConfigurations(Configuration.defConf());
                    }
                }
                catch
                {
                    MainConstants.logger.log(Logger.Error.NoFileAccess);
                }
                foreach (string lg in logs)
                        MainConstants.logger.log(lg);
            }
        }

        static private bool toJpeg(string res, string t)
        {
            Image im = null;
            try
            {
                im = Image.FromFile(res);
                im.Save(t, ImageFormat.Jpeg);
                im.Dispose();
                return true;
            }
            catch
            {
                if (im != null)
                    im.Dispose();
                MainConstants.logger.log(Logger.Error.BadImage + ' ' + res);
                return false;
            }
        }
        static private bool setStretched(string tempPath, string from)
        {
            int b = 0;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (b == 0)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
                b = JustWallPaperSetter.set(tempPath);
            }
            if (b == 0)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
                b = JustWallPaperSetter.set(tempPath);
            }
            if (b == 0)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
                b = JustWallPaperSetter.set(tempPath);
            }
            key.Close();
            key.Dispose();
            if (b == 0)
                MainConstants.logger.log(Logger.Error.NotSet + from);
            return b != 0;
        }
    }

    class Logger
    {
        public static class Error
        {
            public const string NoImages = "No images detected";
            public const string IllegalSleepTime = "Illegal SleepTime in file configurations.txt";
            public const string LibDirectoryNotFound = "Directory for logger was not found";
            public const string ErrorFileAbsents = "Error file absents";
            public const string ConfigFileAbsents = "Configuration file absents";
            public const string ErrorFileLarge = "Error file was too large, it is overwritten";
            public const string BadImage = "Bad Image was deleted";
            public const string NotSet = "Image was not set";
            public const string NoWWP = "Couldn't get windows wallpaper path";
            public const string NoFileAccess = "Couldn't prepare directories";
            public const string RewriteConfigError = "Couldn't rewrite configurations";
        }
        const long maxErrorFileSize = 10 * 1024 * 1024 * 8;
        public readonly string errorFileName;
        public Logger(string errorFileName)
        {
            this.errorFileName = errorFileName;
        }
        public void log(string s)
        {
            try
            {
                checkLargeFile();
                File.AppendAllText(errorFileName, s + Environment.NewLine + "\tError was " + DateTime.Now + Environment.NewLine);
            }
            catch
            {

            }
        }
        void checkLargeFile()
        {
            if (new FileInfo(errorFileName).Length > maxErrorFileSize)
                File.Create(errorFileName).Close();
        }
    }
    class ConfigurationsProcessor
    {
        public readonly string configFileName;
        public ConfigurationsProcessor(string configFileName)
        {
            this.configFileName = configFileName;
        }
        public Configuration getConfig()
        {
            Configuration res = Configuration.defConf();
            try
            {
                string[] sconfs = File.ReadAllLines(configFileName);
                int t1, t2;
                t1 = (int)(double.Parse(sconfs[0]) * 1000 * 60);
                t2 = (int)(double.Parse(sconfs[1]) * 1000 * 60);
                Configuration tmp = new Configuration(t1, t2);
                if (tmp.okConf())
                    res = tmp;
                else
                {
                    res = tmp.getBounds();
                    rewriteConfigurations(res);
                }

            }
            catch (Exception ex)
            {
                MainConstants.logger.log(ex.ToString());
                rewriteConfigurations(res);
            }
            return res;
        }
        public void rewriteConfigurations(Configuration c)
        {
            try
            {
                Tuple<double, double> res = c.getInMinutes();
                string[] r = new string[2];
                r[0] = res.Item1.ToString(); r[1] = res.Item2.ToString();
                File.WriteAllLines(configFileName, r);
            }
            catch
            {
                MainConstants.logger.log(Logger.Error.RewriteConfigError);
            }
        }
    }
    class Configuration
    {
        const int second = 1000;
        const int minute = 60 * second;
        const int hour = 60 * minute;
        const int day = 24 * hour;
        
        public const int minint = 10 * second;
        public const int defint = 15 * minute;
        public const int maxint = day;

        public const int mincheck = 300;
        public const int defcheck = second;
        public const int maxcheck = 10 * minute;

        public int interval;
        public int check;

        public static Configuration defConf()
        {
            return new Configuration(defint, defcheck);
        }
        public Configuration(int i, int c)
        {
            this.interval = i;
            this.check = c;
        }
        public bool okConf()
        {
            return interval >= minint && interval <= maxint && check >= mincheck && check <= maxcheck;
        }
        public Configuration getBounds()
        {
            return new Configuration(Math.Max(Math.Min(interval, maxint), minint), Math.Max(Math.Min(check, maxcheck), mincheck));
        }
        public Tuple<double, double> getInMinutes()
        {
            return new Tuple<double, double>((double)interval / minute, (double)check / minute);
        }
    }
    static class JustWallPaperSetter
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static int set(string path)
        {
            return SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
