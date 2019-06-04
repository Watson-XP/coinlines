using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{    

    private void Update( )
    {   
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, 100))
            {
                TokenController tk = hit.collider.gameObject.GetComponent<TokenController>( );
                if(tk != null) tk.Clicked = true;
            }
        }
    }
}
