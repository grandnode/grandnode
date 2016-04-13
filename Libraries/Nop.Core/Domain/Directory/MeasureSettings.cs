
using Nop.Core.Configuration;

namespace Nop.Core.Domain.Directory
{
    public class MeasureSettings : ISettings
    {
        public string BaseDimensionId { get; set; }
        public string BaseWeightId { get; set; }
    }
}