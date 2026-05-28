using BD.Standard.CRJH.ProgramParse.Utils;
using Kingdee.BOS.Log;
using Kingdee.CDP.WebApi.SDK;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;

namespace BD.Standard.CRJH.ProgramUploadERP.Forms
{
    internal class Stockout
    {
        public Stockout()
        {
        }

        public void StockoutSC(string sql)
        {
            var clienter = new K3CloudApi();
            DBConnection Con = new DBConnection();
            DataSet orgIdsSet = Con.getDataSet(sql);
            if (orgIdsSet.Tables[0].Rows.Count == 0) return;
            int count = 0;
            foreach (DataRow dataH in orgIdsSet.Tables[0].Rows)
            {
                
                DataSet dSbt = Con.getDataSet(string.Format("EXEC CRJH_Stock '{0}','{1}'", dataH.ItemArray[0].ToString(), dataH.ItemArray[1].ToString()));
                foreach (DataRow item in dSbt.Tables[0].Rows)
                {
                    string formid = dataH.ItemArray[1].ToString().Equals("STK_MisDeliveryZC") ? "STK_MisDelivery" : dataH.ItemArray[1].ToString();
                    string respJson = clienter.Save(formid, item.ItemArray[0].ToString());

                    //超时重新触发登录
                    if (JObject.Parse(respJson)["Result"]["ResponseStatus"]["MsgCode"].ToString().Equals("1"))
                    {
                        respJson = clienter.Save(formid, item.ItemArray[0].ToString());
                    }

                    int status = JObject.Parse(respJson)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True") ? 1 : 0;
                    string billno = item.ItemArray[1].ToString();
                    if (billno.Contains("-ZC"))
                    {
                        billno= billno.Replace("-ZC", "");
                    }
                    Con.getDataSet($"update  CRJH_OutStock set status={status},reqJson='{item.ItemArray[0].ToString()}',respJson='{respJson}' where ID='{billno}'");
                }

            }
        }
    }
}
