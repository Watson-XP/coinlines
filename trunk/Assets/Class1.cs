using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ObjectPool;
namespace WSTools
{
    class SpritePool : ObjectPool<SpriteRenderer>
    {
        private GameObject PoolParent;

        public SpritePool(Type T) : base(T)
        {
        }

        public void Init(GameObject patent)
        {
            PoolParent = patent;
        }

        public override Sprite CreateNewObject( )
        {
            Sprite tmp = GameObject.Instantiate<Sprite>( );
            tmp.
            tmp.gameObject.SetActive(true);
            return tmp;
        }


    }
}
