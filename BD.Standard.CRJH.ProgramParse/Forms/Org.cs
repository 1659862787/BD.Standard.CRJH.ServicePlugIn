using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using Newtonsoft.Json.Linq;

namespace BD.Standard.CRJH.ProgramParse.Forms
{
    internal class Org
    {
        private static readonly string Logpath = @"Org\" + DateTime.Now.ToString("yyyyMM");

        public void PostOrg(DBConnection Con)
        {
            Logger logger = new Logger(ConfigurationManager.AppSettings["log"] + Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            try
            {
                //非必填
                JObject Requestbody = new JObject()
                {
                    {"devId",ConfigurationManager.AppSettings["developerId"]},
                    {"bizId","18"}
                };
                string json = Requestbody.ToString();

                var formData = IntegrationUtils.getFormData(json);


                string posturlencoded =
                    new HttpUtils().PosturlencodedAsync("https://api-open-cater.meituan.com/rms/base/v1/auth/resources/get",
                        formData);

                JObject result = JObject.Parse(posturlencoded);

                // ReSharper disable once PossibleNullReferenceException
                if (result["code"].ToString().Equals("OP_SUCCESS"))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Con.getDataSet("delete from CRJH_Org");
                    MidTableData.InsertTable("Org", "resources", result["data"].ToString());
                }

                Con.getDataSet("exec CRJH_OrgUpdate");
                
                logger.WriteLog(result["data"].ToString());
                logger.WriteLog("机构信息同步完成！");
            }
            catch (Exception e)
            {
               
                logger.WriteLog("数据出现异常,错误信息：" + e.Message);
                logger.WriteLog("             堆栈信息：" + e.StackTrace);
                Console.WriteLine(e.Message);
            }
        }
           
    }
}