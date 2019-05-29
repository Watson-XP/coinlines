using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using conilines.engine;
using WSTools;
using UnityEngine.UI;

public class FieldController: MonoBehaviour
{
    //      Well, on a second thought, maybe pool is not required: items addede to a board
    //  only when field created and sometime when new required. At most 1/s for a 2-3 sec at all.
    //  private SpritePool Pool;
    private List<KeyValuePair<int, Transform>> Tokens;
    private List<Transform> Originals;
    private GameField GF;

    public GameObject FieldBase;

    public TheGame Game;
    // Start is called before the first frame update
    void Start( )
    {
        Game = new TheGame( );
        //Pool = new SpritePool( ); 
        Tokens = new List<KeyValuePair<int, Transform>>( );
        Originals = new List<Transform>( );
        FillOriginals( );

        FieldBase = GameObject.Find("Field");

        GF = Game.Field;

        PopulateTokens( );
        DrawField( );
    }

    private void PopulateTokens( )
    {
        Tokens.ForEach(delegate (KeyValuePair<int, Transform> d) {
            try
            {
                if (!GF.HaveID(d.Key))
                    Destroy(d.Value.gameObject);
            }
            catch(System.Exception){ }
        });
        //Tokens.Clear( );

        for(int x = 0; x < GF.SizeL; x++)
            for(int y = 0; y < GF.SizeH; y++)
            {
                GameToken t = GF[x, y];
                Transform Tk;
                if(Tokens.FindIndex(tk => tk.Key == t.ID) > -1)
                {
                     Tk = Tokens.Find(tk => tk.Key == t.ID).Value;
                }
                else
                {
                    Tk = Instantiate(Originals[t.Value]).transform;
                    Tk.parent = FieldBase.transform;
                    Tk.GetComponent<TokenController>( ).value = GF[x, y].Value;
                    //Tk.position = ToSpaceCoordinates(x, y);
                    //Tk.GetComponent<TokenController>( ).SetPosition(ToSpaceCoordinates(x, y));
                    Tokens.Add(new KeyValuePair<int, Transform>(GF[x, y].ID, Tk));
                    Tk.gameObject.name = string.Format("it_{0}", GF[x, y].ID);
                }
                //Tk.GetComponent<TokenController>().SetPosition(ToSpaceCoordinates(x, y));                
                
            }
    }

    private void FillOriginals( )
    {
        Originals.Clear( );
        for(int i = 0; i < 5; i++)
        {
            Transform t = GameObject.Find(string.Format("Token{0}", i)).transform;
            Originals.Add(t);
        }
    }

    private void DrawField( )
    {
        Transform tmp;
        for(int i = 0; i < GF.SizeH; i++)
            for(int j = 0; j < GF.SizeL; j++)
            {
                //SpriteRenderer s = Pool[GF[j, i].ID].GetObject( );
                int id = GF[j, i].ID;
                tmp = Tokens.Find(x => x.Key == id).Value;
                tmp.GetComponent<TokenController>( ).SetPosition(ToSpaceCoordinates(j, i));
                //tmp.position = ToSpaceCoordinates(j, i);
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

    void CreateFieldBySeed(int seed)
    {
        seed = Mathf.Abs(seed);
        Game.CreateField(seed);
        GF = Game.Field;
        PopulateTokens( );

        DrawField( );
    }

    private int GetSeedFromControl( )
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

        catch(System.FormatException) { return -1; }
        //return -2;
    }

    public void GameSeedUpdate( )
    {

        int seed = GetSeedFromControl( );
        if(seed >= 0)
            CreateFieldBySeed(seed);
    }

    public void ClearSolutions( )
    {
        //        GF.Clasterize( );
        GF.GetLines( );
        GF.Slide( );
        PopulateTokens( );
        DrawField( );
    }
}