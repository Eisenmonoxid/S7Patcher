using S7Patcher.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace S7Patcher.Source
{
    public sealed class WebHandler
    {
        private WebHandler() {}
        public void Dispose() {GlobalClient.Dispose();}
        public static WebHandler Instance {get;} = new();
        private static readonly Uri DefinitionURI = new(Resources.DefinitionLink);
        private static readonly HttpClient GlobalClient = new()
        {
            Timeout = TimeSpan.FromMilliseconds(8000),
            DefaultRequestHeaders = {{"User-Agent", "Other"}},
        };

        public async Task<MemoryStream> DownloadDefinitionFile()
        {
            MemoryStream Memory = new();
            Stopwatch Watch = Stopwatch.StartNew();

            try
            {
                var Response = await GlobalClient.GetAsync(DefinitionURI, HttpCompletionOption.ResponseHeadersRead);
                Response.EnsureSuccessStatusCode();
                using Stream Stream = await Response.Content.ReadAsStreamAsync();
                await Stream.CopyToAsync(Memory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("\n[INFO] Download failed! Falling back to embedded file.");
                return null;
            }

            Watch.Stop();
            Console.WriteLine($"[INFO] Download Finished. Downloaded {Memory.Length / (float)1024} KB in {Watch.Elapsed.TotalSeconds} seconds.");
            return Memory;
        }
    }
}
