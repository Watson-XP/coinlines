using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using coinlines;
using WSTools;

public class FieldController : MonoBehaviour
{

    private SpritePool Pool;
    // Start is called before the first frame update
    void Start()
    {
        Pool = new SpritePool(typeof(Sprite));
        
//            CreateNewObject = delegate ( ) { return GameObject.Instantiate<Sprite>( ); }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void CreateField()
    {
        int x = 0;
        int y = 0;
        for (x=0;x < TheGame.field.SizeL; x++)
            for(y = 0; y < TheGame.field.SizeH; y++)
            {

            }


    }
}