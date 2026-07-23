using BD.Standard.FOM.Program.Utils;
using BD.Standard.CRJH.ProgramParse.Forms;
using BD.Standard.CRJH.ProgramParse.Utils;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;

namespace BD.Standard.CRJH.ProgramParse
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("执行开始："+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //string Logpath = @"Pur\" + DateTime.Now.ToString("yyyyMM");
            //Logger logger = new Logger(ConfigurationManager.AppSettings["log"] + Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
           


           string type = "";
            string begintime = "1777566120000";
            string endtime = "1780158120000";

            if (args.Any())
            {
                type = args[0];
                begintime = args[1];
                endtime = args[2];
            }
            else if (string.IsNullOrWhiteSpace(type))
            {

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("入参类型:Base、Base1、Order、Stock、token、updatetoken");
                sb.AppendLine("     Base:获取物料，Base1:供应商数据");
                sb.AppendLine("     Order:获取采购订单数据");
                sb.AppendLine("     Stock:获取出入库");
                sb.AppendLine("     token:根据授权code获取token与updatetoken");
                sb.AppendLine("     updatetoken:刷新token");
                sb.Append("输入入参类型:");
                Console.Write(sb.ToString());
                type = Console.ReadLine();

                //if (type.ToLower().Equals("order") || type.ToLower().Equals("stock"))
                //{
                //    NewMethod(out begintime, out sb);
                //}


            }

            DBConnection Con = new DBConnection();
            //DBConnection Con = null;

            ////获取机构
            var org = new Org();
            org.PostOrg(Con);

            DataSet orgIdsSet1 = null;
            DataSet orgIdsSet0 = Con.getDataSet("select orgId,orgType,inStockType,outStockType from CRJH_org where orgId='3082269'");

            switch (type.ToLower())
            {
                //获取基础资料
                //调整为执行订单出入库程序时统执行
                case "base":
                    
                    ////物品档案
                    var material = new Material();
                    //物品档案
                    material.PostMaterial(orgIdsSet0);

                    break;
                case "base1":
                    
                    ////供应商档案
                    var supplier = new Supplier();
                    //供应商档案
                    supplier.PostSupplier(orgIdsSet0);

                    break;

                case "order":

                    orgIdsSet1 = Con.getDataSet("select orgId,orgType,inStockType,outStockType from CRJH_org where orgType=4 or (orgType=5 and poiStatus=0)  and merchantNo in (select meituanNumber from CRJH_OrgMapping where isenable='true')");


                    //采购订单表头
                    PurchaseOrder purchaseOrder = new PurchaseOrder();
                    purchaseOrder.PostPurchaseOrder(orgIdsSet1, Convert.ToInt64(begintime), Convert.ToInt64(endtime));


                    //采购订单明细
                    //查询当前时间的采购订单的单号，关联查询组织表CRJH_orgId，获取orgId的orgType，作为查询条件。根据orgId分组，得到Array类型的订单号
                    DataSet orgIdsSet2 = Con.getDataSet("select orgId,orgType,purchaseOrderSn from  [dbo].[CRJH_PurchaseOrder] p left join [dbo].[CRJH_Org] o on p.orgInfo_orgId=o.orgId where status=0 ");
                    PurchaseOrderEntry purchaseOrderEntry = new PurchaseOrderEntry();
                    purchaseOrderEntry.PostPurchaseOrderEntry(orgIdsSet2);
                    break;

                //获取出入库单据
                case "stock":


                    orgIdsSet1 = Con.getDataSet("select orgId,orgType,inStockType,outStockType from CRJH_org where orgType=4 or (orgType=5 and poiStatus=0)  and merchantNo in (select meituanNumber from CRJH_OrgMapping where isenable='true')");

                    #region 入库单表头
                    InStock inStock = new InStock();
                    inStock.PostInStock(orgIdsSet1, Convert.ToInt64(begintime), Convert.ToInt64(endtime));

                    DataSet orgIdsSetIn = Con.getDataSet("select orgId,orgType,itemSn from  [dbo].[CRJH_InStock] p left join [dbo].[CRJH_Org] o on p.belongOrg_orgId=o.orgId  where status=0");
                    InStockEntry inStockEntry = new InStockEntry();
                    inStockEntry.PostInStockEntry(orgIdsSetIn);
                    #endregion 入库单表头


                    #region 出库单表头
                    OutStock outStock = new OutStock();
                    outStock.PostOutStock(orgIdsSet1, Convert.ToInt64(begintime), Convert.ToInt64(endtime));

                    DataSet orgIdsSetOut = Con.getDataSet("select orgId,orgType,itemSn from  [dbo].[CRJH_OutStock] p left join [dbo].[CRJH_Org] o on p.belongOrg_orgId=o.orgId  where status=0");
                    OutStockEntry outStockEntry = new OutStockEntry();
                    outStockEntry.PostOutStockEntry(orgIdsSetOut);
                    #endregion 出库单表头

                    


                    #region 收货单与返货单、配送单

                    orgIdsSet1 = Con.getDataSet("select itemSn,belongOrg_orgId,sourceSn from  [dbo].[CRJH_InStock] where status=0 and type_id =3 and deliveryWarehouseCode is null ");
                    Delivery delivery = new Delivery();
                    delivery.PostDelivery(orgIdsSet1, Convert.ToInt64(begintime), Convert.ToInt64(endtime));
                    StringBuilder stringBuilder=new StringBuilder();
                    stringBuilder.AppendLine("select itemSn,sourceSn,'CRJH_InStock' tablename from  [dbo].[CRJH_InStock] where status=0 and type_id=20  and deliveryWarehouseCode is null");
                    stringBuilder.AppendLine("union all");
                    stringBuilder.AppendLine("select itemSn,sourceSn,'CRJH_OutStock' tablename from  [dbo].[CRJH_OutStock] where status=0 and type_id=7  and deliveryWarehouseCode is null");

                    orgIdsSet1 = Con.getDataSet(stringBuilder.ToString());
                    delivery.PostUnDelivery(orgIdsSet1, Convert.ToInt64(begintime), Convert.ToInt64(endtime));

                    orgIdsSet1 = Con.getDataSet("select itemSn,sourceSn from  [dbo].[CRJH_OutStock] where status=0 and type_id in (6,12)");
                    delivery.PostdeliveryOrder(orgIdsSet1, Convert.ToInt64(begintime), Convert.ToInt64(endtime));
                    #endregion 收货单与返货单


                    break;
                case "test":
                    //orgIdsSet1 = Con.getDataSet("select 'FBRK2606230001' itemSn ,'3102122' belongOrg_orgId,'FH2606230003' sourceSn");
                    orgIdsSet1 = Con.getDataSet("select 'FBRK2606230001' itemSn ,'FH2606230003' sourceSn");
                    Delivery delivery1 = new Delivery();
                    delivery1.PostUnDelivery(orgIdsSet1, Convert.ToInt64(begintime), Convert.ToInt64(endtime));



                    break;

                default:

                    Console.WriteLine("\r\n入参类型不正确");

                    break;


            }
            Console.WriteLine("执行结束：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            
        }

        private static void NewMethod(out string begintime, out StringBuilder sb)
        {
            sb = new StringBuilder();
            sb.AppendLine("数据查询开始时间格式:yyyy-MM-dd HH:mm:ss");
            sb.Append("输入开始时间:");
            Console.Write(sb.ToString());
            begintime = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(begintime))
            {
                if (!DateTime.TryParse(begintime, out _))
                {
                    Console.WriteLine("输入时间格式不正确！输入值：" + begintime+"!请重新输入");
                    NewMethod(out begintime, out sb);
                }
            }




            begintime = Timestamps(begintime, 1);
        }

        public static string Timestamps(string time, int type)
        {
            string stamps = string.Empty;
            if (!string.IsNullOrEmpty(time))
            {
                stamps = type == 0 ? ((Convert.ToDateTime(time).ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString() : ((Convert.ToDateTime(time).ToUniversalTime().Ticks - 621355968000000000) / 10000).ToString();
            }
            else
            {
                if (type == 0)
                {
                    long lstamps = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;//10位时间戳生成方式
                    stamps = lstamps.ToString();
                }
                else
                {
                    long lstamps = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;//13位时间戳生成方式
                    stamps = lstamps.ToString();
                }
            }
            return stamps;
        }
    }

}
