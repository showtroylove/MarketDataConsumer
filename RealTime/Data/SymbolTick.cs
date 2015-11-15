using System;
using System.Windows.Media;
using TroyStevens.Market.Data;

namespace TroyStevens.Market.Client.Data
{
    public class SymbolTick
    {
        public SymbolTick(string symbol, double last)
        {
            Symbol = symbol;            
            Last = last;
            CurrentLast = last;
        }
        
        public string Symbol { get; private set; }
        
        private double last;
        public double Last
        {
            get { return last; }
            set 
            {                
                CurrentLast = last;
                last = Math.Abs(value); 
            }
        }

        public SolidColorBrush DeltaColor 
        {
            // See FR17 for more info
            get 
            {
                if (CurrentLast < Last)
                    return new SolidColorBrush(Colors.MediumSeaGreen);
                else if (CurrentLast > Last)
                    return  new SolidColorBrush(Colors.Red);
                else
                    return  new SolidColorBrush(Colors.Black);
            }
        }

        private double CurrentLast { get; set; }

        public static explicit operator SymbolTick(Security data)
        {
            return new SymbolTick(data.Name, data.Value);
        }
    }
}
