# ADHD Supportive HUD - Project Overview

## Project Structure
```
ADHD-Supportive-HUD/
├── .github/
│   └── workflows/
│       └── unity-build.yml         # GitHub Actions workflow for automated builds
├── Assets/
│   └── Scripts/
│       ├── Core/                   # Core functionality scripts
│       │   ├── SceneManager.cs     # Manages scene setup and initialization
│       │   ├── HUDManager.cs       # Manages the head-locked HUD
│       │   ├── TaskManager.cs      # Handles task management
│       │   ├── PomodoroManager.cs  # Manages Pomodoro timer functionality
│       │   ├── HealthTracker.cs    # Tracks health metrics
│       │   ├── DataManager.cs      # Handles data persistence
│       │   ├── AnalyticsManager.cs # Manages analytics and user feedback
│       │   ├── AppSettings.cs      # Manages app configuration
│       │   ├── BuildManager.cs     # Handles build settings and deployment
│       │   ├── DevelopmentSettings.cs # Development and testing settings
│       │   └── QuestBuildValidator.cs  # Validates Quest 3 build requirements
│       ├── UI/                     # UI-related scripts
│       │   ├── HUDCanvas.cs        # Manages HUD layout and appearance
│       │   ├── ArmbandUI.cs        # Manages wrist-mounted interface
│       │   ├── DebugMenu.cs        # Development debug interface
│       │   └── UIPrefabManager.cs  # Manages UI prefabs
│       └── Interaction/            # Interaction-related scripts
│           └── HandInteractionManager.cs # Handles hand tracking interactions
├── README.md                       # Project documentation
└── LICENSE                         # MIT License
```

## Core Features Implemented

### 1. Development Tools
- **DevelopmentSettings**: Manages development mode and testing features
  - ADHD-specific testing scenarios
  - Performance monitoring
  - UI stress testing
  - Focus and overstimulation testing

- **QuestBuildValidator**: Ensures proper Quest 3 configuration
  - Validates required packages
  - Checks build settings
  - Runs performance tests
  - Auto-fixes common issues

- **DebugMenu**: Comprehensive development interface
  - Quick access to testing tools
  - Performance metrics display
  - Build status monitoring
  - Feature testing toggles

### 2. Core Systems
- **HUD System**: Head-locked UI implementation
  - Peripheral vision positioning
  - Semi-transparent display
  - Task and timer integration

- **Task Management**: Basic task tracking
  - Priority-based task display
  - Task completion tracking
  - Visual status indicators

- **Pomodoro Timer**: Time management
  - Work/break session tracking
  - Visual countdown
  - Progress indicators

- **Health Tracking**: Basic health monitoring
  - Water intake tracking
  - Break reminders
  - Health metrics display

### 3. Data Management
- **Data Persistence**: Saves user data
  - Task storage
  - Settings persistence
  - Usage statistics

- **Analytics**: Tracks usage patterns
  - Feature usage metrics
  - User feedback collection
  - Performance analytics

## Technical Implementation

### Build System
- Automated builds via GitHub Actions
- Quest 3-specific optimizations
- Development and production configurations

### Development Workflow
1. Development in Unity 2022 LTS
2. Testing with Quest-specific tools
3. Automated validation before builds
4. Performance monitoring during development

### Testing Framework
- ADHD-specific test scenarios
- Performance benchmarking
- UI stress testing
- Hand tracking validation

## Current Status
- Core development tools implemented
- Basic HUD system in place
- Task management system ready
- Build validation system complete
- Development testing framework active

## Next Steps
1. Implement remaining core features
2. Add advanced ADHD support features
3. Enhance UI/UX for ADHD users
4. Optimize performance for Quest 3
5. Add user testing framework

## Development Guidelines
1. Use development tools for testing
2. Validate builds before deployment
3. Monitor performance metrics
4. Test ADHD-specific scenarios
5. Follow accessibility guidelines

## Getting Started
1. Clone the repository
2. Open in Unity 2022 LTS
3. Install required packages
4. Run QuestBuildValidator
5. Start development with debug tools enabled 