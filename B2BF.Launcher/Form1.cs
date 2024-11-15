using AutoUpdaterDotNET;
using B2BF.Common.Account;
using B2BF.Common.Data;
using B2BF.Common.Helpers;
using B2BF.Common.Networking.GameSpy.CdKey;
using B2BF.Common.Networking.GameSpy.Login;
using B2BF.Common.Networking.GameSpy.Report;
using B2BF.Common.Networking.GameSpy.Search;
using B2BF.Common.Networking.Http;
using B2BF.Common.Updater;
using B2BF.Launcher.Helpers;
using System.Diagnostics;

namespace B2BF.Launcher
{
    public partial class Form1 : Form
    {
        private OldPhoenixUpdater _updater;

        public Form1()
        {
            if (!PlatformHelper.NeedAdmin() && PlatformHelper.IsRunningAsAdmin())
            {

            }

            InitializeComponent();

            _updater = new OldPhoenixUpdater();
            _updater.NotifyAction += OnNotify;
            _updater.ProgressBarAction += OnProgress;
            _updater.StartButtonAction += OnButton;

            AccountInfo.OnAccountInfoChanged += AccountInfoChanged;

            LoginServer.Start();
            SearchServer.Start();
            ReportServer.Start();
            HttpServer.Start();
            CdKeyServer.Start();
            ServerListHelper.Start();

            if (Directory.Exists(Path.Combine(Settings.BF2GamePath, "mods")))
            {
                var directories = Directory.GetDirectories(Path.Combine(Settings.BF2GamePath, "mods")).Select(x => x.Replace(Path.Combine(Settings.BF2GamePath, "mods") + "\\", "")).ToArray();
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(directories);
            }

            checkBox1.Checked = Settings.Fullscreen;
            checkBox2.Checked = Settings.Restart;
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Settings.Language);
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf(Settings.Mod);

            Application.ApplicationExit += Application_ApplicationExit;

            AsyncInit();
        }

        private void Application_ApplicationExit(object? sender, EventArgs e)
        {
            Settings.Fullscreen = checkBox1.Checked;
            Settings.Restart = checkBox2.Checked;
            Settings.Language = comboBox1.SelectedItem.ToString();
            Settings.Mod = comboBox2.SelectedItem.ToString();
        }

        private async Task AsyncInit()
        {
            if (!AccountInfo.HasLoggedInUser())
            {
                button1.Enabled = false;
            }
            else
            {
                if (!await AccountInfo.ValidateTokenAsync())
                {
                    button1.Enabled = false;
                    Settings.RememberMeContainer = "";
                    return;
                }

                label2.Text = "Status: Logged In";
                label3.Text = "Username: " + AccountInfo.Username;
                button2.Visible = false;

                if (string.IsNullOrEmpty(Settings.GamePath))
                {
                    var existingInstall = RegistryHelper.GetBattlefield2Installation();
                    var prompt = string.Join("\n\n",
                        "Detected an existing Battlefield 2 installation at:",
                        existingInstall,
                        "Do you want to use that instead of downloading a fresh copy?"
                    );
                    if (!string.IsNullOrEmpty(existingInstall) &&
                        MessageBox.Show(prompt,
                            "Existing installation detected",
                            MessageBoxButtons.YesNo) ==
                        DialogResult.Yes)
                    {
                        Settings.GamePath = existingInstall;
                    }
                    else
                    {
                        Settings.GamePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Phoenix Games");
                    }

                    RegistryHelper.WriteBattlefield2Installation();
                }

                Task.Factory.StartNew(() => _updater.Start());
            }
        }

        private void OnButton(bool obj)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (Directory.Exists(Path.Combine(Settings.BF2GamePath, "mods")))
                {
                    var directories = Directory.GetDirectories(Path.Combine(Settings.BF2GamePath, "mods")).Select(x => x.Replace(Path.Combine(Settings.BF2GamePath, "mods") + "\\", "")).ToArray();
                    comboBox2.Items.Clear();
                    comboBox2.Items.AddRange(directories);
                }
                comboBox2.SelectedIndex = comboBox2.Items.IndexOf(Settings.Mod);

                button1.Enabled = obj;
                button3.Enabled = obj;
            });
        }

        private void OnProgress(double arg1, double arg2)
        {
            progressBar1.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = (int)arg1;
                progressBar1.Value = (int)arg2;
            });
        }

        private void OnNotify(string obj)
        {
            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = "Status: " + obj;
            });
        }

        private void AccountInfoChanged()
        {
            this.Invoke(new Action(() =>
            {
                button1.Enabled = true;
                button2.Visible = false;
                label2.Text = "Status: Logged In";
                label3.Text = "Username: " + AccountInfo.Username;
                Task.Factory.StartNew(() => _updater.Start());
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var url = AccountInfo.GetLoginUrl();
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = url,
            };
            Process.Start(psi);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (!AccountInfo.HasLoggedInUser())
            {
                MessageBox.Show("Not logged in!");
                return;
            }

            RegistryHelper.DisableBF2HubAutoPatching();

            OnNotify("Preflight check...");
            if (!File.Exists(Path.Combine(Settings.BF2GamePath, "dinput8.dll")))
            {
                // download it, TODO
            }
            using (var fs = new FileStream(Path.Combine(Settings.BF2GamePath, "BF2.exe"), FileMode.Open))
            {
                fs.Position = 0x5627E0; // position of the bf2hub patch
                fs.WriteByte(0x57); // W
                fs.WriteByte(0x53); // S
                fs.WriteByte(0x32); // 2
                fs.WriteByte(0x5F); // _
                fs.WriteByte(0x33); // 3
                fs.WriteByte(0x32); // 2
                fs.WriteByte(0x2E); // .
                fs.WriteByte(0x64); // d
                fs.WriteByte(0x6C); // l
                fs.WriteByte(0x6C); // l
            }

            OnNotify("Fetching key...");
            var cdkey = await AccountInfo.GetCdKeyAsync();
            OnNotify("Launching Game!");

            // Default/vanilla arguments
            var arguments = new List<string>
            {
                $"+modPath \"mods/{Settings.Mod}\"",
                $"+playerName \"{AccountInfo.Username}\"",
                "+playerPassword \"testingitnotimportant\""
            };
            if (!Settings.Fullscreen)
            {
                // Default is 1, only add set to 0
                arguments.Add("+fullscreen 0");
            }
            if (Settings.Restart)
            {
                // This is really a boolean flag, so we cannot set it to 0 (game still would not show intro videos)
                arguments.Add("+restart 1");
            }
            
            // Custom arguments
            arguments.Add($"/gscdk x9392{cdkey}");
            arguments.Add($"/language {Settings.Language}");

            var psi = new ProcessStartInfo(Path.Combine(Settings.BF2GamePath, "BF2.exe"))
            {
                WorkingDirectory = Settings.BF2GamePath,
                Arguments = string.Join(" ", arguments),
            };
            Process.Start(psi);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Fullscreen = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Restart = checkBox2.Checked;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Language = comboBox1.SelectedItem.ToString();
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            Settings.Language = comboBox1.SelectedItem.ToString();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Mod = comboBox2.SelectedItem.ToString();
        }

        private void comboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            Settings.Mod = comboBox2.SelectedItem.ToString();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            AutoUpdater.Mandatory = true;
            AutoUpdater.RunUpdateAsAdmin = PlatformHelper.NeedAdmin();
            AutoUpdater.LetUserSelectRemindLater = false;
            AutoUpdater.TopMost = true;
            //AutoUpdater.ReportErrors = true;
            Task.Factory.StartNew(() => AutoUpdater.Start("https://cdn.phoenixnetwork.net/updater/b2bf/client-launcher.xml"));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                File.Delete(Path.Combine(Settings.BF2GamePath, "version.txt"));
            }
            catch (Exception ex)
            {

            }

            var result = MessageBox.Show(
                "Yes: Check all files, permanently overwrite any modifications.\nNo: Check only B2BF-specific files, leave all other files untouched.",
                "Validate all game files?",
                MessageBoxButtons.YesNoCancel
            );
            if (result == DialogResult.Cancel)
            {
                return;
            }

            _updater.OverWriteExisting = result == DialogResult.Yes;

            button1.Enabled = false;
            button3.Enabled = false;
            Task.Factory.StartNew(() => _updater.Start());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!AccountInfo.HasLoggedInUser())
            {
                MessageBox.Show("Not logged in!");
                return;
            }

            AccountInfo.AccessToken = null;
            AccountInfo.Username = null;
            AccountInfo.UId = null;
            Settings.RememberMeContainer = "";

            this.Invoke(new Action(() =>
            {
                button1.Enabled = false;
                button2.Visible = true;
                button4.Visible = false;
                label2.Text = "Not Logged In";
                label3.Text = "Username: " + AccountInfo.Username;
            }));
        }
    }
}
