using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace CSGO_Font_Manager
{
    partial class MainWindow
    {
        public static bool IsFontExtension(string extension)
        {
            extension = extension.ToLower();
            return extension == ".ttf" || extension == ".tte" || extension == ".otf" ||
                   extension == ".woff" || extension == ".woff2" || extension == ".eot" || extension == ".fon";
        }

        private void setupFontsDirectory(string fontsFile, string fontname, string fontfilename)
        {
            string extension = Path.GetExtension(fontfilename);
            File.WriteAllText(fontsFile, FileTemplates.fonts_conf(fontname, extension, fontfilename, getCSGOPixelSize().ToString().Replace(",", ".")));
        }

        private float getCSGOPixelSize()
        {
            float startValue = 0.9f;
            float zoomFactor = 0.05f;
            MainWindow _mw = (MainWindow)Application.Current.MainWindow;
            switch (_mw.FontSize.Value)
            {
                case 1:
                    return startValue + zoomFactor * -3; // 0.6
                case 2:
                    return startValue + zoomFactor * -2; // 0.7
                case 3:
                    return startValue + zoomFactor * -1; // 0.8
                case 4:
                    return startValue + zoomFactor * 0;  // 0.9
                case 5:
                    return startValue + zoomFactor * 1;  // 1.0
                case 6:
                    return startValue + zoomFactor * 2;  // 1.1
                case 7:
                    return startValue + zoomFactor * 3;  // 1.2
                case 8:
                    return startValue + zoomFactor * 4;  // 1.3
                case 9:
                    return startValue + zoomFactor * 5;  // 1.4
                default:
                    return startValue;
            }
        }

        
        [DllImport("gdi32", EntryPoint = "AddFontResource")]
        public static extern int AddFontResourceA(string lpFileName);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int AddFontResource(string lpszFilename);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int CreateScalableFontResource(uint fdwHidden, string
        lpszFontRes, string lpszFontFile, string lpszCurrentPath);

        private static bool InstallFont(string fontFilePath)
        {
            string fontReg = Path.Combine(Path.GetTempPath(), "fontReg.exe");
            if (!File.Exists(fontReg)) File.WriteAllBytes(fontReg, Properties.Resources.FontReg);

            var info = new ProcessStartInfo()
            {
                FileName = fontReg,
                Arguments = "/copy",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            info.WorkingDirectory = new FileInfo(fontFilePath).DirectoryName;

            try
            {
                var p = Process.Start(info);
                p.WaitForExit();
            }
            catch { return false; }

            bool fileWasCopied = File.Exists(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                Path.GetFileName(fontFilePath)
                ));
            return fileWasCopied;
        }

        public static string GetSystemFontFileName(FontFamily font)
        {
            RegistryKey fonts = null;
            try
            {
                fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                if (fonts == null)
                {
                    fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Fonts", false);
                    if (fonts == null)
                    {
                        throw new Exception("Can't find font registry database.");
                    }
                }
                FamilyTypefaceCollection familyTypefaceCollection = font.FamilyTypefaces;
                var test = FontWeights.Bold;
                string suffix = "";
                foreach (var item in familyTypefaceCollection)
                {
                    if(item.Weight == test)
                        suffix += "(?: Bold)?";
                    if(item.Style == FontStyles.Italic)
                        suffix += "(?: Italic)?";
                }
                //if (familyTypefaceCollection)
                //    suffix += "(?: Bold)?";
                //if (font.Italic)
                //    suffix += "(?: Italic)?";

                var regex = new Regex(@"^(?:.+ & )?" + Regex.Escape(font.Source) + @"(?: & .+)?(?<suffix>" + suffix + @") \(TrueType\)$", RegexOptions.Compiled);

                string[] names = fonts.GetValueNames();

                string name = names.Select(n => regex.Match(n)).Where(m => m.Success).OrderByDescending(m => m.Groups["suffix"].Length).Select(m => m.Value).FirstOrDefault();

                if (name != null)
                {
                    return fonts.GetValue(name).ToString();
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                if (fonts != null)
                {
                    fonts.Dispose();
                }
            }
        }

        Dictionary<string, List<string>> _fontNameToFiles;
        private IEnumerable<string> GetFilesForFont(string fontName)
        {
            if (_fontNameToFiles == null)
            {
                _fontNameToFiles = new Dictionary<string, List<string>>();
                foreach (var fontFile in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)))
                {
                    var fc = new System.Drawing.Text.PrivateFontCollection();
                    try
                    {
                        fc.AddFontFile(fontFile);
                    }
                    catch (FileNotFoundException)
                    {
                        continue; // not sure how this can happen but I've seen it.
                    }
                    if (fc.Families.Length == 0) continue;

                    var name = fc.Families[0].Name;
                    // If you care about bold, italic, etc, you can filter here.
                    List<string> files;
                    if (!_fontNameToFiles.TryGetValue(name, out files))
                    {
                        files = new List<string>();
                        _fontNameToFiles[name] = files;
                    }
                    files.Add(fontFile);
                }
            }
            List<string> result;
            if (!_fontNameToFiles.TryGetValue(fontName, out result))
                return new string[0];
            return result;
        }
    }
}