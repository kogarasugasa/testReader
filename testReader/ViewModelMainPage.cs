using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testReader
{
    public class ViewModelMainPage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelMainPage()
        {
            stringLine = String.Empty;
            tempSensorText = String.Empty;
        }

        private static readonly PropertyChangedEventArgs StringLinePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(StringLine));
        private string stringLine;
        public string StringLine
        {
            get => stringLine;
            set
            {
                stringLine = value;
                this.PropertyChanged?.Invoke(this, StringLinePropertyChangedEventArgs);
            }
        }

        private static readonly PropertyChangedEventArgs TempSensorTextPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(TempSensorText));
        private string tempSensorText;
        public string TempSensorText
        {
            get => tempSensorText;
            set
            {
                tempSensorText = value;
                this.PropertyChanged?.Invoke(this, TempSensorTextPropertyChangedEventArgs);
            }
        }

        private static readonly PropertyChangedEventArgs ReceivedMessagePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(ReceivedMessage));
        private string _ReceivedMessage;
        public string ReceivedMessage
        {
            get => _ReceivedMessage;
            set
            {
                _ReceivedMessage = value;
                this.PropertyChanged?.Invoke(this, ReceivedMessagePropertyChangedEventArgs);
            }
        }


        private static readonly PropertyChangedEventArgs ErrorMessagePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(ErrorMessage));
        private string _ErrorMessage;
        public string ErrorMessage
        {
            get => _ErrorMessage;
            set
            {
                _ErrorMessage = value;
                this.PropertyChanged?.Invoke(this, ErrorMessagePropertyChangedEventArgs);
            }
        }

    }
}
