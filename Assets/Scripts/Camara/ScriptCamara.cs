using UnityEngine;

public class ThirdPersonJRPGCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; // Player
    public Vector3 offset = new Vector3(0, 2f, -4f);

    [Header("Rotación Orbital")]
    public float mouseXSens = 180f;
    public float mouseYSens = 120f;
    public float minPitch = -10f;
    public float maxPitch = 60f;

    [Header("Auto Follow")]
    public float autoFollowDelay = 2f;   // segundos sin input
    public float autoFollowSpeed = 5f;   // suavidad (AJUSTADO, más lento y natural)

    [Header("Modo Combate")]
    public float combatDistance = -6f;
    public float combatHeight = 3f;
    public float transitionSpeed = 3f;

    float yaw;
    float pitch;
    float lastInputTime;
    bool inCombat = false;

    void Start()
    {
        if (!target) return;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Entrada mouse
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            yaw += mouseX * mouseXSens * Time.deltaTime;
            pitch -= mouseY * mouseYSens * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            lastInputTime = Time.time;
        }
        else if (Time.time - lastInputTime > autoFollowDelay)
        {
            // Auto-follow más suave → interpola hacia el forward del jugador
            float targetYaw = target.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, targetYaw, autoFollowSpeed * Time.deltaTime);
        }

        // --- Offset según modo ---
        Vector3 desiredOffset = offset;
        if (inCombat)
            desiredOffset = new Vector3(offset.x, combatHeight, combatDistance);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 finalOffset = Vector3.Lerp(offset, desiredOffset, transitionSpeed * Time.deltaTime);

        transform.position = target.position + rotation * finalOffset;
        transform.rotation = rotation;
    }

    public void SetCombatMode(bool active)
    {
        inCombat = active;
    }
}


