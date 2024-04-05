using Microsoft.Extensions.Configuration;
using SoFitasGanharCardsJob.Models;

namespace SoFitasGanharCardsJob
{
    public class Program
    {
        private static PloomesService _ploomesService;
        private static SankhyaService _sankhyaService;

        static void Main(string[] args)
        {
            //var fullPath = AppContext.BaseDirectory;
            //var soFitasIndex = fullPath.LastIndexOf("\\SoFitas");
            //var directory = fullPath.Substring(0, soFitasIndex);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .SetBasePath("/secrets/")
                .Build();

            CreateServices(configuration);

            int skip = 0;
            int updatedQuotes = 0;

            var count = _ploomesService.GetQuotesCount();

            do
            {
                var quotes = _ploomesService.GetQuotes(skip);

                foreach (var quote in quotes)
                {
                    try
                    {
                        int quoteNumber = quote.OtherProperties.FirstOrDefault(x => x.FieldKey == "quote_82646380-BD59-4E52-856A-0E7E59A175B0").IntegerValue;
                    
                        char? status = _sankhyaService.GetSankhyaStatus(quoteNumber);
                        if (status == null)
                            return;
                        if (status != 'S')
                        {
                            Console.WriteLine($"{DateTime.Now} Proposta de Id {quote.Id} esta NÃO pendente no Sankhya e pendente no Ploomes");

                            var otherPropertie = quote.OtherProperties.FirstOrDefault(x => x.FieldKey == "quote_0DBB4865-D80B-4E2A-8072-2BE8C0A5D25C");

                            var updateQuote = new UpdateQuoteDTO { OtherProperties = new List<UpdateOtherPropertieDTO>(1) };

                            updateQuote.OtherProperties.Add(new UpdateOtherPropertieDTO
                            {
                                FieldKey = otherPropertie.FieldKey,
                                IntegerValue = 402187028
                            });

                            _ploomesService.UpdateQuoteStatus(quote.Id, updateQuote);

                            updatedQuotes++;

                            Console.WriteLine($"{DateTime.Now} Proposta de Id {quote.Id} atualizada no Ploomes!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now} ERRO com a proposta de Id {quote.Id}. Erro: {ex.ToString()}");
                    }
                }

                skip += 300;
            }
            while (skip <= count);

            Console.WriteLine($"Foram atualizadas {updatedQuotes} no Ploomes!");
        }

        private static void CreateServices(IConfiguration configuration)
        {
            _ploomesService = new PloomesService(configuration.GetValue<string>("PloomesUserKey"));
            _sankhyaService = new SankhyaService(configuration);
        }
    }
}