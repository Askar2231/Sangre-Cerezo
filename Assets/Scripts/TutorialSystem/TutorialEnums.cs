using UnityEngine;

/// <summary>
/// Tipo de tutorial a mostrar
/// </summary>
public enum TutorialType
{
    SimpleDialog,   // Diálogo simple en la parte superior
    CardPopover     // Tarjetas emergentes con imágenes
}

/// <summary>
/// Posición del anclaje del texto para diálogos simples
/// </summary>
public enum DialogPosition
{
    TopLeft,
    TopCenter,
    TopRight
}

/// <summary>
/// Tipo de input del jugador
/// </summary>
public enum InputDeviceType
{
    Keyboard,
    XboxController,
    PlayStationController,
    GenericGamepad
}

/// <summary>
/// Acciones de input que se pueden mostrar como iconos
/// </summary>
public enum InputAction
{
    Move,
    Run,
    Interact,
    QTE,
    Parry,
    Confirm,
    Cancel,
    LightAttack,
    HeavyAttack,
    Skill1,
    Skill2,
    EndTurn
}
