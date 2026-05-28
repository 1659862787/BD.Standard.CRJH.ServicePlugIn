using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BD.Standard.CRJH.ProgramParse.Utils
{
    public static class IntegrationUtils
    {

        /// <summary>
        /// 生成公共参数请求
        /// </summary>
        /// <param name="json"></param>
        /// <param name="signService"></param>
        /// <returns></returns>
        public static Dictionary<string, string> getFormData(string json)
        {
            Hashtable ht = new Hashtable();
            ht.Add("biz", json);
            ht.Add("charset", "UTF-8");
            ht.Add("appAuthToken", ConfigurationManager.AppSettings["appAuthToken"]);
            ht.Add("timestamp", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            ht.Add("version", "2");
            ht.Add("businessId", "18");
            ht.Add("developerId", ConfigurationManager.AppSettings["developerId"]);
            ht.Add("sign", CreateSign(ht));

            Dictionary<string, string> formData = ht.Cast<DictionaryEntry>()
                .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value);
            return formData;
        }


        /// <summary>
        /// 生成sign
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string CreateSign(Hashtable param)
        {
            String signKey = ConfigurationManager.AppSettings["signKey"];
            ArrayList keys = new ArrayList(param.Keys);
            keys.Sort(); //按字母顺序进行排序
            string resultStr = "";
            foreach (string key in keys)
            {
                string value = param[key].ToString();
                if (key != "sign" && value != null && value != "")
                {
                    resultStr = resultStr + key + value;
                }
            }
            resultStr = signKey + resultStr;
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] rstRes = sha1.ComputeHash(Encoding.UTF8.GetBytes(resultStr));
            string hex = BitConverter.ToString(rstRes, 0).Replace("-", string.Empty).ToLower();

            return hex;

        }


        /// <summary>
        /// 分页计算
        /// </summary>
        /// <param name="posturlencoded"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNos"></param>
        /// <returns></returns>
        public static JObject getPage(string posturlencoded, long pageSize, out int pageNos)
        {
            //计算分页
            JObject result = JObject.Parse(posturlencoded);
            long totalCount = Convert.ToInt64(result["data"]["page"]["totalCount"].ToString());
            pageNos = Convert.ToInt32(totalCount / pageSize + (totalCount % pageSize > 0 ? 1 : 0));
            return result;
        }
    }
}