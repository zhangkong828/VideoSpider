using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoSpider.Infrastructure
{
    public class HtmlHelper
    {
        private static readonly HttpClient _httpClient;

        static HtmlHelper()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

        public static async Task<string> Get(string url)
        {
            var html = "";
            int tryCount = 3;
        GetHtml:
            bool isError = false;
            try
            {
                html = await _httpClient.GetStringAsync(url);
                isError = string.IsNullOrWhiteSpace(html);
            }
            catch (Exception ex)
            {
                isError = true;
                Logger.Warn("{0}请求失败：{1}", url, ex.Message);
            }
            if (isError)
            {
                if (tryCount > 0)
                {
                    tryCount--;
                    goto GetHtml;
                }
            }
            return html;
        }


        public static byte[] DownLoad(string url)
        {
            int tryCount = 3;
        GetImage:
            try
            {
                using (WebClient client = new WebClient())
                {
                    var result = client.DownloadData(url);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, url);
                if (tryCount > 0)
                {
                    tryCount--;
                    goto GetImage;
                }
                return null;
            }
        }
        
    }
}
