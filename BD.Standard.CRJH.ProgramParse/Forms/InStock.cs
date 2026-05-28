using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using Newtonsoft.Json.Linq;

namespace BD.Standard.CRJH.ProgramParse.Forms
{
    internal class InStock
    {
        private static readonly string Logpath = @"InStock\" + DateTime.Now.ToString("yyyyMM");

        public void PostInStock(DataSet orgIdsSet,long bizTimeBegin, long bizTimeEnd)
        {
            try
            {
                if (orgIdsSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow orgIds in orgIdsSet.Tables[0].Rows)
                    {
                        string orgId = orgIds["orgId"].ToString();
                        int orgType = Convert.ToInt32(orgIds["orgType"].ToString());
                        string inStockType = orgIds["inStockType"].ToString();

                        int pageNos = 2;
                        for (int pageNo = 1; pageNo <= pageNos; pageNo++)
                        {
                            long pageSize = 200;
                            JObject page = new JObject()
                            {
                                { "pageNo", pageNo },
                                { "pageSize", pageSize },
                            };
                            if (inStockType == null) continue;

                        JObject Requestbody = new JObject()
                        {
                            { "orgId", orgId },
                            { "bizTimeBegin", bizTimeBegin },//1736928754000
                            { "bizTimeEnd", bizTimeEnd },//1747296787976
                            { "status", 2 },
                            {
                                "types", new JArray(Array.ConvertAll(inStockType.Split(','), int.Parse))
                            },
                            {"page",page}
                        };

                            SignService signService = new SignService();
                            string json = Requestbody.ToString();

                            var formData = IntegrationUtils.getFormData(json);

                            string posturlencoded = orgType == 4
                                ? new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/inventory/api/v1/chain/stockInItem/query",
                                    formData)
                                : new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/inventory/api/v1/poi/stockInItem/query",
                                    formData);


                            var result = IntegrationUtils.getPage(posturlencoded, pageSize, out pageNos);


                            // ReSharper disable once PossibleNullReferenceException
                            if (result["code"].ToString().Equals("OP_SUCCESS"))
                            {
                                // ReSharper disable once PossibleNullReferenceException
                                MidTableData.InsertTable("InStock", "items", result["data"].ToString());
                            }
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