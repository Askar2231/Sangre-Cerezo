using UnityEngine;

[System.Serializable] // <- opcional, pero útil si quieres ver los datos en el inspector
public class PlayerSnapshot
{
    public Vector3 position;
    public Quaternion rotation;
    public Transform parent;

    public bool movementEnabled;

    public bool rbIsKinematic;
    public Vector3 rbVelocity;
}
