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
    public partial class Form1 : Form
    {
        public static string VersionNumber = "2.15";    // Remember to update stableVersion.txt when releasing a new stable update.
                                                        // This will notify all Font Manager 2.0 clients that there is an update available.
                                                        // To push the notification, commit and push to the master repository on GitHub.
        public static string HomeFolder = $@"C:\Users\{Environment.UserName}\Documents\csgo\";
        public static string FontManagerFolder = HomeFolder + @"Font Manager 2.0\";
        public static string FontsFolder = FontManagerFolder + @"Fonts\";
        public static string DataPath = FontManagerFolder + @"Data\";

        public static string csgoFolder = null;
        public static string csgoFontsFolder = null;

        public static string defaultFontName = "* Default Font";

        public Form1()
        {
            InitializeComponent();
            version_label.Text = "Version " + VersionNumber;
            AllowDrop = true;
        }
        
        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 1)
            {
                pictureBox2.Visible = true;
                apply_button.Visible = true;
                remove_button.Enabled = true;
                showFontPreview();
                // If it's not showing, try again... Might just have been a random failure
                if (!fontPreview_richTextBox.Visible) showFontPreview();
            }
            else
            {
                pictureBox2.Visible = false;
                apply_button.Visible = false;
                remove_button.Enabled = false;
            }
        }

        private void showFontPreview()
        {
            fontPreview_richTextBox.Visible = false;
            string selectedFontName = listBox1.SelectedItem.ToString();

            if (selectedFontName == defaultFontName)
            {
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
                    if (AddFontWindow.isFontExtension(Path.GetExtension(file)))
                    {
                        fontFile = file;
                        break;
                    }
                }

                if (fontFile == null) return;
                
                AddFont(selectedFontName, fontFile);

                FontFamily fontFamily = GetFontFamilyByName(selectedFontName);
                if (fontFamily == null) return;
                fontPreview_richTextBox.Font = new Font(fontFamily, 14);
            }
            else
            {
                MessageBox.Show("Font could not be found...");
                return;
            }
            
            fontPreview_richTextBox.Visible = true;
        }


        private void addFont_button_Click(object sender, EventArgs e)
        {
            //Open the Drag and Drop window
            AddFontWindow AddFontfrm = new AddFontWindow();
            AddFontfrm.ShowDialog();
            refreshFontList();
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
                    DialogResult dialogResult = MessageBox.Show($"Are you sure you want to remove {listBox1.SelectedItem}?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
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
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkForUpdates();
            fontPreview_richTextBox.Visible = false;
            SetupFolderStructure();
            LoadCSGOFolder();
            refreshFontList();
        }

        private void checkForUpdates()
        {
            string currentStableVersionURL =
                "https://raw.githubusercontent.com/WilliamRagstad/Font-Manager-2.0/master/CSGO%20Font%20Manager%202.0/stableVersion.txt";
            var webRequest = WebRequest.Create(currentStableVersionURL);

            using (var response = webRequest.GetResponse())
            using(var content = response.GetResponseStream())
            using(var reader = new StreamReader(content)){
                var newVersion = reader.ReadToEnd();
                if (VersionNumber != newVersion)
                {
                    // New version is released
                    if (MessageBox.Show(
                        "A new version of Font Manager is available!\nDo you want to install it now?\nVersion: " +
                        newVersion, "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        Process.Start("https://gamebanana.com/tools/6732");
                    }
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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox1 AboutFrm = new AboutBox1();
            AboutFrm.Show();
        }

        private void donate_button_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://steamcommunity.com/tradeoffer/new/?partner=112166023&token=CZXW0gba");
        }

        private void refresh_button_Click(object sender, EventArgs e)
        {
            refreshFontList();
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
                }
            }

            // Add default font
            listBox1.Items.Add(defaultFontName);

            pictureBox2.Hide();
            apply_button.Hide();
            remove_button.Enabled = false;
            fontPreview_richTextBox.Visible = false;
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
            string message = "Do you want to apply " + listBox1.GetItemText(listBox1.SelectedItem.ToString()) +
                             " to Counter-Strike Global Offensive?";
            if (listBox1.SelectedItem.Equals(defaultFontName))
            {
                message = "Do you want to reset to the default font for Counter-Strike Global Offensive?";
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
                    
                            MessageBox.Show("Successfully applied selected font!", "Font Applied!", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            #region Match definitions

            string p_programFiles = @"Program[\x20]*Files";
            string p_steam = @"steam";

            #endregion

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

        public static FontFamily GetFontFamilyByName(string name)
        {
            return _privateFontCollection.Families.FirstOrDefault(x =>
            {
                int len = Math.Min(x.Name.Length, name.Length);
                int matchingChars = 0;
                for (int i = 0; i < len; i++)
                {
                    if (char.ToLower(x.Name[i]) == char.ToLower(name[i])) matchingChars++;
                }

                float matchPrecentage = (float) matchingChars / len;
                return matchPrecentage > 0.75f;
            });
        }

        public static void AddFont(string fontname, string fullFileName)
        {
            AddFont(File.ReadAllBytes(fullFileName));

            if (GetFontFamilyByName(fontname) == null)
            {
                // Try adding it using the filename
                _privateFontCollection.AddFontFile(fullFileName);
            }
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
            if (MessageBox.Show("Are you sure you want to reset Font Manager?\n" + 
                                "This will only restore the path to Counter Strike: Global Offensive. If you want to delete all fonts, you must do so through the program.\n\n" + 
                                "Current CS:GO Folder: " + csgoFolder, "Reset?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Directory.Delete(DataPath, true);
                LoadCSGOFolder();
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(
                "https://docs.google.com/forms/d/e/1FAIpQLSfkChgD2T-RYNyfBCRL2EjUQfJ3y8tvPKemGJca2kMU1jV8AQ/viewform?usp=sf_link");
        }
    }
}
