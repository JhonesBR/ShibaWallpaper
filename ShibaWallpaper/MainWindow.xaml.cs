using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;

namespace TrayAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Forms.NotifyIcon _notifyIcon;

        public MainWindow()
        {
            _notifyIcon = new Forms.NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon("shiba.ico");
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Right click to show options";
            
            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Change Wallpaper", System.Drawing.Image.FromFile("exchange.ico"), SetWallpaper);
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", System.Drawing.Image.FromFile("logout.ico"), Exit);

            _notifyIcon.DoubleClick += (s, args) =>
            {
                MessageBox.Show("© João Vitor Oliveira de Melo. All rights reserved.", "Copyright and credits");
            };
            
            InitializeComponent();
            App.Hide();
            App.WindowState = WindowState.Minimized;
            InitOnWindowsStart();
        }

        private void SetWallpaper(object? sender, EventArgs e)
        {
            string url = GetImageUrlFromShibeApi(1);
            if (url != "")
            {
                Wallpaper.Set(new System.Uri(url), Wallpaper.Style.Stretched);
            }
        }

        private string GetImageUrlFromShibeApi(int count)
        {
            try
            {
                string url = $"http://shibe.online/api/shibes?count={count}&urls=true&httpsUrls=true";
                System.Net.WebRequest? request = System.Net.WebRequest.Create(url);
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new System.IO.StreamReader(stream);
                string fullLink = reader.ReadToEnd();
                return fullLink.Substring(2, fullLink.Length - 4);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting image from Shibe API: " + ex.Message);
                return "";
            }
            
        }

        private void Exit(object? sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            Application.Current.Shutdown();
        }
        
        public sealed class Wallpaper
        {
            Wallpaper() { }

            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

            public enum Style : int
            {
                Tiled,
                Centered,
                Stretched
            }

            public static void Set(Uri uri, Style style)
            {
                System.IO.Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

                System.Drawing.Image img = System.Drawing.Image.FromStream(s);
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wallpaper.bmp");
                img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (style == Style.Stretched)
                {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Centered)
                {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Tiled)
                {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }

                SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0,
                    tempPath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
        }

        public void InitOnWindowsStart()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("ShibaWallpaper", System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "ShibaWallpaper.exe"));
        }

    private void Window_Closed(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
}
