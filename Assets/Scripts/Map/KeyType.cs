/// <summary>
/// Enum de tipos de llaves que pueden existir en el juego.
/// Cada llave desbloquea puertas específicas.
/// </summary>
public enum KeyType
{
    LobbyKey,        // Llave del lobby
    GalleryKey,      // Llave de la galería
    BedroomKey,      // Llave de la habitación
    BasementKey,     // Llave del sótano
    StudyKey,        // Llave del estudio (Cap 1 → Cap 2)
    SmallKey,        // Llave pequeña del archivador (Cap 3 → Estudio archivador)
    SingleUseKey     // Llave de un solo uso (Cap 4 → Ala Norte, se consume al usarla)
}