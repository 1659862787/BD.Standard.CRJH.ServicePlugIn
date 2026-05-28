using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using System.Data;

namespace BD.Standard.CRJH.ProgramUploadERP
{
    public class BuildJson
    {
        public static string ERPSaveJson(DataSet datas, DataRow dataH, string FentryKey)
        {
            JArray dataList = new JArray();
            JObject data = new JObject();
            JObject model = new JObject();

            //批量传输,表头明细的关联字段
            string id = string.Empty;

            data = new JObject();
            data["IsAutoSubmitAndAudit"] = "true";
            foreach (DataColumn columns in dataH.Table.Columns)
            {
                string column = columns.ColumnName.ToString();
                if (column.EqualsIgnoreCase("id"))
                {
                    id = dataH["id"].ToString();
                    continue;
                }
                if (column.Contains("_0"))
                {
                    JObject fnumber = new JObject();
                    fnumber.Add(new JProperty("Fnumber", dataH[column].ToString()));
                    model[column.Substring(0, column.Length - 2)] = fnumber;
                }
                else
                {
                    model.Add(new JProperty(column, dataH[column].ToString()));
                }

            }
            if (datas.Tables.Count >= 2)
            {
                for(int i = 1; i < datas.Tables.Count;i++)
                {
                    JArray jsonEntry = new JArray();
                    foreach (DataRow dataE in datas.Tables[i].Rows)
                    {
                        if (!id.Equals(dataE["id"].ToString())) continue;
                        JObject jobE = new JObject();
                        foreach (DataColumn columns in dataE.Table.Columns)
                        {
                            string column = columns.ColumnName.ToString();
                            if (!column.Equals("id"))
                            {
                                if (column.Contains("_0"))
                                {
                                    JObject fnumber = new JObject();
                                    fnumber.Add(new JProperty("Fnumber", dataE[column].ToString()));
                                    jobE[column.Substring(0, column.Length - 2)] = fnumber;
                                }
                                else
                                {
                                    jobE.Add(new JProperty(column, dataE[column].ToString()));
                                }
                            }
                        }
                        if (jobE.Count > 0) jsonEntry.Add(jobE);
                        if (FentryKey.Equals("FFinanceInfo"))
                        {
                            model[FentryKey] = jobE;
                        }
                        else if (FentryKey.Equals("SubHeadEntity"))
                        {
                            data["IsAutoSubmitAndAudit"] = "false";
                            if (i==1)
                            {
                                model[FentryKey] = jobE;
                            }
                            else
                            {
                                model[FentryKey+(i-1)] = jobE;
                            }
                        }
                        else
                        {
                            model[FentryKey] = jsonEntry;
                        }
                    }
                }
            }
            data["model"] = model;
            dataList.Add(data);

            return data.ToString();

        }

    }
}
