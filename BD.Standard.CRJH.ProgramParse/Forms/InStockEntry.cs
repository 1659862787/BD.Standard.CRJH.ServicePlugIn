using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using Newtonsoft.Json.Linq;

namespace BD.Standard.CRJH.ProgramParse.Forms
{
    internal class InStockEntry
    {
        private static readonly string Logpath = @"InStockEntry\" + DateTime.Now.ToString("yyyyMM");

        public void  PostInStockEntry(DataSet orgIdsSet)
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
                            { "orgId", orgId },
                            { "itemSns", new JArray() { itemSn } },
                        };
                        string json = Requestbody.ToString();

                        var formData = IntegrationUtils.getFormData(json);

                        string posturlencoded = orgType.Equals("4")
                            ? new HttpUtils().PosturlencodedAsync(
                                "https://api-open-cater.meituan.com/rms/scmplus/inventory/api/v1/chain/stockIn/list", formData)
                            : new HttpUtils().PosturlencodedAsync(
                                "https://api-open-cater.meituan.com/rms/scmplus/inventory/api/v1/poi/stockIn/list", formData);


                        JObject result = JObject.Parse(posturlencoded);

                        // ReSharper disable once PossibleNullReferenceException
                        if (result["code"].ToString().Equals("OP_SUCCESS"))
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            MidTableData.InsertTableEntry("InStockEntry", "items", result["data"].ToString());
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
            }
           

        }
    }
}