using OKEService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TapsiDOC.Order.Core.Domain.Orders.CommonContract;
using TapsiDOC.Order.Core.Domain.Orders.DomainEvents;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class Order : AggregateRoot
    {
        public string Id { get; set; }
        public string? CancelReason { get; set; }
        public string OrderCode { get; set; }
        public string VendorCode { get; set; }
        public decimal TotalPrice { get; set; }
        public InsuranceType InsuranceType { get; set; }
        public SupplementaryInsuranceType SupplementaryInsuranceType { get; set; }
        public DiscountType DiscountType { get; set; }
        public Discount Discount { get; set; }
        public Delivery Delivery { get; set; }
        public decimal PackingPrice { get; set; }
        public string PrepartionTime { get; set; }
        public string AckTime { get; set; }
        public decimal TaxPrice { get; set; }
        public decimal Vat { get; set; } //  Value Added Tax Coefficient
        public decimal TaxCoefficient { get; set; } // Value Added Tax Percent (= vat*100)
        public decimal FinalPrice { get; set; }
        public string FromDeliveryTime { get; set; }
        public string ToDeliveryTime { get; set; }
        public PaymentType PaymentType { get; set; }
        public Customer Customer { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public List<Vendor> Vendors { get; set; }
        public Doctor Doctor { get; set; }
        public decimal Cash { get; set; }
        public decimal Online { get; set; }
        public decimal Delta { get; set; } // diffrent the amount was paid and addtional amount
        public string Comment { get; set; }
        public decimal SupervisorPharmacyPrice { get; set; }
        public DeclineType DeclineType { get; set; }
        public bool IsSpecialPatient { get; set; }
        public string CreateDateTimeOrder { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public Description Description { get; set; }
        public long CreateDateTimeId { get; set; }
        public string? VendorComment { get; set; }

        public bool HasReview { get; set; } = false;
        public List<Review> Reviews { get; set; } = [];

        public Order()
        {
            OrderDetails = new List<OrderDetail>();
            Vendors = new List<Vendor>();
            Delivery = new Delivery();
            Description = new Description();
        }



        private static Random random = new Random();


        public static string RandomString(int length)
        {
            const string chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static Order CreateOrder(string orderCode, int insuranceTypeId, int supplementaryInsuranceType,
                            string fromDeliveryTime, string toDeliveryTime,
                            string deliveryDate, bool isSpecialPatient,
                            PaymentType paymentType, string phoneNumber,
                            string nationalCode, double lat, double lon,
                            string valueAddress, string titleAddress, string HouseNumber,
                            decimal cash, decimal online, string customerName, string comment,
                            int homeUnit, string vendorCode = "",
                            string? alternateRecipientName = null,
                            string? alternateRecipientMobileNumber = null,
                            List<OrderDetail> orderDetails = null)
        {
            var customer = Customer.Create(phoneNumber, nationalCode, lat, lon,
                                        valueAddress, titleAddress, HouseNumber, homeUnit,
                                        customerName,
                                        alternateRecipientName,
                                        alternateRecipientMobileNumber
                                        );
            Delivery delivery = new();
            delivery = delivery.Create(0, deliveryDate);
            Doctor doctor = new();
            doctor = doctor.Create("", "02111111111", "");

            if (orderDetails == null || !orderDetails.Any())
                throw new ArgumentException("An order must contain at least one item.");

            return new()
            {
                OrderCode = orderCode,
                SupplementaryInsuranceType = SupplementaryInsuranceType.From(supplementaryInsuranceType),
                InsuranceType = InsuranceType.From(insuranceTypeId),
                TotalPrice = 0,
                DiscountType = DiscountType.Non,
                Delivery = delivery,
                IsSpecialPatient = isSpecialPatient,
                PackingPrice = 0,
                PrepartionTime = PersianDateTime(DateTime.Now, 0),
                AckTime = PersianDateTime(DateTime.Now, 30),
                CreateDateTimeOrder = PersianDateTime(DateTime.Now, 0),
                TaxPrice = 0,
                Vat = 0,
                TaxCoefficient = 0,
                FinalPrice = 0,
                FromDeliveryTime = fromDeliveryTime,
                ToDeliveryTime = toDeliveryTime,
                PaymentType = paymentType,
                OrderStatus = OrderStatus.Ack, //GetOrderStatus(orderDetails),
                Customer = customer,
                Cash = cash,
                Delta = 0,
                Doctor = doctor,
                Comment = comment,
                DeclineType = DeclineType.Non,
                Online = online,
                OrderDetails = orderDetails,
                VendorCode = vendorCode,
                CreateDateTime = DateTime.Now.ToString()
            };
        }


        public List<Vendor> SetVendor(List<VendorSelect> vendors)
        {
            Vendor vendor = new();

            if (vendors == null || vendors.Count() == 0)
            {
                vendor = vendor.Create("V00001", 35.715454, 51.4221, "داورخانه ایثار");
                Vendors.Add(vendor);
                return Vendors;
            }
            else
            {
                foreach (var item in vendors)
                {
                    if (item.VendorCode == null)
                    {
                        vendor = vendor.Create("V00001", 35.715454, 51.4221, "داروخانه ایثار");
                        Vendors.Add(vendor);
                        return Vendors;
                    }
                    else
                    {
                        vendor = vendor.Create(item.VendorCode, item.Latitude, item.Longitude, item.VendorName);
                        Vendors.Add(vendor);
                    }

                };
                return Vendors;
            }

        }

        public static OrderStatus GetOrderStatus(List<OrderDetail> orderDetails)
        {
            if (orderDetails == null || orderDetails.Count == 0)
            {
                throw new ArgumentException("OrderDetails cannot be null or empty.");
            }

            return orderDetails.Find(a => a.Type == OrderDetailType.RequestOrder) != null ? OrderStatus.Draft : OrderStatus.Ack;
        }
        public void SetDraftStatus()
        {
            OrderStatus = OrderStatus.Draft;
            AddEvent(new DraftEvent(OrderStatus.Draft, OrderCode));
        }

        public void SetAckStatus()
        {
            if (OrderStatus.Id == OrderStatus.Draft.Id)
            {
                OrderStatus = OrderStatus.Ack;
                AddEvent(new AckEvent(OrderStatus.Ack, OrderCode, "TestPharmacyCode"));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }

        public void SetReject() =>
            OrderStatus = OrderStatus.Reject;


        public void SetPickStatus()
        {
            if (OrderStatus.Id == OrderStatus.APay.Id || OrderStatus.Id == OrderStatus.NFC.Id)
            {
                OrderStatus = OrderStatus.Pick;
                AddEvent(new PickEvent(OrderStatus, OrderCode, VendorCode, TotalPrice));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }

        public void SetAPayStatus()
        {
            if (OrderStatus.Id == OrderStatus.Draft.Id || OrderStatus.Id == OrderStatus.Ack.Id
                    || OrderStatus.Id == OrderStatus.APay.Id || OrderStatus.Id == OrderStatus.Auction.Id)
            {
                OrderStatus = OrderStatus.APay;
                if (Vendors.Count != 0)
                    AddEvent(new APayEvent(OrderStatus, OrderCode, VendorCode, TotalPrice));
                else
                    AddEvent(new APayEvent(OrderStatus, OrderCode, VendorCode, TotalPrice));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }

        public void SetAuctionStatus()
        {
            if (OrderStatus.Id == OrderStatus.Draft.Id || OrderStatus.Id == OrderStatus.Ack.Id ||
              OrderStatus.Id == OrderStatus.APay.Id || OrderStatus.Id == OrderStatus.Auction.Id)
            {
                OrderStatus = OrderStatus.Auction;
                if (Vendors.Count != 0)
                    AddEvent(new AuctionEvent(OrderStatus, OrderCode, VendorCode, TotalPrice));
                else
                    AddEvent(new AuctionEvent(OrderStatus, OrderCode, VendorCode, TotalPrice));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }

        public void SetAcceptStatus()
        {
            if (OrderStatus.Id == OrderStatus.Pick.Id)
            {
                OrderStatus = OrderStatus.Accept;
                AddEvent(new AcceptEvent(OrderStatus, OrderCode, VendorCode));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }

        public void SetAWaitingDeliveryStatus()
        {
            //if (OrderStatus.Id == OrderStatus.Pick.Id)
            //{
                OrderStatus = OrderStatus.ADelivery;
                AddEvent(new ReceiveDeliveryEvent(OrderStatus, OrderCode, VendorCode));
            //}
            //else
            //    throw new InvalidDataException("Invalid Status.");
        }

        public void SetCancelVendorStatus(DeclineType declineType)
        {
            if (OrderStatus.Id != OrderStatus.ADelivery.Id
                && OrderStatus.Id != OrderStatus.SendDelivery.Id
                && OrderStatus.Id != OrderStatus.Deliverd.Id
                && OrderStatus.Id != OrderStatus.CancelCustomer.Id
                && OrderStatus.Id != OrderStatus.CancelVendor.Id)
            {
                OrderStatus = OrderStatus.CancelVendor;
                DeclineType = declineType;
                AddEvent(new ReceiveCancelVendorEvent(OrderStatus, OrderCode, VendorCode, declineType));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }

        public void SetCancelCustomerStatus()
        {
            if (OrderStatus.Id != OrderStatus.ADelivery.Id
                && OrderStatus.Id != OrderStatus.Pick.Id
                && OrderStatus.Id != OrderStatus.SendDelivery.Id
                && OrderStatus.Id != OrderStatus.Deliverd.Id)
            {
                OrderStatus = OrderStatus.CancelCustomer;
                AddEvent(new CancelCustomerEvent(OrderStatus, OrderCode, VendorCode));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }

        public void SetRejectStatus()
        {
            if (OrderStatus.Id != OrderStatus.ADelivery.Id
                && OrderStatus.Id != OrderStatus.SendDelivery.Id
                && OrderStatus.Id != OrderStatus.Deliverd.Id)
            {
                OrderStatus = OrderStatus.Reject;
                AddEvent(new CancelCustomerEvent(OrderStatus, OrderCode, VendorCode));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }


        public void SetNFCStatus()
        {
            if (OrderStatus.Id != OrderStatus.ADelivery.Id
                && OrderStatus.Id != OrderStatus.SendDelivery.Id
                && OrderStatus.Id != OrderStatus.Deliverd.Id
                && OrderStatus.Id != OrderStatus.CancelCustomer.Id
                && OrderStatus.Id != OrderStatus.CancelVendor.Id)
            {
                OrderStatus = OrderStatus.NFC;
                AddEvent(new NFCEvent(OrderStatus, OrderCode, VendorCode));
            }
            else
                throw new InvalidDataException("Invalid Status.");
        }

        public void SetDescription(string comment, string link, decimal totalPrice, decimal packingPrice,
                                    decimal supervisorPrice, decimal deliveryPrice)
        {
            this.Description = Description.Create(comment, link);
            this.Description.SetDescription(comment, link);
            this.TotalPrice = totalPrice;
            this.PackingPrice = packingPrice;
            this.Delivery.DeliveryPrice = deliveryPrice;
            this.SupervisorPharmacyPrice = supervisorPrice;
            this.FinalPrice = this.TotalPrice + this.PackingPrice + this.SupervisorPharmacyPrice + this.Delivery.DeliveryPrice;
        }

        public void SetVendorComment(string vendorComment)
        {
            this.VendorComment = vendorComment;
        }

        public void SetAckState() =>
            this.OrderStatus = OrderStatus.Ack;
        public void SetPickState() =>
                                this.OrderStatus = OrderStatus.Pick;

        public void SetAWaitingDeliveryState() =>
                        this.OrderStatus = OrderStatus.ADelivery;

        public void SetAcceptState() =>
                this.OrderStatus = OrderStatus.Accept;

        public void SetAPayState() =>
                            this.OrderStatus = OrderStatus.APay;

        public void SetAuctionState() =>
                                this.OrderStatus = OrderStatus.Auction;

        public void SetCancelCustomerState() =>
                                this.OrderStatus = OrderStatus.CancelCustomer;

        public void SetRejectState() =>
                          this.OrderStatus = OrderStatus.Reject;

        public void SetCancelVendorState() =>
                                this.OrderStatus = OrderStatus.CancelVendor;

        public void SetDeliveryPrice(decimal deliveryPrice, decimal deliveryDiscount) =>
            Delivery.SetPrice(deliveryPrice, deliveryDiscount);

        public void SetPickupDelivery()
                    => OrderStatus = OrderStatus.SendDelivery;

        public void SetDelivered()
                    => OrderStatus = OrderStatus.Deliverd;

        public void SetCancelBiker()
            => OrderStatus = OrderStatus.CancelBiker;

        public void SetPreprationTime()
            => PrepartionTime = PersianDateTime(DateTime.Now.AddHours(1), 0);

        public void SetPrice(decimal totalPrice, decimal packingPrice, decimal supervisorPharmacyPrice)
        {
            TotalPrice = totalPrice;
            PackingPrice = packingPrice;
            SupervisorPharmacyPrice = supervisorPharmacyPrice;
            //if (price <= 0)
            //    throw new InvalidDataException("price not valid");
            FinalPrice = TotalPrice + PackingPrice + Delivery.FinalPrice + SupervisorPharmacyPrice;
        }

        private static string PersianDateTime(DateTime date, int time)
        {
            PersianCalendar p = new PersianCalendar();
            return string.Format(@"{0:0000}/{1:00}/{2:00} {3:00}:{4:00}",
                p.GetYear(date.AddMinutes(time)),
                p.GetMonth(date.AddMinutes(time)),
                p.GetDayOfMonth(date.AddMinutes(time)),
                p.GetHour(date.AddMinutes(time)),
                p.GetMinute(date.AddMinutes(time)));
        }

        public void SetDelivery(string deliveryDate, string fromDeliveryTime, string toDeliveryTime)
        {
            Delivery.SetDeliveryTime(deliveryDate);
            FromDeliveryTime = fromDeliveryTime;
            ToDeliveryTime = toDeliveryTime;
        }

        public void CalDeliveryPriceRoyal(bool IsSchedule)
        {
            if (IsSchedule)
                Delivery.FinalPrice = 0;
            else
                Delivery.FinalPrice = 750000;
            FinalPrice = TotalPrice + PackingPrice + SupervisorPharmacyPrice + Delivery.FinalPrice;
            //if(Delivery.FinalPrice == 0)
            //{
            //    Delivery.FinalPrice = 750000;
            //    FinalPrice += Delivery.FinalPrice;
            //}
            //else
            //{
            //    FinalPrice -= 750000;
            //    Delivery.FinalPrice = 0;
            //    FinalPrice += Delivery.FinalPrice;
            //}
        }

    }
}
