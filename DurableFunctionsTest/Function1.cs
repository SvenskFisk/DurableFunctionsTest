using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DurableFunctionsTest
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Before"));
            outputs.Add(await context.WaitForExternalEvent<string>("UserInteraction"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "After"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("Function1_Hello")]
        public static string SayHello([ActivityTrigger] string name, TraceWriter log)
        {
            log.Info($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            TraceWriter log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null);

            log.Info($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Function1_UserInteraction")]
        public static async Task<HttpResponseMessage> UserInteraction(
   [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
   [OrchestrationClient]DurableOrchestrationClient starter,
   TraceWriter log)
        {
            var instanceId = req.RequestUri.Query.Substring(1);
            await starter.RaiseEventAsync(instanceId, "UserInteraction", "banan");

            log.Info($"UserInteraction with ID = '{instanceId}'.");

            return req.CreateResponse();
        }
    }
}