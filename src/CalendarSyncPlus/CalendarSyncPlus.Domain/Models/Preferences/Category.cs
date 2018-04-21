using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    [DataContract]
    public class Category : Model
    {
        private string _hexValue;
        private string _categoryName;
        private string _colorNumber;

        [DataMember]
        public string HexValue { get => _hexValue; set => SetProperty(ref _hexValue, value); }
        [DataMember]
        public string CategoryName { get => _categoryName; set => SetProperty(ref _categoryName, value); }
        [DataMember]
        public string ColorNumber { get => _colorNumber; set => SetProperty(ref _colorNumber, value); }
    }
}