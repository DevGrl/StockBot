using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace StockBot
{
    public class Yahoo
    {

        public static double GetStockPriceAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                //      return null;
            }
            string url = $"http://finance.yahoo.com/webservice/v1/symbols/COALINDIA.NS/quote?format=json&view=detail";
            string csv;
            using (WebClient client = new WebClient())
            {
                //csv = await client.DownloadStringTaskAsync(url).ConfigureAwait();
                csv = client.DownloadString(url);
            }
            //string line = csv.Split('/n')[0];
            string price = csv.Split(',')[1];

            double result;
            if (double.TryParse(price, out result))
            {
                return result;
            }

            return 0;
        }

    }
    
}