using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class DeclineType : Enumeration
    {
        public static DeclineType Non = new DeclineType(0, " ");
        public static DeclineType OutofStock = new DeclineType(1, "عدم موجودی دارو");
        public static DeclineType NotActive = new DeclineType(2, "عدم فعالیت داروخانه");
        public static DeclineType NoRegister = new DeclineType(3, "عدم ثبت نام بیمارداروی خاص");
        public static DeclineType OutofBounds = new DeclineType(4, "خارج از محدود بایکر");
        public static DeclineType InvalidInsurance = new DeclineType(5, "بیمه شما غیر فعال شده است");
        public static DeclineType InsuranceTime = new DeclineType(6, "فرا نرسیدن زمان تایید بیمه");
        public static DeclineType InvalidOrderCode = new DeclineType(7, "کدرهگیری اشتباه است");
        public static DeclineType DrugRare = new DeclineType(8, "عدم موجودی_داروی کمیاب");
        public static DeclineType DrugNonDistribution = new DeclineType(9, "عدم موجودی_عدم توزیع دارو");
        public static DeclineType OutofStockInThisPharmacy = new DeclineType(10, "عدم موجودی دارو - در این داروخانه موجود نیست");
        public static DeclineType OutofStockInMarket = new DeclineType(11, "عدم موجودی دارو - عدم توزیع در بازار");
        public static DeclineType NeedRX = new DeclineType(12, "دارو نیاز به نسخه دارد");
        public static DeclineType NationalCodeDoesNotMatch = new DeclineType(13, "عدم تطابق کد ملی با نسخه");
        public static DeclineType WrongReferenceNumber = new DeclineType(14, "کد پیگیری نسخه اشتباه");
        public static DeclineType InsuranceSystem = new DeclineType(15, "قطع بودن سامانه بیمه");
        public static DeclineType Insurance = new DeclineType(16, "غیرفعال بودن بیمه بیمار");
        public static DeclineType NeedFile = new DeclineType(17, "دارو خاص نیاز به تشکیل پرونده");



        public static DeclineType OtherReason = new DeclineType(18, "سایر دلایل");


        public static DeclineType HighShippingCost = new DeclineType(19, "هزینه ارسال زیاد است");
        public static DeclineType HighOrderCost = new DeclineType(20, "هزینه سفارش بیش از حد انتظار من است");
        public static DeclineType HighOrderDeliveryTime = new DeclineType(21, "مدت زمان تحویل سفارش زیاد است");
        public static DeclineType TakeFromPharmacyInPerson = new DeclineType(22, "می خواهم دارو را از داروخانه حضوری بگیرم");
        public static DeclineType AnotherWay = new DeclineType(23, "از طریق دیگری خریداری کردم");
        public static DeclineType StoppedBuying = new DeclineType(24, "از خرید منصرف شدم");
        public static DeclineType DiscontinuingPurchase = new DeclineType(25, "می خواهم تغییراتی در سفارش خود ایجاد کنم");
        public static DeclineType HighOrderAcceptTime = new DeclineType(26, "زمان زیادی برای تایید سفارش منتظر بودم");
        public static DeclineType PriceProduct = new DeclineType(27, "مشخص نبودن قیمت محصولات");
        


        public static DeclineType CancelBeforSendOrder = new DeclineType(28, "نصراف کاربر قبل از ارسال سفارش-بیمار");
        public static DeclineType NonPayment = new DeclineType(29, "عدم پرداخت کاربر-بیمار");
        public static DeclineType SpecificPatient = new DeclineType(30, "ثبت داروی بیماران خاص در داروخانه عادی-بیمار");
        public static DeclineType TechnicalProblem = new DeclineType(31, "اختلالات فنی-بیمار");
        public static DeclineType UserCancellationOfOrderFee = new DeclineType(32, "انصراف کاربر بابت هزینه سفارش-بیمار");
        public static DeclineType OutsideServiceArea = new DeclineType(33, "خارج از محدوده سرویس دهی-بیمار");
        public static DeclineType OutOfStockPharmacy = new DeclineType(34, "عدم موجودی دارو - داروخانه");
        public static DeclineType PharmacyErrorInAcceptingOrder = new DeclineType(35, "خطای داروخانه در پذیرش سفارش-داروخانه");
        public static DeclineType PriceMiscalculation = new DeclineType(36, "محاسبه اشتباه قیمت-داروخانه");
        public static DeclineType TheTrackingCodeIncorrect = new DeclineType(37, "کد رهگیری اشتباه است-داروخانه");
        public static DeclineType PharmacyInactivity = new DeclineType(38, "عدم فعالیت داروخانه-داروخانه");
        public static DeclineType PharmacyProblem = new DeclineType(39, "اختلال فنی و زیرساخت داروخانه-داروخانه");
        public static DeclineType OtherReasonCallCenter = new DeclineType(40, "سایر دلایل");

        public static DeclineType SystemRejectDueToNonAcceptance = new DeclineType(68, "لغو سیستمی به علت عدم پذیرش داروخانه ها");
        public static DeclineType SystemRejectDueToNonPayment = new DeclineType(74, "لغو سیستمی به علت عدم پرداخت ");
        public static DeclineType SystemReject = new DeclineType(79, "لغو سیستمی");

        public static DeclineType CancelOrderbyPharmecy = new DeclineType(80, "عدم پذیرش سفارش توسط داروخانه ها");



        public DeclineType(int id, string name)
            : base(id, name) { }

        public static IEnumerable<DeclineType> GetFilteredDeclineTypes() => new[] {
            HighShippingCost, StoppedBuying, HighOrderCost, HighOrderAcceptTime, DiscontinuingPurchase, TakeFromPharmacyInPerson, AnotherWay, OtherReasonCallCenter
        };

        public static IEnumerable<DeclineType> List() =>
                    new[] { Non, OutofStock, NotActive, NoRegister, OutofBounds , InvalidInsurance , InsuranceTime , InvalidOrderCode ,DrugRare ,DrugNonDistribution ,
                        OutofStockInThisPharmacy ,OutofStockInMarket ,NeedRX ,NationalCodeDoesNotMatch ,WrongReferenceNumber ,
                        InsuranceSystem , Insurance , NeedFile
                    , HighShippingCost , HighOrderCost, HighOrderDeliveryTime ,TakeFromPharmacyInPerson ,AnotherWay,StoppedBuying ,DiscontinuingPurchase,HighOrderAcceptTime , PriceProduct ,OtherReason ,
                     CancelBeforSendOrder,NonPayment,SpecificPatient,TechnicalProblem,UserCancellationOfOrderFee,OutsideServiceArea,
                    OutOfStockPharmacy,PharmacyErrorInAcceptingOrder,PriceMiscalculation,TheTrackingCodeIncorrect,PharmacyInactivity,
                    PharmacyProblem,OtherReasonCallCenter , SystemRejectDueToNonAcceptance ,SystemRejectDueToNonPayment,SystemReject };

        public static DeclineType FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            var state = List().SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }

        public static DeclineType From(int id)
        {
            //if (id == null)
            //    return null;
            var state = List().SingleOrDefault(s => s.Id == id);
            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for decline type: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }
    }
}
