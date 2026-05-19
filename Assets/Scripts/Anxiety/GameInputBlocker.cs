/// <summary>
/// Bloquea interacción del mundo durante cinemáticas de ansiedad.
/// </summary>
public static class GameInputBlocker
{
    public static bool IsBlocked { get; private set; }

    public static void Block()
    {
        IsBlocked = true;
    }

    public static void Unblock()
    {
        IsBlocked = false;
    }
}
