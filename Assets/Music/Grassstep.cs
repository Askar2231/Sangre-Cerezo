using UnityEngine;

public class Grassstep : MonoBehaviour
{
     [Header("Clips de audio")]
    public AudioClip grassAudio;

    [Header("Referencia al jugador")]
    public GameObject player;

    private AudioSource playerAudioSource;

    private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                player.GetComponent<AudioSource>().clip = grassAudio;
            }
           
            
        }


}
