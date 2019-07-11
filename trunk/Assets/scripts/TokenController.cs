using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using conilines.engine;
using System;
namespace conilines.unity
{
    public class TokenController : MonoBehaviour
    {
        public Vector3 GamePosition;
        private readonly float speed = 6.2f;
        public GameToken item { get; private set; }
        public bool InMotion
        {
            get
            {
                return (transform.localPosition - GamePosition).magnitude > 0.2f;
            }
        }

        public int value => (item is null) ? 0 : item.Value;
        public bool perish { get; internal set; }
        public bool death { get; internal set; }
        public bool created;// { get; internal set; }

        public bool Clicked { get; internal set; }
        public bool Created { get => created; set
            {
                created = value;
                gameObject.SetActive(!created);
            }
        }

        private void Awake()
        {
            GamePosition = transform.localPosition;
            perish = false;
            death = false;
            created = true;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
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
                death = true;
            }

            if (death)
            {
                Vector3 t = transform.localScale;
                t.x -= t.x / 20;
                transform.localScale = t;
                if (t.x < 0.1f)
                    Destroy(this.gameObject);
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
            perish = true;
        }

    }
}
