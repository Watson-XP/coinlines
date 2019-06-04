using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TokenController : MonoBehaviour
{
    public Vector3 GamePosition;
    private readonly float speed = 6.2f;
    public int value;

    public bool Clicked { get; internal set; }

    private void Awake( )
    {
        GamePosition = transform.position ;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != GamePosition)
        {
            if(value == 0)
            {
                GamePosition += new Vector3(0, 0, 0.2f); ;
                transform.position = GamePosition;
            }
            else
                transform.position = Vector3.Lerp(transform.position, GamePosition, Time.deltaTime * speed);
        }
    }

    public void SetPosition(Vector3 newpos)
    {
        GamePosition = newpos;
    }

}
