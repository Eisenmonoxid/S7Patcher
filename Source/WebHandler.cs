using S7Patcher.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace S7Patcher.Source
{
    public sealed class WebHandler
    {
        private WebHandler() {}
        public void Dispose() {GlobalClient.Dispose();}
        public static WebHandler Instance {get;} = new();
        private readonly Uri DefinitionURI = new(Resources.DefinitionLink);
        private readonly HttpClient GlobalClient = new()
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
                Helpers.Instance.ConsoleWriteWrapper(ConsoleColorType.ERROR, ex.Message);
                Helpers.Instance.ConsoleWriteWrapper(ConsoleColorType.ERROR, "Download failed! Falling back to embedded file.\n");
                return null;
            }

            Watch.Stop();
            Helpers.Instance.ConsoleWriteWrapper(ConsoleColorType.INFO, $"Download Finished. Downloaded {Memory.Length / (float)1024} KB " +
                $"in {Watch.Elapsed.TotalSeconds} seconds.\n");
            return Memory;
        }
    }
}
