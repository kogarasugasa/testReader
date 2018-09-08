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

        public ViewModelMainPage()
        {
            this.StringLine = String.Empty;
        }

        
    }
}
