﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace wpf_serial20190117
//{
//    /// <summary>
//    /// MainWindow.xaml 的交互逻辑
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        public MainWindow()
//        {
//            InitializeComponent();
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpf_serial20190117
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWPF();        //自己做一个初始化函数


        }
        SerialPort serialPort1 = new SerialPort();
        private void ScanSerialPort(SerialPort MyPort, ComboBox MyBox)
        {
            //扫描可用按钮
            string Buffer;               //缓存
            MyBox.Items.Clear();         //清空端口下拉菜单
            for (int i = 1; i < 20; i++)
            {
                try
                {
                    Buffer = "COM" + i.ToString();
                    MyPort.PortName = Buffer;
                    MyPort.Open();
                    MyBox.Items.Add(Buffer);   //如果串口打开成功，说明可用，将其名称添加到下拉菜单中
                    MyPort.Close();
                }
                catch
                {
                    //ignore
                }
            }
        }

        private void InitializeWPF()
        {
            //打开窗口进行的初始化方法，代替WinForm中的Form1_Load方法
            for (int i = 1; i <= 20; i++)
            {
                SerialPortComboBox.Items.Add("COM" + i.ToString());        //添加端口下拉菜单选择
            }
            SerialPortComboBox.Text = "COM1";                              //设定端口下拉菜单默认值         
            BaudComboBox.Text = "115200";                                    //设定波特率下拉菜单默认值

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);   //必须手动添加串口数据接收事件的处理方法

            ScanSerialPort(serialPort1, SerialPortComboBox);
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //串口数据接收事件
            if (ReceivedValueRadioButton.IsChecked == false) //若接收模式为字符模式
            {
                string str = serialPort1.ReadExisting();    //字符串方式读
                ReceivedRichTextBox.AppendText(str); //接收文本框添加接收数据内容
            }
            else                                  //若接收模式为数值模式
            {
                byte data = (byte)serialPort1.ReadByte(); //将从串口接收的int型数值转换为byte型数值（相当于C语言中的unchar）
                string str = Convert.ToString(data, 16).ToUpper(); //将byte型转为十六进制字符串
                ReceivedRichTextBox.AppendText("0x" + (str.Length % 2 == 1 ? str + "0" : str) + " ");  //若为一位则在前面补0
            }
        }
        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            //扫描可用按钮对应事件
            ScanSerialPort(serialPort1, SerialPortComboBox);
        }

        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            //发送按钮对应事件
            byte[] Data = new byte[1];  //一次发送一个字节
            if (serialPort1.IsOpen || PostTextBox.Text == "")
            {
                if (PostValueRadioButton.IsChecked == false) //若发送模式是字符模式
                {
                    try
                    {
                        serialPort1.WriteLine(PostTextBox.Text);
                    }
                    catch
                    {
                        MessageBox.Show("串口数据写入错误", "错误");
                        serialPort1.Close();        //关闭串口
                        OpenButton.IsEnabled = true;  //打开端口按钮现已可按
                        CloseButton.IsEnabled = false;//关闭端口按钮不可再按
                    }
                }
                else                              //若发送模式是数值模式
                {
                    for (int i = 1; i < (PostTextBox.Text.Length - PostTextBox.Text.Length % 2) / 2; i++) //输入两个字母等于一个字节，故这里除以2
                    {
                        Data[0] = Convert.ToByte(PostTextBox.Text.Substring(i * 2, 2), 16);  //对输入框中的内容依次取2个，并转换为16进制byte型数值
                        serialPort1.Write(Data, 0, 1);    //对Data数组中的内容从偏移量为0的位置写入指定1个字节长度的内容
                    }
                    if (PostTextBox.Text.Length % 2 != 0) //若输入内容长度不为偶数
                    {
                        Data[0] = Convert.ToByte(PostTextBox.Text.Substring(PostTextBox.Text.Length - 1, 1), 16); //单独处理剩下的那个数值
                        serialPort1.Write(Data, 0, 1);
                    }
                }
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            //打开端口对应事件
            try
            {
                serialPort1.PortName = SerialPortComboBox.Text;                //串口名称
                serialPort1.BaudRate = Convert.ToInt32(BaudComboBox.Text, 10); //字符串型数值转换为十进制型赋给串口波特率
                serialPort1.Open();  //打开串口
                OpenButton.IsEnabled = false; //打开端口按钮不可再按
                CloseButton.IsEnabled = true; //关闭端口按钮现已可按
                PostButton.IsEnabled = true;
            }
            catch
            {
                MessageBox.Show("串口打开遇到错误", "错误");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //关闭端口对应事件
            try
            {
                serialPort1.Close();        //关闭串口
                OpenButton.IsEnabled = true;  //打开端口按钮现已可按
                CloseButton.IsEnabled = false;//关闭端口按钮不可再按
                PostButton.IsEnabled = false;
            }
            catch
            {
                //ignore
            }
        }

    }
}
