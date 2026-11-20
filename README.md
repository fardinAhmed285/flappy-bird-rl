# Flappy Bird AI - Unity ML-Agents

A 2D Flappy Bird reinforcement learning project built with **Unity** and **Unity ML-Agents 4.1.0**, using **Proximal Policy Optimization (PPO)** to train an autonomous bird agent.

The agent learns to navigate procedurally spawned pipe obstacles, maintain its position, avoid collisions, and collect extra-life prizes. The project explores reinforcement learning through custom observations, discrete actions, reward engineering, and multi-life episode management.

## Project Overview

This project implements an autonomous Flappy Bird agent that learns through interaction with a Unity 2D environment.

Instead of relying on predefined movement rules, the agent uses a neural network trained with PPO to select actions based on its current observations.

### Key Features

- **Reinforcement Learning:** PPO-based training using Unity ML-Agents 4.1.0.
- **Autonomous Gameplay:** Learns when to flap and when to maintain its current trajectory.
- **Custom Observation Space:** Seven continuous observations describing the bird, upcoming pipes, and nearby prizes.
- **Extra-Life System:** Collectible prizes grant additional lives, up to a configurable maximum.
- **Multi-Life Episodes:** The bird can recover from collisions while lives remain.
- **Procedural Obstacles:** Pipe pairs spawn continuously and move across the environment.
- **Reward Engineering:** Survival rewards, flap penalties, collision penalties, and prize rewards.
- **Inference Support:** Exported ONNX models can be used directly in Unity.

## Gameplay Mechanics

### 1. Bird Agent

The autonomous agent is implemented in `FlappyAgent.cs`.

The bird has two discrete actions:

| Action | Behaviour |
|---|---|
| 0 | Do nothing |
| 1 | Flap upward |

The agent starts with one life and can hold up to three lives by default.

### 2. Pipe Obstacles

The `PipeSpawner.cs` script manages pipe generation and movement.

- Pipe pairs spawn at configurable intervals.
- Both pipes move horizontally at a configurable speed.
- Spawned objects are destroyed after passing a specified boundary.
- The vertical offsets of the top and bottom pipes determine the playable gap.

### 3. Extra-Life Collectibles

The project introduces an additional gameplay mechanic through collectible life prizes.

- Prizes spawn near the pipe gap at a configurable offset.
- Collectibles use a trigger-based 2D collider.
- Picking up a prize grants one additional life.
- The maximum number of lives is configurable.
- Successful collection provides a positive reward.

### 4. Collision and Life Management

The bird can collide with pipes, the ceiling, or the floor.

Collision handling follows this logic:

1. Deduct one life.
2. Apply a collision penalty.
3. If lives remain, recover the bird and continue the episode.
4. If lives reach zero, apply the terminal penalty and end the episode.
5. Reset the environment for the next episode.

The episode reset also removes previously spawned pipes and prizes.

## Reinforcement Learning Design

### Observation Space

The agent receives a seven-dimensional continuous observation vector.

| Index | Observation | Normalisation |
|---|---|---|
| 0 | Bird vertical position | `y / 5f` |
| 1 | Bird vertical velocity | `vy / 10f` |
| 2 | Current remaining lives | `lives / maxLives` |
| 3 | Horizontal distance to nearest upcoming pipe | `dx / 10f` |
| 4 | Vertical offset to nearest upcoming pipe gap | `dy / 5f` |
| 5 | Horizontal distance to nearest upcoming prize | `dx / 10f` |
| 6 | Vertical offset to nearest upcoming prize | `dy / 5f` |

These observations provide information about the bird's physical state and the relative positions of upcoming obstacles and collectibles.

### Action Space

The agent uses a discrete action space with one branch containing two possible actions.

| Action | Description |
|---|---|
| 0 | No flap |
| 1 | Flap upward |

The Decision Requester is configured with a **Decision Period of 3**, meaning the agent receives a new decision every three environment steps.

A heuristic controller is also available for manual testing using the Spacebar or Left Mouse Button.

### Reward Structure

The reward function encourages survival, controlled movement, and prize collection.

| Event | Reward |
|---|---:|
| Surviving a step | `+0.01` |
| Performing a flap | `-0.002` |
| Collecting a life prize | `+0.5` |
| Collision | `-0.5` |
| Running out of lives | `-1.0` |

The survival reward encourages longer episodes, while the flap penalty discourages unnecessary upward actions.

Prize collection provides an additional incentive to navigate towards collectibles rather than focusing exclusively on survival.

## Project Structure

The main project components are organised as follows:

```text
Assets/
├── Scripts/
│   ├── FlappyAgent.cs
│   └── PipeSpawner.cs
│
├── Prefabs/
│   ├── PipePair.prefab
│   └── LifePrize.prefab
│
└── Models/
    └── FlappyBird.onnx

flappy_config.yaml
```

### Main Components

| Component | Responsibility |
|---|---|
| `FlappyAgent.cs` | Agent observations, actions, rewards, collisions, and episode management |
| `PipeSpawner.cs` | Pipe and prize spawning, movement, and object destruction |
| `PipePair.prefab` | Obstacle configuration and playable gap |
| `LifePrize.prefab` | Collectible prize with trigger-based collision detection |
| `flappy_config.yaml` | PPO training hyperparameters |
| `FlappyBird.onnx` | Exported trained neural network |

## Installation and Setup

### Prerequisites

- Unity with the required 2D project packages.
- Python 3.10.
- Unity ML-Agents 4.1.0.
- A configured Python virtual environment or Conda environment.
- Git.

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPOSITORY.git
cd YOUR_REPOSITORY
```

Open the project using Unity Hub.

### 2. Configure Python Environment

Activate your Conda environment:

```powershell
conda activate mlagents31012
```

Verify the installed ML-Agents version:

```powershell
mlagents-learn --help
pip show mlagents
```

### 3. Configure the Unity Agent

Select the Bird GameObject and configure its Behavior Parameters:

| Setting | Value |
|---|---|
| Behavior Name | `FlappyBird` |
| Behavior Type | `Default` |
| Vector Observation Space Size | `7` |
| Continuous Actions | `0` |
| Discrete Branches | `2` |

Configure the Decision Requester with a Decision Period of `3`.

## Training the Agent

The project uses PPO (Proximal Policy Optimization), an on-policy reinforcement learning algorithm.

### 1. PPO Configuration

The training parameters are defined in `flappy_config.yaml`.

```yaml
behaviors:
  FlappyBird:
    trainer_type: ppo

    hyperparameters:
      batch_size: 256
      buffer_size: 2048
      learning_rate: 3.0e-4
      beta: 5.0e-3
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
      learning_rate_schedule: linear

    network_settings:
      normalize: true
      hidden_units: 128
      num_layers: 2

    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0

    max_steps: 200000
    time_horizon: 64
    summary_freq: 5000
```

### 2. Start Training

From the project directory, execute:

```powershell
mlagents-learn flappy_config.yaml --run-id=flappy_run2 --time-scale=5.0
```

Once the trainer displays:

```text
Listening on port 5004...
```

Return to Unity and press **Play**.

The Unity environment will connect to the Python trainer, and the agent will begin learning through interaction.

### 3. Monitor Training

Training progress and checkpoints are stored under:

```text
results/
└── flappy_run2/
```

The training process can be monitored using TensorBoard:

```powershell
tensorboard --logdir results
```

To stop training, exit Play mode in Unity and press `Ctrl + C` in PowerShell.

The trainer exports an ONNX model that can subsequently be used for inference.

## Model Inference

After training, the exported ONNX model can be used directly inside Unity without running the Python trainer.

### Setup

1. Locate the exported model:

   ```text
   results/flappy_run2/FlappyBird.onnx
   ```

2. Copy the model into:

   ```text
   Assets/Models/
   ```

3. Select the Bird GameObject in the Unity Hierarchy.

4. Open its Behavior Parameters component.

5. Assign the exported ONNX model to the Model field.

6. Change Behavior Type to `Inference Only`.

7. Press Play.

The bird will now use the trained neural network to select actions autonomously.

## Troubleshooting

### Invalid `time_scale` Configuration

If training produces:

```text
TrainerConfigError:
The option time_scale was specified in your YAML file
for EnvironmentSettings, but is invalid.
```

Remove `time_scale` from the YAML configuration.

Use the command-line argument instead:

```powershell
mlagents-learn flappy_config.yaml --run-id=flappy_run2 --time-scale=5.0
```

### StrictVersion AttributeError

If the environment reports:

```text
AttributeError:
'StrictVersion' object has no attribute 'version'
```

This can be caused by compatibility issues involving newer setuptools versions and legacy version parsing.

Try:

```powershell
pip install "setuptools<70"
```

### Partial Training Checkpoints

Training interruptions do not necessarily mean all progress is lost.

Check the `results/` directory for exported ONNX checkpoints. Existing checkpoints can be imported into Unity and tested independently.

### Bird Repeatedly Flaps into the Ceiling

Potential causes include:

- Insufficient information about upcoming pipe gaps.
- An unsuitable reward balance.
- Excessive flap frequency.
- Incorrect observation normalisation.
- An unsuitable decision period.

The seven-dimensional observation vector, flap penalty, and three-step decision period are intended to help address these issues.

## Technologies Used

| Technology | Purpose |
|---|---|
| Unity 2D | Game environment |
| C# | Gameplay and agent implementation |
| Unity ML-Agents 4.1.0 | Reinforcement learning integration |
| Python 3.10 | Training environment |
| PPO | Policy optimisation algorithm |
| ONNX | Trained model export and inference |

## Future Improvements

Potential extensions to the project include:

- Training comparison across different PPO hyperparameters.
- Reward function optimisation and experimentation.
- Evaluation across multiple random seeds.
- Performance comparison between different observation configurations.
- Training visualisation and reward convergence analysis.
- More complex obstacle and collectible configurations.
- Comparison between heuristic and trained agent performance.

## Learning Objectives

This project explores practical reinforcement learning concepts, including:

- Designing custom observation and action spaces.
- Implementing reward functions for agent behaviour.
- Training neural network policies using PPO.
- Managing episodic environments and terminal states.
- Exporting trained policies to ONNX.
- Deploying trained reinforcement learning agents inside Unity.

---

**Note:** This repository is an experimental reinforcement learning project. Training performance depends on environment configuration, reward design, and training duration.