using System;
using System.Configuration;
using System.Data;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BD.Standard.CRJH.ProgramParse.Forms
{

    internal class Delivery
    {

        private static readonly string Logpath = @"PostDelivery\" + DateTime.Now.ToString("yyyyMM");
        private static readonly DBConnection Con = new DBConnection();
        public void PostDelivery(DataSet orgIdsSet, long bizTimeBegin, long bizTimeEnd)
        {
            try
            {

                if (orgIdsSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow orgIds in orgIdsSet.Tables[0].Rows)
                    {
                        string orgId = orgIds["belongOrg_orgId"].ToString();
                        string sourceSn = orgIds["sourceSn"].ToString();
                        string itemSn = orgIds["itemSn"].ToString();
                            JObject Requestbody = new JObject()
                            {
                                { "orgId",orgId  },
                                { "itemSns", new JArray() { sourceSn }},
                            };
                            string json = Requestbody.ToString();
                            var formData = IntegrationUtils.getFormData(json);

                            string posturlencoded = new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/distribution/api/v1/poi/acceptanceOrder/list",
                                    formData);

                            var result = JObject.Parse(posturlencoded);

                            if (result["code"].ToString().Equals("OP_SUCCESS"))
                            {
                                string v = result["data"]["items"][0]["details"][0]["deliveryWarehouseInfo"]["code"].ToString();
                                Con.getDataSet("update CRJH_InStock set deliveryWarehouseCode='" + v + "' where itemSn='" + itemSn + "'");
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


        public void PostUnDelivery(DataSet orgIdsSet, long bizTimeBegin, long bizTimeEnd)
        {
            try
            {

                if (orgIdsSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow orgIds in orgIdsSet.Tables[0].Rows)
                    {

                        string sourceSn = orgIds["sourceSn"].ToString();
                        string itemSn = orgIds["itemSn"].ToString();
                        JObject Requestbody = new JObject()
                            {
                                { "orgId","5394763"  },
                                { "orderSns", new JArray() {sourceSn }},
                            };
                        string json = Requestbody.ToString();
                        var formData = IntegrationUtils.getFormData(json);

                        string posturlencoded = new HttpUtils().PosturlencodedAsync(
                                "https://api-open-cater.meituan.com/rms/scmplus/distribution/api/v1/chain/refundOrder/list",
                                formData);

                        var result = JObject.Parse(posturlencoded);

                        if (result["code"].ToString().Equals("OP_SUCCESS"))
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            string v = result["data"]["items"][0]["details"][0]["warehouse"]["code"].ToString();
                            Con.getDataSet("update CRJH_OutStock set deliveryWarehouseCode='" + v + "' where itemSn='" + itemSn + "'");
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


        public void PostdeliveryOrder(DataSet orgIdsSet, long bizTimeBegin, long bizTimeEnd)
        {
            try
            {

                if (orgIdsSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow orgIds in orgIdsSet.Tables[0].Rows)
                    {

                        string sourceSn = orgIds["sourceSn"].ToString();
                        string itemSn = orgIds["itemSn"].ToString();
                        JObject Requestbody = new JObject()
                            {
                                { "orgId","5394763"  },
                                { "itemSns", new JArray() {sourceSn }},
                            };
                        string json = Requestbody.ToString();
                        var formData = IntegrationUtils.getFormData(json);

                        string posturlencoded = new HttpUtils().PosturlencodedAsync(
                                "https://api-open-cater.meituan.com/rms/scmplus/distribution/api/v1/chain/deliveryOrder/list",
                                formData);
                        var result = JObject.Parse(posturlencoded);

                        if (result["code"].ToString().Equals("OP_SUCCESS"))
                        {
                            JArray details = (JArray)result["data"]["items"][0]["details"];
                            StringBuilder sb = new StringBuilder();
                            foreach (var detail in details)
                            {
                                JObject model = (JObject)detail;
                                string id = model["id"].ToString();
                                string actuallyPayPrice = model["actuallyPayPrice"].ToString();
                                string tax = model["tax"].ToString();
                                sb.AppendLine($"update CRJH_OutStockentry set actuallyPayPrice='{actuallyPayPrice}',tax2='{tax}' where itemSn='{itemSn}' and sourceDetailId='{id}';");
                            }

                            Con.getDataSet(sb.ToString());
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