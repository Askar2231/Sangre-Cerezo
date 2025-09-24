using UnityEngine;

public class ParryIconSpawner : MonoBehaviour
{
    public GameObject parryIconPrefab; // una imagen UI con tu estrella
    public Canvas canvas; // referencia al canvas de batalla

    public void ShowParryIcon(Transform enemyTransform, float parryWindowDuration)
    {
        if (parryIconPrefab == null || canvas == null) return;

        GameObject icon = Instantiate(parryIconPrefab, canvas.transform);
        ParryIcon parryIcon = icon.GetComponent<ParryIcon>();
        if (parryIcon != null)
        {
            parryIcon.Initialize(enemyTransform, parryWindowDuration, Vector3.up * 2f);
        }
    }
}

