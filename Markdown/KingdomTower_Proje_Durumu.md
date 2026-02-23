# 🏰 Kingdom Tower - Proje Durumu & Devam Rehberi

## 📖 Oyun Konsepti

**Kingdom Tower**, "Tower War" oyunundan ilham alan bir mobil strateji oyunu. Oyuncu kuleleri kontrol edip, drag-and-drop ile birliklerini düşman kulelere gönderiyor. Kulelerin HP'sine göre seviyesi değişiyor, üretim hızı artıyor ve görsel olarak büyüyorlar. Restoration (tamir) ve lojistik mekanikler ile orijinalinden farklılaşıyor.

**Platform:** Mobile (Unity + URP)
**Engine:** Unity 2022.3 LTS
**Dil:** C#

---

## 🏗️ Proje Mimarisi

### Script Dosyaları (`Assets/Scripts/`)

| Script | Görev |
|--------|-------|
| `GameManager.cs` | Singleton. Global oyun state'i, team yönetimi, victory/defeat conditions |
| `TowerController.cs` | Kule state machine, HP yönetimi, birim üretimi, sahiplik değişimi |
| `InputManager.cs` | Drag-and-drop touch/mouse input, bağlantı oluşturma |
| `UnitController.cs` | Birim hareketi, savaş mekaniği, hedefe yürüme |
| `TowerConnection.cs` | Kuleler arası bağlantı, çift yönlü flow, LineRenderer |
| `TowerStatsSO.cs` | ScriptableObject - kule istatistikleri (hız, üretim, max HP vs.) |
| `TeamData.cs` | Team enum tanımı (BlueTeam, RedTeam, Neutral) |

### Veri Yapısı
- **ScriptableObjects** kullanılıyor tüm stat tanımları için
- **Singleton pattern** GameManager'da
- **State Machine** TowerController'da kule durumları için

---

## ✅ Tamamlanan Görevler (TASK 1-12)

### TASK 1: HP-Based Generation Rate System ✅
- Kulenin HP'sine göre dinamik birim üretim hızı
- 0 HP = üretim yok
- 1-4 HP = 0.2x (tamir modu)
- 5-9 HP = 1.0x (Level 1)
- 10-19 HP = 1.5x (Level 2)
- 20+ HP = 2.0x (Level 3)
- `GetGenerationRateMultiplier()` metodu eklendi

### TASK 2: Tower State Enum & Visual Updates ✅
- `TowerState` enum: Destroyed, Repairing, Level1, Level2, Level3
- `UpdateTowerState()` metodu HP'ye göre state belirliyor
- `UpdateStateVisuals()` ile görsel geri bildirim

### TASK 3: currentUnits → currentHealth Birleştirme ✅
- `currentUnits` field'ı kaldırıldı
- Tüm referanslar `currentHealth` kullanıyor
- HP display (hpText) düzgün çalışıyor
- Max kapasite kontrolleri `stats.MaxUnits` ile

### TASK 4: Tower Visual Scale (Seviye Bazlı Büyüme) ✅
- Level 1 = 1.0x scale
- Level 2 = 1.3x scale
- Level 3 = 1.6x scale
- `Vector3.Lerp` ile smooth geçiş animasyonu
- `scaleTransitionSpeed` ayarlanabilir

### TASK 5: Connection Limits (HP-Based) ✅
- HP'ye göre maksimum bağlantı sayısı (1/2/3 line)
- Stratejik derinlik katıyor

### TASK 6: Send Rate Nerf ✅
- Birden fazla bağlantıda birim gönderim hızı dengelendi

### TASK 7: Connection-Aware Unit Generation ✅
- **Hiç bağlantı yok** → 3 saniyede 1 birim üretim
- **Sadece gelen bağlantı (savunma)** → 3 saniyede 1 birim üretim
- **Giden bağlantı (saldırı)** → Üretim DURUR
- `outgoingConnectionCount` field'ı ile tracking
- `OnConnectionCreatedFrom()` / `OnConnectionRemovedFrom()` public metotlar
- Oyun başlangıcında da idle üretim çalışıyor

### TASK 8: Bidirectional Connection Notification Fix ✅
- `AddReverseFlow()` artık her iki kuleyi de notify ediyor
- Çift yönlü bağlantılarda üretim durma edge-case düzeltildi

### TASK 9: Connection Invalidation Notification ✅
- Bağlantı geçersiz olduğunda kule notify ediliyor
- Kule üretimi tekrar başlayabiliyor

### TASK 10: AI Controller (Temel Düşman Zekası) ✅
- `AIController.cs` oluşturuldu (Red Team)
- 2 saniyede bir karar veriyor
- Zayıf kuleleri reinforce ediyor (HP < 10)
- En yakın + en zayıf hedefi seçiyor
- HP < 8 olan kule saldırmıyor
- LINQ ile temiz tower filtering

### TASK 11: InputManager Programmatic Connection ✅
- `CreateConnection()` public yapıldı
- AI, InputManager üzerinden bağlantı oluşturabiliyor
- Aynı kurallar (limit vs.) AI için de geçerli

### TASK 12: Victory/Defeat Detection ✅
- `CheckGameEnd()` metodu GameManager'a eklendi
- Tüm düşman kuleleri alınırsa → Victory
- Tüm oyuncu kuleleri kaybedilirse → Defeat
- Tower sahiplik değişiminde otomatik kontrol
- `isGameActive` flag ile oyun durma

---

## 🔧 Mevcut Bilinen Sorunlar / Eksikler

1. **Object Pooling yok** - Her birim için `Instantiate()` çağrılıyor, 50-100+ birimde performans sorunu olabilir
2. **Victory/Defeat UI yok** - Sadece console log var, ekranda gösterim yok
3. **Restart butonu yok** - Oyun bitince yeniden başlatma mekanizması eksik
4. **Speed control yok** - 1x/2x/3x hız değiştirme özelliği yok
5. **Particle effects eksik** - Tamir tozu, capture efekti, unit trail'leri yok
6. **Ses efektleri yok**

---

## 🎯 Sıradaki Yapılacaklar (Öncelik Sırasına Göre)

### Öncelik 1: Oyunu Tamamen Oynanabilir Yap
1. **Victory/Defeat UI Ekranı** - Kazanma/kaybetme pop-up'ı
2. **Restart Butonu** - Oyun bitince veya istediğinde yeniden başlat
3. **Pause/Speed Control** - 1x / 2x / 3x hız değiştirme

### Öncelik 2: Visual Polish
1. **Particle Effects** - Tower tamir tozu, capture efekti, unit trail
2. **Unit Animasyonları** - Yürüme, saldırı, ölüm
3. **Tower State Görselleri** - Destroyed → Repair → Level up geçişleri
4. **Connection Line Görselleri** - LineRenderer veya particle

### Öncelik 3: Stratejik Derinlik
1. **Valor (Cesaret) Sistemi** - Geçici buff, hasar x2, altın aura
2. **Özel Binalar:**
   - **Stable** → Hızlı Cavalry birimleri üretir
   - **Mage Tower** → Alan savunması (otomatik projectile)
   - **Church** → Yakındaki birimlere Valor buff'ı verir
   - **Dragon's Nest** → Neutral merkez hedef, ateş püskürtür
3. **Farklı Birim Tipleri** - Cavalry (hızlı), Archer (menzilli)
4. **Line Splitting** - Aynı anda birden fazla hedefe birim gönderme (zaten kısmen var)

### Öncelik 4: Oyun Döngüsü & İlerleme
1. **Level Progression** - Farklı harita düzenleri
2. **AI Zorluk Seviyeleri** - Easy / Medium / Hard
3. **RPG Title/Loadout Sistemi** - Oyun öncesi seçimler
4. **Tutorial** - Yeni oyuncu rehberi
5. **Menü Sistemi** - Ana menü, seviye seçimi
6. **Save/Load** - İlerleme kaydetme

### Öncelik 5: Performans & Mimari
1. **Object Pooling** - Unit Instantiate yerine pool
2. **Optimizasyon** - 100+ unit performansı
3. **Mobile URP** - Render pipeline optimizasyonu

---

## 💻 Teknik Notlar

### Kule HP Sistemi (Core Mechanic)
```
HP = 0       → Destroyed (üretim yok, harabe görünüm)
HP = 1-4     → Repairing (0.2x üretim, tamir efekti)
HP = 5-9     → Level 1 (1.0x üretim, temel kule)
HP = 10-19   → Level 2 (1.5x üretim, 1.3x scale)
HP = 20+     → Level 3 (2.0x üretim, 1.6x scale)
```

### Üretim Kuralları
```
Giden bağlantı VAR (saldırı)    → Üretim DURUR
Giden bağlantı YOK              → 3 saniyede 1 birim (idle)
Gelen bağlantı (savunma)        → 3 saniyede 1 birim (devam)
```

### Bağlantı Limitleri
```
HP bazlı max connection sayısı → 1 / 2 / 3
Birden fazla bağlantıda send rate nerf uygulanır
```

### AI Davranışı
```
Karar döngüsü: 2 saniyede bir
1. HP < 10 olan kendi kulelerini reinforce et
2. En yakın + en zayıf düşman/neutral kuleye saldır
3. HP < 8 olan kule saldırmaz
```

---

## 📂 Proje Klasör Yapısı (Önerilen)

```
Assets/
├── Scripts/
│   ├── GameManager.cs
│   ├── TowerController.cs
│   ├── InputManager.cs
│   ├── UnitController.cs
│   ├── TowerConnection.cs
│   ├── AIController.cs
│   ├── TowerStatsSO.cs
│   └── TeamData.cs
├── ScriptableObjects/
│   └── TowerStats/
├── Prefabs/
│   ├── Tower/
│   └── Unit/
├── Materials/
├── Scenes/
└── UI/
```

---

## 📌 İleri Tarih İçin Notlar

- **Blender 3D Modelleme** - Gameplay tamamlandıktan sonra placeholder'ları gerçek modellerle değiştir
- **Gameplay > Graphics** prensibi - Önce mekanikler, sonra görsellik
- **Claude Code Android** - Bilgisayar başında olmadığında async olarak küçük feature'lar ekletmek için kullanılabilir
- **Commercial Release** düşünülüyor - Asset, multiplayer ve monetization kararları ileride verilecek

---

*Son güncelleme: Ocak 2026 - TASK 1-12 tamamlandı, AI + Victory/Defeat eklendi*
