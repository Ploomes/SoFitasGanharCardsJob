namespace SoFitasGanharCardsJob.Models
{
    public class UpdateQuoteDTO
    {
        public List<UpdateOtherPropertieDTO> OtherProperties { get; set; }
    }

    public class UpdateOtherPropertieDTO
    {
        public string FieldKey { get; set; }
        public int IntegerValue { get; set; }
    }
}