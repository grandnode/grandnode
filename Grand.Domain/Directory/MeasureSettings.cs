using Grand.Domain.Configuration;

namespace Grand.Domain.Directory
{
    public class MeasureSettings : ISettings
    {
        public string BaseDimensionId { get; set; }
        public string BaseWeightId { get; set; }
    }
}