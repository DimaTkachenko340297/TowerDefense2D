# TowerDefense2D

A 2D isometric Tower Defense project in Unity, built with a focus on clean modular architecture, Event-Driven Architecture, and optimized target tracking mechanics.

---

## 🛠 Tech & Requirements

* **Unity Version:** `6000.3.10f1` (Unity 6)
* **Platforms:** PC / Mobile (cross-platform input)
* **Main Scene:** `Assets/Scenes/Gameplay.unity`

---

## 🎮 Controls

The game is fully adapted for both traditional mouse clicks and touch inputs on mobile devices:
* **Selecting and Placing Towers:** Tap/click on the tower panel and place them on the grid.
* **UI Controls:** Interact with pause, restart, and next wave buttons with a single tap/click (or hold the button to enable automatic wave spawning).

---

## 🏗 Architecture & Key Systems

### 1. Game States & Management
* **Local `GameModeManager`:** Non-singleton architecture — UI and scene references are assigned via the Inspector and exposed through events (`Action<GameState>`).
* **Centralized Singletons:** Global core managers (`EnemyMovementManager.Instance`, `GoldManager.Instance`, `ProjectileManager.Instance`) utilize the Singleton pattern for straightforward access by game entities.
* **State Separation:** Distinct states for `Active`, `Paused`, and `GameOver`.
* **Safe Time Management:** Robust `Time.timeScale` handling with built-in safeguards against double-triggering pause upon defeat.

### 2. Decoupled UI & Presentation Layer
* **Decoupled Architecture:** UI components (`PauseMenuUI`, `GameOverUI`, `BaseHealthCounter`) listen to core events and avoid direct mutation of business logic.
* **Independent Screens:** Separate panels for Pause and Game Over (including final completed wave count display).

### 3. Optimized Targeting (`TowerTargeting`)
* **Intersection Baking:** Upon spawning, towers pre-calculate mathematical intersections of their attack range with path segments (`SegmentCircleIntersection`), eliminating heavy per-frame checks.
* **Event-Driven Subscriptions:** Towers activate only when an enemy steps onto a path segment they cover (`OnSegmentActivated`).

### 4. Economy & Object Management
* **`BaseHealth` & `GoldManager`:** Isolated economy and health managers reacting to enemy deaths and base breaches.
* **Object Pooling (`ObjectPool`):** Optimized spawning for enemies and projectiles to eliminate Garbage Collector (GC) allocation spikes.

---

## 🚀 Quick Start

1. Clone the repository:
   ```bash
   git clone https://github.com/DimaTkachenko340297/TowerDefense2D.git
   ```
2. Open the project in **Unity 6 (version 6000.3.10f1)** or newer.
3. Navigate to `Assets/Scenes/` and open **`Gameplay.unity`**.
4. Press **Play** in the Unity Editor.

---

## 🎨 Credits

Art assets used in this project: "Isometric Tower Defense Pack" by Artyom Zagorskiy (used under standard license agreement).

* Original Pack: [Isometric Tower Defense Pack on itch.io](https://artyom-zagorskiy.itch.io/isometric-tower-defense-pack-az)
* Modified Tilemap Version: [GitHub Repository](https://github.com/Gust3948/Tower-Defence-Tilemap-)
* UI Icons:
  * Coin Icon: [Coin-Sprite Repository](https://github.com/Gust3948/Coin-Sprite)
  * Heart Icon: [Heart-Sprite Repository](https://github.com/Gust3948/Heart-Sprite)
