using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LabelEditor.Translator
{
    /// <summary>
    /// Translates text using Azure Translator
    /// </summary>
    internal class Translator
    {
        public List<string> To { get; set; } = new List<string>();
        public string From { get; set; } = String.Empty;
        public string TextToTranslate { get; set; } = string.Empty;

        /// <summary>
        /// Build request route
        /// </summary>
        /// <returns>route as string</returns>
        private string BuildRoute()
        {
            StringBuilder route = new StringBuilder($"/translate?api-version=3.0&from={From}");
            foreach (string item in To)
            {
                route.Append($"&to={item}");
            }
            return route.ToString();
        }

        /// <summary>
        /// Perform the translation call
        /// </summary>
        /// <returns>Dictionary<"languageId","Translated Text"></returns>
        public async Task<Dictionary<string,string>> Translate()
        {
            Dictionary<string, string> translatedResult = new Dictionary<string, string>();

            // Input and output languages are defined as parameters.
            //string route = "/translate?api-version=3.0&from=en&to=fr&to=zu";
            string route = BuildRoute();
            //string textToTranslate = "I would really like to drive your car around the block a few times!";
            object[] body = new object[] { new { Text = TextToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(Settings.TranslationEndpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", Settings.TranslationKey);
                // location required if you're using a multi-service or regional (not global) resource.
                request.Headers.Add("Ocp-Apim-Subscription-Region", Settings.TranslationRegion);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                if (response.IsSuccessStatusCode != true)
                {
                    throw new Exception($"{response.StatusCode} {response.ReasonPhrase}");
                }
                // Read response as a string.
                string webResult = await response.Content.ReadAsStringAsync();
                JArray translatorResult = (JArray)JsonConvert.DeserializeObject(webResult);
                translatedResult = translatorResult
                    .First()
                    .SelectToken("translations")
                    .ToDictionary(x => x["to"].ToString(), x => x["text"].ToString());
            }

            return translatedResult;
        }
    }
}
