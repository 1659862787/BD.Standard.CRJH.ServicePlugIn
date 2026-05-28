using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using Newtonsoft.Json.Linq;

namespace BD.Standard.CRJH.ProgramParse.Forms
{

    internal class OutStock1
    {

        private static readonly string Logpath = @"OutStock\" + DateTime.Now.ToString("yyyyMM");

        public void PostOutStock(DataSet orgIdsSet, long bizTimeBegin, long bizTimeEnd)
        {
            try
            {
                if (orgIdsSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow orgIds in orgIdsSet.Tables[0].Rows)
                    {
                        string orgId = orgIds["orgId"].ToString();
                        int orgType = Convert.ToInt32(orgIds["orgType"].ToString());
                        if (orgType == 4)
                        {
                            continue;
                        }

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
                                { "startTime", bizTimeBegin },
                                { "endTime", bizTimeEnd },
                                { "status", 3 },
                                {"page",page}
                            };

                            string json = Requestbody.ToString();

                            var formData = IntegrationUtils.getFormData(json);

                            string posturlencoded = new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/inventory/api/v1/poi/foodConsumption/query",
                                    formData);

                            var result = IntegrationUtils.getPage(posturlencoded, pageSize, out pageNos);

                            // ReSharper disable once PossibleNullReferenceException
                            if (result["code"].ToString().Equals("OP_SUCCESS"))
                            {
                                // ReSharper disable once PossibleNullReferenceException
                                MidTableData.InsertTable("OutStock", "items", result["data"].ToString());
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