# Solution Architecture

## Dependency Diagram

```mermaid
%%{init: {"theme": "dark"}}%%
graph TB

    classDef l0 fill:#1a0a0a,stroke:#ff6b6b,color:#ff6b6b;
    classDef l1 fill:#0a1a18,stroke:#4ecdc4,color:#4ecdc4;
    classDef l2 fill:#0a0a1a,stroke:#a29bfe,color:#a29bfe;
    classDef l3 fill:#0a1a0a,stroke:#00b894,color:#00b894;
    classDef l4 fill:#1a1a0a,stroke:#fdcb6e,color:#fdcb6e;

    subgraph Layer0[Layer 0 - Domain]
        Core[BattleArena.Core<br/>Domain entities enums interfaces]
    end
    class Layer0 l0;

    subgraph Layer1[Layer 1 - Services and Data]
        Application[BattleArena.Application<br/>Application services and use cases]
        Infrastructure[BattleArena.Infrastructure<br/>Database repositories and persistence]
    end
    class Layer1 l1;

    subgraph Layer2[Layer 2 - Presentation and API]
        Presentation[BattleArena.Presentation<br/>GUI-agnostic playback engine]
        Api[BattleArena.Api<br/>REST API endpoints]
    end
    class Layer2 l2;

    subgraph Layer3[Layer 3 - Applications]
        Demo[BattleArena.Demo<br/>Console demo application]
        Gui[BattleArena.Gui<br/>Avalonia desktop UI]
    end
    class Layer3 l3;

    subgraph Layer4[Layer 4 - Tests]
        UnitTests[BattleArena.UnitTests<br/>xUnit unit tests]
        AcceptanceTests[BattleArena.AcceptanceTests<br/>Reqnroll BDD tests]
    end
    class Layer4 l4;

    Application --> Core
    Infrastructure --> Core
    Presentation --> Core
    Presentation --> Application
    Api --> Application
    Api --> Infrastructure
    Demo --> Core
    Demo --> Application
    Demo --> Presentation
    Gui --> Core
    Gui --> Application
    Gui --> Presentation
    UnitTests --> Core
    UnitTests --> Application
    UnitTests --> Presentation
    UnitTests --> Gui
    AcceptanceTests --> Core
    AcceptanceTests --> Application
    AcceptanceTests --> Presentation
```

## Onion Architecture View

```mermaid
%%{init: {"theme": "dark"}}%%
graph RL

    classDef l0 fill:#1a0a0a,stroke:#ff6b6b,color:#ff6b6b;
    classDef l1 fill:#0a1a18,stroke:#4ecdc4,color:#4ecdc4;
    classDef l2 fill:#0a0a1a,stroke:#a29bfe,color:#a29bfe;
    classDef l3 fill:#0a1a0a,stroke:#00b894,color:#00b894;
    classDef l4 fill:#1a1a0a,stroke:#fdcb6e,color:#fdcb6e;

    subgraph Layer4[Layer 4 - Tests]
        subgraph Layer3[Layer 3 - Applications]
            subgraph Layer2[Layer 2 - Presentation and API]
                subgraph Layer1[Layer 1 - Services and Data]
                    subgraph Layer0[Layer 0 - Domain]
                        Core[BattleArena.Core]
                    end
                    Application[BattleArena.Application]
                    Infrastructure[BattleArena.Infrastructure]
                end
                Presentation[BattleArena.Presentation]
                Api[BattleArena.Api]
            end
            Demo[BattleArena.Demo]
            Gui[BattleArena.Gui]
        end
        UnitTests[BattleArena.UnitTests]
        AcceptanceTests[BattleArena.AcceptanceTests]
    end

    class Layer0 l0;
    class Layer1 l1;
    class Layer2 l2;
    class Layer3 l3;
    class Layer4 l4;

    Application --> Core
    Infrastructure --> Core
    Presentation --> Core
    Presentation --> Application
    Api --> Application
    Api --> Infrastructure
    Demo --> Core
    Demo --> Application
    Demo --> Presentation
    Gui --> Core
    Gui --> Application
    Gui --> Presentation
    UnitTests --> Core
    UnitTests --> Application
    UnitTests --> Presentation
    UnitTests --> Gui
    AcceptanceTests --> Core
    AcceptanceTests --> Application
    AcceptanceTests --> Presentation
```

## Project Responsibilities

| Project | Responsibility |
|---------|---------------|
| BattleArena.Core | Domain entities, enums, interfaces (zero dependencies) |
| BattleArena.Application | Application services and use cases |
| BattleArena.Infrastructure | Database repositories and persistence |
| BattleArena.Presentation | GUI-agnostic playback engine and display state |
| BattleArena.Api | REST API layer |
| BattleArena.Demo | Console demo (render-only, no game logic) |
| BattleArena.Gui | Avalonia desktop user interface |
| BattleArena.UnitTests | xUnit + NSubstitute unit tests |
| BattleArena.AcceptanceTests | Reqnroll BDD acceptance tests |

## Dependency Rules

- **Core** must not reference any other project.
- **Application** may only reference Core.
- **Infrastructure** may only reference Core.
- All other projects must follow the arrow direction in the diagram. Violations should be flagged as architectural issues.
