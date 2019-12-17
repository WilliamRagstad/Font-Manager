using System;
using System.Collections.Generic;
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
        public const string AssemblyVersion = "3.1.0.2";
        public static string VersionNumber = "3.1";    // Remember to update stableVersion.txt when releasing a new stable update.
                                                       // This will notify all Font Manager 2.0 clients that there is an update available.
                                                       // To push the notification, commit and push to the master repository on GitHub.
        private readonly string CurrentVersion = "https://raw.githubusercontent.com/WilliamRagstad/Font-Manager/master/CSGO%20Font%20Manager/stableVersion.txt";
        
        public static string HomeFolder = $@"C:\Users\{Environment.UserName}\Documents\";
        public static string FontManagerFolder = HomeFolder + @"Font Manager\";
        public static string FontsFolder = FontManagerFolder + @"Fonts\";
        public static string DataPath = FontManagerFolder + @"Data\";
        private string SettingsFile = DataPath + "settings.json";

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

        private void AutoFocusRunningInstance()
        {
            // Not implemented
        }

        private void checkForUpdates()
        {
            string versionPattern = @"(\d+\.)(\d+\.?)+";

            // Get new version
            string newVersion = null;
            try
            {
                var webRequest = WebRequest.Create(CurrentVersion);

                using (var response = webRequest.GetResponse())
                using(var content = response.GetResponseStream())
                using(var reader = new StreamReader(content)){
                    newVersion = reader.ReadToEnd().Replace("\n","");


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
                        
            string rawLocalVersion = VersionNumber.Remove(VersionNumber.IndexOf('.') ,1).Replace(".",",").Split(' ')[0];  // Split in case version
            string rawNewVersion = newVersion.Remove(newVersion.IndexOf('.') ,1).Replace(".",",").Split(' ')[0];        // number contains "2.2 Alpha"
            // Convert to a comparable number
            float localVersionRepresentation = float.Parse(rawLocalVersion);
            float newVersionRepresentation   = float.Parse(rawNewVersion);

            if (VersionNumber.Trim() != newVersion.Trim() && newVersionRepresentation > localVersionRepresentation)
            {
                // New updated version is released

                if (MessageBox.Show(
                    $"Version {newVersion} is available to download from the official GitHub Repo!\n\n" +
                    "Do you want to continue getting update notifications?",
                    "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                {
                    Settings.BlockNoficationsForVersion = VersionNumber;
                }
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
            showFontPreview();
        }

        private void showFontPreview()
        {
            trackBar1.Visible = false;
            fontPreview_richTextBox.Visible = false;
            fontPreview_richTextBox.Text = fontPreviewText;
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
            if (Settings.showProTips)
            {
                MessageBox.Show("Protip: If you want you can also just drag-and-drop fonts inside the font list.", "Drag and Drop!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Settings.showProTips = false;
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
                    donate_button.Text = "Donate ♡";
                    donate_button.BackColor = Color.FromArgb(184, 253, 10);

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
                    if (Settings.CSGOPath != null)
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

                refreshFontList();
            }
        }

        public static void LoadCSGOFolder(bool manual = false)
        {
            string csgoDataPath = DataPath + @"\csgopath.dat";
            if (File.Exists(csgoDataPath))
            {
                Settings.CSGOPath = File.ReadAllText(csgoDataPath);
                csgoFontsFolder = Settings.CSGOPath + @"\csgo\panorama\fonts";
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
                                "Current CS:GO Folder: " + Settings.CSGOPath + "\n\nAre you sure you want to reset Font Manager?", "Reset?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            float zoomFactor = 0.2f;
            switch (trackBar1.Value)
            {
                case 1:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * -2;
                    break;
                case 2:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * -1;
                    break;
                case 3:
                    fontPreview_richTextBox.ZoomFactor = 1;
                    break;
                case 4:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * 1;
                    break;
                case 5:
                    fontPreview_richTextBox.ZoomFactor = 1 + zoomFactor * 2;
                    break;
                    
            }
        }

        private void fontPreview_richTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save settings
            SettingsManager.Save(Settings);
        }
    }
}
