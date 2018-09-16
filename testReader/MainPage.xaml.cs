//using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;
//using Windows.Foundation;
//using Windows.Foundation.Collections;
//using Windows.UI.Xaml.Controls.Primitives;
//using Windows.UI.Xaml.Data;
//using Windows.UI.Xaml.Media;
//using Windows.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks;
using Windows.Storage;

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

        private readonly string AzureDeviceId = "Rasp01;";
        private readonly string AzureSharedAccessKey = "9gbtY1yoTNxNME3RwxighF3u8xqc5j/Tcd49EW7saNc=";
        private readonly string AzureDeviceType = "RaspberryPi";
        MyastiaAzure.AzureIotDevice azureIot;

        DispatcherTimer azureSendTimer;
        DispatcherTimer updateDispTimer;
        DispatcherTimer errSensorTimer;

        public ViewModelMainPage ViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = new ViewModelMainPage();

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitI2cDevicesAsync();
            azureIot = new MyastiaAzure.AzureIotDevice(AzureDeviceId, AzureSharedAccessKey, AzureDeviceType);
            azureIot.StartReceiveMessage();
            RunTimer();

            //画面初期化処理
            this.ViewModel.StringLine = String.Empty;
            this.ViewModel.TempSensorText = GetSensorValueText();
            this.ViewModel.ErrorMessage = "エラー無し";

            TextBox1.Focus(FocusState.Keyboard);
        }

        //テキストボックスイベント
        private async void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await DispTextBoxToLcdAsync();
            }

            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                await DipsSensorToLcdAsync();
            }
        }

        private void RunTimer()
        {
            //Azureへセンサーのデータを送るタイマー
            azureSendTimer = new DispatcherTimer();
            azureSendTimer.Interval = new TimeSpan(0, 1, 0);
            async void azureSendHandler(object s, object e)
            {
                await azureIot.SendMessage(apTemp.TMP.ToString(), "温度");
            }
            azureSendTimer.Tick += azureSendHandler;
            azureSendTimer.Start();

            //画面を更新するタイマー
            updateDispTimer = new DispatcherTimer();
            updateDispTimer.Interval = new TimeSpan(0, 0, 1);
            void updateDispHandler(object s, object e)
            {
                this.ViewModel.ReceivedMessage = azureIot.ReceivedMessage;
                this.ViewModel.TempSensorText = GetSensorValueText();
            }
            updateDispTimer.Tick += updateDispHandler;
            updateDispTimer.Start();

            //エラーを表示するタイマー
            errSensorTimer = new DispatcherTimer();
            errSensorTimer.Interval = new TimeSpan(0, 0, 5);
            async void errSensorHandler(object s, object e)
            {
                if(azureIot.Error.Count != 0)
                {
                    this.ViewModel.ErrorMessage = azureIot.Error[0];
                    List<string> ErrTextList = new List<string>(azureIot.Error);
                    ErrTextList.Insert(0, string.Format("********{0}********", DateTime.Now.ToString("yyyyMMdd-HHmmss")));
                    await WriteLogAsync(ErrTextList);
                }
            }
            errSensorTimer.Tick += errSensorHandler;
            errSensorTimer.Start();
        }

        private async Task DipsSensorToLcdAsync()
        {
            this.ViewModel.TempSensorText = GetSensorValueText();
            await apLcd.WriteLineAsync(apTemp.TMP.ToString("F0") + "C" + " " +
                apTemp.HUM.ToString("F0") + "%");
            await apLcd.WriteLineAsync(apTemp.PRE.ToString("F0") + "hPa");
        }

        private async Task DispTextBoxToLcdAsync()
        {
            this.ViewModel.StringLine = TextBox1.Text;
            await apLcd.WriteLineAsync(this.ViewModel.StringLine);
            this.ViewModel.StringLine = String.Empty;
        }

        private async Task InitI2cDevicesAsync()
        {
            await apLcd.InitLcdAsync();
            await apLcd.WriteLineAsync("Waiting");
            await apLcd.WriteLineAsync(" ");
            await apTemp.InitTempSensorAsync();
        }

        private string GetSensorValueText()
        {
            System.Text.StringBuilder SensorText = new System.Text.StringBuilder();
            SensorText.Append("温度 " + apTemp.TMP.ToString("F1") + "℃" + "\r\n");
            SensorText.Append("湿度 " + apTemp.HUM.ToString("F1") + "％" + "\r\n");
            SensorText.Append("気圧 " + apTemp.PRE.ToString("F1") + "hPa");
            return SensorText.ToString();
        }

        private async Task WriteLogAsync(List<string> LogTextLines)
        {
            string logFileName = string.Format("{0}_{1}.txt", new string[] { "SensorLog", DateTime.Today.ToString("yyyyMMdd")});
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile logFile = await storageFolder.CreateFileAsync(logFileName,CreationCollisionOption.OpenIfExists);
            await FileIO.WriteLinesAsync(logFile, LogTextLines);
        }
    }
}
