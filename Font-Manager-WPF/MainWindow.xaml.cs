using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CSGO_Font_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LocateAssemblyLibrary.CurrentDomain_AssemblyResolve;
            InitializeComponent();
        }

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
        private static System.Drawing.Text.PrivateFontCollection _privateFontCollection = new System.Drawing.Text.PrivateFontCollection();

        //private int BaseWidth = 280;
        //private int BaseHeight = 67;

        //private float zoomFactor = 0.1f;

        public static bool running = true;

        public enum FormViews
        {
            Main,
            AddSystemFont
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
                        "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
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
                    App.Current.Shutdown();
                    /*
                    p.WaitForExit();

                    string output = p.StandardOutput.ReadLine();
                    string err = p.StandardError.ReadLine();
                    */
                }
                else if (MessageBox.Show(
                        $"Do you want to continue getting update notifications?\n" +
                        $"You will be required to manually download the next release yourself.",
                        "Update Notifications", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
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
                File.WriteAllBytes(UpdaterFile, Properties.Resources.updater);
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
                App.Current.Shutdown(); //new CancelEventArgs()
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

        private void showFontPreview()
        {
            FontPreview_RichTextBox.Document.Blocks.Clear();
            FontSize.Visibility = Visibility.Hidden;
            FontPreview_RichTextBox.Visibility = Visibility.Hidden;
            FontPreview_RichTextBox.Document.Blocks.Add(new Paragraph(new Run(fontPreviewText)));
            if (FontList.SelectedItem == null) return;
            string selectedFontName = FontList.SelectedItem.ToString();
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

                    System.Drawing.FontFamily fontFamily = GetFontFamilyByName(selectedFontName);
                    if (fontFamily == null) return;
                    System.Windows.Media.FontFamily oMediaFontFamily = new System.Windows.Media.FontFamily(fontFamily.Name);

                    FontPreview_RichTextBox.FontFamily = oMediaFontFamily;
                    FontPreview_RichTextBox.FontSize = 14;
                    //FontPreview_RichTextBox.Font = new Font(fontFamily, 14);
                }
                else
                {
                    MessageBox.Show("Font could not be found...");
                    return;
                }
            }
            else if (CurrentFormView == FormViews.AddSystemFont)
            {
                System.Drawing.FontFamily fontFamily = GetFontFamilyByName(selectedFontName);

                if (fontFamily == null)
                {
                    fontFamily = new System.Drawing.FontFamily(selectedFontName);
                }

                try
                {
                    System.Windows.Media.FontFamily oMediaFontFamily = new System.Windows.Media.FontFamily(fontFamily.Name);
                    FontPreview_RichTextBox.FontFamily = oMediaFontFamily;
                    FontPreview_RichTextBox.FontSize = 14;
                    //FontPreview_RichTextBox.Font = new Font(fontFamily, 14);
                    //FontPreview_RichTextBox.FontFamily = fontFamily;
                }
                catch
                {
                    FontPreview_RichTextBox.Visibility = Visibility.Hidden; // Something went wrong, don't show textbox
                }
            }

            FontPreview_RichTextBox.Visibility = Visibility.Visible;
            if (CurrentFormView == FormViews.Main) FontSize.Visibility = Visibility.Visible;
        }

        private void addFont_button_Click(object sender, EventArgs e)
        {
            if (Settings.ProTips)
            {
                MessageBox.Show("Protip: If you want you can also just drag-and-drop fonts inside the font list.", "Drag and Drop!", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    title_Lable.Content = "CS:GO Fonts";
                    addFont_button.Visibility = Visibility.Visible;
                    removefont_button.Visibility = Visibility.Visible;
                    FontSize.Visibility = Visibility.Visible;
                    Apply_Font.Content = "Apply Selected Font";
                    donate_button.Content = "Support me 🎉";
                    donate_button.Background = new SolidColorBrush(Color.FromArgb(255, 184, 253, 10));
                    search_textBox.Visibility = Visibility.Hidden;

                    refreshFontList();
                    break;

                case FormViews.AddSystemFont:
                    title_Lable.Content = "System Fonts";
                    addFont_button.Visibility = Visibility.Hidden;
                    removefont_button.Visibility = Visibility.Hidden;
                    FontSize.Visibility = Visibility.Hidden;
                    Apply_Font.Content = "Add Selected Font";
                    donate_button.Content = "Cancel";

                    donate_button.Background = new SolidColorBrush(Color.FromArgb(255, 196, 104, 92));//Color.FromArgb(196, 104, 92);
                    search_textBox.Visibility = Visibility.Visible;
                    search_textBox.Text = "";
                    search_textBox.Focus();

                    loadSystemFontList();
                    break;
            }
        }

        private void loadSystemFontList()
        {
            FontList.Items.Clear();
            FontFamily[] fontFamilies;
            // InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            var installedFontCollection = Fonts.SystemFontFamilies;
            // Get the array of FontFamily objects.
            fontFamilies = (FontFamily[])installedFontCollection;

            // The loop below creates a large string that is a comma-separated
            // list of all font family names.

            int count = fontFamilies.Length;
            for (int j = 0; j < count; ++j)
            {
                FontList.Items.Add(fontFamilies[j].FamilyNames);
            }
            FontList.SelectedIndex = 0;
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
            if (FontList.Dispatcher.CheckAccess())
            {
                AddListBoxItemCallback callback = new AddListBoxItemCallback(AddListBoxItem);
                this.Dispatcher.Invoke(callback, new object[] { item });
            }
            else
            {
                this.FontList.Items.Add(item);
            }
        }

        private void refreshFontList()
        {
            int activeFontIndex = 0;
            //refresh list of fonts
            string[] dirs = Directory.GetDirectories(FontsFolder);
            FontList.Items.Clear();
            int index = 0;
            foreach (string dir in dirs)
            {
                index++;

                string foldername = Path.GetFileName(dir);
                if (File.Exists(dir + "\\fonts.conf"))
                {
                    FontList.Items.Add(foldername);
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
            FontList.Items.Insert(0, defaultFontName);
            FontList.SelectedIndex = activeFontIndex;
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

        public static string csgoPath = "";

        public static void LoadCSGOFolder(bool manual = false)
        {
            if (Settings.CsgoPath != null) csgoFontsFolder = Settings.CsgoPath + @"\csgo\panorama\fonts";
            else
            {
                csgoPath = tryLocatingCSGOFolder();
                if (csgoPath == null || manual)
                {
                    InputBox ib = new InputBox();
                    ib.Title = "CSGO PATH";
                    ib.Description.Text = "Please Enter The CS:GO Path";
                    ib.ShowDialog();

                    //csgoPath// = Microsoft.VisualBasic.Interaction.InputBox("Enter path to Counter Strike: Global Offensive", "Enter CS:GO Path", @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive");
                    while (!Directory.Exists(csgoPath) && !File.Exists(csgoPath + @"\csgo.exe"))
                    {
                        if (MessageBox.Show("Counter Strike: Global Offensive was not found. Please enter a valid path", "Invalid path",
                                MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel) return;
                        ib.ShowDialog();
                        // csgoPath// = Microsoft.VisualBasic.Interaction.InputBox("Enter path to Counter Strike: Global Offensive", "Enter CS:GO Path", @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive");
                    }
                }

                if (MessageBox.Show("Successfully found a CS:GO Path! \n" + csgoPath + "\n\nIs this the correct path?",
                        "Path Found!", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
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
                    if (File.Exists(csgoDir + @"\csgo.exe"))//If a user has Multiple CSGO directories choose the one with the csgo.exe present :)
                    {
                        return csgoDir;
                    }
                }
            }

            return null;
        }

        #region Font Management

        public static System.Drawing.FontFamily GetFontFamilyByName(string name)   // name = LemonMilk
        {
            // This is probably unesessary
            foreach (System.Drawing.FontFamily fontFamily in System.Drawing.FontFamily.Families)
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

        private void filterFonts(string name)
        {
            string filterName = name.Trim().ToLower();
            for (int i = 0; i < FontList.Items.Count; i++)
            {
                string fontName = FontList.Items[i].ToString().ToLower();
                if (!fontName.Contains(filterName)) FontList.Items.RemoveAt(i);
                else
                {
                    void insertAt(int index)
                    {
                        object item = FontList.Items[i];
                        FontList.Items.RemoveAt(i);
                        FontList.Items.Insert(index, item);
                    }
                    // The font has a substring of the filter tag
                    if (fontName.StartsWith(filterName)) insertAt(0);
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save settings
            if (Settings != null)
                SettingsManager.Save(Settings);
        }

        private void FontList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            showFontPreview();
            showFontPreview();//I Let the second one stay (even though i have no clue why its there but i thought rather not delete it :)
        }

        private void FontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float zoomFactor = 0.1f;
            float StandardSize = 14.25f;

            if (FontPreview_RichTextBox == null)
                return;

            //This definitely is not perfect but i havent figured out something better
            switch (FontSize.Value)
            {
                case 1:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * -4;
                    break;

                case 2:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * -3;
                    break;

                case 3:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * -2;
                    break;

                case 4:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * -1;
                    break;

                case 5:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * 0;
                    break;

                case 6:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * 1;
                    break;

                case 7:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * 2;
                    break;

                case 8:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * 3;
                    break;

                case 9:
                    FontPreview_RichTextBox.FontSize = StandardSize + zoomFactor * 4;
                    break;
            }
        }

        private void FontList_Drop(object sender, DragEventArgs e)
        {
            FontList.IsEnabled = false;
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
                    if (extension != "") filename = rawfilename.Replace(extension, "");

                    string fontpath = null;

                    string fileFontDirectory = FontsFolder + sanitizeFilename(filename) + @"\";

                    if (Directory.Exists(fileFontDirectory))
                    {
                        if (MessageBox.Show(
                                $"The font '{filename}' is already in your library, do you want to overwrite it?",
                                "Overwrite Font?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
                                    matchedFiles += $"{j + 1} - {Path.GetFileName(fontfiles[j])}\n";
                                }

                                matchedFiles += "\n";

                                string message =
                                    "The zip file was containing multiple files, and you are only able to install ONE.\nPlease select the font you want to install from the list below:\n" +
                                    matchedFiles;
                                string selectedFont = Microsoft.VisualBasic.Interaction.InputBox(
                                    message, "Select font", fontfiles.Count.ToString());
                                int selectedFontIndex;
                                while (!(int.TryParse(selectedFont, out selectedFontIndex) && selectedFontIndex <= fontfiles.Count && selectedFontIndex > 0))
                                {
                                    if (selectedFont == "") // Assume cancel
                                    {
                                        // Remove the font folder
                                        Directory.Delete(fileFontDirectory, true);

                                        FontList.IsEnabled = true;
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
                        File.Copy(filepath, fileFontDirectory + rawfilename, true);
                        fontpath = fileFontDirectory + rawfilename;
                    }
                    else
                    {
                        MessageBox.Show("Unsupported filetype: " + rawfilename + extension, "Unsupported",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    bool fontWasInstalled = InstallFont(fontpath);
                    if (fontWasInstalled)
                    {
                        AddFont(filename, fontpath);

                        // Setup correct files and folders in FontsFolder
                        string fontsFile = fileFontDirectory + "\\fonts.conf";

                        System.Drawing.FontFamily fontFamily = GetFontFamilyByName(filename);
                        if (fontFamily == null)
                        {
                            FontList.IsEnabled = true;
                            return;
                        }

                        setupFontsDirectory(fontsFile, fontFamily.Name, Path.GetFileName(fontpath));

                        if (i == 0) addedFonts += fontFamily.Name;
                        else addedFonts += ", " + filename;
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
                    MessageBox.Show("Success! The following font(s) has been added to your library!\n---\n" + addedFonts, "Font(s) Added!", MessageBoxButton.OK, MessageBoxImage.Information);
                    refreshFontList();
                }
                else
                {
                    MessageBox.Show("Something must have gone wrong, no fonts seems to have been added...", "No Fonts Added", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            FontList.IsEnabled = true;
        }

        private void FontList_DragEnter(object sender, DragEventArgs e) => e.Effects = DragDropEffects.All;

        private void removefont_button_Click(object sender, RoutedEventArgs e)
        {
            if (FontList.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an Item first!", "Failed to Remove!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string curItem = FontList.SelectedItem.ToString();
                if (curItem == defaultFontName)
                {
                    MessageBox.Show("You can't remove the Default font!", "Failed to Remove!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (MessageBox.Show($"Are you sure you want to remove '{curItem}'?", "Remove font?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

                    try
                    {
                        Directory.Delete(FontsFolder + FontList.SelectedItem, true);
                    }
                    catch (Exception exception)
                    {
                        if (!(exception is UnauthorizedAccessException))
                        {
                            MessageBox.Show("Failed to remove " + FontList.SelectedItem + ".\n" + exception.Message, "Failed to Remove", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    refreshFontList();
                }
            }
        }

        private void Feedback_label_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Reset_label_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will only restore the path to Counter Strike: Global Offensive and restore utility programs (in case they need to be updated). If you want to delete all fonts, you must do so through the program.\n\n" +
                    "Current CS:GO Folder: " + Settings.CsgoPath + "\n\nAre you sure you want to reset Font Manager?", "Reset?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Directory.Delete(DataPath, true);
                Environment.Exit(0);
            }
        }

        private void About_label_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void donate_button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFormView == FormViews.Main) Process.Start(new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WMFWT9YN58D26&source=url") { UseShellExecute = true });
            else
            {
                switchView(FormViews.Main);
            }
        }
        [DllImport("shell32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsUserAnAdmin();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            if (!IsUserAnAdmin())
            {
                MessageBox.Show("Hey, for this program to function properly please start it as administrator", "", MessageBoxButton.OK, MessageBoxImage.Exclamation); Application.Current.Shutdown();
            }

            AutoFocusRunningInstance();
            version.Content = "Version: " + VersionNumber;

            SetupFolderStructure();

            SettingsManager = new JsonManager<Settings>(SettingsFile);
            Settings = SettingsManager.Load();

            checkForUpdates();
            LoadCSGOFolder();
            refreshFontList();

            // Update all texts
            switchView(FormViews.Main);
        }

        private void Apply_Font_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFormView == FormViews.Main)
            {
                string message = "Do you want to apply " + FontList.SelectedItem + " to CS:GO?";
                if (FontList.SelectedItem.Equals(defaultFontName))
                {
                    message = "Do you want to reset to the default font for CS:GO?";
                }
                MessageBoxResult MessageBoxResult = MessageBox.Show(message, "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (MessageBoxResult == MessageBoxResult.Yes)
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

                        if (FontList.SelectedItem.Equals(defaultFontName))
                        {
                            string csgoFontsConf = csgoFontsFolder + "\\fonts.conf";

                            File.WriteAllText(csgoFontsConf, FileTemplates.fonts_conf_backup());
                            MessageBox.Show("Successfully reset to the default font!", "Default Font Applied!",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            string fontsConfPath = FontsFolder + FontList.SelectedItem + "\\fonts.conf";
                            string csgoFontsConf = csgoFontsFolder + "\\fonts.conf";

                            // Generate a new fonts.conf
                            string fontFilePath = null;
                            foreach (string file in Directory.GetFiles(FontsFolder + FontList.SelectedItem))
                            {
                                if (IsFontExtension(Path.GetExtension(file)))
                                {
                                    fontFilePath = file;
                                    break;
                                }
                            }

                            if (fontFilePath != null)
                            {
                                System.Drawing.FontFamily fontFamily = GetFontFamilyByName(FontList.SelectedItem.ToString());
                                setupFontsDirectory(fontsConfPath, fontFamily.Name, Path.GetFileName(fontFilePath));
                            }

                            if (File.Exists(fontsConfPath))
                            {
                                //new FileIOPermission( FileIOPermissionAccess.Write ,csgoFontsConf).Demand(); https://github.com/dotnet/docs/issues/21021
                                File.Copy(fontsConfPath, csgoFontsConf, true);

                                // Add font file into csgo path
                                foreach (string file in Directory.GetFiles(FontsFolder + FontList.SelectedItem))
                                {
                                    if (IsFontExtension(Path.GetExtension(file))) File.Copy(file, csgoFontsFolder + @"\" + Path.GetFileName(file), true);
                                }

                                bool csgoIsRunning = Process.GetProcessesByName("csgo").Length != 0;

                                MessageBox.Show($"Successfully applied {FontList.SelectedItem}!" +
                                                (csgoIsRunning ? "\n\nRestart CS:GO for the font to take effect." : ""),
                                    "Font Applied!", MessageBoxButton.OK, MessageBoxImage.Information);

                                Settings.ActiveFont = FontList.SelectedItem.ToString();
                            }
                            else
                            {
                                MessageBox.Show("Failed to apply font " + FontList.SelectedItem + "!\nThe fonts.conf file was not found...", "Failed to Apply", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ooops! Seems like the CS:GO folder is unknown, please try to restart the program...", "No CS:GO Fonts folder", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (CurrentFormView == FormViews.AddSystemFont)
            {
                FontFamily fontFamily = new FontFamily(FontList.SelectedItem.ToString());
                //Font selectedFont = new Font(fontFamily, 14);

                string fontFilePath = null;

                string fontFileName = GetSystemFontFileName(fontFamily); // These return null verry often
                if (fontFileName == null)
                {
                    List<string> matchedFontFileNames = GetFilesForFont(fontFamily.Source).ToList(); // These return null verry often
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
                string fileFontDirectory = FontsFolder + sanitizeFilename(fontFamily.Source) + @"\";
                // Check if font already exists
                if (Directory.Exists(fileFontDirectory))
                {
                    if (MessageBox.Show(
                            $"The font '{fontFamily.Source}' is already in your library, do you want to overwrite it?",
                            "Overwrite Font?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        // Directory.Delete(fileFontDirectory, true);
                        fontAlreadyExisted = true;
                    }
                    else
                    {
                        FontList.IsEnabled = true;
                        return;
                    }
                }
                // AddFont(filename, fontpath);

                if (fontFilePath != null && File.Exists(fontFilePath))
                {
                    // Copy from C:\Windows\Fonts\[FONTNAME] to FontsFolder
                    if (!fontAlreadyExisted) Directory.CreateDirectory(fileFontDirectory);

                    //new FileIOPermission(FileIOPermissionAccess.Read, fontFilePath).Demand();
                    //new FileIOPermission(FileIOPermissionAccess.Write, fileFontDirectory + fontFileName).Demand();
                    File.Copy(fontFilePath, fileFontDirectory + fontFileName, true);

                    // Initialize the font
                    string fontsConfPath = fileFontDirectory + "fonts.conf";
                    setupFontsDirectory(fontsConfPath, fontFamily.Source, Path.GetFileName(fontFilePath));

                    MessageBox.Show("Success! The following font(s) has been added to your library!\n---\n" + fontFamily.Source, "Font(s) Added!", MessageBoxButton.OK, MessageBoxImage.Information);
                    //fontFamily.;
                    switchView(FormViews.Main);
                }
                else
                {
                    MessageBox.Show($"The filepath to '{fontFamily.Source}' could not be found. Please select another font.", "Font not found", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Call itself again
                    addFont_button_Click(null, null);
                }

                refreshFontList();
            }
        }

        private int previousLength = 0;
        private bool lastestWasReload = false;
        private void search_textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
    }
}