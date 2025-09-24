using UnityEngine;

public class ParryIcon : MonoBehaviour
{
    private float duration;
    private float timer;
    private Transform target; // el enemigo al que sigue
    private Vector3 offset;   // para que quede encima de la cabeza

    private RectTransform rectTransform;

    public void Initialize(Transform target, float duration, Vector3 offset)
    {
        this.target = target;
        this.duration = duration;
        this.offset = offset;
        this.timer = 0f;

        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (target == null) return;

        // Seguir al enemigo (posición en pantalla)
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        transform.position = screenPos;

        // Animación (escala: grande al inicio, se reduce después)
        timer += Time.deltaTime;
        float t = timer / duration;

        if (t <= 0.5f)
        {
            // primera mitad: escalar hacia arriba
            float scale = Mathf.Lerp(0.5f, 1.5f, t * 2f);
            rectTransform.localScale = Vector3.one * scale;
        }
        else
        {
            // segunda mitad: escalar hacia abajo
            float scale = Mathf.Lerp(1.5f, 0f, (t - 0.5f) * 2f);
            rectTransform.localScale = Vector3.one * scale;
        }

        // destruir cuando termina
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}

