/// <summary>
/// Interfaz desacoplada para el sistema de puzzles.
/// Otro desarrollador implementará la lógica del puzzle.
/// El sistema de progresión solo necesita saber si el puzzle fue resuelto.
/// </summary>
public interface IPuzzleGate
{
    /// <summary>
    /// ID único del puzzle (ej: "puzzle_estudio_archivador").
    /// </summary>
    string PuzzleId { get; }

    /// <summary>
    /// True si el puzzle ya fue resuelto.
    /// </summary>
    bool IsSolved { get; }

    /// <summary>
    /// Llamado por el sistema externo de puzzles cuando el jugador resuelve el puzzle.
    /// Internamente debe liberar la llave/puerta bloqueada.
    /// </summary>
    void OnPuzzleSolved();
}
