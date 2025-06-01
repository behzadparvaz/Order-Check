using MediatR;
using Microsoft.Extensions.Logging;
using OKEService.Utilities;
using TapsiDOC.Order.Core.Domain.Orders.Entities;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Core.ApplicationService.Orders.Commands.CreateReview;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, bool>
{
    private readonly ILogger<CreateReviewCommandHandler> logger;
    private readonly IOrderCommandRepository command;
    private readonly IOrderQueryRepository query;

    public CreateReviewCommandHandler(OKEServiceServices okeserviceServices,
        IOrderCommandRepository commandRepository,
         ILogger<CreateReviewCommandHandler> logger,
         IOrderQueryRepository orderQueryRepository)
    {
        this.logger = logger;
        this.query = orderQueryRepository;
        this.command = commandRepository;
    }
    public async Task<bool> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.Rating <= 2 && request.DissatisfactionTypes.Count == 0)
            throw new ArgumentException("Rating value less than 2 must be associated with dissatisfactionType  values");
        if (request.Rating > 2 && request.SatisfactionTypes.Count == 0)
            throw new ArgumentException("Rating value greater than 2 must be associated with satisfactionType values");

        if (request.Rating < 1 || request.Rating > 5)
            throw new ArgumentException("Rating value must be greater than 0 and less than or equal to 5");       

        var order = await this.query.FindOrderByVendor(request.OrderCode, request.VendorCode);

        if (order == null)
            throw new Exception("Order not found");

        if (order.HasReview)
            throw new Exception("Order already has a review!");

        //if (order.OrderStatus != OrderStatus.Deliverd)
        //    throw new Exception("Cannot add review to an order that is not delivered");

        var review = Review.Create(request.OrderCode,
            request.VendorCode,
            request.UserPhoneNumber,
            request.Comment,
            request.Rating,
            request.SatisfactionTypes,
            request.DissatisfactionTypes);

        await this.command.AddReview(review);

        return true;
    }
}
