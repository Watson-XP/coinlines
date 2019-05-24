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
    public GameObject FieldBase;
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
        {
            Pool[i].Original.gameObject.SetActive(false);
            Pool[i].Init(GameObject.Find("Field/Pool"));
            Pool[i].Empty( );
        }
        FieldBase = GameObject.Find("Field");

        GF = new gameField(8);
        GF.SetFill(100);
        DrawField( );
    }

    private void DrawField( )
    {
        for(int i = 0; i < Pools; i++)
            Pool[i].Empty( );

        for(int i = 0; i < GF.SizeH; i++)
            for(int j = 0; j < GF.SizeL; j++)
            {
                SpriteRenderer s = Pool[GF.Get(j, i)].GetObject( );
                s.gameObject.transform.position = ToSpaceCoordinates(j, i);
                s.gameObject.transform.SetParent(FieldBase.GetComponent<Transform>( ));
                s.gameObject.name = string.Format("coin_{0}x{1}_{2}", j, i, GF.Get(j, i));
            }
    }

    private Vector3 ToSpaceCoordinates(int x, int y)
    {
        int size = 1;
        return new Vector3(x * size - 8, 4 - y * size, 0);
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

    void CreateFieldBySeed(int seed)
    {
        seed = Mathf.Abs(seed);
        GF.SetFill(seed);
        DrawField( );
    }

    private int GetSeedFromControl()
    {
        GameObject go = GameObject.Find("interface_debug/inp_seed/Text");

        if(go == null)
            return -1;

        Text If = go.GetComponent<Text>( );
        try
        {
           int newseed = System.Convert.ToInt32(If.text);
            return newseed;
        }

        catch(System.FormatException s) { return -1; }
        //return -2;
    }

    public void GameSeedUpdate()
    {

        int seed = GetSeedFromControl( );
        if(seed >= 0)
            CreateFieldBySeed(seed);
    }

    public void ClearSolutions()
    {
        GF.Clasterize( );
        GF.Slide( );
        DrawField( );
    }
}