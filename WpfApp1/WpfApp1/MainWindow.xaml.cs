using BD.Standard.CRJH.ProgramParse.Utils;
using BD.Standard.FOM.Program.Utils;
using MySqlConnector;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private string DownLoadexePath = ConfigurationManager.AppSettings["DownLoadexePath"];
        private string UpLoadexePath = ConfigurationManager.AppSettings["UpLoadexePath"];

        private static string Stockbegin = "";
        private static string Stockend = "";


        public MainWindow()
        {
            InitializeComponent();

            orderbeginDate.SelectedDate = DateTime.Now;
            orderendDate.SelectedDate = DateTime.Now;
            stockbeginDate.SelectedDate = DateTime.Now;
            stockendDate.SelectedDate = DateTime.Now;

            OrderDownLoadButton.IsEnabled = false;
            OrderUpLoadButton.IsEnabled = false;
            StockDownLoadButton.IsEnabled = false;
            StockUpLoadButton.IsEnabled = false;
            Check.IsChecked = false;

        }

        ///// <summary>
        ///// 基础资料获取
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private async void BaseDownLoad_Click(object sender, RoutedEventArgs e)
        //{
        //    var bizTimeBeginorder = Timestamps(DateTime.Now.ToString(), 1);
        //    var bizTimeEndorder = Timestamps(DateTime.Now.ToString(), 1);

        //    //下载程序路径
        //    string exePath = @DownLoadexePath;
        //    string args = $"base {bizTimeBeginorder} {bizTimeEndorder}";

        //    await Clicks("获取基础资料", baseDownLoadButton, baseUpLoadButton, BaseResult, exePath, args,false);
        //}



        /// <summary>
        /// 订单获取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OrderDownLoad_Click(object sender, RoutedEventArgs e)
        {
            getAppAuthToken();

            string orderbegin = DateTime.Parse(orderbeginDate.Text).ToString("yyyy-MM-dd 00:00:00");
            string orderend = DateTime.Parse(orderendDate.Text).ToString("yyyy-MM-dd 23:59:59");

            var bizTimeBeginorder = Timestamps(orderbegin, 1);
            var bizTimeEndorder = Timestamps(orderend, 1);


            //下载程序路径
            string exePath = @DownLoadexePath;
            string args = $"Order {bizTimeBeginorder} {bizTimeEndorder}";


            await Clicks("获取订单", OrderDownLoadButton, OrderUpLoadButton, OrderResult, exePath, args, false);
        }


        /// <summary>
        /// 出入库获取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StockDownLoad_Click(object sender, RoutedEventArgs e)
        {

            getAppAuthToken();
            string stockbegin = DateTime.Parse(stockbeginDate.Text).ToString("yyyy-MM-dd 00:00:00");
            string stockend = DateTime.Parse(stockendDate.Text).ToString("yyyy-MM-dd 23:59:59");

            var bizTimeBeginorder = Timestamps(stockbegin, 1);
            var bizTimeEndorder = Timestamps(stockend, 1);

            //下载程序路径
            string exePath = @DownLoadexePath;
            string args = $"Stock {bizTimeBeginorder} {bizTimeEndorder}";
            await Clicks("获取出入库", StockDownLoadButton, StockUpLoadButton, StockResult, exePath, args, false);
        }


        ///// <summary>
        ///// 基础资料生成
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private async void BaseUpLoad_Click(object sender, RoutedEventArgs e)
        //{

        //    //上传程序路径
        //    string exePath = @UpLoadexePath;
        //    string args = "base";
        //    await Clicks("生成基础资料", baseDownLoadButton, baseUpLoadButton, BaseResult, exePath, args,false);


        //}

        /// <summary>
        /// 订单生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OrderUpLoad_Click(object sender, RoutedEventArgs e)
        {
            string orderbegin = DateTime.Parse(orderbeginDate.Text).ToString("yyyy-MM-dd 00:00:00");
            string orderend = DateTime.Parse(orderendDate.Text).ToString("yyyy-MM-dd 23:59:59");
            //上传程序路径
            string exePath = @UpLoadexePath;
            string args = "Order";
            await Clicks("生成订单", OrderDownLoadButton, OrderUpLoadButton, OrderResult, exePath, args, false);
            DBConnection Con = new DBConnection();
            long v = Timestamps(orderbegin);
            long v1 = Timestamps(orderend);
            DataSet orgIdsSet = Con.getDataSet($"select status from CRJH_PurchaseOrder where status=0 and createTime>={Timestamps(orderbegin)}  and createTime<={Timestamps(orderend)}");

            string count = orgIdsSet.Tables[0].Rows.Count.ToString();

            OrderResult.Text = OrderResult.Text + "\r\n " + $"订单生成结束，共有{count}条数据生成失败！！";

        }


        /// <summary>
        /// 出入库生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StockUpLoad_Click(object sender, RoutedEventArgs e)
        {
            string stockbegin = DateTime.Parse(stockbeginDate.Text).ToString("yyyy-MM-dd 00:00:00");
            string stockend = DateTime.Parse(stockendDate.Text).ToString("yyyy-MM-dd 23:59:59");

            Stockbegin = stockbegin;
            Stockend = stockend;

            //上传程序路径
            string exePath = @UpLoadexePath;
            string args = "Stock";
            await Clicks("生成出入库", StockDownLoadButton, StockUpLoadButton, StockResult, exePath, args, true);
            DBConnection Con = new DBConnection();

            DataSet orgIdsSet = Con.getDataSet($"select status from CRJH_InStock where status=0 and bizTime>={Timestamps(stockbegin)}  and bizTime<={Timestamps(stockend)} union all select status from CRJH_OutStock where status=0 and bizTime>={Timestamps(stockbegin)}  and bizTime<={Timestamps(stockend)}");

            string count = orgIdsSet.Tables[0].Rows.Count.ToString();

            StockResult.Text = StockResult.Text + "\r\n " + $"出入库生成结束，共有{count}条数据生成失败！！";
        }


        /// <summary>
        /// 公共方法
        /// </summary>
        /// <param name="name">单据名称</param>
        /// <param name="DownLoadButton"></param>
        /// <param name="UpLoadButton"></param>
        /// <param name="Result"></param>
        /// <param name="exePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task Clicks(string name, Button DownLoadButton, Button UpLoadButton, TextBox Result, string exePath, string args, Boolean flag)
        {
            try
            {
                DownLoadButton.IsEnabled = false;
                UpLoadButton.IsEnabled = false;


                DateTime begin = DateTime.Now;
                if (string.IsNullOrWhiteSpace(Result.Text))
                {
                    Result.Text = begin + $":正在{name}数据，请稍等...";
                }
                else
                {
                    Result.Text = Result.Text + "\r\n" + begin + $":正在{name}数据，请稍等...";
                }


                // 给UI一个刷新的机会
                await Task.Delay(1);

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = exePath,
                    Arguments = args,
                    UseShellExecute = false, // 必须设为false才能捕获输出
                    RedirectStandardOutput = true,    // 重定向标准输出
                    RedirectStandardError = true,     // 重定向错误输出
                    CreateNoWindow = true,            // 不显示控制台窗口
                    StandardOutputEncoding = Encoding.UTF8  // 设置编码，支持中文
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    StringBuilder outputBuilder = new StringBuilder();
                    StringBuilder errorBuilder = new StringBuilder();

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorBuilder.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    // 开始异步读取输出
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await Task.Run(() => process.WaitForExit());

                    // 最终结果显示
                    string finalOutput = outputBuilder.ToString();
                    string finalError = errorBuilder.ToString();

                    if (!string.IsNullOrEmpty(finalError))
                    {
                        Result.Text = Result.Text + "\r\n" + DateTime.Now + $":执行完成（有错误）\n{finalError}";
                        Result.Foreground = Brushes.Red;
                    }
                    else
                    {
                        Result.Text = Result.Text + "\r\n" + DateTime.Now + $":{name}数据执行完成！";
                        Result.Foreground = Brushes.Green;

                        TimeSpan difference = begin - DateTime.Now;

                        Result.Text = Result.Text + "\r\n耗时：" + difference.ToString(@"hh\:mm\:ss");
                    }
                    DownLoadButton.IsEnabled = true;
                    UpLoadButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Result.Text = ex.Message;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 今天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void datebuton_Click(object sender, RoutedEventArgs e)
        {
            orderbeginDate.SelectedDate = DateTime.Now;
            orderendDate.SelectedDate = DateTime.Now;
            stockbeginDate.SelectedDate = DateTime.Now;
            stockendDate.SelectedDate = DateTime.Now;
        }

        /// <summary>
        /// 上月
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void datebuton1_Click(object sender, RoutedEventArgs e)
        {
            orderbeginDate.SelectedDate = Convert.ToDateTime(DateTime.Now.AddMonths(-1).ToString("yyyy-MM-01 00:00:00"));
            orderendDate.SelectedDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01 23:59:59")).AddDays(-1);
            stockbeginDate.SelectedDate = Convert.ToDateTime(DateTime.Now.AddMonths(-1).ToString("yyyy-MM-01 00:00:00"));
            stockendDate.SelectedDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01 23:59:59")).AddDays(-1);
        }

        /// <summary>
        /// 本月
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void datebuton2_Click(object sender, RoutedEventArgs e)
        {
            orderbeginDate.SelectedDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            orderendDate.SelectedDate = DateTime.Now;
            stockbeginDate.SelectedDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01 00:00:00"));
            stockendDate.SelectedDate = DateTime.Now;
        }

        public void getAppAuthToken()
        {
            // 1. 定义连接字符串（请替换为你的实际信息）
            string connectionString =
                "Server=127.0.0.1;Database=rejidb;User ID=root;Password=123@qq.com;Pooling=false;";
            Logger logger = new Logger("C:\\MeiTuanApi\\WpfApp1\\log", DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            try
            {
                // 2. 使用using语句确保资源被正确释放
                using (var connection = new MySqlConnection(connectionString))
                {
                    // 3. 打开连接
                    connection.Open();
                    logger.WriteLog("数据库连接成功！");
                    // 4. 编写SQL查询语句（使用参数化查询）
                    string sql = "select * from  meituantoken LIMIT 1";

                    // 5. 创建Command对象
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        // 7. 执行查询并获取DataReader
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // 8. 循环读取结果
                            while (reader.Read())
                            {
                                // 通过列名或索引获取数据
                                string accessToken = reader.GetString("accessToken");

                                logger.WriteLog("accessToken！" + accessToken);

                                    // 1. 创建 ExeConfigurationFileMap 实例
                                    ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                                    // 2. 设置外部配置文件的完整路径
                                    configFileMap.ExeConfigFilename = @"C:\MeiTuanApi\BD.Standard.CRJH.ProgramParse\BD.Standard.CRJH.ProgramParse.exe.config";

                                    // 3. 使用 OpenMappedExeConfiguration 加载配置文件
                                    //    ConfigurationUserLevel.None 表示加载顶层的配置文件[reference:2][reference:3]
                                    Configuration config = ConfigurationManager.OpenMappedExeConfiguration(
                                        configFileMap,
                                        ConfigurationUserLevel.None
                                    );
                                    // 4. 修改 appSettings
                                    if (config.AppSettings.Settings["appAuthToken"] != null)
                                    {
                                        config.AppSettings.Settings["appAuthToken"].Value = accessToken;
                                    }

                                    // 5. 保存更改
                                    config.Save(ConfigurationSaveMode.Modified);
                            }
                        }
                    }
                }
                

            }
            catch (Exception ex)
            {
                logger.WriteLog($"外层异常: {ex.Message}");
                logger.WriteLog($"内部异常: {ex.InnerException?.Message}");
                logger.WriteLog($"内部异常的堆栈: {ex.InnerException?.StackTrace}");

            }

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)Check.IsChecked)
            {
                OrderDownLoadButton.IsEnabled = false;
                OrderUpLoadButton.IsEnabled = false;
                StockDownLoadButton.IsEnabled = false;
                StockUpLoadButton.IsEnabled = false;
            }
            else
            {
                OrderDownLoadButton.IsEnabled = true;
                OrderUpLoadButton.IsEnabled = true;
                StockDownLoadButton.IsEnabled = true;
                StockUpLoadButton.IsEnabled = true;
            }






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


        public static long Timestamps(string time)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = Convert.ToDateTime(time).ToUniversalTime() - origin;

            return Convert.ToInt64(diff.TotalMilliseconds.ToString());
        }


        // 添加获取剩余数量的方法
        private int GetRemainingCount()
        {
            try
            {
                DBConnection Con = new DBConnection();
                string query = $"select status from CRJH_InStock where status=0 and bizTime>={Timestamps(Stockbegin)}  and bizTime<={Timestamps(Stockend)} union all select status from CRJH_OutStock where status=0 and bizTime>={Timestamps(Stockbegin)}  and bizTime<={Timestamps(Stockend)}";

                DataSet dataSet = Con.getDataSet(query);
                return dataSet.Tables[0].Rows.Count;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

    }



}




