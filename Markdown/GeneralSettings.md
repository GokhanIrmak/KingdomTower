# ⚙️ Kingdom Tower - General Settings & Conventions

## 📁 Markdown Folder Rules

All documentation, settings, and notes for this project live in:
```
f:\Unity\KingdomTower\Markdown\
```

| File | Purpose |
|------|---------|
| `KingdomTower_Proje_Durumu.md` | Game progress, task list, architecture, known issues |
| `GeneralSettings.md` | This file — project-wide settings, conventions, rules |

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

---

## 🛠️ Project Settings

**Engine:** Unity 2022.3 LTS  
**Platform:** Mobile (Android/iOS)  
**Render Pipeline:** URP  
**Language:** C#  
**Scene:** Single scene (for now)  
**Scripts Path:** `Assets/Scripts/`  
**ScriptableObjects Path:** `Assets/ScriptableObjects/TowerStats/`  

---

## 📐 Code Conventions

- **Naming:** PascalCase for classes/methods, camelCase for fields
- **Singleton:** Only `GameManager` uses Singleton pattern
- **ScriptableObjects:** All tower stats defined via SO, not hardcoded
- **State Machines:** Prefer enums + switch for state management (see `TowerController.cs`)
- **AI Logic:** Lives in `AIController.cs`, acts every 2 seconds via InvokeRepeating

---

## 🗓️ Update Log

| Date | Note |
|------|------|
| Feb 2026 | GeneralSettings.md created, Markdown folder established |
| Jan 2026 | TASK 1–12 completed (see Proje Durumu) |
