namespace SoFitasGanharCardsJob.Models
{
    public class Basic<T>
    {
        public List<T> value { get; set; }
    }

    public class Quote
    {
        public int Id { get; set; }
        public List<OtherPropertie> OtherProperties { get; set; }
    }

    public class OtherPropertie
    {
        public string FieldKey { get; set; }
        public int IntegerValue { get; set; }
        public int? ObjectValueId { get; set; }
        public string? ObjectValueName { get; set; }

        public bool ShouldSerializeObjectValueName()
        {
            return false;
        }

        public bool ShouldSerializeIntegerValue()
        {
            return false;
        }
    }
}