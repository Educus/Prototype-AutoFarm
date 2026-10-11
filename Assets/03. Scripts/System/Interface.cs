using System;
using System.Collections.Generic;

public interface ILeftInteractable : IClickPriority
{
    void OnLInteract();
}

public interface IRightInteractable : IClickPriority
{
    void OnRInteract();
}

[Flags]
public enum ClickTarget
{
    None = 0,
    Npc = 1 << 0,
    Animal = 1 << 1,
    BuildingPoint = 1 << 2,  // 건물 내부 상호작용 지점
    Building = 1 << 3,
    All = ~0
}

public static class ClickPriorities
{
    public const int None = -1;
    public const int Npc = 100;
    public const int Animal = 50;
    public const int BuildingPoint = 20;  // 건물 내부 상호작용 지점
    public const int Building = 0;
}

public interface IClickPriority
{
    int ClickPriority { get; }
    ClickTarget Target { get; }
}