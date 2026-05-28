using BD.Standard.CRJH.ProgramParse.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Linq;

namespace BD.Standard.CRJH.ProgramParse
{
    public class access_token
    {
        public string gettoken()
        {
            Hashtable ht = new Hashtable();

            ht.Add("charset", "UTF-8");
            ht.Add("code", ConfigurationManager.AppSettings["code"]);
            ht.Add("timestamp", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            ht.Add("businessId", "18");
            ht.Add("developerId", ConfigurationManager.AppSettings["developerId"]);
            ht.Add("grantType", "authorization_code");
            ht.Add("sign", IntegrationUtils.CreateSign(ht));

            Dictionary<string, string> formData = ht.Cast<DictionaryEntry>()
                .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value);


            var posturlencodedAsync = new HttpUtils().PosturlencodedAsync(
                "https://api-open-cater.meituan.com/oauth/token", formData);


            return posturlencodedAsync;

            //{"code":0,"data":{"accessToken":"V2-53d968b5badb4f2eafc45be5fe2817acc2d75a29765aa0ae1f47897a3b102add3e60809bfbee7a650ceadb8627aa8dbb607646213a317fe94c991d35d3ce84fd","expireIn":2592000,"opBizCode":"143432990","refreshToken":"7a1633abb20728a55bb801e387fc0d915e05f0af119f5a8c8679e7d4bb6dc3081267867bf78316cc6e834dd43b5079fdb997196a0837fe02b99d0019134a6e29","scope":"all"},"traceId":"-1938940837616781121"}
        }
    }
}