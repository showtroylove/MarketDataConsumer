using System.ServiceModel;
using TroyStevens.Market.Data;

namespace TroyStevens.Market.Providers
{
    [ServiceContract(Namespace = "TroyStevens.Market.Provider", SessionMode = SessionMode.Required,
                     CallbackContract = typeof(IMarketDataProviderCallback))]
    public interface IMarketDataProvider
    {
        [OperationContract(IsOneWay=true, IsInitiating = true)]
        void GetAllProviderEvents();
        [OperationContract(IsOneWay = true)]
        void EndSession();
    }
        
    public interface IMarketDataProviderCallback
    {
        [OperationContract(IsOneWay = true)]
        void PublishMarketData(Security data);
    }    
}
