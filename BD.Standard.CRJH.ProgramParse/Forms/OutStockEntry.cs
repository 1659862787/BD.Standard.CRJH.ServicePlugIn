using BD.Standard.CRJH.ProgramParse.Utils;
using BD.Standard.FOM.Program.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using static System.Net.WebRequestMethods;

namespace BD.Standard.CRJH.ProgramParse.Forms
{
    internal class OutStockEntry
    {
        private static readonly string Logpath = @"Org\" + DateTime.Now.ToString("yyyyMM");

        public void PostOutStockEntry(DataSet orgIdsSet)
        {
            try
            {
                if (orgIdsSet.Tables[0].Rows.Count > 0)
                {

                    Dictionary<string, JArray> dics = new Dictionary<string, JArray>();

                    foreach (DataRow orgIds in orgIdsSet.Tables[0].Rows)
                    {
                        string orgId = orgIds["orgId"].ToString();
                        string orgType = orgIds["orgType"].ToString();
                        string itemSn = orgIds["itemSn"].ToString();
                        //itemSn = "ZHJG2506250001";
                        //if (dics.TryGetValue(orgId + "-" + orgType, out JArray value))
                        //{
                        //    value.Add(itemSn);
                        //}
                        //else
                        //{
                        //    dics.Add(orgId + "-" + orgType, new JArray() { itemSn });
                        //}
                        //}

                        //foreach (var dic in dics)
                        //{

                        JObject page = new JObject()
                        {
                            { "pageNo", "1" },
                            { "pageSize", "2000" },
                        };
                        JObject Requestbody = new JObject()
                        {
                            { "orgId",orgId  },
                            { "itemSns", new JArray() { itemSn }},
                        };
                        string json = Requestbody.ToString();
                        var formData = IntegrationUtils.getFormData(json);

                        string posturlencoded = orgType.Equals("4")
                            ? new HttpUtils().PosturlencodedAsync(
                                "https://api-open-cater.meituan.com/rms/scmplus/inventory/api/v1/chain/stockOut/list", formData)
                            
                            : new HttpUtils().PosturlencodedAsync(
                                "https://api-open-cater.meituan.com/rms/scmplus/inventory/api/v1/poi/stockOut/list", formData);


                        JObject result = JObject.Parse(posturlencoded);

                        // ReSharper disable once PossibleNullReferenceException
                        if (result["code"].ToString().Equals("OP_SUCCESS"))
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            MidTableData.InsertTableEntry("OutStockEntry", "items", result["data"].ToString());
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Logger logger = new Logger(ConfigurationManager.AppSettings["log"] + Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                logger.WriteLog("数据出现异常,错误信息：" + e.Message);
                logger.WriteLog("             堆栈信息：" + e.StackTrace);
                Console.WriteLine(e.Message);
                Console.ReadLine();


            }
            

        }
    }
}