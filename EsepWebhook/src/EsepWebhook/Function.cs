using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            var eventData = JsonConvert.DeserializeObject<GitHubWebhookEventData>(input.Body);
            var issueUrl = eventData.Issue.HtmlUrl;

            // Post message to Slack
            var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");
            var slackMessage = new { text = $"New issue created: {issueUrl}" };
            var httpClient = new HttpClient();
            var response = httpClient.PostAsync(slackUrl, new StringContent(JsonConvert.SerializeObject(slackMessage))).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to post message to Slack. Status code: {response.StatusCode}");
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(new { message = "Success" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public class GitHubWebhookEventData
        {
            [JsonProperty("issue")]
            public IssueData Issue { get; set; }

            public class IssueData
            {
                [JsonProperty("html_url")]
                public string HtmlUrl { get; set; }
            }
        }
    }
}
