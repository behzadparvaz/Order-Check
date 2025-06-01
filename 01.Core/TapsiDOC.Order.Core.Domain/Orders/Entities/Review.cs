using System;
using System.Collections.Generic;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities;

public class Review
{
    private Review() { }
    public string OrderCode { get; private set; }
    public string VendorCode { get; private set; }
    public string UserPhoneNumber { get; private set; }
    public string? Comment { get; private set; }
    public short Rating { get; private set; }
    public List<SatisfactionType> SatisfactionTypes { get; private set; } = [];
    public List<DissatisfactionType> DissatisfactionTypes { get; private set; } = [];
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Review Create(string orderCode, string vendorCode,
        string userPhoneNumber,
        string comment,
        short rating,
        List<SatisfactionType> satisfactionTypes,
        List<DissatisfactionType> dissatisfactionTypes)
    {
        return new Review()
        {
            Comment = comment,
            OrderCode = orderCode,
            VendorCode = vendorCode,
            Rating = rating,
            DissatisfactionTypes = dissatisfactionTypes,
            SatisfactionTypes = satisfactionTypes,
            CreatedAt = DateTime.Now
        };
    }
}

public enum SatisfactionType
{
    CorrectDrug = 1,
    InTimeDelivery = 2,
    ReasonablePrice = 3,
    ConsideringDescription = 4,
    BikerGoodAttitude = 5,
    GoodPacking = 6
}

public enum DissatisfactionType
{
    IncorrectDrug = 1,
    LateDelivery = 2,
    UnreasonablePrice = 3,
    IgnoringDescription = 4,
    BikerBadAttitude = 5,
    BadPacking = 6
} 
