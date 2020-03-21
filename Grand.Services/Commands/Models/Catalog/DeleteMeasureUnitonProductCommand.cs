using MediatR;

namespace Grand.Services.Commands.Models.Catalog
{
    public class DeleteMeasureUnitOnProductCommand : IRequest<bool>
    {
        public string MeasureUnitId { get; set; }
    }
}
