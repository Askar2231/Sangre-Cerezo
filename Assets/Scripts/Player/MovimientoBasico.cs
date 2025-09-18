using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovimientoBasico : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 4.5f;
    public float rotationSpeed = 720f;
    public Transform cameraTransform;

    CharacterController cc;

    void Awake() => cc = GetComponent<CharacterController>();

    void Update()
    {
        // --- Entrada WASD ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f);

        // --- Dirección relativa a la cámara ---
        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cameraTransform.right;
        Vector3 moveDir = (camForward * input.z + camRight * input.x).normalized;

        // --- Si hay input, rotamos hacia la dirección de movimiento ---
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // --- Movimiento final ---
        cc.SimpleMove(moveDir * moveSpeed);
    }
}




