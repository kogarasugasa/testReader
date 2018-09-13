using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.I2c;
using Microsoft.IoT.Lightning.Providers;

namespace testReader
{
    class ApplePiLcd
    {
        private I2cDevice LCD;
        private byte LCD_ADR = 0x3e;
        private List<string> TextLines = new List<string>();
        private List<byte> DispLines = new List<byte>();

        public ApplePiLcd()
        {
            DispLines.Add(0xc0); //2行目アドレス
            DispLines.Add(0x80); //1行目アドレス
            TextLines.Add(" ");
        }

        ~ApplePiLcd()
        {
            LCD.Dispose();
        }

        public async Task InitLcdAsync()
        {
            if (LightningProvider.IsLightningEnabled)
            {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
            }

            var i2c = await I2cController.GetDefaultAsync();
            if (i2c == null)
            {
                LCD = null;
                
                return ;
            }
            LCD = i2c.GetDevice(new I2cConnectionSettings(LCD_ADR));
            
            //LCD初期化
            await Task.Delay(50).ConfigureAwait(false);
            await WriteCmdAsync(0x38, 1);
            await WriteCmdAsync(0x39, 1);
            await WriteCmdAsync(0x14, 1);
            await WriteCmdAsync(0x71, 1);
            await WriteCmdAsync(0x56, 1);
            await WriteCmdAsync(0x6c, 250);
            await WriteCmdAsync(0x38, 1);
            await WriteCmdAsync(0x0c, 1);
            await WriteCmdAsync(0x01, 200);
        }

        public async Task WriteLineAsync(string msg)
        {
            int MaxLineLength = 8;
            int MaxLine = DispLines.Count;

            //空文字列はエラーになるのでスペースに置き換え
            if (msg == String.Empty)
            {
                msg = " ";
            }

            //表示する文字を格納
            while (TextLines.Count >= MaxLine)
            {
                TextLines.RemoveAt(0);
            }
            if (msg.Length > MaxLineLength)
            {
                msg = msg.Substring(0, MaxLineLength);
            }
            TextLines.Add(msg);

            //文字を表示
            await WriteCmdAsync(0x01, 100);
            for (int i = TextLines.Count - 1; i >= 0; i--)
            {
                await WriteCmdAsync(DispLines[i], 1);
                await WriteDispAsync(TextLines[i]);
            }
        }

        private async Task DispLCDAsync()
        {
            //LCDクリア
            await WriteCmdAsync(0x01, 100);

            //1行目
            await WriteCmdAsync(0x80, 1);
            await WriteDispAsync("RPi3");

            //2行目
            await WriteCmdAsync(0xc0, 1);
            await WriteDispAsync("ApplePi");
        }

        private async Task WriteDispAsync(string msg)
        {
            byte[] bytemsg = Encoding.ASCII.GetBytes(msg);

            for (int i = 0; i <= msg.Length - 2; i++)
            {
                LCD.Write(new byte[] { 0x40, bytemsg[i] });
            }
            LCD.Write(new byte[] { 0x40, bytemsg[msg.Length - 1] });
            await Task.Delay(1).ConfigureAwait(false);
        }

        private async Task WriteCmdAsync(byte cmd, int time)
        {
            LCD.Write(new byte[] { 0, cmd });
            await Task.Delay(time).ConfigureAwait(false);
        }
    }
}
