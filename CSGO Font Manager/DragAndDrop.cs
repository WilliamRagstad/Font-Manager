using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CSGO_Font_Manager_2._0
{
    public partial class Form1
    {
        private void fontLibrary_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

        private void fontLibrary_DragDrop(object sender, DragEventArgs e)
        {
            listBox1.Enabled = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileList = e.Data.GetData(DataFormats.FileDrop) as string[];
                string addedFonts = "";

                for (int i = 0; i < fileList.Length; i++)
                {
                    string filepath = fileList[i];
                    string extension = Path.GetExtension(filepath).ToLower();
                    string rawfilename = Path.GetFileName(filepath);
                    string filename = rawfilename;
                    if (extension != "") filename = rawfilename.Replace(extension,"");

                    string fontpath = null;

                    string fileFontDirectory = FontsFolder + sanitizeFilename(filename);

                    if (Directory.Exists(fileFontDirectory))
                    {
                        if (MessageBox.Show(
                                $"The font '{filename}' is already in your library, do you want to overwrite it?",
                                "Overwrite Font?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            Directory.Delete(fileFontDirectory, true);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (extension == ".zip")
                    {
                        ZipFile.ExtractToDirectory(Path.GetFullPath(filepath), fileFontDirectory);

                        // If multiple fonts are extracted, let the user select one to install...
                        string[] extractedFiles = Directory.GetFiles(fileFontDirectory);
                        if (extractedFiles.Length > 1)
                        {
                            List<string> fontfiles = new List<string>();
                            foreach (string extractedFile in extractedFiles)
                            {
                                if (IsFontExtension(Path.GetExtension(extractedFile)))
                                {
                                    fontfiles.Add(extractedFile);
                                }
                                else
                                {
                                    // Delete it
                                    File.Delete(extractedFile);
                                }
                            }

                            if (fontfiles.Count > 1)
                            {
                                string matchedFiles = "";
                                for (int j = 0; j < fontfiles.Count; j++)
                                {
                                    matchedFiles += $"{j+1} - {Path.GetFileName(fontfiles[j])}\n";
                                }

                                matchedFiles += "\n";

                                string message =
                                    "The zip file was containing multiple files, and you are only able to install ONE.\nPlease select the font you want to install from the list below:\n" +
                                    matchedFiles;
                                string selectedFont = Microsoft.VisualBasic.Interaction.InputBox(
                                    message, "Select font", fontfiles.Count.ToString());
                                int selectedFontIndex;
                                while (!(int.TryParse(selectedFont, out selectedFontIndex) && selectedFontIndex <= fontfiles.Count && selectedFontIndex > 0 ) )
                                {
                                    if (selectedFont == "") // Assume cancel
                                    {
                                        // Remove the font folder
                                        Directory.Delete(fileFontDirectory, true);

                                        listBox1.Enabled = true;
                                        return;
                                    }
                                    selectedFont = Microsoft.VisualBasic.Interaction.InputBox(
                                        message, "Select font", fontfiles.Count.ToString());
                                }

                                // Remove all fonts that isn't on the selectedFontIndex
                                for (int j = 0; j < fontfiles.Count; j++)
                                {
                                    if (j != selectedFontIndex - 1)
                                    {
                                        File.Delete(fontfiles[j]);
                                    }
                                }

                                fontpath = fontfiles[selectedFontIndex - 1];
                            }
                            else
                            {
                                fontpath = fontfiles[0];
                            }
                            
                        }
                        else if (extractedFiles.Length == 1)
                        {
                            fontpath = extractedFiles[0];
                        }
                        else
                        {
                            MessageBox.Show($"The zip file '{filename}' did not contain any files.");
                        }
                    }
                    else if (IsFontExtension(extension))
                    {
                        Directory.CreateDirectory(fileFontDirectory);
                        File.Copy(filepath, fileFontDirectory + rawfilename + extension, true);
                        fontpath = filepath;
                    }
                    else
                    {
                        MessageBox.Show("Unsupported filetype: " + rawfilename + extension, "Unsupported",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    bool fontWasInstalled = InstallFont(fontpath);
                    if (fontWasInstalled)
                    {
                        // Setup correct files and folders in FontsFolder
                        string fontsFile = fileFontDirectory + "\\fonts.conf";

                        Form1.AddFont(filename, fontpath);
                        FontFamily fontFamily = Form1.GetFontFamilyByName(filename);
                        if (fontFamily == null)
                        {
                            listBox1.Enabled = true;
                            return;
                        }

                        setupFontsDirectory(fontsFile, fontFamily.Name, Path.GetFileName(fontpath));

                        if (i == 0) addedFonts += filename;
                        else  addedFonts += ", " + filename;
                    }
                    else
                    {
                        // Font was not installed
                        // Remove the font folder
                        Directory.Delete(fileFontDirectory, true);
                    }
                }

                if (addedFonts.Length > 0)
                {
                    MessageBox.Show("Success! The following font(s) has been added to your library!\n---\n" + addedFonts, "Font(s) Added!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    refreshFontList();
                }
                else
                {
                    MessageBox.Show("Something must have gone wrong, no fonts seems to have been added...", "No Fonts Added", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            listBox1.Enabled = true;
        }


        public static bool IsFontExtension(string extension)
        {
            extension = extension.ToLower();
            return extension == ".ttf" || extension == ".tte" || extension == ".otf" ||
                   extension == ".woff" || extension == ".woff2" || extension == ".eot" || extension == ".fon";
        }

        private void setupFontsDirectory(string fontsFile, string fontname, string fontfilename)
        {
            string extension = Path.GetExtension(fontfilename);
            File.WriteAllText(fontsFile, FileTemplates.fonts_conf(fontname, extension, fontfilename));
        }

        #region  External Code
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

        public static string GetSystemFontFileName(Font font)
        {
            RegistryKey fonts = null;
            try{
                fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                if(fonts == null)
                {
                    fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Fonts", false);
                    if(fonts == null)
                    {
                        throw new Exception("Can't find font registry database.");
                    }
                }

                string suffix = "";
                if(font.Bold)
                    suffix += "(?: Bold)?";
                if(font.Italic)
                    suffix += "(?: Italic)?";

                var regex = new Regex(@"^(?:.+ & )?"+Regex.Escape(font.Name)+@"(?: & .+)?(?<suffix>"+suffix+@") \(TrueType\)$", RegexOptions.Compiled);

                string[] names = fonts.GetValueNames();

                string name = names.Select(n => regex.Match(n)).Where(m => m.Success).OrderByDescending(m => m.Groups["suffix"].Length).Select(m => m.Value).FirstOrDefault();

                if(name != null)
                {
                    return fonts.GetValue(name).ToString();
                }else{
                    return null;
                }
            }finally{
                if(fonts != null)
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
            if (!_fontNameToFiles.TryGetValue(fontName,  out result))
                return new string[0];
            return result;
        }

        #endregion
    }
}
