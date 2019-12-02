using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CSGO_Font_Manager
{
    public partial class Form1 : Form
    {
        public static string VersionNumber = "3.0";    // Remember to update stableVersion.txt when releasing a new stable update.
                                                       // This will notify all Font Manager 2.0 clients that there is an update available.
                                                       // To push the notification, commit and push to the master repository on GitHub.
        public static string HomeFolder = $@"C:\Users\{Environment.UserName}\Documents\csgo\";
        public static string FontManagerFolder = HomeFolder + @"Font Manager\";
        public static string FontsFolder = FontManagerFolder + @"Fonts\";
        public static string DataPath = FontManagerFolder + @"Data\";
        public static string UpdaterPath = DataPath + "FontManagerUpdater.exe";
        public static string FirstStartupPath = DataPath + "firstStartup.txt";

        protected static string UpdaterToken = "cf38519aeddfb438e69e1f8a4b1412dd";

        public static string csgoFolder = null;
        public static string csgoFontsFolder = null;
        public static bool isFirstStartup = true;

        public static string defaultFontName = "Default Font";

        public static string fontPreviewText = "The quick brown fox jumps over the lazy dog. 100 - + / = 23.5";

        public static FormViews CurrentFormView = FormViews.Main;

        public enum FormViews
        {
            Main,
            AddSystemFont
        }

        public Form1()
        {
            InitializeComponent();
            version_label.Text = "Version " + VersionNumber;
            AllowDrop = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupFolderStructure();
            checkForUpdates();
            LoadCSGOFolder();
            refreshFontList();

            // Check if first startup
            if (File.Exists(FirstStartupPath))
            {
                isFirstStartup = File.ReadAllText(FirstStartupPath) != "1";
                File.WriteAllText(FirstStartupPath, "1");
            }
            else
            {
                isFirstStartup = true;
                File.WriteAllText(FirstStartupPath, "1");
            }


            // Update all texts
            switchView(FormViews.Main);
        }

        private void checkForUpdates()
        {
            // Extract the updater binary
            try
            {
                if (!File.Exists(UpdaterPath)) File.WriteAllBytes(UpdaterPath, Properties.Resources.FontManagerUpdater);
            }
            catch { }

            string programPath = System.Reflection.Assembly.GetEntryAssembly().Location; // Get the location where the program (.exe) was started from
            ProcessStartInfo psi = new ProcessStartInfo(UpdaterPath, $"\"{UpdaterToken}\" \"{VersionNumber}\" \"{programPath}\"");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true; 
            var p = Process.Start(psi);
            p.WaitForExit();
            string response = p.StandardOutput.ReadToEnd();
            string errors = p.StandardError.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(errors))
            {
                // throw new Exception(errors);
            }

        }

        private static void SetupFolderStructure()
        {
            Directory.CreateDirectory(FontsFolder);
            Directory.CreateDirectory(FontManagerFolder);
            Directory.CreateDirectory(HomeFolder);
            Directory.CreateDirectory(DataPath);
        }
        
        private void listBox1_Click(object sender, EventArgs e)
        {
            showFontPreview();
        }

        private void showFontPreview()
        {
            fontPreview_richTextBox.Visible = false;
            fontPreview_richTextBox.Text = fontPreviewText;
            string selectedFontName = listBox1.SelectedItem.ToString();
            if (selectedFontName == defaultFontName) return;
            if (CurrentFormView == FormViews.Main)
            {
                if (selectedFontName == defaultFontName)
                {
                    fontPreview_richTextBox.Text = "";
                    fontPreview_richTextBox.Visible = true;
                    return;
                }

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

                fontPreview_richTextBox.Font = new Font(fontFamily, 14);
            }
            
            
            fontPreview_richTextBox.Visible = true;
        }

        private void addFont_button_Click(object sender, EventArgs e)
        {
            if (isFirstStartup)
            {
                MessageBox.Show("Protip: If you want you can also just drag-and-drop fonts inside the font list.", "Drag and Drop!", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    apply_button.Text = "Apply Selected Font";
                    donate_button.Text = "Donate ♡";
                    donate_button.BackColor = Color.FromArgb(184, 253, 10);

                    refreshFontList();
                    break;
                case FormViews.AddSystemFont:
                    title_label.Text = "System Fonts";
                    addFont_button.Visible = false;
                    remove_button.Visible = false;
                    apply_button.Text = "Add Selected Font";
                    donate_button.Text = "Cancel";
                    donate_button.BackColor = Color.FromArgb(196, 104, 92);

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
                .Replace("!", "")
                .Replace("(", "")
                .Replace(")", "");

            // Clean up filename
            sanitized = sanitized.Replace("_", " ");
            sanitized = char.ToUpper(sanitized[0]) + sanitized.Remove(0,1); // Capital letter

            return sanitized;
        }

        delegate void AddListBoxItemCallback(string text);
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
                    //DialogResult dialogResult = MessageBox.Show($"Are you sure you want to remove {listBox1.SelectedItem}?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    //if (dialogResult == DialogResult.Yes)
                    //{
                    for (int v = 0; v < listBox1.SelectedItems.Count; v++)
                    {
                        string removeFontName = listBox1.GetItemText(listBox1.SelectedItem);
                        try
                        {
                            Directory.Delete(FontsFolder + removeFontName, true);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show("Failed to remove " + removeFontName + ".\n" + exception.Message,
                                "Failed to Remove", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        refreshFontList();
                    }
                    //}
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/WilliamRagstad/Font-Manager");
        }

        private void donate_button_Click(object sender, EventArgs e)
        {
            if (CurrentFormView == FormViews.Main) System.Diagnostics.Process.Start("http://steamcommunity.com/tradeoffer/new/?partner=112166023&token=CZXW0gba");
            else
            {
                switchView(FormViews.Main);
            }
        }

        private void refreshFontList()
        {
            //refresh list of fonts
            string[] dirs = Directory.GetDirectories(FontsFolder);
            listBox1.Items.Clear();
            foreach (string dir in dirs)
            {
                string foldername = Path.GetFileName(dir);
                if (File.Exists(dir + "\\fonts.conf"))
                {
                    listBox1.Items.Add(foldername);

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
            }

            // Add default font
            listBox1.Items.Insert(0, defaultFontName);
            listBox1.SelectedIndex = 0;
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
                    if (csgoFolder != null)
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
            else
            {
                FontFamily fontFamily = new FontFamily(listBox1.SelectedItem.ToString());
                Font selectedFont = new Font(fontFamily, 14);

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

                if (fontFilePath != null && File.Exists(fontFilePath))
                {
                    // Copy from C:\Windows\Fonts\[FONTNAME] to FontsFolder
                    if (!fontAlreadyExisted) Directory.CreateDirectory(fileFontDirectory);
                    File.Copy(fontFilePath, fileFontDirectory + fontFileName, true);

                    setupFontsDirectory(fileFontDirectory + "fonts.conf", selectedFont.Name, Path.GetFileName(fontFilePath));
                
                    // MessageBox.Show("Success! The following font(s) has been added to your library!\n---\n" + selectedFont.Name, "Font(s) Added!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectedFont.Dispose();
                    switchView(FormViews.Main);
                }
                else
                {
                    MessageBox.Show($"The filepath to '{selectedFont.Name}' could not be found. Please select another font.", "Font not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Call itself again
                    addFont_button_Click(null, null);
                }
            }
        }

        public static void LoadCSGOFolder(bool manual = false)
        {
            string csgoDataPath = DataPath + @"\csgopath.dat";
            if (File.Exists(csgoDataPath))
            {
                csgoFolder = File.ReadAllText(csgoDataPath);
                csgoFontsFolder = csgoFolder + @"\csgo\panorama\fonts";
            }
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
                    File.WriteAllText(csgoDataPath, csgoPath);
                    LoadCSGOFolder(); // Update the csgo folder path
                }
                else
                {
                    LoadCSGOFolder(true);
                }
            }
        }

        private static string tryLocatingCSGOFolder()
        {
            string csgoFolder = null;
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            string p_programFiles = @"Program[\x20]*Files";
            string p_steam = @"steam";

            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady)
                {
                    foreach (string subDirectory_drive in Directory.GetDirectories(drive.Name))
                    {
                        string directoryName_drive = Path.GetFileName(subDirectory_drive);
                        if (Regex.IsMatch(directoryName_drive, p_programFiles, RegexOptions.IgnoreCase))
                        {
                            // Search its sub folders for the Steam folder
                            foreach (string subDirectory_progfiles in Directory.GetDirectories(subDirectory_drive))
                            {
                                string directoryName_progfiles = Path.GetFileName(subDirectory_progfiles);
                                if (Regex.IsMatch(directoryName_progfiles, p_steam, RegexOptions.IgnoreCase))
                                {
                                    // Steam folder is found, check if csgo.exe exists inside that folder
                                    string csgo = subDirectory_progfiles + @"\steamapps\common\Counter-Strike Global Offensive\";
                                    if (Directory.Exists(csgo))
                                    {
                                        csgoFolder = csgo;
                                        return csgoFolder;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return csgoFolder;
        }



        #region Font Management

        private static PrivateFontCollection _privateFontCollection = new PrivateFontCollection();

        public static FontFamily GetFontFamilyByName(string name)   // name = LemonMilk
        {
            // This is probably unesessary
            foreach (FontFamily fontFamily in System.Drawing.FontFamily.Families)
            {
                if (fontFamily.Name == name) return fontFamily;
            }
            
            return _privateFontCollection.Families.FirstOrDefault(x => // x = Lemon/Milk
            {
                return StringSimilarity(name, x.Name) > 0.75f;
            });
            
            return null;
        }

        private static float StringSimilarity(string a, string b)
        {
            int matchingChars = 0;
            int offset = 0;
            int minLen = Math.Min(a.Length, b.Length);
            for (int i = 0; i < minLen - offset; i++)
            {
                char c = char.ToLower(a[i+offset]);
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

        #endregion

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show("This will only restore the path to Counter Strike: Global Offensive and restore utility programs (in case they need to be updated). If you want to delete all fonts, you must do so through the program.\n\n" + 
                                "Current CS:GO Folder: " + csgoFolder + "\n\nAre you sure you want to reset Font Manager?", "Reset?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Directory.Delete(DataPath, true);
                LoadCSGOFolder();
                checkForUpdates();
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(
                "https://docs.google.com/forms/d/e/1FAIpQLSfkChgD2T-RYNyfBCRL2EjUQfJ3y8tvPKemGJca2kMU1jV8AQ/viewform?usp=sf_link");
        }
    }
}
