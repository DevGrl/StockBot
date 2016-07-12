using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace StockBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        static string lastStock = string.Empty;
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {                
                StockLUIS stLuis = await LUISStockClient.ParseUserInput(activity.Text);
                string strReturn = string.Empty;
                string strStock = activity.Text;
                
                if (stLuis.intents.Count() > 0)
                {
                    switch (stLuis.intents[0].intent)
                    {
                        case "StockPrice":
                            if (stLuis.entities.Count() > 0)
                            { 
                                strReturn = await GetStock(stLuis.entities[0].entity);
                                lastStock = stLuis.entities[0].entity;
                            }
                            else goto case "None";
                            break;
                        case "LastStockValue":
                            if (lastStock.Length > 0)
                                strReturn = await GetStock(lastStock);
                            else
                                strReturn = "There was no previous valid stock";
                            break;
                        case "None":
                            strReturn = await GetStock(stLuis.query);
                            //Need to fix this part. It doesn't set the lastStock value
                            break;
                        default:
                            break;
                    }
                }
                
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = activity.CreateReply(strReturn);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }//end if (activity.Type == ActivityTypes.Message)

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
    
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
                return message.CreateReply("Typing");
            }
            else if (message.Type == ActivityTypes.Ping)
            {
                return message.CreateReply("Ping");
            }      

            return null;
        }

        private async Task<string> GetStock(string strStock)
        {
            string strReturn = string.Empty;
            double? dblStock = await Yahoo.GetStockPriceAsync(strStock);            

            if (!(dblStock == null))
               strReturn = string.Format("Stock: {0}, Value: {1}", strStock.ToUpper(), dblStock);
            else
                strReturn = string.Format("Stock {0} doesn't appear to be valid", strStock);

            return strReturn;
        }
    }
}