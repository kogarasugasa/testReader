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
        ApplePiLcd applePi = new ApplePiLcd();

        public ViewModelMainPage ViewModel { get; set; }

        public MainPage()
        {
            ViewModel = new ViewModelMainPage();
            this.ViewModel.StringLine = String.Empty;
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await applePi.InitLCD();
            await applePi.WriteLine("Waiting");
        }

        private async void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await applePi.InitLCD();
                await applePi.WriteLine(this.ViewModel.StringLine);
                this.ViewModel.StringLine = String.Empty;
            }
            else
            {
                this.ViewModel.StringLine += e.Key.ToString();
            }
        }
    }
}
