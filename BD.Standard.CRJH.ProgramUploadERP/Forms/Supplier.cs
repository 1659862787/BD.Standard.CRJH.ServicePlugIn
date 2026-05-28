using BD.Standard.CRJH.ProgramParse.Utils;
using Kingdee.CDP.WebApi.SDK;
using Newtonsoft.Json.Linq;
using System.Data;

namespace BD.Standard.CRJH.ProgramUploadERP
{
    internal class Supplier
    {
        public void PostSupplier(string sql)
        {
            var clienter = new K3CloudApi();
            DBConnection Con = new DBConnection();
            DataSet orgIdsSet = Con.getDataSet(sql);
            if (orgIdsSet.Tables[0].Rows.Count == 0) return;
            foreach (DataRow dataH in orgIdsSet.Tables[0].Rows)
            {
                string reqJson = BuildJson.ERPSaveJson(orgIdsSet, dataH, "FFinanceInfo");
                
                string respJson = clienter.Save("BD_Supplier", reqJson);
                int status = JObject.Parse(respJson)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True") ? 1 : 0;
                Con.getDataSet($"update  CRJH_Supplier set status={status},reqJson='{reqJson}',respJson='{respJson}' where ID='{dataH["id"]}'");

            }
        }
    }
}