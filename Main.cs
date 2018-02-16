using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using jLib;
using MetroFramework;
using MetroFramework.Forms;
using Microsoft.VisualBasic.FileIO;
using SevenZipExtractor;
using WinSCP;
using SearchOption = System.IO.SearchOption;

namespace Tweak_Installer
{
    public partial class Main : MetroForm
    {
        public Main()
        {
            InitializeComponent();
        }

        private const string version = "2.0.4";
        public bool enabled;
        static bool verbose;
        public List<string> tweaks = new List<string>(), skip = new List<string>(), filenamesshort = new List<string>();
        public Session session;
        public Crawler crawler;
        public bool uicache, jtool, convert, dont_sign;
        public bool update;

        private void Form1_Load(object sender, EventArgs e)
        {
            metroTabControl1.SelectedIndex = 0;
            Text = $"Tweak Installer v{version}";
            if (Environment.GetCommandLineArgs().Contains("dont-update")) update = false;

            deleteIfExists("tic.exe");

            ContextMenu installmenu = new ContextMenu();
            installmenu.MenuItems.Add("Install Filza");
            installmenu.MenuItems[0].Click += InstallFilza;

            Install.ContextMenu = installmenu;

            ContextMenu uninstallmenu = new ContextMenu();
            uninstallmenu.MenuItems.Add("Uninstall Filza");
            uninstallmenu.MenuItems[0].Click += RemoveFilza;

            Uninstall.ContextMenu = uninstallmenu;
            if (!File.Exists("settings"))
            {
                string[] def = { "192.168.1.1", "22", "" };
                File.WriteAllLines("settings", def);
            }
            string[] data = File.ReadAllLines("settings"); //get ssh settings
            for (int i = 0; i != data.Length; i++)
            {
                data[i] = data[i].Split('#')[0];
            }
            IP.Text = data[0];
            Port.Text = data[1];
            Password.Text = data[2];
            if (Port.Text == "" || Port.Text == "root")
            {
                Port.Text = "22";
            }
        }

        #region Funcs
        private void RemoveFilza(object sender, EventArgs e)
        {
            MessageBox.Show("This could take up to a minute");
            Session s = getSession(IP.Text, "root", Password.Text, int.Parse(Port.Text));
            s.ExecuteCommand("rm -r /Applications/Filza.app");
            s.ExecuteCommand("uicache");
            s.Close();
        }
        private void InstallFilza(object sender, EventArgs e)
        {
            MessageBox.Show("This could take up to a minute");
            Session s = getSession(IP.Text, "root", Password.Text, int.Parse(Port.Text));
            s.ExecuteCommand("rm Filza.tar");
            s.ExecuteCommand("wget dl.sparko.me/Filza.tar");
            s.ExecuteCommand("tar -xf Filza.tar");
            s.ExecuteCommand("rm -r /Applications/Filza.app");
            s.ExecuteCommand("mv Filza.app /Applications/Filza.app");
            s.ExecuteCommand("uicache");
            s.Close();
        }

        public void log(string s)
        {
            if (!File.Exists("log.txt")) File.Create("log.txt").Close();
            try
            {
                File.AppendAllText("log.txt", s + Environment.NewLine);
                Console.WriteLine(s);
                output.Text += s + Environment.NewLine;
                output.SelectionStart = output.Text.Length;
            }
            catch
            {
                Thread.Sleep(1000);
                log(s);
            }
        }
        
        public void EmptyDir(string path, bool verbose1 = false)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                if (verbose1) log("Deleted " + path);
            }
            Directory.CreateDirectory(path);
            if (verbose1) log("Created directory " + path);
        }

        public void deleteIfExists(string path, bool verbose1 = false)
        {
            if (verbose1) log("Searching for " + path);
            if (File.Exists(path))
            {
                if (verbose1) log("Deleting " + path);
                File.Delete(path);
                if (verbose1) log("Deleted " + path);
            }
        }
        public void clean()
        {
            deleteIfExists("JMWCrypto.dll");
            EmptyDir("temp");
            deleteIfExists("data.tar");
            deleteIfExists("control.tar");
            deleteIfExists("control");
            deleteIfExists("postinst");
            deleteIfExists("prerm");
            deleteIfExists("postrm");
        }

        public void getOptions()
        {
            skip = File.Exists("skip.list") ? File.ReadAllLines("skip.list").ToList() : new List<string>();
        }
        public void extractDeb(string path)
        {
            clean();
            log("Extracting " + path);
            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(path))
                {
                    if (verbose) log("Extracting data.tar.lzma || data.tar.gz");
                    archiveFile.Extract("temp");
                    if (verbose) log("Extracted");
                }
                if (verbose) log("Extracting data.tar");
                var p = Process.Start(@"7z.exe", "e " + "temp\\data.tar." + (File.Exists("temp\\data.tar.lzma") ? "lzma" : "gz") + " -o.");
                if (verbose) log("Waiting for subprocess to complete");
                p.WaitForExit();
                if (verbose) log("Extracting control file");
                p = Process.Start(@"7z.exe", "e " + "temp\\control.tar.gz -o.");
                p.WaitForExit();
                if (verbose) log("Successfully extracted data.tar");
                using (ArchiveFile archiveFile = new ArchiveFile("data.tar"))
                {
                    if (verbose) log("Extracting deb files");
                    archiveFile.Extract("files");
                    if (verbose) log("Extracted");
                }
                using (ArchiveFile archiveFile = new ArchiveFile("control.tar"))
                {
                    archiveFile.Extract(".");
                }
                Dictionary<string, string> control = new Dictionary<string, string>();
                foreach (string i in File.ReadAllLines("control"))
                {
                    var j = i.Split(':');
                    if (j.Length < 2) return;
                    control.Add(j[0].ToLower().Replace(" ", ""), j[1]);
                }
                if (Directory.Exists("files\\Applications") && control.ContainsKey("skipsigning"))
                {
                    using (ArchiveFile archiveFile = new ArchiveFile("data.tar"))
                    {
                        archiveFile.Extract("temp");
                    }
                    foreach (string app in Directory.GetDirectories("temp\\Applications\\"))
                    {
                        File.Create(app.Replace("temp\\", "files\\") + "\\skip-signing").Close();
                    }
                }
                clean();
            }
            catch (Exception e)
            {
                log("Not a valid deb file / Access Denied");
                throw e;
            }
        }
        public Session getSession(string ip, string user, string password, int port)
        {
            log("Connecting");
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = ip,
                UserName = user,
                Password = password,
                PortNumber = port,
                GiveUpSecurityAndAcceptAnySshHostKey = true
            };
            Session session1 = new Session();
            try
            {
                session1.Open(sessionOptions);
            }
            catch (SessionRemoteException e)
            {
                if (e.ToString().Contains("refused")) MessageBox.Show("Error: SSH Connection Refused\nAre you jailbroken?\nHave you entered your devices IP and port correctly?");
                else if (e.ToString().Contains("Access denied")) MessageBox.Show("Error: SSH Connection Refused due to incorrect credentials. Are you sure you typed your password correctly?");
                else if (e.ToString().Contains("Cannot initialize SFTP protocol")) MessageBox.Show("Error: SFTP not available. Make sure you have sftp installed by default. For Yalu or Meridian, please install \"SCP and SFTP for dropbear\" by coolstar. For LibreIOS, make sure SFTP is moved to /usr/bin/.");
                else
                {
                    Clipboard.SetText(e.ToString());
                    MessageBox.Show("Unknown Error. Please use the big red bug report link and include some form of crash report. Error report copying to clipboard.");
                    throw e;
                }
                Environment.Exit(0);
            }
            log("Connected to SSH");
            return session1;
        }
        public void getJailbreakSpecificOptions(Session session1)
        {
            if (session1.FileExists("/usr/lib/SBInject"))
            {
                if (verbose) log("You're running Electa. I'll convert tweaks to that format & add entitlements to applications");
                convert = true;
                if (!session1.FileExists("/bootstrap/Library/Themes"))
                {
                    session1.CreateDirectory("/bootstrap/Library/Themes");
                    session1.ExecuteCommand("touch /bootstrap/Library/Themes/dont-delete");
                    log("Themes folder missing. Touching /bootstrap/Library/Themes/dont-delete to prevent this in future");
                }
                jtool = true;
            }
            if (session1.FileExists("/jb/"))
            {
                if (verbose) log("You're running LibreiOS. I'll add entitlements to applications");
                jtool = true;
            }
        }
        public void extractIPA(string path)
        {
            clean();
            log("Extracting IPA " + path);
            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(path))
                {
                    if (verbose) log("Extracting payload");
                    archiveFile.Extract("temp");
                }
                createDirIfDoesntExist("files\\Applications");
                foreach (string app in Directory.GetDirectories("temp\\Payload\\"))
                {
                    if (verbose) log("Moving payload");
                    Directory.Move(app, "files\\Applications\\" + new DirectoryInfo(app).Name);
                    if (verbose) log("Moved payload");
                }
            }
            catch (Exception e)
            {
                log("Not a valid IPA / Access Denied");
                throw e;
            }
        }
        public void createDirIfDoesntExist(string path, bool verbose1 = false)
        {
            if (!Directory.Exists(path))
            {
                if (verbose1) log("Creating directory " + path);
                Directory.CreateDirectory(path);
                if (verbose1) log("Created directory " + path);
            }
            else
            {
                if (verbose1) log("No need to create " + path + " as it already exists");
            }
        }
        public void moveDirIfPresent(string source, string dest, string parent = null, bool verbose1 = false)
        {
            if (Directory.Exists(source))
            {
                if (verbose1) log("Found " + source);
                if (parent != null)
                {
                    createDirIfDoesntExist(parent);
                    if (verbose1) log("Created " + parent);
                }
                FileSystem.MoveDirectory(source, dest, true);
                if (verbose1) log("Moved " + source + " to " + dest);
            }
        }
        public void extractZip(string path)
        {
            log("Extracting Zip " + path);
            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(path))
                {
                    if (verbose) log("Extracting zip");
                    archiveFile.Extract("temp");
                    if (verbose) log("Extracted zip");
                }
            }
            catch (Exception e)
            {
                log("Not a valid ZIP archive / Access Denied");
                throw e;
            }
            if (Directory.Exists("temp\\bootstrap\\"))
            {
                log("Found bootstrap");
                if (Directory.Exists("temp\\bootstrap\\Library\\SBInject\\"))
                {
                    createDirIfDoesntExist("files\\usr\\lib\\SBInject");
                    foreach (string file in Directory.GetFiles("temp\\bootstrap\\Library\\SBInject\\"))
                    {
                        File.Move(file, "files\\usr\\lib\\SBInject\\" + new FileInfo(file).Name);
                    }
                    foreach (string file in Directory.GetDirectories("temp\\bootstrap\\Library\\SBInject\\"))
                    {
                        Directory.Move(file, "files\\usr\\lib\\SBInject\\" + new DirectoryInfo(file).Name);
                    }
                    Directory.Delete("temp\\bootstrap\\Library\\SBInject", true);
                }
                moveDirIfPresent("temp\\bootstrap\\Library\\Themes\\", "files\\bootstrap\\Library\\Themes\\");
                foreach (string dir in Directory.GetDirectories("temp"))
                {
                    FileSystem.MoveDirectory(dir, "files\\" + new DirectoryInfo(dir).Name, true);
                }
                foreach (string file in Directory.GetFiles("temp"))
                {
                    File.Copy(file, "files\\" + new FileInfo(file).Name, true);
                }
            }
            else
            {
                log("Unrecognised format. Determining ability to install");
                List<string> exts = new List<string>();
                List<string> directories = new List<string>();
                foreach (string dir in Directory.GetDirectories("temp", "*", SearchOption.AllDirectories))
                {
                    directories.Add(new DirectoryInfo(dir).Name);
                }
                if (directories.Contains("bootstrap"))
                {
                    log("Found bootstrap");
                    foreach (string dir in Directory.GetDirectories("temp", "*", SearchOption.AllDirectories))
                    {
                        if (new DirectoryInfo(dir).Name == "bootstrap")
                        {
                            createDirIfDoesntExist("files\\bootstrap\\");
                            FileSystem.CopyDirectory(dir, "files\\bootstrap");
                            moveDirIfPresent("files\\bootstrap\\SBInject", "files\\bootstrap\\Library\\SBInject", "files\\bootstrap\\Library\\SBInject");
                            break;
                        }
                    }
                }
                else
                {
                    foreach (string i in Directory.GetFiles("temp"))
                    {
                        string ext = new FileInfo(i).Extension;
                        if (!exts.Contains(ext)) exts.Add(ext);
                    }
                    if (exts.Count == 2 && exts.Contains(".dylib") && exts.Contains(".plist"))
                    {
                        log("Substrate Addon. Installing");
                        createDirIfDoesntExist("files\\usr\\lib\\SBInject");
                        foreach (string i in Directory.GetFiles("temp"))
                        {
                            File.Copy(i, "files\\usr\\lib\\SBInject\\" + new FileInfo(i).Name, true);
                        }
                        moveDirIfPresent("files\\Library\\PreferenceBundles\\", "files\\bootstrap\\Library\\PreferenceBundles\\");
                        moveDirIfPresent("files\\Library\\PreferenceLoader\\", "files\\bootstrap\\Library\\PreferenceLoader\\");
                        moveDirIfPresent("files\\Library\\LaunchDaemons\\", "files\\bootstrap\\Library\\LaunchDaemons\\");
                    }
                    else
                    {
                        MessageBox.Show("Unsafe to install. To install this tweak you must do so manually. Press enter to continue...");
                        Environment.Exit(0);
                    }
                }
            }
        }
        public string convert_path(string i, bool unix = false)
        {
            if (!unix)
            {
                return i.Replace("\\", "/");//.Replace(" ", "\\ ").Replace("(", "\\(").Replace(")", "\\)").Replace("'", "\\'").Replace("@", "\\@");
            }

            return i.Replace("\\", "/").Replace(" ", "\\ ").Replace("(", "\\(").Replace(")", "\\)").Replace("'", "\\'").Replace("@", "\\@");
        }
        public void convertTweaks()
        {
            log("Converting to electra tweak format");
            createDirIfDoesntExist("files\\bootstrap");
            createDirIfDoesntExist("files\\bootstrap\\Library");
            if (Directory.Exists("files\\Library\\MobileSubstrate\\"))
            {
                if (verbose) log("Found MobileSubstrate");
                createDirIfDoesntExist("files\\usr\\lib\\SBInject");
                foreach (string file in Directory.GetFiles("files\\Library\\MobileSubstrate\\DynamicLibraries\\"))
                {
                    if (verbose) log("Moving Substrate file " + file + " to SBInject");
                    File.Move(file, "files\\usr\\lib\\SBInject\\" + new FileInfo(file).Name);
                }
                foreach (string file in Directory.GetDirectories("files\\Library\\MobileSubstrate\\DynamicLibraries\\"))
                {
                    if (verbose) log("Moving Substrate dir " + file + " to SBInject");
                    Directory.Move(file, "files\\usr\\lib\\SBInject\\" + new DirectoryInfo(file).Name);
                }
                Directory.Delete("files\\Library\\MobileSubstrate", true);
                if (verbose) log("Deleted MobileSubstrate folder");
            }
            moveDirIfPresent("files\\Library\\Themes\\", "files\\bootstrap\\Library\\Themes\\");
            moveDirIfPresent("files\\Library\\LaunchDaemons\\", "files\\bootstrap\\Library\\LaunchDaemons\\");
            moveDirIfPresent("files\\Library\\PreferenceBundles\\", "files\\bootstrap\\Library\\PreferenceBundles\\");
            moveDirIfPresent("files\\Library\\PreferenceLoader\\", "files\\bootstrap\\Library\\PreferenceLoader\\");
        }

        

        public void getFiles()
        {
            if (verbose) log("Getting all files");
            crawler = new Crawler(Environment.CurrentDirectory + "\\files"); //gets all files in the tweak
            crawler.Remove("DS_STORE");
        }
        public void installFiles(Session session1)
        {
            if (session1.FileExists("/entitlements.ent"))
            {
                session1.RemoveFiles("/entitlements.ent");
                if (verbose) log("Removed old entitlements file from the device");
            }
            createDirIfDoesntExist("backup");
            if (Directory.Exists("files\\Applications") && jtool)
            {
                File.Copy("entitlements.ent", "files\\entitlements.ent", true);
                if (verbose) log("Entitlements needed. Copying entitlements file");
            }
            if (Directory.Exists("files\\Applications\\electra.app"))
            {
                if (verbose) log("please no");
                MetroMessageBox.Show(this, "Please do not try this");
                Environment.Exit(0);
            }
            if (verbose) log("Creating directory list");
            string[] directories = Directory.GetDirectories("files", "*", searchOption: SearchOption.AllDirectories);
            if (verbose) log("Got list. Creating backup folders");
            foreach (string dir in directories)
            {
                if (!Directory.Exists("backup\\" + dir.Replace("files\\", "\\")))
                {
                    Directory.CreateDirectory("backup\\" + dir.Replace("files\\", "\\"));
                }
            }
            log("Preparing to install");

            if (verbose) log("Creating local file list");
            List<string> local = new List<string>();
            crawler.Files.ForEach(i => local.Add(convert_path(i)));

            if (verbose) log("Creating remote file list");
            List<string> remote = new List<string>();
            foreach (string i in Directory.GetDirectories("files"))
            {
                string dir = new DirectoryInfo(i).Name;
                if (dir == "System")
                {
                    log("This tweak may take longer than usual to process (45 second max)");
                }
                session1.ExecuteCommand("find /" + dir + " > ~/files.list");
                session1.GetFiles("/var/root/files.list", "files.list");
                foreach (string file in File.ReadAllLines("files.list"))
                {
                    remote.Add(file);
                }
                File.Delete("files.list");
            }

            List<string> duplicates = new List<string>();
            foreach (string i in local)
            {
                if (remote.Contains(i))
                {
                    duplicates.Add(i);
                }
            }
            bool overwrite = false;
            foreach (var i in duplicates)
            {
                bool go = false, action = false;
                if (!overwrite)
                {
                    if (verbose) log(convert_path(i) + " already exists");
                    var dialog = new YNAD("Do you want to backup and overwrite " + convert_path(i) + "? (y/n/a)");
                    dialog.ShowDialog();
                    while (true)
                    {
                        switch (dialog.result)
                        {
                            case 1:
                                go = true;
                                action = true;
                                break;
                            case 3:
                                go = true;
                                action = true;
                                overwrite = true;
                                break;
                            case 2:
                                go = true;
                                break;
                        }
                        log("\n");
                        if (go) break;
                    }
                }
                else
                {
                    action = true;
                }
                if (!action)
                {
                    if (verbose) log("Skipping file " + i);
                    File.Delete("files\\" + i);
                    if (!skip.Contains(i))
                    {
                        skip.Add(i);
                    }
                }
                session1.GetFiles(convert_path(i), @"backup\" + i.Replace("/", "\\"));
            }
            log("Installing");
            foreach (string dir in Directory.GetDirectories("files"))
            {
                if (verbose) log("Installing directory " + dir);
                session1.PutFiles(dir, "/"); //put directories
            }
            foreach (string file in Directory.GetFiles("files"))
            {
                if (verbose) log("Installing file " + file);
                session1.PutFiles(file, "/"); //put files
            }
            File.WriteAllLines("skip.list", skip);
            if (Directory.Exists("files\\Applications") && jtool)
            {
                if (verbose) log("Entitlements needed");
                session1.PutFiles("entitlements.ent", "/");
                if (verbose) log("Sending entitlements");
                log("Signing applications");
                foreach (var app in Directory.GetDirectories("files\\Applications\\"))
                {
                    uicache = true;
                    if (verbose) log("Signing " + convert_path(app.Replace("files\\", "\\")));
                    crawler = new Crawler(app);
                    crawler.Files.ForEach(i =>
                    {
                        bool sign = new FileInfo(i).Name.Split('.').Length < 2;
                        if (!sign)
                        {
                            if (i.Split('.').Last() == "dylib") sign = true;
                        }
                        i = convert_path(i);
                        if (File.Exists(app + "\\skip-signing"))
                        {
                            sign = false;
                            if (verbose) log("Skipped Signing " + i);
                        }
                        if (sign && !dont_sign)
                        {
                            session1.ExecuteCommand("jtool -e arch -arch arm64 " + convert_path(app.Replace("files\\", "\\")) + i);
                            session1.ExecuteCommand("mv " + convert_path(app.Replace("files\\", "\\")) + i + ".arch_arm64 " + convert_path(app.Replace("files\\", "\\")) + i);
                            session1.ExecuteCommand("jtool --sign --ent /entitlements.ent --inplace " + convert_path(app.Replace("files\\", "\\")) + i);
                            if (verbose) log("Signed " + convert_path(app.Replace("files\\", "\\")) + i);
                        }
                    });
                    crawler = new Crawler("files");
                    crawler.Files.ForEach(i =>
                    {
                        session1.ExecuteCommand("chmod 777 " + convert_path(i.Replace("\\files", "")));
                    });
                }
            }
            if (File.Exists("postinst"))
            {
                if (verbose) log("Running postinst script");
                session1.PutFiles("postinst", "script");
                session1.ExecuteCommand("./script && rm script");
            }
            clean();
            Finish(session1);
            log("Done");
        }
        public void Finish(Session session1)
        {
            if (uicache && auto.Checked)
            {
                log("Running uicache (may take up to 30 seconds)");
                session1.ExecuteCommand("uicache"); //respring
            }
            if (auto.Checked)
            {
                log("Respringing...");
                session1.ExecuteCommand("killall -9 SpringBoard"); //respring
            }
            session1.Close();
        }

        private void Respring_Click(object sender, EventArgs e)
        {
            session = getSession(IP.Text, "root", Password.Text, int.Parse(Port.Text));
            log("Respringing");
            session.ExecuteCommand("killall -9 SpringBoard");
            log("Done");
            log("");
        }

        private void UiCache_Click(object sender, EventArgs e)
        {
            session = getSession(IP.Text, "root", Password.Text, int.Parse(Port.Text));
            log("Running uicache");
            session.ExecuteCommand("uicache");
            session.ExecuteCommand("Done");
            log("");
            session.Close();
        }

        private void auto_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = "";
            var i = openFileDialog1.ShowDialog();
            switch (i)
            {
                case DialogResult.OK:
                    session = getSession(IP.Text, "root", Password.Text, int.Parse(Port.Text));
                    session.PutFiles(openFileDialog1.FileName, "file");
                    session.ExecuteCommand("jtool -e arch -arch arm64 file && mv file.arch_arm64 file && jtool --ent file >> new.ent");
                    session.GetFiles("new.ent", new FileInfo(openFileDialog1.FileName).Name + ".ent");
                    session.ExecuteCommand("rm new.ent file");
                    session.Close();
                    output.Text += "Extracted entitlements to Tweak Installer directory" + Environment.NewLine;
                    break;
            }
        }

        private void customentitlements_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = "Entitlements|*.ent|All Files|*.*";
            var i = openFileDialog1.ShowDialog();
            switch (i)
            {
                case DialogResult.OK:
                    if (File.Exists(openFileDialog1.FileName))
                    {
                        deleteIfExists("entitlements.ent");
                        File.Copy(openFileDialog1.FileName, "entitlements.ent");
                        output.Text += "Using custom entitlements" + Environment.NewLine;
                    }
                    break;
            }
        }

        private void defaultentitlements_Click(object sender, EventArgs e)
        {
            if (File.Exists("platform-binary.ent"))
            {
                deleteIfExists("entitlements.ent");
                File.Copy("platform-binary.ent", "entitlements.ent");
                output.Text += "Using default entitlements" + Environment.NewLine;
            }
            else
            {
                using (WebClient c = new WebClient())
                {
                    c.DownloadFile("https://raw.githubusercontent.com/josephwalden13/tweak-installer/master/bin/Debug/platform-binary.ent", "platform-binary.ent");
                    deleteIfExists("entitlements.ent");
                    File.Copy("platform-binary.ent", "entitlements.ent");
                    output.Text += "Using default entitlements" + Environment.NewLine;
                }
            }
        }

        private void DontSign_Click(object sender, EventArgs e)
        {
            dont_sign = true;
            output.Text += "Won't sign applications" + Environment.NewLine;
        }

        private void verbosemode_Click(object sender, EventArgs e)
        {
            verbose = true;
            output.Text += "Verbose mode" + Environment.NewLine;
        }

        private void openterminal_Click(object sender, EventArgs e)
        {
            Process.Start("putty.exe", IP.Text + ":" + Port.Text + " -l root -pw " + Password.Text);
        }

        private void metroButton1_Click_1(object sender, EventArgs e)
        {
            Process.Start("https://github.com/josephwalden13/tweak-installer/issues");
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            Process.Start("https://twitter.com/jmw_2468");
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            Process.Start("https://twitter.com/passivemodding");
        }

        private void metroButton9_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/PassiveModding");
        }

        private void metroButton8_Click(object sender, EventArgs e)
        {

        }

        private void metroButton7_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/ZKXqt2a");
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.reddit.com/user/josephwalden/");
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/josephwalden13/");
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            Process.Start("http://paypal.me/JosephWalden");
        }

        private void metroButton10_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.reddit.com/r/jailbreak/");
        }

        private void metroButton11_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/3Bs98ga");
        }

        private void metroButton8_Click_1(object sender, EventArgs e)
        {
            Process.Start("https://coolstar.org/electra/");
        }

        public void emptyDir(string path, bool verbose1 = false)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                if (verbose1) log("Deleted " + path);
            }
            Directory.CreateDirectory(path);
            if (verbose1) log("Created directory " + path);
        }
        public void uninstallFiles(Session session1)
        {
            log("Preparing to uninstall");
            bool overwrite = false;
            List<string> remove = new List<string>();
            crawler.Files.ForEach(i =>
            {
                if (!skip.Contains(i))
                {
                    bool go = false, action = false;
                    if (File.Exists("backup" + i) && !overwrite)
                    {
                        if (verbose) log("You have a backup of this file");
                        var dialog = new YNAD("Do you want to restore " + convert_path(i) + " from your backup? (y/n/a)");
                        dialog.ShowDialog();
                        while (true)
                        {
                            switch (dialog.result)
                            {
                                case 1:
                                    go = true;
                                    action = true;
                                    break;
                                case 3:
                                    go = true;
                                    action = true;
                                    overwrite = true;
                                    break;
                                case 2:
                                    go = true;
                                    break;
                            }
                            log("\n");
                            if (go) break;
                        }
                    }
                    if (action || overwrite)
                    {
                        string path = i.Replace(i.Substring(i.LastIndexOf('\\')), "");
                        session1.PutFiles(new FileInfo("backup" + convert_path(i)).ToString().Replace("/", "\\"), convert_path(path) + "/" + new FileInfo(i).Name);
                        if (verbose) log("Reinstalled " + i);
                    }
                    else
                    {
                        remove.Add(convert_path(i, true));
                    }
                }
            });
            log("Uninstalling");
            string script = "";
            foreach (string i in remove)
            {
                script += "rm " + i + "\n";
            }
            File.WriteAllText("script.sh", script);
            session1.PutFiles("script.sh", "script.sh");
            session1.ExecuteCommand("sh script.sh");
            if (Directory.Exists("files\\Applications"))
            {
                if (verbose) log("uicache refresh required");
                uicache = true;
            }
            log("Locating and removing *some* empty folders");
            session1.ExecuteCommand("find /System/Library/Themes/ -type d -empty -delete");
            session1.ExecuteCommand("find /usr/ -type d -empty -delete");
            session1.ExecuteCommand("find /Applications/ -type d -empty -delete");
            session1.ExecuteCommand("find /Library/ -type d -empty -delete");
            session1.ExecuteCommand("find /bootstrap/Library/Themes/* -type d -empty -delete");
            session1.ExecuteCommand("find /bootstrap/Library/PreferenceLoader/* -type d -empty -delete");
            session1.ExecuteCommand("find /bootstrap/Library/PreferenceBundles/* -type d -empty -delete");
            session1.ExecuteCommand("find /bootstrap/Library/SBInject/* -type d -empty -delete");
            if (File.Exists("postrm"))
            {
                if (verbose) log("Running postrm script");
                session1.PutFiles("postrm", "script");
                session1.ExecuteCommand("./script && rm script");
            }
            clean();
            Finish(session1);
            log("Done");
        }
        #endregion
        private void Install_Click(object sender, EventArgs e)
        {
            EmptyDir("files");
            if (!enabled)
            {
                MessageBox.Show("Please select a deb, ipa or zip first");
                return;
            }
            clean();
            getOptions();
            session = getSession(IP.Text, "root", Password.Text, int.Parse(Port.Text));
            getJailbreakSpecificOptions(session);
            foreach (string tweak in tweaks)
            {
                clean();
                if (tweak.Contains(".deb"))
                {
                    extractDeb(tweak);
                }
                else if (tweak.Contains(".ipa"))
                {
                    extractIPA(tweak);
                }
                else
                {
                    extractZip(tweak);
                }
            }
            if (convert) convertTweaks();
            getFiles();
            installFiles(session);
            log("");
        }

        private void Select_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "Tweaks|*.deb;*.zip;*.ipa";
            tweaks.Clear();
            TweakList.Text = "";
            var f = openFileDialog1.ShowDialog();
            switch (f)
            {
                case DialogResult.OK:
                {
                    enabled = true;
                    foreach (string i in openFileDialog1.FileNames)
                    {
                        tweaks.Add(i);
                    }
                    foreach (string i in openFileDialog1.SafeFileNames)
                    {
                        filenamesshort.Add(i);
                    }
                    break;
                }
                default:
                    enabled = false;
                    break;
            }

            TweakList.Lines = filenamesshort.ToArray();
        }
        private void Uninstall_Click(object sender, EventArgs e)
        {
            emptyDir("files");
            if (!enabled)
            {
                MessageBox.Show("Please select a deb, ipa or zip first");
                return;
            }
            clean();
            getOptions();
            session = getSession(IP.Text, "root", Password.Text, int.Parse(Port.Text));
            getJailbreakSpecificOptions(session);
            foreach (string tweak in tweaks)
            {
                clean();
                if (tweak.Contains(".deb"))
                {
                    extractDeb(tweak);
                }
                else if (tweak.Contains(".ipa"))
                {
                    extractIPA(tweak);
                }
                else
                {
                    extractZip(tweak);
                }
            }
            if (convert) convertTweaks();
            if (File.Exists("prerm"))
            {
                if (verbose) log("Running prerm script");
                session.PutFiles("prerm", "script");
                session.ExecuteCommand("./script && rm script");
            }
            getFiles();
            uninstallFiles(session);
            log("");
        }
    }
}
