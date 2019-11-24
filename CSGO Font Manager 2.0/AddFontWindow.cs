using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CSGO_Font_Manager_2._0.Properties;

namespace CSGO_Font_Manager_2._0
{
    public partial class AddFontWindow : Form
    {

        public AddFontWindow()
        {
            InitializeComponent();
        }

        void AddFontWindow_DragEnter(object sender, DragEventArgs e)
        {
            this.AllowDrop = true;
            e.Effect = DragDropEffects.All;
        }

        void AddFontWindow_DragDrop(object sender, DragEventArgs e)
        {
            this.AllowDrop = true;
            e.Effect = DragDropEffects.All;

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

                    // Clean up filename
                    filename = filename.Replace("_", " ");
                    filename = char.ToUpper(filename[0]) + filename.Remove(0,1); // Capital letter

                    string fileFontDirectory = Form1.FontsFolder + filename;

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
                                if (isFontExtension(Path.GetExtension(extractedFile)))
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
                    else if (isFontExtension(extension))
                    {
                        Directory.CreateDirectory(fileFontDirectory);
                        File.Copy(filepath, fileFontDirectory + rawfilename + extension);
                        fontpath = filepath;
                    }
                    else
                    {
                        MessageBox.Show("Unsupported filetype: " + rawfilename + extension, "Unsupported",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    InstallFont(fontpath);
                    // Setup correct files and folders in FontsFolder
                    string fontsFile = fileFontDirectory + "\\fonts.conf";

                    Form1.AddFont(filename, fontpath);
                    FontFamily fontFamily = Form1.GetFontFamilyByName(filename);
                    if (fontFamily == null) return;

                    setupFontsDirectory(fontsFile, fontFamily.Name, Path.GetFileName(fontpath));

                    if (i == 0) addedFonts += filename;
                    else  addedFonts += ", " + filename;
                }

                if (addedFonts.Length > 0)
                {
                    MessageBox.Show("Success! The following font(s) has been added to your library!\n---\n" + addedFonts, "Font(s) Added!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Something must have gone wrong, no fonts seems to have been added...", "No Fonts Added", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                this.Close();
            }
        }

        private void setupFontsDirectory(string fontsFile, string fontname, string fontfilename)
        {
            string extension = Path.GetExtension(fontfilename);
            File.WriteAllText(fontsFile, FileTemplates.fonts_conf(fontname, extension, fontfilename));
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.dafont.com/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public static bool isFontExtension(string extension)
        {
            extension = extension.ToLower();
            return extension == ".ttf" || extension == ".tte" || extension == ".otf" ||
                   extension == ".woff" || extension == ".woff2" || extension == ".eot" || extension == ".fon";
        }

        private void addSysFont_button_Click(object sender, EventArgs e)
        {
            FontDialog fntDialog = new FontDialog();
            fntDialog.ShowColor = false;  
            fntDialog.ShowApply = false;  
            fntDialog.ShowEffects = false;  
            fntDialog.ShowHelp = false;
            fntDialog.MinSize = 22;
            fntDialog.MaxSize = 22;
            fntDialog.FontMustExist = true;
            if (fntDialog.ShowDialog() == DialogResult.OK)
            {
                Font selectedFont = fntDialog.Font;
                string fontFilePath = null;

                string fontFileName = GetSystemFontFileName(selectedFont);
                if (fontFileName == null)
                {
                    List<string> matchedFontFileNames = GetFilesForFont(selectedFont.Name).ToList();
                    if (matchedFontFileNames.Count > 0)
                    {
                        string[] mffn = matchedFontFileNames[0].Split('\\');
                        fontFileName = mffn[mffn.Length - 1];
                        fontFilePath = matchedFontFileNames[0];
                    }
                }
                else
                {
                    fontFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts) + "\\" + fontFileName;
                }

                if (fontFilePath != null && File.Exists(fontFilePath))
                {
                    // Copy from C:\Windows\Fonts\[FONTNAME] to FontsFolder
                    Directory.CreateDirectory(Form1.FontsFolder + selectedFont.Name);
                    File.Copy(fontFilePath, Form1.FontsFolder + selectedFont.Name + @"\" + fontFileName, true);

                    string fontsFile = Form1.FontsFolder + selectedFont.Name + "\\fonts.conf";

                    setupFontsDirectory(fontsFile, selectedFont.Name, Path.GetFileName(fontFilePath));
                
                    MessageBox.Show("Success! The following font(s) has been added to your library!\n---\n" + selectedFont.Name, "Font(s) Added!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectedFont.Dispose();
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"The filepath to '{selectedFont.Name}' could not be found. Please select another font.", "Font not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Call itself again
                    addSysFont_button_Click(null, null);
                }
            }
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
            if (!File.Exists(fontReg)) File.WriteAllBytes(fontReg, Resources.FontReg);

            var info = new ProcessStartInfo()
            {
                FileName = fontReg,
                Arguments = "/copy",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            info.WorkingDirectory = new FileInfo(fontFilePath).DirectoryName;

            var p = Process.Start(info);
            p.WaitForExit();

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

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
