using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using conilines.engine;

public class TokenController: MonoBehaviour
{
    public Vector3 GamePosition;
    private readonly float speed = 2.2f;
    public int value { get { return Owner.Value; } }
    public int ID { get { return Owner.ID; } }
    FieldController parent;
    public GameToken Owner;

    public bool Paused;
    public TokenController Follower;
    public Vector3 FollowStartPosition;

    public static bool operator ==(TokenController a, TokenController b)
    {
        if (object.ReferenceEquals(a, null))
        {
            return object.ReferenceEquals(b, null);
        }

        return a.Equals(b);
    }

    public static bool operator !=(TokenController a, TokenController b) => !(a == b);
    private float ItemSize => GetComponent<BoxCollider>().size.x;

    public bool Clicked
    {
        get { return false; }
        set
        {
            if(value == true)
                if(parent != null)
                {
                    parent.Selected = this;
                }
        }
    }

    private void Awake( )
    {
        gameObject.SetActive(true);
        if (parent != null)
        {
            if (parent.Selected == this)
                if (Input.GetMouseButtonDown(0))
                {
                    //ToSpaceCoordinates
                }
        }
        else
        {
            GamePosition = transform.position;
            try
            {
                parent = GameObject.Find("Field").GetComponent<FieldController>();
            }
            catch (System.Exception) { Debug.Log("Field object not found"); }            
        }
        Follower = null;
        FollowStartPosition = Vector3.zero;
        Paused = false;
    }
    // Start is called before the first frame update
    void Start( )
    {

    }

    // Update is called once per frame
    void Update( )
    {
        if (Paused) return;

        if(transform.position != GamePosition)
        {
            if (value == 0)
            {
                //GamePosition += new Vector3(0, 0, 0.2f); ;
                transform.position = GamePosition;
            }
            else
            {
//                if ((transform.position - GamePosition).magnitude > ItemSize )
//                    transform.position += ((GamePosition - transform.position).normalized);
//                else
                    transform.position = Vector3.Lerp(transform.position, GamePosition, Time.deltaTime * speed);
                if (Follower != null)
                {
                    if ((transform.position.x - FollowStartPosition.x) < -0.5f)
                    {
                        Follower.Paused = false;
                        Follower.gameObject.SetActive(true);
                        Follower = null;
                        FollowStartPosition = Vector3.zero;                        
                    }
                }
            }
        }
    }

    public void SetPosition(Vector3 newpos)
    {
        GamePosition = newpos;
    }

    public override bool Equals(object obj)
    {     
        if (obj == null) return false;
        if (obj.GetType() != typeof(TokenController)) return false;
        TokenController tok = (TokenController)obj;
        return (this.ID == tok.ID);
    }
}
