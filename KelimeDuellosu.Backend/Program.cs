using Microsoft.EntityFrameworkCore;
using KelimeDuellosu.Backend.Data;
using KelimeDuellosu.Backend.Services;
using KelimeDuellosu.Backend.Hubs;
using KelimeDuellosu.Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVİS VE VERİTABANI KAYITLARI
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<WordService>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientPermission", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true) // Geçici olarak tüm dünyadan gelen bağlantılara izin ver
              .AllowCredentials();
    });
});
var app = builder.Build();

// 2. MİDDLEWARE VE YÖNLENDİRMELER
app.UseCors("ClientPermission");
app.MapHub<GameHub>("/gamehub");

// 3. İNTERNETSİZ VE KALICI DEV KELİME HAVUZU YÜKLEMESİ (SEED İŞLEMİ)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // 1. MEVCUT KELİMELERİ KORUMA: Veritabanındaki kelimeleri hafızaya alıyoruz.
    var mevcutKelimeler = db.Words.Select(w => w.Content).ToHashSet();

    // 2. 500 KELİMELİK DEV SÖZLÜK (Sadece 5, 6, 7 ve 8 harfli)
    string[] devSozluk = {
        // Teknoloji, Yazılım ve İş Dünyası
        "YAZILIM", "BİLİŞİM", "KODLAMA", "KLAVYE", "BELLEK", "SİSTEM", "SUNUCU", "İSTEMCİ", 
        "MİMARİ", "KURULUM", "AYARLAR", "GÜVENLİK", "ŞİFRE", "KAYDET", "GÜNCEL", "SÜRÜM", 
        "PROJE", "TASARIM", "ARAYÜZ", "DONANIM", "BAĞLANTI", "VERİLER", "TABLO", "SORGULAR",
        "KİOSK", "SİPARİŞ", "FATURA", "MÜŞTERİ", "İŞLETME", "MAĞAZA", "MARKET", "REKLAM",
        "TİCARET", "ŞİRKET", "YATIRIM", "ÜRETİM", "TÜKETİM", "MALİYET", "KAZANÇ", "ZARAR",
        "HESAP", "MAKİNE", "MOTOR", "CİHAZ", "EKRAN", "KABLO", "İLETİŞİM", "TELEFON", "MESAJ",
        "SİNYAL", "ŞEBEKE", "ÇEVRİM", "DESTEK", "UZMAN", "ÇÖZÜM", "BAKIM", "ONARIM", "MODÜL",
        "BİLEŞEN", "SENTEZ", "ANALİZ",
        
        // Doğa, Uzay ve Çevre
        "ORMAN", "AĞAÇLAR", "YAPRAK", "TOPRAK", "ÇİÇEK", "BÖCEK", "GÜNEŞ", "YILDIZ", "BULUT", 
        "YAĞMUR", "RÜZGAR", "FIRTINA", "ŞİMŞEK", "YILDIRIM", "DENİZ", "DALGA", "SAHİL", "KUMSAL", 
        "OKYANUS", "KÖRFEZ", "LİMAN", "BOĞAZ", "ADALAR", "YARIMADA", "DAĞLAR", "TEPELER", "VADİLER", 
        "OVALAR", "YAYLA", "NEHİR", "IRMAK", "ŞELALE", "GÖLLER", "BARAJ", "HAVUZ", "ÇÖLLER", 
        "İKLİM", "MEVSİM", "İLKBAHAR", "SONBAHAR", "DOĞAL", "MANZARA", "KAMPÜS", "MEYDAN", "SOKAK", 
        "CADDE", "BULVAR", "KAVŞAK", "KÖPRÜ", "TÜNEL", "GEZEGEN", "EVREN", "KOZMOS", "BOŞLUK", 
        "DÜNYA", "GÖKYÜZÜ", "METEOR", "KRATER", "KUTUP", "EKVATOR",

        // İnsan, Duygular ve Özellikler
        "İNSAN", "ÇOCUK", "BEBEK", "KADIN", "ERKEK", "KARDEŞ", "AKRABA", "AİLELER", "TOPLUM", 
        "MİLLET", "DEVLET", "HALKLAR", "LİDER", "BAŞKAN", "YÖNETİM", "KANUN", "ADALET", "HUKUK", 
        "AHLAK", "VİCDAN", "SEVGİ", "SAYGI", "ŞEFKAT", "MERHAMET", "NEFRET", "KORKU", "CESARET", 
        "SEVİNÇ", "HÜZÜN", "NEŞELİ", "MUTLU", "UMUTLU", "GURUR", "KIVANÇ", "BAŞARI", "ZAFER", 
        "MAĞLUP", "GALİP", "ZENGİN", "FAKİR", "YENİDEN", "ESKİDEN", "GÜZEL", "ÇİRKİN", "BÜYÜK", 
        "KÜÇÜK", "UZUN", "KISA", "ŞİŞMAN", "ZAYIF", "GÜÇLÜ", "GÜÇSÜZ", "SAĞLAM", "HASSAS", 
        "ZEKİ", "AKILLI", "KURNAZ", "SAFDİL", "DÜRÜST", "YALANCI", "SAMİMİ", "CİDDİ", "KOMİK", 
        "EĞLENCE", "ŞAKACI", "SİNİRLİ", "SAKİN", "HUZUR", "BARIŞ", "SAVAŞ",

        // Ev, Eşyalar ve Araç Gereç
        "KOLTUK", "KANEPE", "YATAK", "YORGAN", "YASTIK", "BATTANİYE", "ÇARŞAF", "PERDE", "KİLİM", 
        "HALILAR", "DOLAP", "ÇEKMECE", "VİTRİN", "MASA", "SANDALYE", "SEHPA", "TABLO", "ÇERÇEVE", 
        "AYNALAR", "LAMBA", "FENER", "AVİZE", "TELEVİZYON", "RADYO", "HOPARLÖR", "MUTFAK", "BANYO", 
        "SALON", "KORİDOR", "BALKON", "TERAS", "BAHÇE", "GARAJ", "BODRUM", "ÇATI", "DUVAR", "TAVAN", 
        "ZEMİN", "PENCERE", "KAPILAR", "KİLİT", "ANAHTAR", "PASPAS", "SÜPÜRGE", "FIRÇA", "KÜREK", 
        "KOVALAR", "SÜNGER", "SABUN", "ŞAMPUAN", "DETERJAN", "HAVLU", "PEÇETE", "TABAK", "BARDAK", 
        "ÇATAL", "KAŞIK", "BIÇAK", "TENCERE", "TAVA",

        // Yiyecek, İçecek ve Mutfak
        "EKMEK", "ÇÖREK", "BÖREK", "SİMİT", "POĞAÇA", "MANTI", "MAKARNA", "PİLAV", "BULGUR", 
        "NOHUT", "FASULYE", "MERCİMEK", "BAMYA", "KABAK", "SOĞAN", "SARIMSAK", "TAVUK", "BALIK", 
        "YUMURTA", "PEYNİR", "ZEYTİN", "TEREYAĞI", "SÜT", "YOĞURT", "AYRAN", "KEFİR", "MEYVESUYU", 
        "KAHVE", "KAKAO", "ŞEKER", "REÇEL", "PEKMEZ", "TAHİN", "ÇORBA", "SALATA", "TATLI", "PASTA", 
        "ÇİKOLATA", "ŞEKERLEME", "LOKUM", "HELVA", "BAKLAVA", "KADAYIF", "KÜNEFE", "DONDURMA", 
        "BİSKÜVİ", "ÇEREZ", "BADEM", "CEVİZ", "FINDIK", "FISTIK", "LEBLEBİ", "ELMA", "ARMUT", 
        "ÇİLEK", "KİRAZ", "VİŞNE", "KARPUZ", "KAVUN",

        // Fiiller, Eylemler ve Durumlar
        "YAPMAK", "ETMEK", "GİTMEK", "GELMEK", "GÖRMEK", "BAKMAK", "ALMAK", "VERMEK", "SEVMEK", 
        "SAYMAK", "BİLMEK", "BULMAK", "ÇÖZMEK", "YAZMAK", "ÇİZMEK", "SİLMEK", "OKUMAK", "ANLAMAK", 
        "DİNLEMEK", "SÖYLEMEK", "KONUŞMAK", "SUSMAK", "GÜLMEK", "AĞLAMAK", "KOŞMAK", "YÜRÜMEK", 
        "DURMAK", "BEKLEMEK", "İZLEMEK", "GÖZLEMEK", "ARAMAK", "SORMAK", "BULUŞMAK", "GÖRÜŞMEK", 
        "AYRILMAK", "KAVUŞMAK", "BİRLEŞME", "DAĞILMAK", "KIRMAK", "YAPILMAK", "KURMAK", "YIKMAK", 
        "SÖKMEK", "TAKMAK", "AÇMAK", "KAPATMAK", "ÇEKMEK", "İTMEK", "VURMAK", "ATMAK", "TUTMAK", 
        "YAKALAMA", "BIRAKMAK", "SATMAK", "ALINMAK", "SATILMAK", "SEÇMEK", "SEVİLMEK", "DOĞMAK", "ÖLMEK",

        // Renkler, Vücut, Giyim ve Çeşitli
        "BEYAZ", "SİYAH", "KIRMIZI", "YEŞİL", "MAVİ", "SARIMSI", "TURUNCU", "PEMBE", "MORLUK", 
        "LACİVERT", "KAHVE", "GRİ", "PARLAK", "MATLIK", "KOYULUK", "AÇIKLIK", "GÖVDE", "BİLEK", 
        "DİRSEK", "TOPUK", "KİRPİK", "KAŞLAR", "GÖBEK", "BACAK", "AYAK", "ELLER", "PARMAK", "TIRNAK", 
        "OMUZ", "BOYUN", "BOĞAZ", "GÖZLER", "KULAK", "BURUN", "AĞIZ", "DUDAK", "DİŞLER", "SAÇLAR", 
        "SAKAL", "BIYIK", "GÖĞÜS", "KARIN", "SIRT", "DAMAR", "KEMİK", "KASLAR", "DERİ", "ELBİSE", 
        "GİYSİ", "GÖMLEK", "KAZAK", "HIRKA", "CEKET", "PALTO", "MONT", "KABAN", "PANTOLON", "ETEK", 
        "ŞORT", "ÇORAP", "ELDİVEN", "ATKILAR", "BERELER", "ŞAPKA", "KEMER", "KRAVAT", "MENDİL", 
        "CÜZDAN", "ÇANTA", "SAATLER",

        // Eğitim, Meslekler ve Sanat
        "OKUL", "SINIF", "KURSLAR", "DERSLER", "SINAV", "KARNE", "TATİL", "MEZUN", "DİPLOMA", "BURSLAR", 
        "ÖĞRENCİ", "ÖĞRETMEN", "MÜDÜR", "ASİSTAN", "PROFESÖR", "DOKTOR", "MÜHENDİS", "MİMAR", "AVUKAT", 
        "HAKİM", "SAVCI", "POLİS", "ASKER", "SUBAY", "MEMUR", "İŞÇİ", "PATRON", "ŞOFÖR", "PİLOT", 
        "KAPTAN", "GEMİCİ", "KASAP", "BAKKAL", "MANAV", "TERZİ", "BERBER", "KUAFÖR", "ATÖLYE", 
        "FABRİKA", "OFİSLER", "BÜROLAR", "ŞANTİYE", "HASTANE", "SAĞLIK", "SANAT", "RESİM", "MÜZİK", 
        "TİYATRO", "SİNEMA", "SAHNE", "PERDE", "OYUNCU", "ROLLER", "YÖNETMEN", "SENARYO", "KİTAP", 
        "DERGİ", "GAZETE", "ROMAN"
    };

    var eklenecekler = new List<Word>();

    foreach (var kelime in devSozluk)
    {
        // Güvenlik Duvarı: Kelime daha önce eklenmemiş mi? Uzunluğu 5-8 arası mı?
        if (!mevcutKelimeler.Contains(kelime) && kelime.Length >= 5 && kelime.Length <= 8)
        {
            eklenecekler.Add(new Word { 
                Content = kelime, 
                Length = kelime.Length 
            });
        }
    }

    // Eğer eklenecek yeni kelime varsa veritabanına bas
    if (eklenecekler.Any())
    {
        db.Words.AddRange(eklenecekler.DistinctBy(w => w.Content));
        db.SaveChanges();
        Console.WriteLine($"{eklenecekler.Count} YENİ kelime eklendi! Toplam kelime sayısı: {db.Words.Count()}");
    }
    else
    {
        Console.WriteLine($"Tüm kelimeler zaten mevcut. Toplam kelime sayısı: {db.Words.Count()}");
    }
}

// 4. UYGULAMAYI BAŞLAT
app.Run();