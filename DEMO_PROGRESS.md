# MechLab VR — журнал для смены чата

Обновлено: 2026-11-26. Читать этот файл. Не сканировать Mirror, Samples и пакеты.

## Запись видео
1. Открыть `Assets/_Project/Scenes/ServerScene.unity`.
2. Play, нажать Start Server. Поднимется host и загрузится `ClientScene`.
3. Это VR: риг `XR Origin (XR Rig)`. Ходить стиком контроллера. Курок или клавиша E выполняет шаг. Над текущей деталью жёлтая стрелка и подпись. При ходьбе края экрана темнеют.
4. Бокс закрытый: стены гаража и крыша `BayRoof`. Задание на планшете `TaskStand`.
5. Восемь шагов левого тормоза. Детали узла прячутся и появляются по шагу.

Полный Play в чате не прогонялся.

## Сцена
- `ClientScene`: гараж со стенами и крышей `BayRoof`, седан `Saloon`, узел `BrakeAssembly`, VR-риг `XR Origin (XR Rig)`, планшет `TaskStand`. `DemoPlayer` и экранная панель `UI` выключены.
- Ассеты CC0: `Assets/_Project/Art/Environment/` (гараж и седан с 3dassets.dev), `Assets/_Project/Art/Brake/`.
- Скрипты: `Assets/_Project/Scripts/Demo/DemoWalker.cs`, `DemoTaskSpace.cs`.
- `ServerUI` сам стартует сессию после загрузки клиента.

## Если ломается
Консоль после Start Server. Пол `WalkFloor` страхует падение. Стабилизаторы луча на риге выключены, иначе Burst сыпет исключениями.
