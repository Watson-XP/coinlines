using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using coinlines;
using WSTools;
using UnityEngine.UI;

public class FieldController: MonoBehaviour
{

    private List<SpritePool> Pool;
    private int Pools = 5;
    private gameField GF= TheGame.field;
    // Start is called before the first frame update
    void Start( )
    {
        Pool = new List<SpritePool>( );
        for(int i = 0; i < Pools; i++)
            Pool.Add(new SpritePool(typeof(SpriteRenderer)));
        Pool[0].Original = GameObject.Find("placeholder").GetComponent<SpriteRenderer>( );
        Pool[1].Original = GameObject.Find("placececoin1").GetComponent<SpriteRenderer>( );
        Pool[2].Original = GameObject.Find("placececoin2").GetComponent<SpriteRenderer>( );
        Pool[3].Original = GameObject.Find("placececoin3").GetComponent<SpriteRenderer>( );
        Pool[4].Original = GameObject.Find("placececoin4").GetComponent<SpriteRenderer>( );

        for(int i = 0; i < Pools; i++)
        { Pool[i].Original.gameObject.SetActive(false); }

        GF = new gameField(8);
        GF.SetFill(100);
        DrawField( );
    }

    private void DrawField( )
    {
        int size = 1;
        for(int i = 0; i < GF.SizeH; i++)
            for(int j = 0; j < GF.SizeL; j++)
            {
                SpriteRenderer s = Pool[GF.Get(i, j)].GetObject( );
                s.gameObject.transform.position = new Vector3(i * size - 8, 4 - j * size, 0);
                s.gameObject.name = string.Format("coin_{0}x{1}_{2}", i, j, GF.Get(i, j));
            }
    }

    // Update is called once per frame
    void Update( )
    {

    }


    void CreateField( )
    {
        int x = 0;
        int y = 0;
        for(x = 0; x < TheGame.field.SizeL; x++)
            for(y = 0; y < TheGame.field.SizeH; y++)
            {

            }
    }

    public void GameSeedUpdate()
    {
        GameObject go = GameObject.Find("interface_debug/inp_seed/Text");
        if(go == null) return;
        Text If = go.GetComponent<Text>( );
        int newseed = System.Convert.ToInt32(If.text);
        newseed = Mathf.Abs(newseed);
        GF.SetFill(newseed);
        for(int i = 0; i < Pools; i++)
            Pool[i].Empty( );
        DrawField( );
    }
}