using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BD.Standard.CRJH.ProgramParse.Utils
{
    public class DBConnection : IDisposable
    {
        public static String str = ConfigurationManager.AppSettings["con"];

        static SqlConnection con;

        public DBConnection()
        {
            con = new SqlConnection(str);
        }

        public DataSet getDataSet(string sql)
        {
            SqlDataAdapter sda = new SqlDataAdapter(sql, con);
            sda.SelectCommand.CommandTimeout = 1000;
            DataSet ds = new DataSet();
            sda.Fill(ds);
            return ds;
        }

        public void Dispose()
        {
            con.Close();
            con.Dispose();
        }
    }
}
