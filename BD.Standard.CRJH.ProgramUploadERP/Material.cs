using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BD.Standard.YCWH.ProgramUploadERP
{
    public class Material
    {
        public void PostMaterial()
        {
            JObject page = new JObject()
            {
                {"pageNo","1"},
                {"pageSize","2000"},

            };
            JObject Requestbody = new JObject()
            {
                {"orgId",""},
                {"status","1"},
                {"page",page},
            };




        }
    }
}
