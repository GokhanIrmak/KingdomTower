# ⚙️ Kingdom Tower - General Settings & Conventions

## 📁 Markdown Folder Rules

All documentation, settings, and notes for this project live in:
```
f:\Unity\KingdomTower\Markdown\
```

| File | Purpose |
|------|---------|
| `KingdomTower_Proje_Durumu.md` | Game progress, task list, bugs, architecture, technical notes |
| `GeneralSettings.md` | This file — project-wide settings, conventions, rules, design decisions |

### When to Edit vs. Create a New File
- **Game mechanics, tasks, bugs, architecture** → Edit `KingdomTower_Proje_Durumu.md`
- **New topic that doesn't fit the above** → Create a new `.md` file in this folder

---

## 👷 AI Role: Senior Game Engineer

The AI assistant acts as the **Senior Game Engineer** on this project.

**Responsibilities:**
- Design and review all game systems with production-quality thinking
- Flag architectural issues, performance risks, or bad game-design patterns proactively — don't wait to be asked
- Think in terms of: *scalability, mobile performance, game feel, and maintainability*
- The human is an experienced software engineer learning game dev — explain game-specific reasoning when it differs from general software patterns
- Always consider: *"Does this system scale to 50+ units / 10+ towers / future features?"*

---

## 🤖 AI Assistant Rules

- Always look in `Markdown/` first before creating new documentation
- Never duplicate content that already exists in `KingdomTower_Proje_Durumu.md`
- When a task is completed, mark it in `KingdomTower_Proje_Durumu.md` with ✅
- New files in this folder should be linked/referenced in this `GeneralSettings.md`
- Code review: always check for performance implications on mobile (GC alloc, per-frame physics queries, material instances)

---

## 🛠️ Project Settings

| Setting | Value |
|---------|-------|
| **Engine** | Unity 2022.3 LTS |
| **Platform** | Mobile (Android/iOS) |
| **Render Pipeline** | URP |
| **Language** | C# |
| **Input System** | New Input System (`UnityEngine.InputSystem`) |
| **Scene** | Single scene (for now) |
| **Scripts Path** | `Assets/Scripts/` |
| **ScriptableObjects Path** | `Assets/ScriptableObjects/TowerStats/` |
| **Orientation** | TBD |
| **Min Android API** | TBD |
| **Target FPS** | TBD |

---

## 📦 Third-Party Dependencies

| Package | Usage | Notes |
|---------|-------|-------|
| **TextMeshPro** | HP text display on towers | Unity built-in package |
| **Unity Input System** | Touch + mouse input handling | New Input System, not legacy |

Yeni dependency eklendiğinde bu tabloya kayıt düşülecek.

---

## 🏛️ Architecture Conventions

### Event-Driven Communication (Hedef)
- Sistemler arası iletişimde C# `Action` event'leri kullanılacak
- Doğrudan singleton referansı yerine event subscribe tercih edilecek
- Örnek event'ler: `OnTowerCaptured`, `OnGameEnd`, `OnUnitSpawned`, `OnConnectionCreated`
- **Mevcut durum:** Event yok, direkt referanslar kullanılıyor. TASK 14 ile refactor edilecek.

### ScriptableObjects
- **Sadece statik veri:** Max HP, hız, üretim oranı, UnitSendPercentage
- **Runtime veri SO'da tutulmaz:** Current HP, current team, active connections — bunlar MonoBehaviour field'larında yaşar
- Yeni stat eklendiğinde `TowerStatsSO.cs`'ye eklenir, asla hardcode yapılmaz

### Unit Movement
- `Vector3.MoveTowards` kullanılır (Update içinde)
- `Rigidbody` veya fizik motoru unit hareketi için **kullanılmaz**
- Performans nedeni: mobilde fizik hesaplaması pahalı, basit hareket yeterli

### Object Pooling (Hedef)
- Unit ve ileride projectile'lar için pool **zorunlu** olacak
- `Instantiate/Destroy` yerine pool activate/deactivate
- **Mevcut durum:** Pooling yok. TASK 15 ile eklenecek.

### Material Handling (Hedef)
- Team renk değişimleri için `MaterialPropertyBlock` kullanılacak
- `meshRenderer.material.color` atanmayacak (her atama yeni material instance oluşturur → memory leak)
- **Mevcut durum:** Direkt material.color kullanılıyor. TASK 18 ile düzeltilecek.

---

## 📐 Code Conventions

- **Naming:** PascalCase for classes/methods, camelCase for private fields
- **Singleton:** Sadece `GameManager` kullanır — başka singleton eklenmeyecek
- **ScriptableObjects:** Tüm tower stat'ları SO ile tanımlanır, hardcode yasak
- **State Machines:** Enum + switch pattern (bkz. `TowerController.cs` → `TowerState`)
- **Namespace:** Tüm script'ler `KingdomTower` namespace'i altında
- **Regions:** Her script region'larla organize edilir (#region Unity Lifecycle, Initialization, vs.)
- **SerializeField:** Public yerine `[SerializeField] private` tercih edilir

---

## 🎮 Game Design Decisions Log

Kesinleşmiş ve kesinleşmemiş tasarım kararlarının kaydı.

### Kesinleşmiş Kararlar
| Karar | Açıklama | Tarih |
|-------|----------|-------|
| HP = Birim | Kule HP'si ve birim sayısı aynı değer (currentHealth) | Ocak 2026 |
| Idle Regen | Giden bağlantı yokken 3 saniyede 1 HP regen | Ocak 2026 |
| Saldırıda Üretim Durur | Giden bağlantı varken idle regen durur | Ocak 2026 |
| 1v1 Unit Collision | Düşman unit'ler karşılaşınca ikisi de yok olur | Şubat 2026 |
| Line Cutting | Boş alana sürükleyerek bağlantı kesilebilir | Şubat 2026 |

### Açık Kararlar (TBD)
| Konu | Seçenekler | Notlar |
|------|-----------|--------|
| Unit sınırsız mı? | A) Sınırsız (şu anki) B) HP'den düşsün | Balance testi gerekli |
| Kule el değiştirince mevcut bağlantılar ne olur? | A) Kalır B) Hepsi kesilir | Test edilmedi |
| AI zorluk sistemi | Timer-based mi? Durum analizi mi? | AI henüz yok |

---

## 🔧 Git & Versiyon Kontrol

- **Repository:** [github.com/GokhanIrmak/KingdomTower](https://github.com/GokhanIrmak/KingdomTower)
- **Branch strategy:** TBD (şu an tek branch: main)
- **`.gitignore`:** Mevcut, Unity standart ignore dosyası kullanılıyor
- **Commit convention:** TBD

---

## 📱 Build & Deploy

| Setting | Value |
|---------|-------|
| **Target Platform** | Android (öncelikli), iOS (ileride) |
| **Orientation** | TBD |
| **Min Android API** | TBD |
| **Test Cihazı** | TBD |
| **Target Resolution** | TBD |

---

## 🗓️ Update Log

| Date | Note |
|------|------|
| Şubat 2026 | GeneralSettings.md kapsamlı güncelleme — architecture conventions, dependencies, design decisions eklendi |
| Şubat 2026 | Proje durumu koda göre güncellendi — TASK 10-12 tamamlanmadı olarak düzeltildi |
| Ocak 2026 | TASK 1–9 tamamlandı |
| Şubat 2026 | GeneralSettings.md oluşturuldu, Markdown folder yapısı kuruldu |
