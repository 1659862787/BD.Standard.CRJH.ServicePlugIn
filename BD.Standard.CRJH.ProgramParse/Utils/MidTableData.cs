using BD.Standard.FOM.Program.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace BD.Standard.CRJH.ProgramParse.Utils
{
    internal class MidTableData
    {
        private static readonly DBConnection Con = new DBConnection();
        public static void InsertTable(string formid,string type, string data)
        {
            //data = data.ToLower();
            try
            {
                #region 获取解析数据，创建插入语句

                string sql = $"exec CRJH_parseJson '{formid}'";
                DataSet dataSet = Con.getDataSet(sql);
                JObject json = JObject.Parse(data.Replace("'", "’"));

                #region 分页逻辑

                JObject page = (JObject)json["page"];

                #endregion 分页逻辑


                JArray records = (JArray)json[type];
                foreach (var record in records)
                {
                    JObject model = (JObject)record;
                    StringBuilder sb = new StringBuilder();
                    StringBuilder column = new StringBuilder();
                    StringBuilder value = new StringBuilder();
                    sb.Append(string.Format(@"/*dialect*/  insert into {0} ", "CRJH_" + formid));
                    value.Append("  values ");
                    value.Append("\r\n('" + Regex.Replace(model.ToString(), @"\s+", "") + "',");
                    DataRow dataH = dataSet.Tables[0].Rows[0];
                    column.Append("(model,");
                    string Id = "";
                    string belongOrg_orgId = "";
                    string lastModifyTime = "";
                    string updateset = "";
                    foreach (DataColumn columns in dataH.Table.Columns)
                    {
                        string filed = columns.ColumnName;
                        if (filed.Equals("Id")) Id = model[dataH[filed].ToString()].ToString();
                        if (filed.Contains("_"))
                        {
                            var strings = filed.Split('_');
                            if (model.Property(strings[0]) != null)
                            {
                                string str = strings.Length == 2
                                    ? model[strings[0]][strings[1]].ToString()
                                    : model[strings[0]][strings[1]][strings[2]].ToString();

                                column.Append("" + filed + ",");
                                value.Append("'" + str + "',");

                                if (filed.Equals("belongOrg_orgId"))
                                {
                                    belongOrg_orgId = model[strings[0]][strings[1]].ToString();
                                }
                               

                            }
                        }
                        else if (model.Property(dataH[filed].ToString()) != null)
                        {
                            column.Append("" + filed + ",");
                            value.Append("'" + model[dataH[filed].ToString()] + "',");
                        }

                    }

                    column.Remove(column.Length - 1, 1);
                    if (!string.IsNullOrWhiteSpace(column.ToString())) sb.Append(column.Append(")"));
                    value.Remove(value.Length - 1, 1);
                    value.Append(" ),");
                    sb.Append(value.ToString());
                    sb.Remove(sb.Length - 1, 1);

                    StringBuilder sb1 = new StringBuilder();
                    if (string.IsNullOrWhiteSpace(Id))
                    {
                        sb1 = sb;
                        Con.getDataSet(sb.ToString());
                    }
                    else if (formid.Equals("Supplier"))
                    {
                        sb1.AppendLine($" IF NOT EXISTS (select 1 from  CRJH_{formid} where Id='{Id}' and belongOrg_orgId='{belongOrg_orgId}') ");
                        sb1.AppendLine(" BEGIN ");
                        sb1.AppendLine(sb.ToString());
                        sb1.AppendLine(" END");
                        Con.getDataSet(sb1.ToString());
                    }
                    else if (formid.Equals("MATERIAL"))
                    {
                        sb1.AppendLine($" IF NOT EXISTS (select 1 from  CRJH_{formid} where Id='{Id}') ");
                        sb1.AppendLine(" BEGIN ");
                        sb1.AppendLine(sb.ToString());
                        sb1.AppendLine(" END");
                        Con.getDataSet(sb1.ToString());
                    }
                    else
                    {

                        sb1.AppendLine($" IF NOT EXISTS (select 1 from  CRJH_{formid} where Id='{Id}') ");
                        sb1.AppendLine(" BEGIN ");
                        sb1.AppendLine(sb.ToString());
                        sb1.AppendLine(" END");
                        Con.getDataSet(sb1.ToString());

                    }
                    

                }

                #endregion 获取解析数据，创建插入语句
            }
            catch(Exception e)
            {
                Logger logger = new Logger(ConfigurationManager.AppSettings["log"] , DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                logger.WriteLog("数据出现异常,错误信息：" + e.Message);
                logger.WriteLog("             堆栈信息：" + e.StackTrace);
                Console.WriteLine(e.Message);
            }



        }


        public static void InsertTableEntry(string formid, string type, string data)
        {
            //data = data.ToLower();

            #region 获取解析数据，创建插入语句

            string sql = $"exec CRJH_parseJson '{formid}'";
            DataSet dataSet = Con.getDataSet(sql);
            JObject json = JObject.Parse(data.Replace("'", "’"));

            #region 分页逻辑

            JObject page = (JObject)json["page"];

            #endregion 分页逻辑


            JArray records = (JArray)json[type];
            foreach (var record in records)
            {
                JObject items = (JObject)record;
                JArray details = (JArray)items["details"];
                foreach (var detail in details)
                {
                    JObject model = (JObject)detail;
                    StringBuilder sb = new StringBuilder();
                    StringBuilder column = new StringBuilder();
                    StringBuilder value = new StringBuilder();
                    sb.Append(string.Format(@"/*dialect*/  insert into {0} ", "CRJH_" + formid));
                    value.Append("  values ");
                    value.Append("\r\n('" + Regex.Replace(model.ToString(), @"\s+", "") + "',");

                    DataRow dataH = dataSet.Tables[0].Rows[0];
                    column.Append("(model,");
                    string Id = "";
                    foreach (DataColumn columns in dataH.Table.Columns)
                    {
                        string filed = columns.ColumnName;
                        if (filed.Equals("Id")) Id = model[dataH[filed].ToString()].ToString();
                        //出入库明细特殊处理
                        if (filed.Equals("itemSn"))
                        {
                            JObject item = (JObject)items["item"];
                            
                            column.Append("" + filed + ",");
                            value.Append("'" + item[dataH[filed].ToString()] + "',");

                        }
                        else if (filed.Contains("_"))
                        {
                            var strings = filed.Split('_');
                            if (model.Property(strings[0]) != null)
                            {
                                string str = strings.Length == 2
                                    ? model[strings[0]][strings[1]].ToString()
                                    : model[strings[0]][strings[1]][strings[2]].ToString();

                                column.Append("" + filed + ",");
                                value.Append("'" + str + "',");
                            }
                        }
                        else if (model.Property(dataH[filed].ToString()) != null)
                        {
                            column.Append("" + filed + ",");
                            value.Append("'" + model[dataH[filed].ToString()] + "',");
                        }
                    }
                    column.Remove(column.Length - 1, 1);
                    if (!string.IsNullOrWhiteSpace(column.ToString())) sb.Append(column.Append(")"));
                    value.Remove(value.Length - 1, 1);
                    value.Append(" ),");
                    sb.Append(value.ToString());
                    sb.Remove(sb.Length - 1, 1);


                    if (string.IsNullOrWhiteSpace(Id))
                    {
                        Con.getDataSet(sb.ToString());
                    }
                    else
                    {
                        StringBuilder sb1 = new StringBuilder();
                        sb1.AppendLine($" IF NOT EXISTS (select 1 from  CRJH_{formid} where Id='{Id}') ");
                        sb1.AppendLine(" BEGIN ");
                        sb1.AppendLine(sb.ToString());
                        sb1.AppendLine(" END");
                        Con.getDataSet(sb1.ToString());
                    }
                }
            }

            #endregion 获取解析数据，创建插入语句

        }

    }
}
