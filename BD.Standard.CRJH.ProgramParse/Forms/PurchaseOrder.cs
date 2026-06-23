using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using Newtonsoft.Json.Linq;

namespace BD.Standard.CRJH.ProgramParse.Forms
{
    internal class PurchaseOrder
    {
        private static readonly string Logpath = @"Pur\" + DateTime.Now.ToString("yyyyMM");

        public void  PostPurchaseOrder(DataSet orgIdsSet, long bizTimeBegin, long bizTimeEnd)
        {
            Logger logger = new Logger(ConfigurationManager.AppSettings["log"] + Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            try
            {
                if (orgIdsSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow orgIds in orgIdsSet.Tables[0].Rows)
                    {
                        string orgId = orgIds["orgId"].ToString();
                        int orgType = Convert.ToInt32(orgIds["orgType"].ToString());

                        int pageNos = 2;
                        for (int pageNo = 1; pageNo <= pageNos; pageNo++)
                        {
                            long pageSize = 200;
                            JObject page = new JObject()
                            {
                                { "pageNo", pageNo },
                                { "pageSize", pageSize },
                            };
                            JObject Requestbody = new JObject()
                            {
                                { "orgId", orgId },
                                { "orderStartTime", bizTimeBegin },
                                { "orderEndTime", bizTimeEnd },
                                { "orderStatusList", new JArray() { 60 } },
                                { "page", page },
                            };
                            string json = Requestbody.ToString();

                            var formData = IntegrationUtils.getFormData(json);

                            string posturlencoded = orgType == 4
                                ? new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/demand/api/v1/chain/purchaseOrderItem/query",
                                    formData)
                                : new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/demand/api/v1/poi/purchaseOrderItem/query",
                                    formData);

                            var result = IntegrationUtils.getPage(posturlencoded, pageSize, out pageNos);

                            // ReSharper disable once PossibleNullReferenceException
                            if (result["code"].ToString().Equals("OP_SUCCESS"))
                            {
                                // ReSharper disable once PossibleNullReferenceException
                                MidTableData.InsertTable("PurchaseOrder", "items", result["data"].ToString());
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                //Logger logger = new Logger(ConfigurationManager.AppSettings["log"] + Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                logger.WriteLog("数据出现异常,错误信息：" + e.Message);
                logger.WriteLog("             堆栈信息：" + e.StackTrace);
                Console.WriteLine(e.Message);
            }
           
        }
    }
}