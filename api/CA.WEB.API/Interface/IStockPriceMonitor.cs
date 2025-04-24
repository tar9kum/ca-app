
using CA.WEB.API.Model;
namespace CA.WEB.API.Interface
{
    public interface IStockPriceMonitor
    {
        Stock GetStock(int id);
        event EventHandler<Stock> StockUpdated;
        void StartUpdatingPrices(); // Add a Start method
        void StopUpdatingPrices();
    }
}