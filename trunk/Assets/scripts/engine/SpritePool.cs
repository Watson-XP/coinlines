using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ObjectPool;

namespace WSTools
{
    public class SpritePool : ObjectIdPool<SpriteRenderer>
    {
        private GameObject PoolParent;
        private SpriteRenderer original;
        public SpriteRenderer Original
        {
            get { return original; }
            set {
                Empty(true );
                original = value;
                original.gameObject.SetActive(false);
            }
        }

        public SpritePool(Type T) : base()
        {
        }

        public void Init(GameObject patent)
        {
            PoolParent = patent;
        }

        public override SpriteRenderer CreateNewObject( )
        {
            SpriteRenderer tmp = GameObject.Instantiate<SpriteRenderer>(Original);
            //tmp.gameObject.SetActive(true);
            tmp.gameObject.SetActive(false);
            return tmp;
        }
        public override void DestroyObject(SpriteRenderer obj)
        {
            GameObject.Destroy(obj.gameObject);
        }

        public new SpriteRenderer GetObject()
        {
            SpriteRenderer s = base.GetObject( );
            s.gameObject.SetActive(true);
            return s;
        }

    }
}
