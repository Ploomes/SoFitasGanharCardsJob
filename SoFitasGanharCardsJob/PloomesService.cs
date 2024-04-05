using SoFitasGanharCardsJob.Models;
using Newtonsoft.Json;
using System.Net;

namespace SoFitasGanharCardsJob
{
    public class PloomesService
    {
        private readonly HttpClient _httpClient;

        public PloomesService(string userKey)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api2.ploomes.com/");
            _httpClient.DefaultRequestHeaders.Add("User-Key", userKey);
        }

        public List<Quote> GetQuotes(int skip)
        {
            try
            {
                string url = $"Quotes?$select=Id&$skip={skip}&$filter=OtherProperties/any(op: op/FieldKey+eq+'quote_0DBB4865-D80B-4E2A-8072-2BE8C0A5D25C'+and+op/ObjectValueName+eq+'Sim')+and+Deal/StatusId+ne+3&$expand=OtherProperties($filter=FieldKey+eq+'quote_0DBB4865-D80B-4E2A-8072-2BE8C0A5D25C'+or+FieldKey+eq+'quote_82646380-BD59-4E52-856A-0E7E59A175B0';$select=FieldKey,IntegerValue,ObjectValueName)";

                HttpStatusCode statusCode = HttpStatusCode.OK;
                HttpResponseMessage response = null;

                do
                {
                    response = _httpClient.GetAsync(url).Result;
                    statusCode = response.StatusCode;
                    if (statusCode is HttpStatusCode.TooManyRequests)
                        Thread.Sleep(2000);
                }
                while (statusCode is HttpStatusCode.TooManyRequests);

                var result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"result: {result.ToString()}");

                var quotes = JsonConvert.DeserializeObject<Basic<Quote>>(result);

                return quotes.value;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateQuoteStatus(int id, UpdateQuoteDTO otherPropertie)
        {
            try
            {
                string url = $"Quotes({id})";

                var body = JsonConvert.SerializeObject(otherPropertie);

                var content = new StringContent(body);
                HttpStatusCode statusCode = HttpStatusCode.OK;

                do
                {
                    var response = _httpClient.PatchAsync(url, content).Result;
                    statusCode = response.StatusCode;
                    if(statusCode is HttpStatusCode.TooManyRequests)
                        Thread.Sleep(2000);
                }
                while (statusCode is HttpStatusCode.TooManyRequests);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public int GetQuotesCount()
        {
            try
            {
                string url = "Quotes?$select=Id&$count=true&$filter=OtherProperties/any(op: op/FieldKey+eq+'quote_0DBB4865-D80B-4E2A-8072-2BE8C0A5D25C'+and+op/ObjectValueName+eq+'Sim')+and+Deal/StatusId+ne+3";

                HttpStatusCode statusCode = HttpStatusCode.OK;
                HttpResponseMessage response = null;

                do
                {
                    response = _httpClient.GetAsync(url).Result;
                    statusCode = response.StatusCode;
                    if (statusCode is HttpStatusCode.TooManyRequests)
                        Thread.Sleep(2000);
                }
                while (statusCode is HttpStatusCode.TooManyRequests);

                var result = response.Content.ReadAsStringAsync().Result;

                var count = JsonConvert.DeserializeObject<QuotesCount>(result).Count;

                return count;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }

    public record QuotesCount
    {
        [JsonProperty("@odata.count")]
        public int Count { get; set; }
    }
}