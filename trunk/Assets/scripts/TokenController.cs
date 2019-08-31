using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using conilines.engine;
using System;
namespace conilines.unity
{
    /// <summary>
    /// Visual presentaion of token
    /// </summary>
    public class TokenController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// coordinates on GameField 
        /// </summary> 
        public Vector3 GamePosition;
        /// <summary>
        /// mouse dragged token to whis coordinates
        /// </summary>
        public Vector3 DragPosition;// { get; internal set; }
        /// <summary>
        /// coordinates from where token was dragged by mouse
        /// </summary>
        public Vector3 OldPosition;

        private readonly float speed = 6.2f;
        /// <summary>
        /// points to GameToken represented
        /// </summary>
        public GameToken Item { get; private set; }
        /// <summary>
        /// Transform of object that active while whis instance selected 
        /// </summary>
        public Transform Selector;
        /// <summary>
        /// Field representation, this instance belong to
        /// </summary>
        public FieldController parent;

        /// <summary>
        /// false if token on it's position, true otherwise
        /// i.e. it's moving to posiion or already there
        /// </summary>
        public bool InMotion
        {
            get
            {
                return (transform.localPosition - GamePosition).magnitude > 0.2f;
            }
        }
        
        /// <summary>
        /// id of token which this instance represent
        /// </summary>
        public int Id => (Item is null) ? 0 : Item.ID;
        /// <summary>
        /// Value of token which this instance represent
        /// </summary>
        public int Value => (Item is null) ? 0 : Item.Value;
        /// <summary>
        /// Token is dead
        /// </summary>
        public bool Perish => (Item is null) ? true : !Item.Alive;
        /// <summary>
        /// alll finalization done and this instance ready to be removed
        /// </summary>
        public bool Death { get; internal set; }
        /// <summary>
        /// instance just created and not moved to it's place
        /// </summary>
        public bool _created{ get; internal set; }

        /// <summary>
        /// instance just created and not moved to it's place
        /// can be set to temorary disable this instance
        /// </summary>        
        public bool Created
        {
            get => _created;
            set{
                _created = value;
                gameObject.SetActive(!_created);
            }
        }
        /// <summary>
        /// Being dragged
        /// </summary>
        public bool Drag;

        private void Awake()
        {
            GamePosition = transform.localPosition;
            
            Death = false;
            _created = true;
            Drag = false;            
            Selector = transform.Find("selector");
            Selector.gameObject.SetActive(false);
            parent = null;
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
            if (parent is null) return;
            if (Drag)
            {
                if (transform.position != DragPosition)
                {
                    transform.position = Vector3.Lerp(transform.position, DragPosition, Mathf.Clamp(Time.deltaTime * 4*speed,0.1f,1.0f));
                }
                return;
            }
            if (!Created)
                if (transform.localPosition != GamePosition)
                {
                    if (Value == 0)
                    {
                        GamePosition += new Vector3(0, 0, 0.2f); ;
                        transform.localPosition = GamePosition;
                    }
                    else
                    {
                        if (Perish && (transform.localPosition - GamePosition).magnitude < 0.4f)
                        {
                            transform.localPosition = GamePosition;
                        }
                        else
                            transform.localPosition = Vector3.Lerp(transform.localPosition, GamePosition, Time.deltaTime * speed);
                    }
                }
            
            if (Perish && !InMotion)
            {
                Vector3 t = transform.localScale;
                t.x -= 1.0f / 20.0f;                
                if (t.x < 0.1f)
                {
                    Death = true;                    
                    t.x = 0.1f;
                    gameObject.name = "dead";
                    //Destroy(this.gameObject);
                }
                transform.localScale = t;
            }
            

        }

        /// <summary>
        /// <para>Set GameToken which this instance should represent,
        /// does not affect visuals </para>
        /// <para>Use only for initialisation!</para>
        /// </summary>
        /// <param name="gt"></param>
        public void Associate(GameToken gt)
        {
            Item = gt;            
            if (Value == 0)
            {
                GetComponent<BoxCollider>().enabled = false;
            }
        }

        /// <summary>
        /// Position where whit instance should be - GamePosition
        /// </summary>
        /// <param name="vector3"></param>
        internal void SetPosition(Vector3 vector3)
        {
            GamePosition = vector3;
        }

        /// <summary>
        /// Immediantly place this token to its GamePosition
        /// </summary>
        internal void Teleport()
        {
            transform.localPosition = GamePosition;
        }

        public void Kill()
        {
            //perish = true;
            Item.Kill();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {            
            Selector.gameObject.SetActive(false);
            _created = false;
            Drag = false;
            if (eventData.pointerCurrentRaycast.worldPosition != Vector3.zero)
                if (Mathf.Abs((eventData.pointerCurrentRaycast.worldPosition - OldPosition).magnitude) > 1)
            {
                    SendMessageUpwards("OnSwapTokens", new SwapTokensData(Item.ID, DragDirection(eventData.pointerCurrentRaycast.worldPosition)));
                    switch (DragDirection(eventData.pointerCurrentRaycast.worldPosition))
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

        Directions DragDirection(Vector3 newPosition)
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
                    switch (DragDirection(eventData.pointerCurrentRaycast.worldPosition))
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
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            OldPosition = transform.position;
            Selector.gameObject.SetActive(true);
            _created = true;
            Drag = true;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {

        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            //Selector.gameObject.SetActive(true);
            parent.OnCursorShow(this.transform);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            //Selector.gameObject.SetActive(false);
            parent.OnCursorHide();
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
