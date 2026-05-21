# Versive

> Une expérience poétique immersive en réalité virtuelle.  
> Le joueur entre dans une salle de classe silencieuse, puis plonge dans un jardin vivant où les mots du poème *La Tulipe* de Téophile Gautier prennent forme autour de lui.

---

## Table des matières

1. [Le projet](#le-projet)
2. [Choix de la stack technique](#choix-de-la-stack-technique)
3. [Architecture du projet](#architecture-du-projet)
4. [Accessibilité](#accessibilité)
5. [Installation](#installation)
6. [Comment jouer](#comment-jouer)
7. [Équipe](#équipe)

---

## Le projet

**Versive** est une expérience narrative en VR conçue pour le Meta Quest 3. Elle explore la poésie sous une forme sensorielle et interactive : le joueur n'est pas spectateur, il est *dans* le poème.

### Déroulé de l'expérience

| Étape | Description |
|---|---|
| **Accueil** | Le joueur spawn dans une salle de classe. Un menu apparaît sur le tableau noir. |
| **Sélection de voix** | En cliquant sur le Lego Batman posé sur le bureau, la voix de l'audiodescription bascule (féminine → batman). |
| **Lancement** | Le joueur clique sur *Démarrer l'aventure* → transition vers le jardin. |
| **Intro vidéo** | 5 secondes après le chargement, une vidéo du poème est projetée. |
| **Expérience** | Des tulipes éclosent progressivement. Des objets du poème (oignon, diamant, robe, jupe, calice) apparaissent. |
| **Interaction** | Le joueur glisse les mots manquants du poème dans les bons emplacements (fill-in-the-blank). |
| **Fin** | La vidéo se termine, le canvas texte final apparaît. |

---

## Choix de la stack technique

### Pourquoi Unity plutôt que WebXR ou Unreal ?

| Critère | Unity 6 ✅ | WebXR ❌ | Unreal ❌ |
|---|---|---|---|
| Performance Quest 3 | 72 fps stables natif | Limité, pas de rendu complex | Trop lourd pour mobile standalone |
| Accès aux API casque | Complet (bouton Menu, tracking, audio) | Partiel | Complet mais courbe d'apprentissage++ |
| Déploiement standalone | APK direct sans PC | Navigateur requis | APK possible mais complexe |
| Écosystème XR | XR Interaction Toolkit natif | Three.js / A-Frame | Plugin OpenXR |
| Pipeline rendu mobile | URP optimisé Quest | N/A | Pas natif |

### Stack complète

```
Engine          Unity 6 (6000.4.7f1)
Render          Universal Render Pipeline (URP)
Cible           Meta Quest 3 (Android ARM64)
XR SDK          OpenXR + com.unity.modules.xr
Input           Unity New Input System (mode Both)
UI 3D           World Space Canvas + TrackedDeviceGraphicRaycaster
UI overlay      Screen Space - Camera Canvas
Vidéo           Unity VideoPlayer + RenderTexture (H.264 / MP4)
Texte 3D        TextMesh Pro
Audio           AudioSource + Unity Mixer
Versioning      Git + GitHub
```

---

## Architecture du projet

```
Assets/
├── Animations/
│   └── PoemeFemme.mp4          # Vidéo intro (H.264, converti depuis .mov CFHD)
│
├── Scenes/
│   ├── MainMenu.unity           # Salle de classe · menu tableau noir
│   └── Game.unity               # Jardin · expérience poétique complète
│
├── Scripts/
│   ├── Managers/
│   │   └── GameStateManager.cs  # Singleton · états MainMenu/Playing/Paused
│   ├── Menu/
│   │   ├── MenuButton.cs        # Hover / press / UnityEvent par bouton
│   │   ├── MenuButtonController.cs  # Index sélection (clavier + manette)
│   │   ├── AnimatorFunctions.cs # Callbacks animation → joue la voix active
│   │   ├── MainMenuActions.cs   # Commencer · Paramètres · Quitter
│   │   ├── MenuSoundManager.cs  # Banque de sons (femme / batman) · voix active
│   │   └── LegoBatmanInteraction.cs  # Clic batman → switch de voix
│   ├── UI/
│   │   ├── MainMenuUI.cs        # Affiche/cache selon GameState
│   │   └── PauseMenuUI.cs       # Échap PC + bouton Menu Quest 3
│   ├── AddTulips/
│   │   └── TulipFieldGenerator.cs  # Spawn progressif des tulipes (Lerp)
│   ├── FillInTheBlank/
│   │   ├── DragObject3D.cs      # Glisser-déposer d'objets 3D
│   │   └── Slot3D.cs            # Zones de réception des mots
│   ├── SpawnObjects/
│   │   └── ObjectsDrop.cs       # Apparition des objets du poème
│   └── PoemVideoController.cs   # Délai 5s → lecture vidéo → affiche canvas texte
│
├── Prefabs/
│   ├── tulip.fbx / tulipred.fbx # Tulipes (rouge + défaut)
│   └── onion.fbx                # Bulbe / oignon
│
├── Sounds/
│   └── FX/Menu/
│       ├── femme/               # angele-commencer · parametres · quitter
│       └── batman/              # batman-commencer · parametres · quitter
│
└── skirt-1-from-a-historical-engraving/  # Modèle jupe historique
```

### Flux d'états (GameStateManager)

```
[MainMenu] ──► [Playing] ──► [Paused] ──► [Playing]
                    │
                    └──► [MainMenu]  (Retour au menu)
```

---

## Accessibilité

Versive a été conçu pour être accessible à différents profils de joueurs.

### Audiodescription multi-voix

Le menu principal propose **deux voix d'audiodescription** pour les boutons :

| Voix | Déclenchement | Fichiers |
|---|---|---|
| **Féminine (Angèle)** | Par défaut au lancement | `femme/angele-*.wav` |
| **Batman** | Clic sur le Lego Batman dans la scène | `batman/batman-*.wav` |

Le switch est réversible (toggle) — cliquer à nouveau sur le batman revient à la voix féminine.

### Contrôles multi-dispositifs

| Action | PC (éditeur) | Meta Quest 3 |
|---|---|---|
| Naviguer dans le menu | Flèches ↑↓ | Joystick gauche |
| Valider un bouton | Entrée | Trigger |
| Pause | Échap | Bouton Menu (manette gauche) |
| Reprendre | Échap | Bouton Menu |
| Interagir (objets 3D) | Clic souris | Trigger + Ray Interactor |

### Confort VR

- Pas de locomotion (expérience statique) → réduit la cinétose
- Aucune contrainte de temps sur les interactions
- Textes lisibles en TextMesh Pro (police Inter / Amoria)
- Tulipes qui poussent progressivement (pas d'apparition brutale)

---

## Installation

### Prérequis

- **Unity 6** (version `6000.4.7f1`) — [Télécharger Unity Hub](https://unity.com/download)
- **Meta Quest 3** avec mode développeur activé (pour le build APK)
- **Git** installé sur la machine

### Cloner le projet

```bash
git clone https://github.com/aarena18/ImmersivePoem.git
cd ImmersivePoem
git checkout main
```

### Ouvrir dans Unity

1. Lance **Unity Hub** → **Open** → sélectionne le dossier `ImmersivePoem`
2. Unity installe automatiquement les packages requis (XR, TextMesh Pro, Input System…)
3. Si Unity demande d'importer **TMP Essentials** → accepte

### Configurer les scènes (Build Settings)

`File → Build Settings` → vérifier l'ordre :

| Index | Scène |
|---|---|
| 0 | `Assets/Scenes/MainMenu.unity` |
| 1 | `Assets/Scenes/Game.unity` |

### Paramètres requis

`Edit → Project Settings → Player` :
- **Active Input Handling** → `Both`
- **Scripting Backend** → `IL2CPP`
- **Target Architecture** → `ARM64` uniquement

`Edit → Project Settings → XR Plug-in Management` → onglet **Android** :
- ✅ OpenXR
- Interaction Profiles → `Meta Quest Touch Pro Controller`

---

## Comment jouer

### Tester dans l'éditeur (PC)

1. Ouvre la scène `MainMenu`
2. Clique sur ▶️ **Play**
3. **Flèches ↑↓** pour naviguer entre les boutons
4. **Entrée** pour sélectionner
5. **Échap** pour ouvrir/fermer le menu pause

### Tester avec le casque (Quest Link — Windows uniquement)

1. Connecte le Quest 3 via câble USB-C
2. Dans le casque → accepte **Quest Link**
3. Dans Unity → `Edit → Project Settings → XR Plug-in Management → PC` → coche **OpenXR**
4. Lance le **Play Mode** → le rendu s'affiche dans le casque

### Déployer sur le casque (build APK)

```bash
# Vérifier que le casque est connecté et reconnu
adb devices
```

Puis dans Unity :
`File → Build Settings → Android → Build And Run`

L'app apparaît dans la bibliothèque Quest sous **"Sources inconnues"**.

### Dans l'expérience

| Moment | Action |
|---|---|
| Menu principal | Regarder les boutons, pointer avec la manette, appuyer sur trigger |
| Changer de voix | Pointer et cliquer sur le **Lego Batman** sur le bureau |
| Lancer l'expérience | Sélectionner **"Démarrer l'aventure"** |
| Pendant la vidéo | Attendre la fin (la vidéo se lance automatiquement après 5 secondes) |
| Fill-in-the-blank | Attraper les objets 3D et les glisser dans les emplacements du texte |
| Pause | Appuyer sur le bouton **Menu** de la manette gauche |

---

## Équipe

| Rôle | Personne |
|---|---|
| Direction artistique, narration & game design | Ambre Arena |
| Développement VR, Unity & intégration | Mathis Dardé |

---

## Notes techniques

> **Vidéo** : `PoemeFemme.mov` (codec CFHD, 752MB) a été converti en `PoemeFemme.mp4` (H.264, 3.9MB) pour compatibilité Unity et GitHub. Le `.mov` original est ignoré par git (`.gitignore`).

> **Audio** : en cas d'erreur FMOD au lancement dans l'éditeur avec Quest Link connecté, changer la sortie audio Windows sur les haut-parleurs PC (Quest Link redirige l'audio par défaut vers le casque).

---

*Versive — 2026 · Projet académique*
