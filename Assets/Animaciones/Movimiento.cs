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

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown("w"))
            {
                anim.SetBool("corre", true);

            }
            else
            {
                anim.SetBool("corre", false);
            }
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
        else
        {
            anim.SetBool("semueve", false);
            
        }
    }
}

