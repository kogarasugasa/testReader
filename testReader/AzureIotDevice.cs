using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace MyastiaAzure
{
    public class AzureIotDevice
    {
        private readonly DeviceClient deviceClient;
        private readonly KeyValuePair<string, string> deviceProperty;
        private StringBuilder cnString = new StringBuilder();
        private readonly string HostName = "MyastiaIot.azure-devices.net";

        public AzureIotDevice(string pDeviceId,string pSharedAccessKey,string pDeviceType)
        {
            this.DeviceId = pDeviceId;
            deviceProperty = new KeyValuePair<string, string>("DeviceType", pDeviceType);

            cnString.Append("HostName=" + HostName + ";");
            cnString.Append("DeviceId=" + pDeviceId + ";");
            cnString.Append("SharedAccessKey=" + pSharedAccessKey);
            deviceClient = DeviceClient.CreateFromConnectionString(cnString.ToString(), TransportType.Http1);
        }
        ~AzureIotDevice()
        {
            deviceClient.Dispose();
        }
        //********************************************************************************
        //プロパティ
        //********************************************************************************
        public string ReceivedMessage { get; private set; } = string.Empty;
        public string DeviceId { get; private set; } = string.Empty;
        public bool ReceivingMessage { get; private set; } = false;
        public List<string> Error { get; private set; } = new List<string>();

        //********************************************************************************
        //メソッド
        //********************************************************************************
        public async Task SendMessage(string pTo,string pRelation)
        {
            try
            {
                var data = new GraphRecord();
                data.From = DeviceId;
                data.To = pTo;
                data.Relation = pRelation;

                var dataString = JsonConvert.SerializeObject(data);
                using (var message = new Message(Encoding.UTF8.GetBytes(dataString)))
                {
                    message.Properties.Add(deviceProperty);
                    await deviceClient.SendEventAsync(message);
                }
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                this.Error.Add(JsonConvert.SerializeObject(e));
            }
        }

        public void StopStartReceiveMessage()
        {
            this.ReceivingMessage = false;
        }

        public async Task StartReceiveMessage()
        {
            this.ReceivingMessage = true;
            try
            {
                while (this.ReceivingMessage)
                {
                    using (var message = await deviceClient.ReceiveAsync())
                    {
                        if (message == null)
                        {
                            await Task.Delay(5000);
                            continue;
                        }
                        var messageText = Encoding.UTF8.GetString(message.GetBytes());
                        await deviceClient.CompleteAsync(message);
                        this.ReceivedMessage = messageText;
                    }
                }
            }
            catch (Exception e)
            {
                this.Error.Add(JsonConvert.SerializeObject(e));
            }
        }
    }

    class GraphRecord
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Relation { get; set; } = string.Empty;
    }
}
