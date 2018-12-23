using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ListeDeMotDumper
{
    public static class Program
    {
        private const string baseUrl = "https://www.listesdemots.net/";
        private const int maxPage = 898;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Début de l'aspiration du site www.listesdemots.net");

            Uri baseUri = new Uri(baseUrl);
            HttpClient httpClient = new HttpClient();

            List<string> allWords = new List<string>();

            for (int page = 1; page <= maxPage; page++)
            {
                Console.WriteLine($"Aspiration de la page {page}");

                Uri url = new Uri(baseUri, GetRelativeUrl(page));
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

                HttpResponseMessage response = await httpClient.SendAsync(httpRequest);

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (Stream decompressed = new GZipStream(stream, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(decompressed))
                {
                    string htmlContent = reader.ReadToEnd();

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    HtmlNode node = doc.DocumentNode.SelectSingleNode("//span[@class='mot']").FirstChild;
                    string[] words = node.InnerText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    allWords.AddRange(words);
                }

                Console.WriteLine($"Page {page} aspirée");
            }

            File.WriteAllLines("result.txt", allWords);

            httpClient.Dispose();
        }

        private static string GetRelativeUrl(int page)
        {
            if (page == 1)
                return "touslesmots.htm";
            else
                return $"touslesmotspage{page}.htm";
        }
    }
}
