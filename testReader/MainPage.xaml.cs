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
using System.Threading.Tasks;

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
            ViewModel = new ViewModelMainPage();

            Loaded += MainPage_Loaded;

            
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitI2cDevices();

            this.ViewModel.StringLine = String.Empty;
            this.ViewModel.TempSensorText = SensorText();

            TextBox1.Focus(FocusState.Keyboard);
        }

        private async void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                this.ViewModel.StringLine = TextBox1.Text;
                await apLcd.WriteLine(this.ViewModel.StringLine);
                this.ViewModel.StringLine = String.Empty;
            }

            this.ViewModel.TempSensorText = SensorText();
        }

        private async Task InitI2cDevices()
        {
            await apLcd.InitLCD();
            await apLcd.WriteLine("Waiting");
            await apLcd.WriteLine(" ");

            await apTemp.InitTempSensor();
        }

        private string SensorText()
        {
            System.Text.StringBuilder SensorText = new System.Text.StringBuilder();
            SensorText.Append("温度 " + apTemp.TMP.ToString("F1") + "℃" + "\r\n");
            SensorText.Append("湿度 " + apTemp.HUM.ToString("F1") + "％" + "\r\n");
            SensorText.Append("気圧 " + apTemp.PRE.ToString("F1") + "hPa");
            return SensorText.ToString();
        }
    }
}
