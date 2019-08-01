using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using conilines.engine;
using System;
namespace conilines.unity
{
    public class TokenController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Vector3 GamePosition;
        public Vector3 DragPosition;// { get; internal set; }
        public Vector3 OldPosition;
        private readonly float speed = 6.2f;
        public GameToken item { get; private set; }
        public Transform Selector;

        public bool InMotion
        {
            get
            {
                return (transform.localPosition - GamePosition).magnitude > 0.2f;
            }
        }

        public int value => (item is null) ? 0 : item.Value;
        public bool perish => (item is null) ? false : !item.Alive; 
        public bool death { get; internal set; }
        public bool created;// { get; internal set; }

        public bool Clicked { get; internal set; }
        public bool Created
        {
            get => created; set
            {
                created = value;
                gameObject.SetActive(!created);
            }
        }
        public bool Drag;

        private void Awake()
        {
            GamePosition = transform.localPosition;
            
            death = false;
            created = true;
            Drag = false;            
            Selector = transform.Find("selector");
            Selector.gameObject.SetActive(false);
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //if (item is null)
            //    Destroy(this.gameObject);

            if (Drag)
            {
                if (transform.position != DragPosition)
                {
                    transform.position = Vector3.Lerp(transform.position, DragPosition, Mathf.Clamp(Time.deltaTime * speed,0.1f,1.0f));
                }
                return;
            }
            if (!Created)
                if (transform.localPosition != GamePosition)
                {
                    if (value == 0)
                    {
                        GamePosition += new Vector3(0, 0, 0.2f); ;
                        transform.localPosition = GamePosition;
                    }
                    else
                    {
                        if (perish && (transform.localPosition - GamePosition).magnitude < 0.4f)
                        {
                            transform.localPosition = GamePosition;
                        }
                        else
                            transform.localPosition = Vector3.Lerp(transform.localPosition, GamePosition, Time.deltaTime * speed);

                    }
                }
            
            if (perish && !InMotion)
            {
                Vector3 t = transform.localScale;
                t.x -= 1.0f / 20.0f;                
                if (t.x < 0.1f)
                {
                    death = true;
                    t.x = 0.1f;
                    //Destroy(this.gameObject);
                }
                transform.localScale = t;
            }
            

        }

        public void Associate(GameToken gt)
        {
            item = gt;
            if (value == 0)
            {
                GetComponent<BoxCollider>().enabled = false;
            }
        }

        public void updateValue()
        {
            Transform t = GameObject.Find(string.Format("Token{0}", item.Value)).transform;
            Sprite sp = t.GetComponent<SpriteRenderer>().sprite;
            if (this.GetComponent<SpriteRenderer>().sprite.name != sp.name)
            {
                this.GetComponent<SpriteRenderer>().sprite = sp;
            }

        }

        internal void SetPosition(Vector3 vector3)
        {
            GamePosition = vector3;
        }

        internal void Teleport()
        {
            transform.localPosition = GamePosition;
        }

        public void Kill()
        {
            //perish = true;
            item.Kill();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("DRAG END");
            Selector.gameObject.SetActive(false);
            created = false;
            Drag = false;
            if (eventData.pointerCurrentRaycast.worldPosition != Vector3.zero)
                if (Mathf.Abs((eventData.pointerCurrentRaycast.worldPosition - OldPosition).magnitude) > 1)
            {
                    SendMessageUpwards("OnSwapTokens", new SwapTokensData(item.ID, DragDirction(eventData.pointerCurrentRaycast.worldPosition)));
                    switch (DragDirction(eventData.pointerCurrentRaycast.worldPosition))
                    {
                        case Directions.Up:
                            DragPosition = OldPosition + Vector3.up;
                            break;
                        case Directions.Down:
                            DragPosition = OldPosition + Vector3.down;
                            break;
                        case Directions.Left:
                            DragPosition = OldPosition + Vector3.left;
                            break;
                        case Directions.Right:
                            DragPosition = OldPosition + Vector3.right;
                            break;
                    }
                }
        }

        Directions DragDirction(Vector3 newPosition)
        {
            Vector3 distance = newPosition - OldPosition;
            if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
            {
                if (distance.x > 0)
                    return Directions.Right;
                else
                    return Directions.Left;
            }
            else
            {
                if (distance.y > 0)
                    return Directions.Up;
                else
                    return Directions.Down;
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //Vector3 oldPosition = transform.position;
            //transform.position = eventData.pointerCurrentRaycast.worldPosition;
            if (eventData.pointerCurrentRaycast.worldPosition != Vector3.zero)
                if (Mathf.Abs((eventData.pointerCurrentRaycast.worldPosition - OldPosition).magnitude) < 1)
                    DragPosition = eventData.pointerCurrentRaycast.worldPosition;// - 2 * Vector3.forward;
                else
                {
                    //Vector3 distance = eventData.pointerCurrentRaycast.worldPosition - OldPosition;
                    switch (DragDirction(eventData.pointerCurrentRaycast.worldPosition))
                    {
                        case Directions.Up:
                            DragPosition = OldPosition + Vector3.up;
                            break;
                        case Directions.Down:
                            DragPosition = OldPosition + Vector3.down;
                            break;
                        case Directions.Left:
                            DragPosition = OldPosition + Vector3.left;
                            break;
                        case Directions.Right:
                            DragPosition = OldPosition + Vector3.right;
                            break;
                    }
                }

            DragPosition.z = -2;
            /*
            if ((transform.worldToLocalMatrix* eventData.pointerCurrentRaycast.worldPosition).magnitude < 5)
            //if ((transform.localPosition - GamePosition).magnitude > 4)
            {
                DragPosition = eventData.pointerCurrentRaycast.worldPosition - 2*Vector3.forward;
                
                //transform.position = eventData.pointerCurrentRaycast.worldPosition;//transform.position = oldPosition;
            }*/
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            OldPosition = transform.position;
            Selector.gameObject.SetActive(true);
            created = true;
            Drag = true;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            //Clicked = true;
            //Debug.Log(item.ToString());
            //item.NextValue();
            //updateValue();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            //Selector.gameObject.SetActive(true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            //Selector.gameObject.SetActive(false);
        }
    }

    internal class SwapTokensData
    {
        public int iD;
        public Directions directions;

        public SwapTokensData(int iD, Directions directions)
        {
            this.iD = iD;
            this.directions = directions;
        }
    }
}
