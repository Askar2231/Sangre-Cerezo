using UnityEngine;

public class Movimientosprite : MonoBehaviour
{
    public float amplitud = 1f; 
    public float velocidad = 2f;
    private Vector3 posicionInicial;
    public Transform MyCameraTransform;
	private Transform MyTransform;
	public bool alignNotLook = true;


    void Start()
    {
        posicionInicial = transform.position;
        MyTransform = this.transform;
		MyCameraTransform = Camera.main.transform;
    }
    void Update()
    {
        float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * velocidad) * amplitud;
        transform.position = new Vector3(posicionInicial.x, nuevaY, posicionInicial.z);
    }
    private void LateUpdate()
    {
        if (alignNotLook)
			MyTransform.forward = MyCameraTransform.forward;
		else
			MyTransform.LookAt (MyCameraTransform, Vector3.up);
	
    }
}
