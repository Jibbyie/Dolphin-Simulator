# Dolphin Simulator

Dolphin Simulator is an absurdist-surrealist comedy prototype built in Unity. Developed as a final MSc project with the intention of evolving into a full commercial release on Steam, the game casts you as a weapon-wielding dolphin navigating a weird underwater world of chaos, comedy, and combat.

This project is being developed by two people:
- **Programming**: Jibby (that’s me)
- **Design, Modeling & Animation**: Collaborating with a designer friend

---

## Core Mechanics Implemented So Far

### 🐬 Dolphin Movement
- Full 3D underwater movement with floating and sinking mechanics
- Sprinting with stamina (sprint bar UI + speed boost)
- Basic rotation and wall collision handling

### 🌊 Breathing System
- Dolphin has a limited breath timer while underwater
- Breath depletes below a defined water surface height
- Breath regenerates at the surface
- If breath reaches zero, the dolphin dies

### 🚀 Shooting
- Dolphin can shoot projectiles from a designated fire point (left-click)
- Projectile hits are detected with Unity colliders
- Enemies take damage when hit and die when health reaches 0

### ❤️ Health System
- Player and enemies have max/current health
- Health is displayed in the UI and as floating text above enemies
- Death triggers proper destruction or game over logic

### ⚡ Sprinting System
- Sprint meter depletes and recovers based on movement
- Sprinting increases movement speed temporarily

### ☠️ Damage & Healing Zones
- Damage zones (like danger areas) deal damage over time or instantly
- Healing zones work similarly to restore health

### 🔊 Audio Feedback
- Swimming, sprinting, floating, sinking, dying, breath loss/recovery
- Adds immersion and feedback to interactions

### 💬 Basic Interaction Prototype
- Cube NPC with dialogue choices
- Buttons for different reactions
- Reactions cause audio, dialogue, or the cube running away

---

## Project Goals

- Build a ridiculous but playable prototype by May (MSc deadline)
- Evolve the project into a full commercial release
- Add more absurd weapons, abilities, fish fights, and weird underwater zones
- Polish game feel, animations, and interactions with designer support

---

## Technologies Used
- **Unity 2022+** (URP pipeline)
- **C#** scripting
- **Git + Git LFS** for version control
- **TextMeshPro** for floating health UI
- **Custom audio + animations**

---

## Repository Structure
- `Assets/Scripts/`: All gameplay logic (movement, health, breathing, shooting, etc.)
- `Assets/Prefabs/`: Dolphin, enemies, UI, GameManager
- `Assets/Audio/`: All sound FX
- `Assets/Scenes/`: Prototype scenes for speedrun, combat, testing
- `Assets/Materials/`, `Models/`, `Animations/`: All visual assets from our designer

---

## Known Bugs & Work In Progress
- Health bar rotation needs polish
- Animation transitions are basic
- Damage zones don’t differentiate player vs enemy yet
- Dialogue system is placeholder but functional

---

## How to Play (Prototype)
- **WASD** to move
- **Space** to float, **F** to sink
- **Shift** to sprint
- **Left-click** to shoot
- **E** near cubes to interact

---

## Screenshots & GIFs
<!-- Add your screenshots or GIFs here -->

---

## License
This project is currently under development and not yet licensed for public distribution. All assets and code are part of a university coursework submission and a planned commercial release.

---

## Contact
Created by [Jibby](https://github.com/Jibbyie) for MSc final project. Reach out via GitHub or Steam if you want to follow development.

---

**Wishlist it on Steam (someday) 😅**
