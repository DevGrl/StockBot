using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace StockBot
{
    public class Yahoo
    {
        public static async Task<double?> GetStockPriceAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            string url = $"http://finance.yahoo.com/d/quotes.csv?s={symbol}&f=sl1";
            string csv;

            using (WebClient client = new WebClient())
                csv = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);

            string line = csv.Split('\n')[0];
            string price = csv.Split(',')[1];

            double result;
            if (double.TryParse(price, out result))
                return result;            

            return null;
        }

    }
    
}