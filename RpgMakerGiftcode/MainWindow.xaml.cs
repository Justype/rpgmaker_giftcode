using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.IO;

namespace RpgMakerGiftcode
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> ResultList { get; set; } = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            string[] args = Environment.GetCommandLineArgs();

            // 第一个环境变量是当前程序所在路径，第二个环境变量是拖拽的信息
            string folderName = GetFolder(args.Count() > 1 ? args[1] : args[0]);

            string dataFolder = Path.Combine(folderName, "www", "data");
            if (Directory.Exists(dataFolder))
                ChangeResult(GetGiftcodes(dataFolder, "礼包码", 8));

        }

        /// <summary>
        /// 更新结果列表
        /// </summary>
        /// <param name="text">输入文本</param>
        private void ChangeResult(string text)
        {
            ResultList.Clear();
            ResultList.Add(text);
        }

        /// <summary>
        /// 更新结果列表
        /// </summary>
        /// <param name="texts">输出结果集合</param>
        private void ChangeResult(IEnumerable<string> texts)
        {
            ResultList.Clear();
            if (texts.Count() == 0)
                ResultList.Add("找不到礼包码：文件被加密or不含礼包码这3个字");
            else
                foreach (string code in texts.Distinct())
                {
                    ResultList.Add(code);
                }
        }

        /// <summary>
        /// 返回文件目录，如果是目录直接返回
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        private string GetFolder(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory) ?
                path : Path.GetDirectoryName(path);
        }

        /// <summary>
        /// 利用关键词遍历寻找目录下的文件
        /// </summary>
        /// <param name="path">目录</param>
        /// <param name="keyword">关键词</param>
        /// <param name="number">所含数字位数</param>
        /// <returns></returns>
        private IEnumerable<string> GetGiftcodes(string path, string keyword, int number)
        {
            var files = Directory.GetFiles(path);
            List<string> giftcodes = new List<string>();

            foreach (string file in files)
            {
                string text = File.ReadAllText(file, Encoding.UTF8);
                if(text.Contains(keyword)) // 查找文件内是否含有礼包码
                {
                    var results = Regex.Matches(text, "\\d{" + number + "}"); // 礼包码是8位数
                    foreach (var result in results)
                    {
                        giftcodes.Add(result.ToString());
                    }
                }
            }

            return giftcodes;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)); //打开超链接
            e.Handled = true;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            int number;
            try
            {
                number = int.Parse(numberTextBox.Text);
            }
            catch (Exception)
            {
                ChangeResult("请输入数字");
                return;
            }
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                ChangeResult("请拖入文件夹");
            else
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // RPG Maker MV 必然会有 /www/data 文件夹
                string dataFolder = Path.Combine(GetFolder(files[0]), "www", "data");
                if (Directory.Exists(dataFolder))
                    ChangeResult(GetGiftcodes(dataFolder, keywordTextBox.Text, number));
                else
                    ChangeResult("www/data文件夹不存在：不是RPG Maker MV游戏，或者游戏文件被打包");
            }
        }
    }

}
