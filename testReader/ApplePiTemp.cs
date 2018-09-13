using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.I2c;
using Microsoft.IoT.Lightning.Providers;

namespace testReader
{
    class ApplePiTemp
    {
        private I2cDevice TempSensor;
        private Timer periodicTimer;
        private const byte TEMP_SENSOR_ADR = 0x76;

        //キャリブレーションデータ用変数
        private UInt16 T1;
        private Int16 T2;
        private Int16 T3;
        private UInt16 P1;
        private Int16 P2;
        private Int16 P3;
        private Int16 P4;
        private Int16 P5;
        private Int16 P6;
        private Int16 P7;
        private Int16 P8;
        private Int16 P9;
        private byte H1;
        private Int16 H2;
        private byte H3;
        private Int16 H4;
        private Int16 H5;
        private Int16 H6;

        //データレジスタ
        private byte HumLsbAddr = 0xfe;
        private byte HumMsbAddr = 0xfd;
        private byte TmpXlsbAddr = 0xfc;
        private byte TmpLsbAddr = 0xfb;
        private byte TmpMsbAddr = 0xfa;
        private byte PreXlsbAddr = 0xf9;
        private byte PreLsbAddr = 0xf8;
        private byte PreMsbAddr = 0xf7;

        //データ校正用変数
        private Int32 t_fine = Int32.MinValue;

        //キャリブレーションデータアドレス
        enum Register : byte
        {
            dig_T1 = 0x88,
            dig_T2 = 0x8a,
            dig_T3 = 0x8c,

            dig_P1 = 0x8e,
            dig_P2 = 0x90,
            dig_P3 = 0x92,
            dig_P4 = 0x94,
            dig_P5 = 0x96,
            dig_P6 = 0x98,
            dig_P7 = 0x9a,
            dig_P8 = 0x9c,
            dig_P9 = 0x9e,

            dig_H1 = 0xa1,
            dig_H2 = 0xe1,
            dig_H3 = 0xe3,
            dig_H4 = 0xe4,
            dig_H5 = 0xe5,
            dig_H6 = 0xe7,
        }

        enum Cmd : byte
        {
            READ_TMP = 0xfa,
            READ_PRE = 0xf7,
            READ_HUM = 0xfd,
        }

        public double TMP { get; set; }
        public double PRE { get; set; }
        public double HUM { get; set; }

        public ApplePiTemp()
        {
            this.TMP = 0;
            this.PRE = 0;
            this.HUM = 0;
        }

        ~ApplePiTemp()
        {
            TempSensor.Dispose();
            periodicTimer.Dispose();
        }

        public async Task InitTempSensorAsync()
        {
            if (LightningProvider.IsLightningEnabled)
            {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
            }
            var i2c = await I2cController.GetDefaultAsync();
            if (i2c == null)
            {
                TempSensor = null;
                return;
            }
            TempSensor = i2c.GetDevice(new I2cConnectionSettings(TEMP_SENSOR_ADR));
            
            //初期化
            uint osrs_t = 3;
            uint osrs_p = 3;
            uint osrs_h = 3;
            uint mode = 3;
            uint t_sb = 5;
            uint filter = 0;
            uint spi3w_en = 0;

            uint ctrlMeasReg = (osrs_t << 5) | (osrs_p << 2) | mode;
            uint configReg = (t_sb << 5) | (filter << 2) | spi3w_en;
            uint ctrlHumReg = osrs_h;

            TempSensor.Write(new byte[] { 0xf2, (byte)ctrlHumReg });
            TempSensor.Write(new byte[] { 0xf4, (byte)ctrlMeasReg });
            TempSensor.Write(new byte[] { 0xf5, (byte)configReg });
            await Task.Delay(10).ConfigureAwait(false);

            //温度
            T1 = ReadUint16((byte)Register.dig_T1);
            T2 = (Int16)ReadUint16((byte)Register.dig_T2);
            T3 = (Int16)ReadUint16((byte)Register.dig_T3);

            //気圧
            P1 = ReadUint16((byte)Register.dig_P1);
            P2 = (Int16)ReadUint16((byte)Register.dig_P2);
            P3 = (Int16)ReadUint16((byte)Register.dig_P3);
            P4 = (Int16)ReadUint16((byte)Register.dig_P4);
            P5 = (Int16)ReadUint16((byte)Register.dig_P5);
            P6 = (Int16)ReadUint16((byte)Register.dig_P6);
            P7 = (Int16)ReadUint16((byte)Register.dig_P7);
            P8 = (Int16)ReadUint16((byte)Register.dig_P8);
            P9 = (Int16)ReadUint16((byte)Register.dig_P9);

            //湿度
            H1 = ReadByte((byte)Register.dig_H1);
            H2 = (Int16)ReadUint16((byte)Register.dig_H2);
            H3 = ReadByte((byte)Register.dig_H3);
            H4 = (short)(ReadByte((byte)Register.dig_H4) << 4 | 
                ReadByte((byte)Register.dig_H4 + 1) & 0xf);
            H5 = (short)(ReadByte((byte)Register.dig_H5 + 1) << 4 | 
                ReadByte((byte)Register.dig_H5) >> 4);
            H6 = (sbyte)ReadByte((byte)Register.dig_H6);

            periodicTimer = new Timer(TimerCallBack, null, 0, 1000);

            await Task.Delay(50).ConfigureAwait(false);
        }

        new public string ToString()
        {
            StringBuilder SensorText = new StringBuilder();
            SensorText.Append("温度 " + this.TMP.ToString("F1") + "℃" + "\r\n");
            SensorText.Append("湿度 " + this.HUM.ToString("F1") + "％" + "\r\n");
            SensorText.Append("気圧 " + this.PRE.ToString("F1") + "hPa");
            return SensorText.ToString();
        }

        private async void TimerCallBack(object state)
        {
            this.TMP = await ReadTmpAsync();
            this.PRE = await ReadPreAsync();
            this.HUM = await ReadHumAsync();
        }

        private async Task<double> ReadHumAsync()
        {
            byte hmsb = ReadByte(HumMsbAddr);
            byte hlsb = ReadByte(HumLsbAddr);
            int humRaw = (hmsb << 8) | hlsb;

            Int32 H;
            H = t_fine - 76800;
            H = (((((humRaw << 14) - (((Int32)H4) << 20) - ((Int32)H5 * H)) +
                ((Int32)16384)) >> 15) * (((((((H * ((Int32)H6)) >> 10) * (((H * 
                ((Int32)H3)) >> 11) + ((Int32)32768))) >> 10) + ((Int32)2097152)) * 
                ((Int32)H2) + 8192) >> 14));
            H = (H - (((((H >> 15) * (H >> 15)) >> 7) * ((Int32)H1)) >> 4));
            H = (H < 0 ? 0 : H);
            H = (H > 419430400 ? 419430400 : H);

            await Task.Delay(1).ConfigureAwait(false);
            return (UInt32)((H >> 12) / 1000);
        }

        private async Task<double> ReadPreAsync()
        {
            if(t_fine == Int32.MinValue)
            {
                this.TMP = await ReadTmpAsync();
            }

            byte pmsb = ReadByte(PreMsbAddr);
            byte plsb = ReadByte(PreLsbAddr);
            byte pxlsb = ReadByte(PreXlsbAddr);

            Int32 preRaw = (pmsb << 12) | (plsb << 4) | (pxlsb >> 4);

            Int64 var1, var2, P;
            var1 = t_fine - 128000;
            var2 = var1 * var1 * (Int64)P6;
            var2 = var2 + ((var1 * (Int64)P5) << 17);
            var2 = var2 + ((Int64)P4 << 35);
            var1 = ((var1 * var1 * (Int64)P3) >> 8) + ((var1 * (Int64)P2) << 12);
            var1 = (((((Int64)1 << 47) + var1)) * (Int64)P1) >> 33;
            if (var1 == 0)
            {
                return 0;
            }

            P = 1048576 - preRaw;
            P = (((P << 31) - var2) * 3125) / var1;
            var1 = ((Int64)P9 * (P >> 13)) >> 25;
            var2 = ((Int64)P8 * P) >> 19;
            P = ((P + var1 + var2) >> 8) + ((Int64)P7 << 4);
            
            await Task.Delay(1).ConfigureAwait(false);
            return (double)(P / 256 / 100);
        }

        private async Task<double> ReadTmpAsync()
        {
            byte tmsb = ReadByte(TmpMsbAddr);
            byte tlsb = ReadByte(TmpLsbAddr);
            byte txlsb = ReadByte(TmpXlsbAddr);
            Int32 tmpRaw = (tmsb << 12) | (tlsb << 4) | (txlsb >> 4);

            double var1, var2, T;
            var1 = ((tmpRaw / 16384.0) - (T1 / 1024.0)) * T2;
            var2 = ((tmpRaw / 131072.0) - (T1 / 8192.0)) * T3;
            t_fine = (Int32)(var1 + var2);
            T = (var1 + var2) / 5120.0;
            await Task.Delay(1).ConfigureAwait(false);
            return T;
        }

        private byte ReadByte(byte register)
        {
            byte[] writeBuf = new byte[] { 0x00 };
            byte[] readBuf = new byte[] { 0x00 };
            writeBuf[0] = register;
            TempSensor.WriteRead(writeBuf, readBuf);

            return readBuf[0];
        }

        private UInt16 ReadUint16(byte register)
        {
            byte[] writeBuf = new byte[] { 0x00 };
            byte[] readBuf = new byte[] { 0x00, 0x00 };
            writeBuf[0] = register;
            TempSensor.WriteRead(writeBuf, readBuf);

            int h = readBuf[1] << 8;
            int l = readBuf[0];

            return (UInt16)(h + l);
        }


    }
}
