using MediatR;
using System.Text.Json.Serialization;
using TapsiDOC.Order.Core.Domain.Orders.Entities;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateReview;

public class CreateReviewCommand : IRequest<bool>
{
    public string OrderCode { get; set; }
    public string VendorCode { get; set; }
    [JsonIgnore]
    public string? UserPhoneNumber { get; set; }
    public string? Comment { get; set; }
    public short Rating { get; set; }
    public List<SatisfactionType> SatisfactionTypes { get; set; } = [];
    public List<DissatisfactionType> DissatisfactionTypes { get; set; } = [];
}
