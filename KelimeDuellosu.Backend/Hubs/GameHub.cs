using Microsoft.AspNetCore.SignalR;
using KelimeDuellosu.Backend.Services;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

namespace KelimeDuellosu.Backend.Hubs
{
    public class GameHub : Hub
    {
        private readonly WordService _wordService;
        private static readonly ConcurrentDictionary<string, string> RoomTargets = new();

        public GameHub(WordService wordService)
        {
            _wordService = wordService;
        }

        public async Task JoinRoom(string roomId, string playerName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", "SİSTEM", $"{playerName} odaya katıldı!");

            if (!RoomTargets.ContainsKey(roomId))
            {
                await StartNewGame(roomId, 5); // Varsayılan 5 harfli başla
            }
        }

        // YENİ: Zorluk Seçimi / Yeni Rastgele Oyun Başlatma
        public async Task StartNewGame(string roomId, int length)
        {
            string? targetWord = await _wordService.GetRandomWordAsync(length);
            if (targetWord != null)
            {
                RoomTargets[roomId] = targetWord;
                await Clients.Group(roomId).SendAsync("WordLengthChanged", length);
                await Clients.Group(roomId).SendAsync("ReceiveMessage", "SİSTEM", $"Yeni oyun başladı! {length} harfli kelime seçildi.");
            }
        }

        // YENİ: Rakibe Özel Kelime Gönderme (Meydan Okuma)
        public async Task SetRoomWord(string roomId, string playerName, string customWord)
        {
            customWord = customWord.ToUpper();
            RoomTargets[roomId] = customWord;
            await Clients.Group(roomId).SendAsync("WordLengthChanged", customWord.Length);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", "SİSTEM", $"{playerName} yeni bir kelime gönderdi! ({customWord.Length} harfli)");
        }

        // GÜNCELLENDİ: Sözlük kontrolü kaldırıldı, deneme sayısı (attemptCount) eklendi
        public async Task SubmitGuess(string roomId, string playerName, string guess, int attemptCount)
        {
            if (RoomTargets.TryGetValue(roomId, out string? targetWord))
            {
                // Uzunluk kontrolü (Eğer 6 harfli kelimeye 5 harf girerse uyar)
                if (guess.Length != targetWord.Length) return;

                var result = _wordService.EvaluateGuess(guess, targetWord);
                await Clients.Group(roomId).SendAsync("ReceiveGuessResult", playerName, guess, result);

                bool isWinner = result.All(r => r.Color == "GREEN");
                if (isWinner)
                {
                    // Kazandı
                    await Clients.Group(roomId).SendAsync("GameOver", true, targetWord, playerName);
                    RoomTargets.TryRemove(roomId, out _);
                }
                else if (attemptCount >= 6)
                {
                    // 6 hakkı bitti ve bilemedi
                    await Clients.Group(roomId).SendAsync("GameOver", false, targetWord, playerName);
                    RoomTargets.TryRemove(roomId, out _);
                }
            }
        }

        public async Task SendMessage(string roomId, string playerName, string message)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", playerName, message);
        }
    }
}