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
        private static readonly PropertyChangedEventArgs StringLinePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(StringLine));
        private static readonly PropertyChangedEventArgs TempSensorTextPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(TempSensorText));

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

        public ViewModelMainPage()
        {
            stringLine = String.Empty;
            tempSensorText = String.Empty;
        }   
    }
}
