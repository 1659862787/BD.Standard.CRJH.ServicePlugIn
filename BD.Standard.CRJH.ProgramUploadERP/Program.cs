
using BD.Standard.CRJH.ProgramUploadERP.Forms;
using System;
using System.Configuration;
using System.Linq;
using System.Text;

namespace BD.Standard.CRJH.ProgramUploadERP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("执行开始：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            string type = "";
            if (args.Any())
            {
                type = args[0];
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("入参类型:Base、Base1、Order、stock");
                sb.AppendLine("     Base:生成物料，Base1:供应商数据");
                sb.AppendLine("     Order:生成采购订单数据");
                sb.AppendLine("     stock:生成入库单、出库单数据");
                sb.Append("输入入参类型:");
                Console.Write(sb.ToString());
                type = Console.ReadLine();

            }

            //var fdate = DateTime.Now.ToString();



            switch (type.ToLower())
            {
                //获取基础资料
                case "base":

                    var material = new Material();
                    material.PostMaterial("exec CRJH_erp_Material");

                    break;
                case "base1":

                    var supplier = new Supplier();
                    supplier.PostSupplier("exec CRJH_erp_Supplier");

                    break;


                //获取订单数据
                case "order":
 
                    PurchaseOrder purchaseOrder = new PurchaseOrder();
                    purchaseOrder.PostPurchaseOrder("exec CRJH_erp_PurchaseOrder");
                    break;

                //入库单据生成
                case "stock":

                    Stockin stockin = new Stockin();
                    stockin.StockinSC("exec CRJH_Main");

                //出库单据生成

                    Stockout stockout = new Stockout();
                    stockout.StockoutSC("exec CRJH_Main");
                    break;
            }

            Console.WriteLine("执行结束：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

}
