# 🏰 Kingdom Tower - Proje Durumu & Devam Rehberi

## 📖 Oyun Konsepti

**Kingdom Tower**, "Tower War" oyunundan ilham alan bir mobil strateji oyunu. Oyuncu kuleleri kontrol edip, drag-and-drop ile birliklerini düşman kulelere gönderiyor. Kulelerin HP'sine göre seviyesi değişiyor, üretim hızı artıyor ve görsel olarak büyüyorlar. Bağlantı hattı üzerinde karşılaşan düşman birimler 1-1 birbirini yok ediyor. Line cutting mekaniği ile bağlantılar kesilebiliyor.

**Platform:** Mobile (Unity + URP)
**Engine:** Unity 2022.3 LTS
**Dil:** C#

---

## 🏗️ Proje Mimarisi

### Script Dosyaları (`Assets/Scripts/`)

| Script | Görev |
|--------|-------|
| `GameManager.cs` | Singleton. Global oyun state'i, pause/resume, restart. Victory/Defeat henüz stub. |
| `TowerController.cs` | Kule state machine, HP yönetimi, idle HP regen, sahiplik değişimi, birim spawn |
| `InputManager.cs` | Drag-and-drop input, connection oluşturma/yönetimi, line cutting, connection limits |
| `UnitController.cs` | Birim hareketi (MoveTowards), düşman collision (1v1 yok etme), hedefe varınca TakeDamage |
| `TowerConnection.cs` | Kuleler arası bağlantı, çift yönlü flow, LineRenderer görselleştirme |
| `TowerStatsSO.cs` | ScriptableObject — kule istatistikleri (hız, üretim, max HP vs.) |
| `TeamData.cs` | Team enum tanımı (BlueTeam, RedTeam, Neutral) |
| `LookAtCamera.cs` | HP text'in kameraya bakmasını sağlayan yardımcı script |

### Veri Yapısı
- **ScriptableObjects** kullanılıyor tüm statik stat tanımları için
- **Singleton pattern** sadece GameManager'da
- **State Machine** TowerController'da kule durumları için (enum + switch)
- **New Input System** touch ve mouse desteği için

---

## ✅ Tamamlanan Görevler (TASK 1-9)

### TASK 1: HP-Based Generation Rate System ✅
- Kulenin HP'sine göre dinamik birim üretim hızı
- 0 HP = üretim yok
- 1-4 HP = Repairing state
- 5-9 HP = Level 1
- 10-19 HP = Level 2
- 20+ HP = Level 3
- Giden bağlantı varken üretim durur, yoksa idle regen (3 saniyede 1 HP)

### TASK 2: Tower State Enum & Visual Updates ✅
- `TowerState` enum: Destroyed, Repairing, Level1, Level2, Level3
- `UpdateTowerState()` metodu HP'ye göre state belirliyor
- `UpdateStateVisuals()` ile scale değişimi

### TASK 3: currentUnits → currentHealth Birleştirme ✅
- `currentUnits` field'ı kaldırıldı
- Tüm referanslar `currentHealth` kullanıyor
- HP display (TextMeshPro hpText) düzgün çalışıyor

### TASK 4: Tower Visual Scale (Seviye Bazlı Büyüme) ✅
- Destroyed = 0.8x, Repairing = 0.9x
- Level 1 = 1.0x, Level 2 = 1.3x, Level 3 = 1.6x
- `Vector3.Lerp` ile smooth geçiş animasyonu

### TASK 5: Connection Limits (HP-Based) ✅
- HP ≤ 10 → max 1 bağlantı
- HP ≤ 30 → max 2 bağlantı
- HP > 30 → max 3 bağlantı

### TASK 6: Send Rate Nerf ✅
- 2 bağlantıda 1.15x interval (15% yavaş)
- 3+ bağlantıda 1.35x interval (35% yavaş)

### TASK 7: Connection-Aware Unit Generation ✅
- Giden bağlantı VAR → Üretim DURUR
- Giden bağlantı YOK → 3 saniyede 1 HP idle regen
- `activeOutgoingConnections` field'ı ile tracking

### TASK 8: Bidirectional Connection ✅
- Aynı iki kule arasında çift yönlü flow desteği
- `AddReverseFlow()` ile mevcut connection'a ters yön ekleme
- Gradient line renkleri (A takım → B takım)

### TASK 9: Line Cutting Mechanic ✅
- Boş alana dokunup sürükleyerek bağlantı hattı kesme
- 2D line intersection algoritması
- TrailRenderer ile görsel cut efekti
- Kesilen bağlantılarda tower notify (`RemoveOutgoingConnection`)

### Ek: Unit Collision (1v1 Yok Etme) ✅
- Farklı takım unit'leri karşılaştığında ikisi de yok oluyor
- `Physics.OverlapSphere` ile collision detection
- Stratejik savunma mekaniği sağlıyor

---

## 🐛 Bilinen Hatalar & Teknik Borç

### Aktif Bug'lar
1. ~~**Connection Invalidation Notify Eksik** — TASK 16 ile düzeltildi (Şubat 2026).~~
2. **Material Instance Leak** — Her `meshRenderer.material.color` ataması yeni material instance oluşturuyor (`TowerController` ve `UnitController`). `MaterialPropertyBlock` kullanılmalı.

### Performans Riskleri
3. **OverlapSphere Her Frame** — `UnitController.CheckEnemyCollision()` her unit için her frame `Physics.OverlapSphere` çağırıyor. 50+ unit'te ciddi performans sorunu. Optimizasyon: aynı connection line üzerindeki unit'leri kontrol et veya spatial hashing kullan.
4. **Object Pooling Yok** — Her unit `Instantiate()` ile spawn, `Destroy()` ile siliniyor. GC spike'lara neden olur.

### Mimari Sorunlar
5. **InputManager Aşırı Sorumluluk** — Hem input, hem connection management, hem line cutting, hem connection limits. Single Responsibility ihlali. İleride ayrılmalı (InputHandler, ConnectionManager, CutManager).
6. **Event System Yok** — Tüm iletişim direkt referanslarla. Yeni sistem eklemek (UI, ses, particle) mevcut kodu değiştirmeyi gerektiriyor.
7. **GameManager Pasif** — Hiçbir sistem GameManager'ı çağırmıyor. Victory/defeat stub'ları boş.
8. **CreateConnection() Private** — `InputManager.CreateConnection()` private metot. AI Controller yazıldığında programatik connection oluşturmak için ya public yapılmalı ya da event/interface üzerinden erişim sağlanmalı.

### Bilinen Edge Cases (Test Edilmedi)
8. **Unit hedefe giderken kule el değiştirirse** — Unit eski hedefine TakeDamage yapar, yeni sahip takıma hasar verebilir (kendi takımına saldırma senaryosu)
9. **Tüm kuleler aynı takım olduğunda** — Oyun bitmiyor, devam ediyor (Victory detection yok)
10. **Birden fazla bağlantı aynı anda kesilirse** — Race condition potansiyeli

---

## 🎯 Yapılacaklar (TASK 10+)

### Öncelik 1: Oyunu Oynanabilir Yap
| TASK | Açıklama | Durum |
|------|----------|-------|
| 10 | **Victory/Defeat Detection** — Tüm kuleler tek takıma geçince oyun bitsin | ⬜ |
| 11 | **Victory/Defeat UI Ekranı** — Kazanma/kaybetme pop-up'ı | ⬜ |
| 12 | **Restart Butonu** — Oyun bitince veya istediğinde yeniden başlat | ⬜ |
| 13 | **Pause / Speed Control** — 1x / 2x / 3x hız değiştirme | ⬜ |

### Öncelik 2: Teknik Borç & Temel İyileştirmeler
| TASK | Açıklama | Durum |
|------|----------|-------|
| 14 | **Event System Refactor** — C# Action event'leri ile decoupling | ⬜ |
| 15 | **Object Pooling** — Unit spawn/destroy yerine pool activate/deactivate | ⬜ |
| 16 | **Connection Invalidation Bug Fix** — Flow durduğunda tower notify | ✅ |
| 17 | **Collision Optimization** — OverlapSphere yerine verimli yöntem | ⬜ |
| 18 | **Material Instance Leak Fix** — MaterialPropertyBlock kullanımı | ⬜ |

### Öncelik 3: Gameplay Derinliği & Polish
| TASK | Açıklama | Durum |
|------|----------|-------|
| 19 | **AI Controller** — Düşman takım için temel yapay zeka | ⬜ |
| 20 | **Game Feel (Juice)** — Tower shake, unit pop, HP bar lerp | ⬜ |
| 21 | **Particle Effects** — Tamir tozu, capture efekti, unit trail | ⬜ |
| 22 | **Ses Efektleri** — Temel ses geri bildirimi | ⬜ |

### Öncelik 4: İçerik Genişletme
| TASK | Açıklama | Durum |
|------|----------|-------|
| 23 | **Valor (Cesaret) Sistemi** — Geçici buff, hasar x2, altın aura | ⬜ |
| 24 | **Özel Binalar** — Stable, Mage Tower, Church, Dragon's Nest | ⬜ |
| 25 | **Farklı Birim Tipleri** — Cavalry (hızlı), Archer (menzilli) | ⬜ |
| 26 | **Level Progression** — Farklı harita düzenleri | ⬜ |
| 27 | **AI Zorluk Seviyeleri** — Easy / Medium / Hard | ⬜ |
| 28 | **Menü Sistemi** — Ana menü, seviye seçimi | ⬜ |
| 29 | **Tutorial** — Yeni oyuncu rehberi | ⬜ |
| 30 | **Save/Load** — İlerleme kaydetme | ⬜ |

### Öncelik 5: Performans & Yayın Hazırlığı
| TASK | Açıklama | Durum |
|------|----------|-------|
| 31 | **Mobile URP Optimizasyonu** — Render pipeline, batching | ⬜ |
| 32 | **3D Model Değişimi** — Placeholder → Blender modelleri | ⬜ |
| 33 | **Performans Profiling** — Hedef FPS'e göre optimizasyon | ⬜ |

---

## 💻 Teknik Notlar

### Kule HP Sistemi (Core Mechanic)
```
HP = 0       → Destroyed (üretim yok, 0.8x scale)
HP = 1-4     → Repairing (0.9x scale)
HP = 5-9     → Level 1 (1.0x scale)
HP = 10-19   → Level 2 (1.3x scale)
HP = 20+     → Level 3 (1.6x scale)
```

### Üretim Formülü
```
Üretim Hızı = Temel Hız × Bağlantı Durumu

Temel Hız: 3 saniyede 1 HP (idle regen)
Bağlantı Durumu:
  - Giden bağlantı VAR (saldırı) → Çarpan = 0 (üretim durur)
  - Giden bağlantı YOK           → Çarpan = 1 (idle regen aktif)
  - Gelen bağlantı               → Üretimi etkilemez
```

### Bağlantı Limitleri
```
HP ≤ 10  → Max 1 bağlantı
HP ≤ 30  → Max 2 bağlantı
HP > 30  → Max 3 bağlantı

Send Rate Nerf:
  1 bağlantı  → 0.5s interval (normal)
  2 bağlantı  → 0.575s interval (1.15x)
  3+ bağlantı → 0.675s interval (1.35x)
```

### Unit Gönderim Sistemi
```
Bağlantı oluşturulduğunda:
  - TowerConnection sürekli olarak SendSingleUnit() çağırır
  - Unit'ler sınırsız üretilir (HP'den düşmez)
  - HP sadece unit hedefe vardığında TakeDamage ile değişir
  - Düşman unit'ler yolda karşılaşırsa 1-1 yok olur
```

### Savaş Mekaniği
```
Unit hedefe varınca:
  - Aynı takım → HP eklenir (reinforce)
  - Farklı takım → HP düşer (saldırı)
  - HP ≤ 0 → Kule el değiştirir, HP = |kalan HP| (min 1)

Unit collision:
  - Farklı takım unit'leri karşılaşınca 1-1 yok olur
  - Physics.OverlapSphere (collisionRadius: 0.3f)
```

---

## 📐 Açık Tasarım Kararları (TBD)

Bu kararlar henüz kesinleşmediği için burada kayıt altına alınıyor:

1. **Unit sınırsız mı olmalı?** — Şu an bağlantı aktifken sınırsız unit üretiliyor. Bu balance açısından sorun yaratabilir. Alternatif: unit gönderimi kule HP'sinden düşsün.
2. **Hedef FPS:** 30 mı 60 mı? (TBD)
3. **Max unit sayısı hedefi:** 50? 100? 200? (TBD)
4. **Max tower sayısı:** 10? 15? 20? (TBD)
5. **Orientation:** Landscape mı portrait mı? (TBD)
6. **Minimum Android API level:** (TBD)
7. **Test cihazı:** (TBD)

---

## 📂 Proje Klasör Yapısı

```
Assets/
├── Scripts/
│   ├── GameManager.cs
│   ├── TowerController.cs
│   ├── InputManager.cs
│   ├── UnitController.cs
│   ├── TowerConnection.cs
│   ├── TowerStatsSO.cs
│   ├── TeamData.cs
│   └── LookAtCamera.cs
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

- **Blender 3D Modelleme** — Gameplay tamamlandıktan sonra placeholder'ları gerçek modellerle değiştir
- **Gameplay > Graphics** prensibi — Önce mekanikler, sonra görsellik
- **Claude Code Android** — Bilgisayar başında olmadığında async olarak küçük feature'lar ekletmek için
- **Commercial Release** düşünülüyor — Asset, multiplayer ve monetization kararları ileride verilecek
- **InputManager Refactor** — Connection management ayrı bir `ConnectionManager.cs`'e taşınmalı

---

*Son güncelleme: Şubat 2026 — TASK 1-9 tamamlandı, gerçek durum koda göre güncellendi*
