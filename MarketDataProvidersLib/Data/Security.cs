using System;
using System.Runtime.Serialization;

namespace TroyStevens.Market.Data
{
    [KnownType(typeof(IMarketValue))]
    [DataContract]
    public class Security : IMarketValue
    {
        [DataMember]
        public string ProviderId { get; internal set; }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public double Value { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; private set; }

        public Security(string providerid, string name, double value)
        {
            ProviderId = providerid;
            Name = name;
            Value = value;
            TimeStamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{TimeStamp:u}\t{Name}\t{Value:C}\t{ProviderId}";;
        }
    }
        
    public interface IMarketValue
    {
        string ProviderId { get; }
        string Name { get; }
        double Value { get; }
    }
}
