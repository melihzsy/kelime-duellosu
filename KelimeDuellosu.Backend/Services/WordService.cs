using KelimeDuellosu.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace KelimeDuellosu.Backend.Services
{
    public class WordService
    {
        private readonly AppDbContext _db;

        public WordService(AppDbContext db)
        {
            _db = db;
        }

        // 1. OYUN BAŞLARKEN: Veritabanından belirtilen harf sayısına göre rastgele kelime seçer
        public async Task<string?> GetRandomWordAsync(int length)
        {
            var words = await _db.Words.Where(w => w.Length == length).ToListAsync();
            if (!words.Any()) return null;

            var random = new Random();
            int index = random.Next(words.Count);
            return words[index].Content;
        }

        // 2. KONTROL: Oyuncunun girdiği kelime gerçekten sözlüğümüzde (PostgreSQL) var mı?
        public async Task<bool> IsValidWordAsync(string word)
        {
            return await _db.Words.AnyAsync(w => w.Content == word.ToUpper());
        }

        // 3. OYUNUN BEYNİ: Tahmin edilen kelimedeki harflerin Sarı/Yeşil/Gri durumunu hesaplar
        public List<LetterResult> EvaluateGuess(string guess, string target)
        {
            guess = guess.ToUpper();
            target = target.ToUpper();

            // Sonuçları tutacağımız liste
            var result = new List<LetterResult>(new LetterResult[guess.Length]);
            
            // Hedef kelimedeki harflerin adetlerini sayalım (Aynı harften 2 tane varsa kafası karışmasın diye)
            var targetCharCounts = new Dictionary<char, int>();
            foreach (var c in target)
            {
                if (targetCharCounts.ContainsKey(c)) targetCharCounts[c]++;
                else targetCharCounts[c] = 1;
            }

            // 1. AŞAMA: Önce nokta atışı doğruları (YEŞİL) bulup işaretleyelim
            for (int i = 0; i < guess.Length; i++)
            {
                if (guess[i] == target[i])
                {
                    result[i] = new LetterResult { Letter = guess[i].ToString(), Color = "GREEN" };
                    targetCharCounts[guess[i]]--; // Bu harfi kullandık, sayısını düş
                }
            }

            // 2. AŞAMA: Kalan harfler için SARI (yanlış yer) ve GRİ (yok) kontrolü yapalım
            for (int i = 0; i < guess.Length; i++)
            {
                if (result[i] != null) continue; // Eğer Yeşilse zaten işimiz bitti, atla

                // Harf hedef kelimede varsa VE henüz o harfin hakkını tüketmediysek
                if (targetCharCounts.ContainsKey(guess[i]) && targetCharCounts[guess[i]] > 0)
                {
                    result[i] = new LetterResult { Letter = guess[i].ToString(), Color = "YELLOW" };
                    targetCharCounts[guess[i]]--; // Harfin hakkını düş
                }
                else
                {
                    // Ne yeşil olabildi ne de sarı. Demek ki bu harf kelimede yok (veya hakkı bitti).
                    result[i] = new LetterResult { Letter = guess[i].ToString(), Color = "GRAY" };
                }
            }

            return result;
        }
    }

    // Arayüze (Next.js) göndereceğimiz şablon
    public class LetterResult
    {
        public string Letter { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}