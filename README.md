Markdown
# ⚔️ Kelime Düellosu (Multiplayer Wordle Clone)

Kelime Düellosu, arkadaşlarınızla karşılıklı olarak oynayabileceğiniz, gerçek zamanlı (real-time) ve rekabetçi bir kelime tahmin oyunudur. C# .NET Core tabanlı SignalR mimarisi ile Next.js kullanılarak Full-Stack olarak geliştirilmiştir.

## 🚀 Özellikler

- **Gerçek Zamanlı Senkronizasyon:** SignalR WebSockets altyapısı sayesinde rakibinizin hamlelerini (harf renklerini) kelimeyi görmeden anlık olarak izleyin.
- **Dinamik Lobi ve Oda Sistemi:** İstediğiniz bir kullanıcı adı ve oda kodu belirleyerek arkadaşlarınızla özel odalarda eşleşin.
- **Rakibe Meydan Okuma:** Sadece sistemin verdiği kelimeleri değil, "Rakibe Özel Kelime Gönder" özelliği ile kendi seçtiğiniz 5-8 harfli kelimeleri rakibinize sorarak zorluk seviyesini artırın.
- **Zorluk Seçimi:** 5, 6, 7 ve 8 harfli rastgele kelime modlarıyla tek oyunculu veya çok oyunculu antrenmanlar yapın.
- **Canlı Etkileşim (Sataşma!):** Oyun esnasında rakibinize anlık mesajlar gönderin veya ekranda patlayan devasa emojiler (😎, 😢, 😂 vs.) ile rekabeti artırın.
- **Akıllı Sanal Klavye:** Harf tahminlerinize göre anında renk değiştiren (Yeşil, Sarı, Gri) tam Türkçe destekli QWERTY sanal klavye.

## 🛠️ Kullanılan Teknolojiler

**Frontend (Kullanıcı Arayüzü):**
- React & Next.js (App Router)
- Tailwind CSS (Responsive tasarım)
- TypeScript
- `@microsoft/signalr` (Client-side bağlantı)

**Backend (Sunucu Mimarisi):**
- C# .NET Core
- SignalR (Hub mimarisi)
- In-Memory Concurrent Dictionary (Oda ve durum yönetimi için)

## 🎮 Kurulum ve Çalıştırma

Projeyi lokalinizde çalıştırmak için aşağıdaki adımları izleyebilirsiniz:

### 1. Backend'i Başlatma
```bash
cd KelimeDuellosu.Backend
dotnet run
Sunucu varsayılan olarak http://localhost:5244 portunda çalışacaktır.

2. Frontend'i Başlatma
Bash
cd kelimeduellosu-frontend
npm install
npm run dev
Arayüz http://localhost:3000 adresinde ayağa kalkacaktır.

🌍 Uzaktan Oynama (Tunneling)
Projeyi farklı ağlardaki cihazlarla (Örn: arkadaşınızın telefonuyla) oynamak için Cloudflare Tunnels (veya Ngrok) kullanabilirsiniz.

Backend için: npx cloudflared tunnel --url http://127.0.0.1:5244

Frontend için: npx cloudflared tunnel --url http://127.0.0.1:3000
(Frontend page.tsx içerisindeki BACKEND_URL değişkenini Cloudflare backend linki ile değiştirmeyi unutmayın).

💡 Projenin Amacı ve Geliştirme Süreci
Bu proje, modern web teknolojilerindeki WebSocket mimarisi pratiğini geliştirmek, state yönetimini optimize etmek ve iki farklı uygulamanın (Next.js & .NET) birbiriyle kusursuz haberleşmesini sağlamak amacıyla geliştirilmiştir.
