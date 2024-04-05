using SoFitasGanharCardsJob.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace SoFitasGanharCardsJob
{
    public class SankhyaService
    {
        private readonly HttpClient _httpClient;
        private string _authToken;

        public SankhyaService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("token", configuration.GetValue<string>("SankhyaToken"));
            _httpClient.DefaultRequestHeaders.Add("appkey", configuration.GetValue<string>("SankhyaAppKey"));
            _httpClient.DefaultRequestHeaders.Add("username", configuration.GetValue<string>("SankhyaUserName"));
            _httpClient.DefaultRequestHeaders.Add("password", configuration.GetValue<string>("SankhyaUserPassword"));
            Console.WriteLine($"token: {configuration.GetValue<string>("SankhyaToken")}");
            Console.WriteLine($"appkey: {configuration.GetValue<string>("SankhyaAppKey")}");
            Console.WriteLine($"username: {configuration.GetValue<string>("SankhyaUserName")}");
            Console.WriteLine($"password: {configuration.GetValue<string>("SankhyaUserPassword")}");
        }

        public char? GetSankhyaStatus(int quoteNumber)
        {
            try
            {
                CheckIfTokenExists();

                string url = "https://api.sankhya.com.br/gateway/v1/mge/service.sbr?serviceName=CRUDServiceProvider.loadRecords&outputType=json";

                string body = "{'serviceName': 'CRUDServiceProvider.loadRecords', 'requestBody': {'dataSet': {'rootEntity': 'CabecalhoNota','includePresentationFields': 'S','offsetPage': '0','criteria': {'expression': {'$': '(this.NUNOTA= ?)'},'parameter': {'$': " + quoteNumber + ",'type': 'N'}},'entity': {'fieldset': {'list': 'PENDENTE'}}}}}";

                var requesMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Content = new StringContent(body),
                };

                requesMessage.Headers.Add("Authorization", _authToken);

                var response = _httpClient.SendAsync(requesMessage).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine($"{DateTime.Now} Internal server error ao buscar proposta {quoteNumber} no Sankhya");
                    return 'S';
                }

                var result = response.Content.ReadAsStringAsync().Result;

                var jsonResult = JObject.Parse(result);
                Console.WriteLine($"jsonResult: {jsonResult.ToString()}");

                if (!IsNullOrEmpty(jsonResult.SelectToken("statusMessage")) && jsonResult.SelectToken("statusMessage").Value<string>().Equals("Não autorizado.", StringComparison.OrdinalIgnoreCase))
                {
                    GenerateToken();
                    return GetSankhyaStatus(quoteNumber);
                }

                if(jsonResult.SelectToken("responseBody.entities.total").Value<int>() == 0)
                {
                    Console.WriteLine($"{DateTime.Now} Proposta de número {quoteNumber} não encontrada no Sankhya");
                    return 'S';
                }

                var status = jsonResult.SelectToken("responseBody.entities.entity.f0.$").Value<char>();

                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"exceção no método GetSankhyaStatus: {ex.ToString()}");
                return null;
            }
        }

        private void GenerateToken()
        {
            var content = new StringContent(string.Empty);

            var response = _httpClient.PostAsync("https://api.sankhya.com.br/login", content).Result;

            var result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"login result: {result}");

            var token = JsonConvert.DeserializeObject<BearerTokenResponse>(result);

            _authToken = "Bearer " + token.bearerToken;
            Console.WriteLine($"_authToken: {_authToken}");
        }

        private void CheckIfTokenExists()
        {
            if (string.IsNullOrEmpty(_authToken))
                GenerateToken();
        }

        private bool IsNullOrEmpty(JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
}
