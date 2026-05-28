using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using Newtonsoft.Json.Linq;

namespace BD.Standard.CRJH.ProgramParse.Forms
{
    public class Supplier
    {
        private static readonly string Logpath = @"Supplier\" + DateTime.Now.ToString("yyyyMM");

        public void PostSupplier(DataSet orgIdsSet)
        {
            Logger logger = new Logger(ConfigurationManager.AppSettings["log"] + Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            try
            {
               
                if (orgIdsSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow orgIds in orgIdsSet.Tables[0].Rows)
                    {
                        string orgId = orgIds["orgId"].ToString();
                        if (!orgId.Equals("3082269"))
                        {
                            continue;
                        }
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
                                { "status", "1" },
                                { "page", page },
                            };
                            string json = Requestbody.ToString();

                            var formData = IntegrationUtils.getFormData(json);

                            string posturlencoded = orgType == 1
                                ? new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/partner/api/v1/chain/supplier/query",
                                    formData)
                                : new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/partner/api/v1/poi/supplier/query",
                                    formData);

                            var result = IntegrationUtils.getPage(posturlencoded, pageSize, out pageNos);


                            if (result["code"].ToString().Equals("OP_SUCCESS"))
                            {
                                
                                MidTableData.InsertTable("Supplier", "items", result["data"].ToString());
                            }
                        }
                    }
                }
                logger.WriteLog("供应商信息同步完成！");
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