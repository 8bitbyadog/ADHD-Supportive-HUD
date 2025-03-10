# ADHD-Supportive Quest 3 HUD

A Mixed Reality application for the Oculus Quest 3 that provides an "Iron Man-style" heads-up display (HUD) to support users with ADHD. The app displays persistent visual reminders, task management tools, and time/health trackers in the user's peripheral vision.

## Features

### Core Features (MVP)
- Passthrough AR HUD with head-locked UI
- Dual-interface system (HUD + Armband UI)
- Basic task management with priority display
- Pomodoro timer with visual progress
- Simple health tracking (water intake)

### Technical Requirements
- Unity 2022 LTS or newer
- Oculus Quest 3
- Required Packages:
  - Android Build Support
  - OpenXR Plugin
  - Meta XR SDK
  - Oculus Integration

## Project Structure
```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── HUDManager.cs
│   │   ├── TaskManager.cs
│   │   ├── PomodoroManager.cs
│   │   └── HealthTracker.cs
│   ├── UI/
│   │   ├── HUDCanvas.cs
│   │   └── ArmbandUI.cs
│   └── Interaction/
│       └── HandInteractionManager.cs
├── Prefabs/
│   ├── HUD/
│   └── UI/
├── Scenes/
│   └── MainScene.unity
└── Resources/
    └── Config/
```

## Setup Instructions
1. Clone this repository
2. Open the project in Unity 2022 LTS or newer
3. Install required packages via Package Manager
4. Configure XR Plugin Management for Oculus
5. Build and deploy to Quest 3

## Development Guidelines
- Follow Unity best practices for XR development
- Maintain 70+ FPS for optimal comfort
- Use simple shaders and minimal draw calls
- Test in various lighting conditions
- Ensure UI elements are readable but not distracting

## License
MIT License - See LICENSE file for details 