using UnityEngine;

public class TransicionesAnim : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey("w"))
        {
            anim.SetBool("semueve", true);
        }
        else if (Input.GetKey("s"))
        {
            anim.SetBool("semueve", true);
        }
        else if (Input.GetKey("d"))
        {
            anim.SetBool("semueve", true);
        }
        else if (Input.GetKey("a"))
        {
            anim.SetBool("semueve", true);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            anim.SetBool("corre", true);
            Debug.Log("se esta oprimientdo");
        }
        else
        {
            anim.SetBool("semueve", false);
            anim.SetBool("corre", false);
        }
    }
}

