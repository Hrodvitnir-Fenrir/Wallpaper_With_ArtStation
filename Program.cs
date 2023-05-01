using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;

namespace WallpaperWithArtStation
{
    class Program
    {
        static async Task<string> FetchRandomArtworkUrlAsync()
        {
            using HttpClient client = new HttpClient();
            string apiUrl = "https://www.artstation.com/random_project.json";
            string response = await client.GetStringAsync(apiUrl);

            JsonDocument jsonDoc = JsonDocument.Parse(response);
            string imageUrl = jsonDoc.RootElement.GetProperty("assets")[0].GetProperty("image_url").GetString();

            if (imageUrl == null)
            {
                imageUrl = "https://cdnb.artstation.com/p/assets/images/images/053/557/633/4k/kyle-enochs-week-09-assignment-01-image-01.jpg?1662489961";
            }
            return imageUrl;
        }

        static async Task<string> DownloadImageAsync(string imageUrl)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "wallpaper.jpg");

            using HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync(imageUrl);
            using FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);

            return tempFilePath;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static void SetWallpaper(string imagePath)
        {
            const int SPI_SETDESKWALLPAPER = 0x0014;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDCHANGE = 0x02;

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        static async Task RunMainLogic()
        {
            string imageUrl = await FetchRandomArtworkUrlAsync();
            string imagePath = await DownloadImageAsync(imageUrl);
            SetWallpaper(imagePath);
        }

        static async Task Main(string[] args)
        {
            var periodTimeSpan = TimeSpan.FromMinutes(1);

            var timer = new System.Threading.Timer(async (e) =>
            {
                await RunMainLogic();
                File.Delete(Path.Combine(Path.GetTempPath(), "wallpaper.jpg"));
            }, null, TimeSpan.Zero, periodTimeSpan);

            await Task.Delay(Timeout.Infinite);
        }
    }
}
