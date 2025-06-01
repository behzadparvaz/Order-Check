using OKEService.Core.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TapsiDOC.Order.Core.Domain.Orders.SeedWork;

namespace TapsiDOC.Order.Core.Domain.Orders.Entities
{
    public class SupplementaryInsuranceType : Enumeration
    {
        public static SupplementaryInsuranceType Non = new SupplementaryInsuranceType(0, "ندارد");
        public static SupplementaryInsuranceType Tamin = new SupplementaryInsuranceType(1, "بیمه تامین اجتماعی");
        public static SupplementaryInsuranceType KhadamatDarmani = new SupplementaryInsuranceType(2, "خدمات درمانی");
        public static SupplementaryInsuranceType KhadamatDarmaniNirouhayeMosalah = new SupplementaryInsuranceType(3, "خدمات درمانی نیروهای مسلح");
        public static SupplementaryInsuranceType TejaratBank = new SupplementaryInsuranceType(4, "بانک تجارت");
        public static SupplementaryInsuranceType Shahrdariha = new SupplementaryInsuranceType(5, "شهرداری ها");
        public static SupplementaryInsuranceType Komisaria = new SupplementaryInsuranceType(6, "کمیساریای عالی");
        public static SupplementaryInsuranceType Komiteh = new SupplementaryInsuranceType(7, "کمیته امداد امام خمینی");
        public static SupplementaryInsuranceType Foulad = new SupplementaryInsuranceType(8, "شرکت ملی فولاد ایران");
        public static SupplementaryInsuranceType RefahBank = new SupplementaryInsuranceType(9, "بانک رفاه");
        public static SupplementaryInsuranceType MaskanBank = new SupplementaryInsuranceType(10, "بانک مسکن");
        public static SupplementaryInsuranceType Mokhaberat = new SupplementaryInsuranceType(11, "شرکت مخابرات ایران");
        public static SupplementaryInsuranceType Havapeymayi = new SupplementaryInsuranceType(12, "هواپیمایی جمهوری اسلامی ایران");
        public static SupplementaryInsuranceType Mes = new SupplementaryInsuranceType(13, "شرکت صنایع مس ایران");
        public static SupplementaryInsuranceType Banader = new SupplementaryInsuranceType(14, "سازمان بنادر و کشتیرانی");
        public static SupplementaryInsuranceType SedaVaSima = new SupplementaryInsuranceType(15, "سازمان صدا و سیما");
        public static SupplementaryInsuranceType BankToseeSaderat = new SupplementaryInsuranceType(16, "بانک توسعه صادرات");
        public static SupplementaryInsuranceType SherkatNaft = new SupplementaryInsuranceType(17, "شرکت نفت");
        public static SupplementaryInsuranceType KeshavarziBank = new SupplementaryInsuranceType(18, "بانک کشاورزی");
        public static SupplementaryInsuranceType MarkaziBank = new SupplementaryInsuranceType(19, "بانک مرکزی ایران");
        public static SupplementaryInsuranceType SepahBank = new SupplementaryInsuranceType(20, "بانک سپه");
        public static SupplementaryInsuranceType MelliBank = new SupplementaryInsuranceType(21, "بانک ملی");
        public static SupplementaryInsuranceType SanatVaMadanBank = new SupplementaryInsuranceType(22, "بانک صنعت و معدن");
        public static SupplementaryInsuranceType SaderatBank = new SupplementaryInsuranceType(23, "بانک صادرات");
        public static SupplementaryInsuranceType Bonyad = new SupplementaryInsuranceType(24, "بنیاد مستضعفان و جانبازان");
        public static SupplementaryInsuranceType JahadKeshavarzi = new SupplementaryInsuranceType(25, "جهاد کشاورزی");
        public static SupplementaryInsuranceType SangAhan = new SupplementaryInsuranceType(26, "سنگ آهن");
        public static SupplementaryInsuranceType SazmanBehzisti = new SupplementaryInsuranceType(27, "سازمان بهزیستی");
        public static SupplementaryInsuranceType ZoghalSangAlborz = new SupplementaryInsuranceType(28, "ذغال سنگ البرز شرق");
        public static SupplementaryInsuranceType Ahangari = new SupplementaryInsuranceType(29, "شرکت آهنگری تراکتورسازی ایران");
        public static SupplementaryInsuranceType SazmanZendanha = new SupplementaryInsuranceType(30, "سازمان زندان ها");
        public static SupplementaryInsuranceType RikhteGari = new SupplementaryInsuranceType(31, "شرکت ریخته گری تراکتورسازی ایران");
        public static SupplementaryInsuranceType Arzi = new SupplementaryInsuranceType(32, "هیئت امنای ارزی");
        public static SupplementaryInsuranceType Tehran = new SupplementaryInsuranceType(33, "شرکت شهر سالم تهران");
        public static SupplementaryInsuranceType Kish = new SupplementaryInsuranceType(34, "توسعه کیش");
        public static SupplementaryInsuranceType RiasatJomhouri = new SupplementaryInsuranceType(35, "ریاست جمهوری");
        public static SupplementaryInsuranceType GostareshVaNosazi = new SupplementaryInsuranceType(36, "سازمان گسترش و نوسازی صنایع");
        public static SupplementaryInsuranceType Markaz = new SupplementaryInsuranceType(37, "مرکز پژوهش های مجلس");
        public static SupplementaryInsuranceType NaftFalat = new SupplementaryInsuranceType(38, "شرکت نفت فلات قاره ایران");
        public static SupplementaryInsuranceType Sandogh = new SupplementaryInsuranceType(39, "صندوق بازنشستگی شرکت ملی نفت");
        public static SupplementaryInsuranceType Kanoun = new SupplementaryInsuranceType(40, "کانون سر دفتران و دفتر یاران");
        public static SupplementaryInsuranceType Ketabkhane = new SupplementaryInsuranceType(41, "کتابخانه مجلس شورای اسلامی");
        public static SupplementaryInsuranceType Majles = new SupplementaryInsuranceType(42, "مجلس شورای اسلامی");
        public static SupplementaryInsuranceType Aseman = new SupplementaryInsuranceType(43, "هواپیمایی آسمان");
        public static SupplementaryInsuranceType PanzdahKhordad = new SupplementaryInsuranceType(44, "پانزده خرداد");
        public static SupplementaryInsuranceType Sarmad = new SupplementaryInsuranceType(45, "سرمد");
        public static SupplementaryInsuranceType Mihan = new SupplementaryInsuranceType(46, "میهن");
        public static SupplementaryInsuranceType SOS = new SupplementaryInsuranceType(47, "SOS کمک رسان ایران");
        public static SupplementaryInsuranceType AyandeSaz = new SupplementaryInsuranceType(48, "آینده ساز");
        public static SupplementaryInsuranceType Taavon = new SupplementaryInsuranceType(49, "تعاون");
        public static SupplementaryInsuranceType Kosar = new SupplementaryInsuranceType(50, "کوثر");
        public static SupplementaryInsuranceType Etekayi = new SupplementaryInsuranceType(51, "اتکایی ایرانیان");
        public static SupplementaryInsuranceType Asmari = new SupplementaryInsuranceType(52, "آسماری");
        public static SupplementaryInsuranceType Pasargad = new SupplementaryInsuranceType(53, "پاسارگاد");
        public static SupplementaryInsuranceType Dey = new SupplementaryInsuranceType(54, "دی");
        public static SupplementaryInsuranceType IranMoein = new SupplementaryInsuranceType(55, "ایران معین");
        public static SupplementaryInsuranceType Asia = new SupplementaryInsuranceType(56, "آسیا");
        public static SupplementaryInsuranceType Alborz = new SupplementaryInsuranceType(57, "البرز");
        public static SupplementaryInsuranceType Iran = new SupplementaryInsuranceType(58, "ایران");
        public static SupplementaryInsuranceType Parsian = new SupplementaryInsuranceType(59, "پارسیان");
        public static SupplementaryInsuranceType Sina = new SupplementaryInsuranceType(60, "سینا");
        public static SupplementaryInsuranceType KarAfarin = new SupplementaryInsuranceType(61, "کارآفرین");
        public static SupplementaryInsuranceType Moalem = new SupplementaryInsuranceType(62, "معلم");
        public static SupplementaryInsuranceType Novin = new SupplementaryInsuranceType(63, "نوین");
        public static SupplementaryInsuranceType Dana = new SupplementaryInsuranceType(64, "دانا");
        public static SupplementaryInsuranceType Mellat = new SupplementaryInsuranceType(65, "ملت");
        public static SupplementaryInsuranceType Saman = new SupplementaryInsuranceType(66, "سامان");
        public static SupplementaryInsuranceType Arman = new SupplementaryInsuranceType(67, "آرمان");
        public static SupplementaryInsuranceType Tosee = new SupplementaryInsuranceType(68, "توسعه");
        public static SupplementaryInsuranceType Omid = new SupplementaryInsuranceType(69, "امید");
        public static SupplementaryInsuranceType Razi = new SupplementaryInsuranceType(70, "رازی");
        public static SupplementaryInsuranceType Hafez = new SupplementaryInsuranceType(71, "حافظ");
        public static SupplementaryInsuranceType Azad = new SupplementaryInsuranceType(72, "آزاد");
        public static SupplementaryInsuranceType VezaratBehdasht = new SupplementaryInsuranceType(73, "وزارت بهداشت");
        public static SupplementaryInsuranceType AtiyeSazanHafez = new SupplementaryInsuranceType(74, "آتیه سازان حافظ");
        public static SupplementaryInsuranceType Ma = new SupplementaryInsuranceType(75, "ما");
        public static SupplementaryInsuranceType Sapop = new SupplementaryInsuranceType(76, "ساپوپ");
        public static SupplementaryInsuranceType GardeshgariSalamat = new SupplementaryInsuranceType(77, "گردشگری سلامت");
        public static SupplementaryInsuranceType Sobhan = new SupplementaryInsuranceType(78, "سبحان");
        public static SupplementaryInsuranceType EtekayiAmin = new SupplementaryInsuranceType(79, "اتکایی امین");


        public SupplementaryInsuranceType(int id, string name)
            : base(id, name) { }

        public static IEnumerable<SupplementaryInsuranceType> List() =>
                    new[] { Non, Ma, Dey, Asia, Kosar, Tamin,
                            KhadamatDarmani, KhadamatDarmaniNirouhayeMosalah,
                            TejaratBank,Shahrdariha,Komisaria,Komiteh,Foulad,
                            RefahBank,MaskanBank,Mokhaberat,Havapeymayi,
                            Mes,Banader,SedaVaSima,BankToseeSaderat,SherkatNaft,
                            KeshavarziBank,MarkaziBank,SepahBank,MelliBank,SanatVaMadanBank,
                            SaderatBank,Bonyad,JahadKeshavarzi,SangAhan,SazmanBehzisti,
                            ZoghalSangAlborz,Ahangari,SazmanZendanha,RikhteGari,Arzi,Tehran,
                            Kish,RiasatJomhouri,GostareshVaNosazi,Markaz,NaftFalat,Sandogh,
                            Kanoun,Ketabkhane,Majles,Aseman,PanzdahKhordad,Sarmad,Mihan,
                            SOS,AyandeSaz,Taavon,Etekayi,Asmari,Pasargad,IranMoein,Alborz,
                            Iran,Parsian,Sina,KarAfarin,Moalem,Novin,Dana,Mellat,Saman,Arman,
                            Tosee,Omid,Razi,Hafez,Azad,VezaratBehdasht,AtiyeSazanHafez,Sapop,
                            GardeshgariSalamat,Sobhan,EtekayiAmin };


        public static SupplementaryInsuranceType FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            var state = List().SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }

        public static SupplementaryInsuranceType From(int? id)
        {
            if (id == null)
                return null;
            var state = List().SingleOrDefault(s => s.Id == id);
            if (state == null)
                throw new InvalidValueObjectStateException($"Possible values for discount type: {String.Join(",", List().Select(s => s.Name))}");

            return state;
        }
    }
}
