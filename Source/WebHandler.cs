using System;
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

        private static readonly HttpClient GlobalClient = new()
        {
            Timeout = TimeSpan.FromMilliseconds(8000),
            DefaultRequestHeaders = {{"User-Agent", "Other"}},
        };

        private static readonly Uri DefinitionURI = new("https://github.com/Eisenmonoxid/S7Patcher/raw/refs/heads/main/Definitions/Definitions.bin");

        public async Task<MemoryStream> DownloadDefinitionsFile()
        {
            MemoryStream Memory = new();
            try
            {
                var Response = await GlobalClient.GetAsync(DefinitionURI, HttpCompletionOption.ResponseHeadersRead);
                Response.EnsureSuccessStatusCode();
                using Stream Stream = await Response.Content.ReadAsStreamAsync();
                await Stream.CopyToAsync(Memory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }

            Console.WriteLine("INFO: Download Finished!");
            return Memory;
        }
    }
}
