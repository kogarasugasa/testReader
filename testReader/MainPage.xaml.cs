using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace testReader
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplePiLcd apLcd = new ApplePiLcd();
        ApplePiTemp apTemp = new ApplePiTemp();

        public ViewModelMainPage ViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            
            Loaded += MainPage_Loaded;

            ViewModel = new ViewModelMainPage();
            this.ViewModel.StringLine = String.Empty;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await apLcd.InitLCD();
            await apLcd.WriteLine("Waiting");
            await apLcd.WriteLine(" ");

            await apTemp.InitTempSensor();
        }

        private async void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                this.ViewModel.StringLine = TextBox1.Text;
                await apLcd.WriteLine(this.ViewModel.StringLine);
                this.ViewModel.StringLine = String.Empty;
            }
            SensorTxt.Text = "温度 " + apTemp.TMP.ToString("F1") + "\r\n" +
                "湿度 " + apTemp.PRE.ToString("F1") + "\r\n" +
                "気圧 " + apTemp.HUM.ToString("F1");
        }
    }
}
