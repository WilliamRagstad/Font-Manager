﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CSGO_Font_Manager
{
    public partial class Form1 : Form
    {
        public const string AssemblyVersion = "3.5.0.0";
        public static string VersionNumber = "3.5";    // Remember to update stableVersion.txt when releasing a new stable update.

                                                       // This will notify all Font Manager 2.0 clients that there is an update available.
                                                       // To push the notification, commit and push to the master repository on GitHub.
        private readonly string CurrentVersion = "https://raw.githubusercontent.com/WilliamRagstad/Font-Manager/master/CSGO%20Font%20Manager/stableVersion.txt";

        public static string OldFontManagerFolder = $@"C:\Users\{Environment.UserName}\Documents\Font Manager\";
        public static string HomeFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\";

        public static string FontManagerFolder = HomeFolder + @"Font Manager\";
        public static string FontsFolder = FontManagerFolder + @"Fonts\";
        public static string DataPath = FontManagerFolder + @"Data\";

        private static string SettingsFile = DataPath + "settings.json";
        private static string UpdaterFile = DataPath + "updater.exe";
        private static string UpdaterExecName = "FontManagerUpdater";

        public static Settings Settings;
        public static JsonManager<Settings> SettingsManager;

        public static string csgoFontsFolder = null;

        public static string defaultFontName = "Default Font";
        public static string fontPreviewText = "The quick brown fox jumps over the lazy dog. 100 - + / = 23.5";

        public static FormViews CurrentFormView = FormViews.Main;
        private static PrivateFontCollection _privateFontCollection = new PrivateFontCollection();

        public enum FormViews
        {
            Main,
            AddSystemFont
        }

        public Form1()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LocateAssemblyLibrary.CurrentDomain_AssemblyResolve;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AutoFocusRunningInstance();
            version_label.Text = "Version " + VersionNumber;

            SetupFolderStructure();

            SettingsManager = new JsonManager<Settings>(SettingsFile);
            Settings = SettingsManager.Load();

            checkForUpdates();
            LoadCSGOFolder();
            refreshFontList();

            // Update all texts
            switchView(FormViews.Main);
        }

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        private void AutoFocusRunningInstance()
        {
            string title = Process.GetCurrentProcess().ProcessName;
            Process[] runningFM = Process.GetProcessesByName(title);
            if (runningFM.Length > 1)
            {
                SetForegroundWindow(runningFM[0].MainWindowHandle);
                Environment.Exit(0);
            }
        }

        private void checkForUpdates()
        {
            if (Settings.HideNewUpdates) return;
            if (UpdaterIsRunning()) return;

            string versionPattern = @"(\d+\.)(\d+\.?)+";

            // Get new version
            string newVersion = null;
            try
            {
                var webRequest = WebRequest.Create(CurrentVersion);

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    newVersion = reader.ReadToEnd().Replace("\n", "");
                }
            }
            catch (Exception e)
            {
                // User is probably offline
                Console.WriteLine(e);
            }

            if (!Regex.IsMatch(newVersion, versionPattern))
            {
                Console.WriteLine("New version number is in an invalid format.");
                return;
            }

            string rawLocalVersion = VersionNumber.Remove(VersionNumber.IndexOf('.'), 1).Replace(".", ",").Split(' ')[0];  // Split in case version
            string rawNewVersion = newVersion.Remove(newVersion.IndexOf('.'), 1).Replace(".", ",").Split(' ')[0];        // number contains "2.2 Alpha"
            // Convert to a comparable number
            float localVersionRepresentation = float.Parse(rawLocalVersion);
            float newVersionRepresentation = float.Parse(rawNewVersion);

            if (VersionNumber.Trim() != newVersion.Trim() && newVersionRepresentation > localVersionRepresentation)
            {
                // New updated version is released

                if (MessageBox.Show(
                        $"Version {newVersion} is available to download from the official GitHub Repo!\n\n" +
                        "Do you want to download the update now?",
                        "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    // Call updater.exe
                    string fmExe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var processInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        FileName = UpdaterFile,
                        Arguments = $"\"{VersionNumber}\" \"{fmExe}\""
                    };
                    Process p = Process.Start(processInfo);
                    Application.Exit();
                    /*
                    p.WaitForExit();

                    string output = p.StandardOutput.ReadLine();
                    string err = p.StandardError.ReadLine();
                    */
                }
                else if (MessageBox.Show(
                        $"Do you want to continue getting update notifications?\n" +
                        $"You will be required to manually download the next release yourself.",
                        "Update Notifications", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                {
                    Settings.HideNewUpdates = true;
                }
            }
        }

        private static bool UpdaterIsRunning()
        {
            Process[] processes = Process.GetProcessesByName(UpdaterExecName);
            return processes.Length != 0;
        }

        private static void ExtractUpdater()
        {
            if (UpdaterIsRunning()) return;

            try
            {
                // Extract updater
                //File.WriteAllBytes(UpdaterFile, Properties.Resources.updater);
            }
            catch { }
        }

        private static void SetupFolderStructure()
        {
            if (Directory.Exists(HomeFolder))
            {
                Directory.CreateDirectory(FontManagerFolder);
                Directory.CreateDirectory(FontsFolder);
                Directory.CreateDirectory(DataPath);
            }
            else
            {
                MessageBox.Show("Appdata does not exist... You're not running this on linux or mac huh..?", "No can do");
                Application.Exit(new CancelEventArgs());
            }
            ExtractUpdater();

            // Transfer all files from old (if existing) FontManager folder to the new one in AppData
            if (Directory.Exists(OldFontManagerFolder))
            {
                if (Directory.Exists(OldFontManagerFolder + @"Fonts\"))
                    foreach (string fontFolder in Directory.GetFileSystemEntries(OldFontManagerFolder + @"Fonts\"))
                    {
                        string dir = FontsFolder + Path.GetFileName(fontFolder) + @"\";
                        if (Directory.Exists(dir)) continue;
                        Directory.CreateDirectory(dir);
                        foreach (string file in Directory.GetFiles(fontFolder))
                        {
                            string dst = dir + Path.GetFileName(file);
                            if (!File.Exists(dst))
                                File.Copy(file, dst);
                        }
                    }

                if (File.Exists(OldFontManagerFolder + @"Data\settings.json") && !File.Exists(SettingsFile))
                    File.Copy(OldFontManagerFolder + @"Data\settings.json", SettingsFile);

                if (Directory.Exists(OldFontManagerFolder))
                    Directory.Delete(OldFontManagerFolder, true);
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            showFontPreview();
            showFontPreview();
        }

        private void showFontPreview()
        {
            trackBar1.Visible = false;
            fontPreview_richTextBox.Visible = false;
            fontPreview_richTextBox.Text = fontPreviewText;
            if (listBox1.SelectedItem == null) return;
            string selectedFontName = listBox1.SelectedItem.ToString();
            if (selectedFontName == defaultFontName) return;
            if (CurrentFormView == FormViews.Main)
            {
                if (selectedFontName == defaultFontName) return;

                // Load the font file inside the folder
                string fontFolder = FontsFolder + selectedFontName;
                if (Directory.Exists(fontFolder))
                {
                    // find the font file
                    string fontFile = null;
                    foreach (string file in Directory.GetFiles(fontFolder))
                    {
                        if (IsFontExtension(Path.GetExtension(file)))
                        {
                            fontFile = file;
                            break;
                        }
                    }
                    if (fontFile == null) return;

                    FontFamily fontFamily = GetFontFamilyByName(selectedFontName);
                    if (fontFamily == null) return;
                    fontPreview_richTextBox.Font = new Font(fontFamily, 14);
                }
                else
                {
                    MessageBox.Show("Font could not be found...");
                    return;
                }
            }
            else if (CurrentFormView == FormViews.AddSystemFont)
            {
                FontFamily fontFamily = GetFontFamilyByName(selectedFontName);
                if (fontFamily == null)
                {
                    fontFamily = new FontFamily(selectedFontName);
                }

                try
                {
                    fontPreview_richTextBox.Font = new Font(fontFamily, 14);
                }
                catch
                {
                    fontPreview_richTextBox.Visible = false; // Something went wrong, don't show textbox
                }
            }

            fontPreview_richTextBox.Visible = true;
            if (CurrentFormView == FormViews.Main) trackBar1.Visible = true;
        }

        private void addFont_button_Click(object sender, EventArgs e)
        {
            if (Settings.ProTips)
            {
                MessageBox.Show("Protip: If you want you can also just drag-and-drop fonts inside the font list.", "Drag and Drop!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Settings.ProTips = false;
            }

            switchView(FormViews.AddSystemFont);
            showFontPreview();
        }

        private void switchView(FormViews view)
        {
            CurrentFormView = view;
            switch (view)
            {
                case FormViews.Main:
                    title_label.Text = "CS:GO Fonts";
                    addFont_button.Visible = true;
                    remove_button.Visible = true;
                    trackBar1.Visible = true;
                    apply_button.Text = "Apply Selected Font";
                    donate_button.Text = "Support me 🎉";
                    donate_button.BackColor = Color.FromArgb(184, 253, 10);
                    search_textBox.Visible = false;

                    refreshFontList();
                    break;

                case FormViews.AddSystemFont:
                    title_label.Text = "System Fonts";
                    addFont_button.Visible = false;
                    remove_button.Visible = false;
                    trackBar1.Visible = false;
                    apply_button.Text = "Add Selected Font";
                    donate_button.Text = "Cancel";
                    donate_button.BackColor = Color.FromArgb(196, 104, 92);
                    search_textBox.Visible = true;
                    search_textBox.Text = "";
                    search_textBox.Focus();

                    loadSystemFontList();
                    break;
            }
        }

        private void loadSystemFontList()
        {
            listBox1.Items.Clear();
            FontFamily[] fontFamilies;
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();

            // Get the array of FontFamily objects.
            fontFamilies = installedFontCollection.Families;

            // The loop below creates a large string that is a comma-separated
            // list of all font family names.

            int count = fontFamilies.Length;
            for (int j = 0; j < count; ++j)
            {
                listBox1.Items.Add(fontFamilies[j].Name);
            }
            listBox1.SelectedIndex = 0;
        }

        private string sanitizeFilename(string filename)
        {
            string sanitized = Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(filename));

            sanitized = sanitized
                .Replace("?", "")
                .Replace("&", "")
                .Replace("\"", "")
                .Replace("/", "")
                .Replace("\\", "")
                .Replace("!", "")
                .Replace("(", "")
                .Replace(")", "");

            // Clean up filename
            sanitized = sanitized.Replace("_", " ");
            sanitized = char.ToUpper(sanitized[0]) + sanitized.Remove(0, 1); // Capital letter

            return sanitized;
        }

        private delegate void AddListBoxItemCallback(string text);

        public void AddListBoxItem(object item)
        {
            if (this.InvokeRequired)
            {
                AddListBoxItemCallback callback = new AddListBoxItemCallback(AddListBoxItem);
                this.Invoke(callback, new object[] { item });
            }
            else
            {
                this.listBox1.Items.Add(item);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Item first!", "Failed to Remove!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string curItem = listBox1.SelectedItem.ToString();
                if (curItem == defaultFontName)
                {
                    MessageBox.Show("You can't remove the Default font!", "Failed to Remove!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (MessageBox.Show($"Are you sure you want to remove '{curItem}'?", "Remove font?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                    try
                    {
                        Directory.Delete(FontsFolder + listBox1.SelectedItem, true);
                    }
                    catch (Exception exception)
                    {
                        if (!(exception is UnauthorizedAccessException))
                        {
                            MessageBox.Show("Failed to remove " + listBox1.SelectedItem + ".\n" + exception.Message, "Failed to Remove", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    refreshFontList();
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/WilliamRagstad/Font-Manager/blob/master/README.md#introduction");
        }

        private void donate_button_Click(object sender, EventArgs e)
        {
            if (CurrentFormView == FormViews.Main) Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WMFWT9YN58D26&source=url");
            else
            {
                switchView(FormViews.Main);
            }
        }

        private void refreshFontList()
        {
            int activeFontIndex = 0;
            //refresh list of fonts
            string[] dirs = Directory.GetDirectories(FontsFolder);
            listBox1.Items.Clear();
            int index = 0;
            foreach (string dir in dirs)
            {
                index++;

                string foldername = Path.GetFileName(dir);
                if (File.Exists(dir + "\\fonts.conf"))
                {
                    listBox1.Items.Add(foldername);
                    if (Settings.ActiveFont != null && Settings.ActiveFont.Equals(foldername)) activeFontIndex = index;

                    // Add the font to the private font collection
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        if (IsFontExtension(Path.GetExtension(file)))
                        {
                            // _privateFontCollection.AddFontFile(file);
                            AddFont(foldername, file);
                        }
                    }
                }
                else
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch { }
                }
            }

            // Add default font
            listBox1.Items.Insert(0, defaultFontName);
            listBox1.SelectedIndex = activeFontIndex;
        }

        private List<string> GetFiles(string path, string pattern)
        {
            var files = new List<string>();

            try
            {
                files.AddRange(Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly));
                foreach (var directory in Directory.GetDirectories(path))
                    files.AddRange(GetFiles(directory, pattern));
            }
            catch (UnauthorizedAccessException) { }

            return files;
        }

        private void apply_button_Click(object sender, EventArgs e)
        {
            if (CurrentFormView == FormViews.Main)
            {
                string message = "Do you want to apply " + listBox1.SelectedItem + " to CS:GO?";
                if (listBox1.SelectedItem.Equals(defaultFontName))
                {
                    message = "Do you want to reset to the default font for CS:GO?";
                }
                DialogResult dialogResult = MessageBox.Show(message, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Get the csgo path (home folder...) if data file not found...
                    if (Settings.CsgoPath != null)
                    {
                        // Make sure the folders exists
                        string csgoConfD = csgoFontsFolder + "\\conf.d";
                        if (!Directory.Exists(csgoFontsFolder)) Directory.CreateDirectory(csgoFontsFolder);
                        if (!Directory.Exists(csgoConfD)) Directory.CreateDirectory(csgoConfD);

                        // Remove all existing font files (.ttf, .otf, etc...)
                        foreach (string file in Directory.GetFiles(csgoFontsFolder))
                        {
                            if (IsFontExtension(Path.GetExtension(file))) File.Delete(file);
                        }

                        if (listBox1.SelectedItem.Equals(defaultFontName))
                        {
                            string csgoFontsConf = csgoFontsFolder + "\\fonts.conf";

                            File.WriteAllText(csgoFontsConf, FileTemplates.fonts_conf_backup());
                            MessageBox.Show("Successfully reset to the default font!", "Default Font Applied!",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string fontsConfPath = FontsFolder + listBox1.SelectedItem + "\\fonts.conf";
                            string csgoFontsConf = csgoFontsFolder + "\\fonts.conf";

                            // Generate a new fonts.conf
                            string fontFilePath = null;
                            foreach (string file in Directory.GetFiles(FontsFolder + listBox1.SelectedItem))
                            {
                                if (IsFontExtension(Path.GetExtension(file)))
                                {
                                    fontFilePath = file;
                                    break;
                                }
                            }

                            if (fontFilePath != null)
                            {
                                FontFamily fontFamily = GetFontFamilyByName(listBox1.SelectedItem.ToString());
                                setupFontsDirectory(fontsConfPath, fontFamily.Name, Path.GetFileName(fontFilePath));
                            }

                            if (File.Exists(fontsConfPath))
                            {
                                new FileIOPermission(FileIOPermissionAccess.Write, csgoFontsConf).Demand();
                                File.Copy(fontsConfPath, csgoFontsConf, true);

                                // Add font file into csgo path
                                foreach (string file in Directory.GetFiles(FontsFolder + listBox1.SelectedItem))
                                {
                                    if (IsFontExtension(Path.GetExtension(file))) File.Copy(file, csgoFontsFolder + @"\" + Path.GetFileName(file), true);
                                }

                                bool csgoIsRunning = Process.GetProcessesByName("csgo").Length != 0;

                                MessageBox.Show($"Successfully applied {listBox1.SelectedItem}!" +
                                                (csgoIsRunning ? "\n\nRestart CS:GO for the font to take effect." : ""),
                                    "Font Applied!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                Settings.ActiveFont = listBox1.SelectedItem.ToString();
                            }
                            else
                            {
                                MessageBox.Show("Failed to apply font " + listBox1.SelectedItem + "!\nThe fonts.conf file was not found...", "Failed to Apply", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ooops! Seems like the CS:GO folder is unknown, please try to restart the program...", "No CS:GO Fonts folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (CurrentFormView == FormViews.AddSystemFont)
            {
                FontFamily fontFamily = new FontFamily(listBox1.SelectedItem.ToString());
                Font selectedFont = new Font(fontFamily, 14);

                string fontFilePath = null;

                string fontFileName = GetSystemFontFileName(selectedFont); // These return null verry often
                if (fontFileName == null)
                {
                    List<string> matchedFontFileNames = GetFilesForFont(selectedFont.Name).ToList(); // These return null verry often
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

                bool fontAlreadyExisted = false;
                string fileFontDirectory = FontsFolder + sanitizeFilename(selectedFont.Name) + @"\";
                // Check if font already exists
                if (Directory.Exists(fileFontDirectory))
                {
                    if (MessageBox.Show(
                            $"The font '{selectedFont.Name}' is already in your library, do you want to overwrite it?",
                            "Overwrite Font?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        // Directory.Delete(fileFontDirectory, true);
                        fontAlreadyExisted = true;
                    }
                    else
                    {
                        listBox1.Enabled = true;
                        return;
                    }
                }
                // AddFont(filename, fontpath);

                if (fontFilePath != null && File.Exists(fontFilePath))
                {
                    // Copy from C:\Windows\Fonts\[FONTNAME] to FontsFolder
                    if (!fontAlreadyExisted) Directory.CreateDirectory(fileFontDirectory);

                    new FileIOPermission(FileIOPermissionAccess.Read, fontFilePath).Demand();
                    new FileIOPermission(FileIOPermissionAccess.Write, fileFontDirectory + fontFileName).Demand();
                    File.Copy(fontFilePath, fileFontDirectory + fontFileName, true);

                    // Initialize the font
                    string fontsConfPath = fileFontDirectory + "fonts.conf";
                    setupFontsDirectory(fontsConfPath, fontFamily.Name, Path.GetFileName(fontFilePath));

                    MessageBox.Show("Success! The following font(s) has been added to your library!\n---\n" + selectedFont.Name, "Font(s) Added!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectedFont.Dispose();
                    switchView(FormViews.Main);
                }
                else
                {
                    MessageBox.Show($"The filepath to '{selectedFont.Name}' could not be found. Please select another font.", "Font not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Call itself again
                    addFont_button_Click(null, null);
                }

                refreshFontList();
            }
        }

        public static void LoadCSGOFolder(bool manual = false)
        {
            if (Settings.CsgoPath != null) csgoFontsFolder = Settings.CsgoPath + @"\csgo\panorama\fonts";
            else
            {
                string csgoPath = tryLocatingCSGOFolder();
                if (csgoPath == null || manual)
                {
                    csgoPath = Microsoft.VisualBasic.Interaction.InputBox("Enter path to Counter Strike: Global Offensive", "Enter CS:GO Path", @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive");
                    while (!Directory.Exists(csgoPath) && !File.Exists(csgoPath + @"\csgo.exe"))
                    {
                        if (MessageBox.Show("Counter Strike: Global Offensive was not found. Please enter a valid path", "Invalid path",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel) return;
                        csgoPath = Microsoft.VisualBasic.Interaction.InputBox("Enter path to Counter Strike: Global Offensive", "Enter CS:GO Path", @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive");
                    }
                }

                if (MessageBox.Show("Successfully found a CS:GO Path! \n" + csgoPath + "\n\nIs this the correct path?",
                        "Path Found!", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    SetupFolderStructure(); // Make sure all folders exists...
                    Settings.CsgoPath = csgoPath;
                    LoadCSGOFolder(); // Update the csgo fonts folder path
                }
                else
                {
                    LoadCSGOFolder(true);
                }
            }
        }

        // https://developer.valvesoftware.com/wiki/Counter-Strike:_Global_Offensive_Game_State_Integration#Locating_CS:GO_Install_Directory
        // Improved csgo installation detection by bernieplayshd #14
        private static string tryLocatingCSGOFolder()
        {
            // Locate the Steam installation directory
            string steamDir = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", ""),
                   libsFile = steamDir + @"\steamapps\libraryfolders.vdf";

            Regex regex = new Regex("\"\\d+\".*\"(.*?)\"", RegexOptions.Compiled);

            List<string> libraries = new List<string> { steamDir.Replace('/', '\\') };

            // Find all Steam game libraries
            if (File.Exists(libsFile))
            {
                foreach (string line in File.ReadAllLines(libsFile))
                {
                    foreach (Match match in regex.Matches(line))
                    {
                        if (match.Success && match.Groups.Count != 0)
                        {
                            libraries.Add(match.Groups[1].Value.Replace("\\\\", "\\"));
                            break;
                        }
                    }
                }
            }

            // Search them for the CS:GO installation
            foreach (string lib in libraries)
            {
                string csgoDir = lib + @"\steamapps\common\Counter-Strike Global Offensive";
                if (Directory.Exists(csgoDir))
                {
                    return csgoDir;
                }
            }

            return null;
        }

        #region Font Management

        public static FontFamily GetFontFamilyByName(string name)   // name = LemonMilk
        {
            // This is probably unesessary
            foreach (FontFamily fontFamily in FontFamily.Families)
            {
                if (SimplifyName(fontFamily.Name) == SimplifyName(name)) return fontFamily;
            }

            return _privateFontCollection.Families.FirstOrDefault(x => // x = Lemon/Milk
            {
                return StringSimilarity(SimplifyName(name), SimplifyName(x.Name)) > 0.75f;
            });
        }

        private static string SimplifyName(string str) => str.Replace(" ", "").ToLower();

        private static float StringSimilarity(string a, string b)
        {
            int matchingChars = 0;
            int offset = 0;
            int minLen = Math.Min(a.Length, b.Length);
            for (int i = 0; i < minLen - offset; i++)
            {
                char c = char.ToLower(a[i + offset]);
                char d = char.ToLower(b[i]);
                if (c == d)
                {
                    matchingChars++;
                }
                else
                {
                    bool offsetFound = false;
                    // Check forwards for a matching char and change the offset to there in such case
                    while (i + offset < a.Length)
                    {
                        char e = char.ToLower(a[i + offset]);
                        if (e == d)
                        {
                            matchingChars++;
                            offsetFound = true;
                            break;
                        }
                        offset++;
                    }

                    if (!offsetFound) offset = 0;
                }
            }

            float matchPrecentage = (float)matchingChars / minLen;
            return matchPrecentage;
        }

        public static void AddFont(string fontname, string fullFileName)
        {
            AddFont(File.ReadAllBytes(fullFileName));
            _privateFontCollection.AddFontFile(fullFileName);
        }

        public static void AddFont(byte[] fontBytes)
        {
            var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
            IntPtr pointer = handle.AddrOfPinnedObject();
            try
            {
                _privateFontCollection.AddMemoryFont(pointer, fontBytes.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        #endregion Font Management

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show("This will only restore the path to Counter Strike: Global Offensive and restore utility programs (in case they need to be updated). If you want to delete all fonts, you must do so through the program.\n\n" +
                                "Current CS:GO Folder: " + Settings.CsgoPath + "\n\nAre you sure you want to reset Font Manager?", "Reset?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Directory.Delete(DataPath, true);
                Environment.Exit(0);
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(
                "https://docs.google.com/forms/d/e/1FAIpQLSfkChgD2T-RYNyfBCRL2EjUQfJ3y8tvPKemGJca2kMU1jV8AQ/viewform?usp=sf_link");
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            float zoomFactor = 0.1f;
            switch (trackBar1.Value)
            {
                case 1:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * -4;
                    break;

                case 2:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * -3;
                    break;

                case 3:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * -2;
                    break;

                case 4:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * -1;
                    break;

                case 5:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * 0;
                    break;

                case 6:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * 1;
                    break;

                case 7:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * 2;
                    break;

                case 8:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * 3;
                    break;

                case 9:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * 4;
                    break;
            }
        }

        private void fontPreview_richTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private int previousLength = 0;
        private bool lastestWasReload = false;

        private void search_textBox_TextChanged(object sender, EventArgs e)
        {
            if (search_textBox.Text.Length > previousLength)
            {
                filterFonts(search_textBox.Text);
                lastestWasReload = false;
            }
            else if (!lastestWasReload)
            {
                loadSystemFontList();
                filterFonts(search_textBox.Text);
                lastestWasReload = true;
            }

            previousLength = search_textBox.Text.Length;
        }

        private void filterFonts(string name)
        {
            string filterName = name.Trim().ToLower();
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                string fontName = listBox1.Items[i].ToString().ToLower();
                if (!fontName.Contains(filterName)) listBox1.Items.RemoveAt(i);
                else
                {
                    void insertAt(int index)
                    {
                        object item = listBox1.Items[i];
                        listBox1.Items.RemoveAt(i);
                        listBox1.Items.Insert(index, item);
                    }
                    // The font has a substring of the filter tag
                    if (fontName.StartsWith(filterName)) insertAt(0);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save settings
            if (Settings != null)
                SettingsManager.Save(Settings);
        }
    }
}