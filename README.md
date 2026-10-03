# Game Cerdas - Praktikum 05: Enemy FSM (Finite State Machine)

Project Unity 6 (Universal Render Pipeline / URP) ini berisi implementasi **Enemy AI berbasis Finite State Machine (FSM)** yang terintegrasi dengan **AI Navigation (NavMeshAgent)**, **Perception (FOV & Raycast Line of Sight)**, **Health & Combat**, serta **Kamera 3rd Person POV**.

---

## 🎮 Kontrol Game (Controls)

| Tombol | Aksi |
| :--- | :--- |
| **W, A, S, D** / **Panah** | Bergerak (relatif terhadap arah pandang kamera) |
| **Klik Kanan Mouse + Geser** | Memutar Kamera 3rd POV (Orbit 360°) |
| **Spasi** | Menyerang Enemy Zombie |

---

## 🧠 Perilaku AI Enemy Zombie (FSM States)

1. **Patrol** — Zombie berpatroli mengelilingi titik *Waypoints*.
2. **Alert** — Zombie berhenti dan waspada selama 1.5 detik saat pertama melihat Player sebelum mengejar.
3. **Chase** — Zombie mengejar Player menggunakan `NavMeshAgent`.
4. **Attack** — Zombie menyerang Player dalam jarak dekat dengan damage dan cooldown.
5. **Search** — Saat Player hilang di balik tembok, Zombie mengingat posisi terakhir (`lastSeenPosition`) dan mencarinya selama 3 detik.
6. **Flee** — Zombie berlari kabur menuju `SafePoint` ketika darahnya kritis ($\le 30$ HP).
7. **Heal** — Di `SafePoint`, Zombie memulihkan HP-nya sampai penuh ($100$ HP) sebelum kembali berpatroli.
8. **Dead** — Zombie mati ketika HP mencapai 0.

---

## 🚀 Cara Menjalankan Project (Setup Guide untuk Penguji / Teman)

1. **Persyaratan Version:**
   - Gunakan **Unity 6** (atau Unity 2022.3+ dengan **Universal Render Pipeline / URP**).
2. **Buka Project:**
   - Jalankan **Unity Hub** $\rightarrow$ **Open** $\rightarrow$ Pilih folder `GameCerdas_Praktikum05_EnemyFSM`.
3. **Buka Scene:**
   - Di jendela Project, buka `Assets/Scenes/Praktikum05_FSM.unity`.
4. **NavMesh Check (Penting):**
   - Jika Enemy tidak bergerak saat di-Play, buka GameObject `Navigation` di Hierarchy, lalu klik tombol **Bake** di komponen `NavMeshSurface`.
5. **Jalankan:**
   - Tekan tombol **Play** di Unity Editor.
