using BD.Standard.CRJH.ProgramParse.Utils;
using Kingdee.CDP.WebApi.SDK;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;

namespace BD.Standard.CRJH.ProgramUploadERP.Forms
{
    internal class Stockin
    {
        public Stockin()
        {
        }

        public void StockinSC(string sql)
        {
            var clienter = new K3CloudApi();
            DBConnection Con = new DBConnection();
            DataSet orgIdsSet = Con.getDataSet(sql);
            if (orgIdsSet.Tables[1].Rows.Count == 0) return;
            int count = 0;
            foreach (DataRow dataH in orgIdsSet.Tables[1].Rows)
            {

                DataSet dSbt = Con.getDataSet(string.Format("EXEC CRJH_Stock '{0}','{1}'", dataH.ItemArray[0].ToString(), dataH.ItemArray[1].ToString()));

                
                foreach (DataRow item in dSbt.Tables[0].Rows)
                {
                    string respJson = clienter.Save(dataH.ItemArray[1].ToString(), item.ItemArray[0].ToString());

                    //超时重新触发登录
                    if (JObject.Parse(respJson)["Result"]["ResponseStatus"]["MsgCode"].ToString().Equals("1"))
                    {
                        respJson = clienter.Save(dataH.ItemArray[1].ToString(), item.ItemArray[0].ToString());
                    }

                    int status = JObject.Parse(respJson)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True") ? 1 : 0;
                    Con.getDataSet($"update  CRJH_InStock set status={status},reqJson='{item.ItemArray[0].ToString()}',respJson='{respJson}' where ID='{item.ItemArray[1].ToString()}'");
                }
            }
        }
    }
}
