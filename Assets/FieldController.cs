using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using coinlines;

public class FieldController : MonoBehaviour
{

    private List<Sprite> Pool;
    // Start is called before the first frame update
    void Start()
    {
        
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
