using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

namespace SelfIntroductoryBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                //Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");

                string intent = getIntent(activity.Text);
                string reply;

                switch (intent)
                {
                    case "intent_hello":
                        reply = "Hello! I am a chatbot. Ask me any questions about me.";
                        break;

                    case "intent_whoworkedonthisproject":
                        reply = "The idea was proposed by Jenil Shah but it has been to reality by Akshay Bhatnagar and Sunny Nagpal.";
                        break;

                    case "intent_technology":
                        reply = "Well, I use quite a few technologies, namely, Microsoft Bot Framework, LUIS, .Net, Neo4j, Node.JS.";
                        break;

                    case "intent_usp":
                        reply = "You can interact with me like a human. I understand only some English now but the more you talk to me, the smarter I will get.";
                        break;

                    case "intent_businessValue":
                        reply = "I can help answer a lot of routine and repititive questions so that you humans can focus on greater things. I can be used to create a 24/7 helpdesk for your project, HR, IT and any other smart idea you have.";
                        break;

                    case "intent_capabilities":
                        reply = "Currently, I can answer questions about myself but I am being trained to answer more questions.";
                        break;
                       
                    case "intent_bye":
                        reply = "So soon :(, I was enjoying the discussion!";
                        break;

                    default:
                        reply = "Sorry, I didn't understand you";
                        break;

                }



                Activity activityReply = activity.CreateReply(reply);
                await connector.Conversations.ReplyToActivityAsync(activityReply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
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
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private string createRequest(string queryString)
        {
            queryString = WebUtility.UrlEncode(queryString);
            string UrlRequest = "https://api.projectoxford.ai/luis/v1/application?id=347f5254-c18b-4252-8ef6-169740788605&subscription-key=0cabcb280003496a9b9c16f678d7dae6&q="+queryString ;            
            Console.WriteLine(UrlRequest);
            return (UrlRequest);
        }

        public string makeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));

                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(RootObject));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    RootObject jsonResponse
                    = objResponse as RootObject;
                    return jsonResponse.intents[0].intent;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private string getIntent(string text)
        {
            string requestUrl = createRequest(text);
            return makeRequest(requestUrl);
        }

    }

    public class Intent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class RootObject
    {
        public string query { get; set; }
        public List<Intent> intents { get; set; }
        public List<object> entities { get; set; }
    }
}