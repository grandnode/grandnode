using Grand.Core.Configuration;

namespace Grand.Core.Domain.Directory
{
    public class MeasureSettings : ISettings
    {
        public string BaseDimensionId { get; set; }
        public string BaseWeightId { get; set; }
    }
}