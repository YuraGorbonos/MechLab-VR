# MechLab VR – Контекст проекта для AI-ассистента

## Общая информация
- **Проект:** MechLab VR – VR-симулятор диагностики и ремонта автомобиля (образовательный).
- **Платформа:** Meta Quest 2 (Android), ПК-VR.
- **Режимы:** одиночный / мультиплеер (до 2 игроков), обучение / экзамен (экзамен в будущем).
- **Стек:** Unity 6.3 LTS (URP, VR Template), Mirror (сеть), Zenject (DI), HTML-отчёты (System.IO + встроенный браузер).

## Архитектурный неймспейс – SkillForge
- `SkillForge.Core` – базовые интерфейсы и абстракции.
- `SkillForge.Education` – обучение/экзамен (TrainingSystem, ExamSystem – в будущем).
- `SkillForge.Scenarios` – данные сценариев (ScriptableObject: ScenarioData, ScenarioStepData, StepCondition).
- `SkillForge.WorkContexts` – реализации IWorkContext для предметных областей (CarWorkContext).
- `SkillForge.Players` – управление игроками (PlayerManager).
- `SkillForge.Validation` – валидация намерений (ValidationService).
- `SkillForge.Highlight` – серверная логика подсветки (HighlightService).
- `SkillForge.UI` – клиентские View (TabletUI, ServerUI, SessionEndScreen).
- `SkillForge.VR` – VR-взаимодействие (PlayerController, ToolInteractionHandler).
- `SkillForge.Networking` – Mirror-специфичные компоненты (GameManager, CustomNetworkManager).
- `SkillForge.Utils` – вспомогательные классы.

## Паттерны и стиль кода
- Архитектурный паттерн: **MVP** (Model-View-Presenter) с пассивной View через Mirror SyncVar/RPC.
- Внедрение зависимостей: **Zenject**. Два контекста – серверный и клиентский.
- Именование: **Microsoft C# Coding Conventions**:
  - Интерфейсы: `I` префикс (IEducationModule, IWorkContext).
  - Классы/методы/свойства: PascalCase.
  - Приватные поля: `_camelCase`.
  - Параметры/локальные переменные: camelCase.
  - Пространства имён: PascalCase (SkillForge.Core, SkillForge.Education).
- Придерживайся строгой типизации, избегай `FindObjectOfType`, используй DI.

## Ключевые интерфейсы и классы (основа для реализации)

### Core (базовые интерфейсы)
```csharp
// IEducationModule – фасад обучения/экзамена
public interface IEducationModule
{
    void Initialize(ScenarioConfig config);
    ScenarioStep GetCurrentStep();
    bool IsStepCompleted(string stepId);
    void OnActionLogged(Intention intent, bool success, string errorCode);
    void AdvanceStep();
    bool AllStepsCompleted();
    SessionReport GenerateReport();
    event Action<ScenarioStep> OnStepChanged;
}

// IWorkContext – абстракция над объектом ремонта (автомобиль и т.п.)
public interface IWorkContext
{
    ValidationResult ValidateAction(Intention intent, ScenarioStep currentStep);
    void ApplyAction(Intention intent);
    bool CheckCondition(StepCondition condition);
    bool IsWorkCompleted();
    DiagnosticResult GetDiagnosticStatus(string subsystem);
    event Action OnWorkStateChanged;
}

// IValidationService – валидация намерений
public interface IValidationService
{
    ValidationResult Validate(Intention intent, ScenarioStep currentStep, IWorkContext workContext);
}

// IPlayerManager – управление игроками и занятостью объектов
public interface IPlayerManager
{
    bool TryAcquireObject(int playerId, string objectId);
    void ReleaseObject(int playerId, string objectId);
    bool IsObjectFree(string objectId);
}

// IHighlightService – управление подсветкой (серверная часть)
public interface IHighlightService
{
    void SetHighlight(string targetId, HighlightType type);
    void ClearHighlights();
}

// IDiagnosticService – диагностика подсистем
public interface IDiagnosticService
{
    DiagnosticResult Diagnose(string subsystem, CarState state);
}

public interface IReportGenerator
{
    string GenerateHtml(SessionReport report); // возвращает путь к .html
    void OpenReport(string filePath);          // открывает в браузере
}