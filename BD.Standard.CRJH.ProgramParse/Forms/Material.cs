using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Utils;
using Newtonsoft.Json.Linq;

namespace BD.Standard.CRJH.ProgramParse.Forms
{
    public class Material
    {
        private static readonly string Logpath = @"Material\" + DateTime.Now.ToString("yyyyMM");

        public void PostMaterial(DataSet orgIdsSet)
        {
            Logger logger = new Logger(ConfigurationManager.AppSettings["log"] + Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            try
            {
               
                logger.WriteLog("物料信息同步开始！");
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

                        //循环分页for,pageNo++
                        int pageNos = 2;
                        for (int pageNo = 1; pageNo <= pageNos; pageNo++)
                        {
                            long pageSize = 2000;
                            JObject page = new JObject()
                            {
                                {"pageNo",pageNo},
                                {"pageSize",pageSize},
                            };
                            JObject Requestbody = new JObject()
                            {
                                {"orgId",orgId},
                                {"status","1"},
                                {"page",page},
                            };
                            string json = Requestbody.ToString();

                            var formData = IntegrationUtils.getFormData(json);


                            string posturlencoded = orgType == 1
                                ? new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/goods/api/v1/chain/goods/query", formData)
                                : new HttpUtils().PosturlencodedAsync(
                                    "https://api-open-cater.meituan.com/rms/scmplus/goods/api/v1/poi/goods/query", formData);

                            var result = IntegrationUtils.getPage(posturlencoded, pageSize, out pageNos);


                            if (result["code"].ToString().Equals("OP_SUCCESS"))
                            {
                                MidTableData.InsertTable("MATERIAL", "items", result["data"].ToString());
                            }
                        }
                    }

                }
                logger.WriteLog("物料信息同步完成！");
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
