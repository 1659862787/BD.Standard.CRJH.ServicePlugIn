using Newtonsoft.Json.Linq;
using System.Configuration;
using System;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace BD.Standard.CRJH.ProgramParse
{
    public class UpdateToken
    {

        private static readonly string Logpath = @"UpdateToken\" + DateTime.Now.ToString("yyyyMM");


        public void Update()
        {
            Logger logger = new Logger(ConfigurationManager.AppSettings["log"] + Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            try
            {
                // 读取现有值、获取时间差
                //string time = ConfigurationManager.AppSettings["JST.time"];
                //TimeSpan timeSpan = DateTime.Now.Subtract(Convert.ToDateTime(time));
                //if (timeSpan.Days >= 29)
                //{
                    //获取刷新token
                    Hashtable ht = new Hashtable();

                    ht.Add("charset", "UTF-8");
                    ht.Add("timestamp", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                    ht.Add("businessId", "18");
                    ht.Add("developerId", ConfigurationManager.AppSettings["developerId"]);
                    ht.Add("grantType", "refresh_token");
                    ht.Add("scope", "all");
                    ht.Add("refreshToken", ConfigurationManager.AppSettings["refreshToken"]);
                    ht.Add("sign", IntegrationUtils.CreateSign(ht));

                    Dictionary<string, string> formData = ht.Cast<DictionaryEntry>()
                        .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value);

                    var result = new HttpUtils().PosturlencodedAsync(
                        "https://api-open-cater.meituan.com/oauth/refresh", formData);
                    var jObject = JObject.Parse(result);
                    logger.WriteLog(jObject.ToString());
                    // 修改配置
                    UpdateAppConfig("appAuthToken", (string)jObject["data"]["accessToken"]);
                    UpdateAppConfig("refreshToken", (string)jObject["data"]["refreshToken"]);
                    UpdateAppConfig("tokenUpdateTime", DateTime.Now.ToString("yyyy-MM-dd"));

                    ConfigurationManager.RefreshSection("appSettings");

                //}
            }
            catch (Exception e)
            {
                
                logger.WriteLog("数据出现异常,错误信息：" + e.Message);
                logger.WriteLog("             堆栈信息：" + e.StackTrace);
                Console.WriteLine(e.Message);
            }
            
        }

        public void UpdateAppConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }
            config.Save(ConfigurationSaveMode.Modified);
        }


    }
}