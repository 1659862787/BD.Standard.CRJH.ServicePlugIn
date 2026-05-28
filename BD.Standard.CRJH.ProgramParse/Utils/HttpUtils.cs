using System;
using System.Collections.Generic;
using System.Net.Http;

namespace BD.Standard.CRJH.ProgramParse.Utils
{
    public  class HttpUtils
    {
        public string PosturlencodedAsync(string apiUrl,Dictionary<string,string> formData)
        {
            string responseString ="";
            using (var httpClient = new HttpClient())
            {


                // 将字典转换为 FormUrlEncodedContent
                var content = new FormUrlEncodedContent(formData);

                try
                {
                    HttpResponseMessage response = httpClient.PostAsync(apiUrl, content).Result;

                    // 确保响应成功
                    response.EnsureSuccessStatusCode();

                    // 同步读取响应内容
                    responseString = response.Content.ReadAsStringAsync().Result;
                    return responseString;
                }
                catch (AggregateException ex)
                {
                    // 处理异步操作抛出的异常
                    Console.WriteLine($"Error: {ex.InnerException?.Message ?? ex.Message}");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP Error: {ex.Message}");
                }
            }
            return responseString;
        }
    }
}
