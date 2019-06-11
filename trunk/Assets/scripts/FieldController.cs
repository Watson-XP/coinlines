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
    private TokenController selected;
    private bool NeedUpdate;

    public TokenController Selected
    {
        get { return selected; }
        set
        {
            if (selected != null)
                selected.Clicked = false;
            Text txt = GameObject.Find("SelectedItemName").GetComponent<Text>( );
            txt.text = value.name;            
            selected = value;
        }
    }

    // Start is called before the first frame update
    void Start( )
    {
        Game = new TheGame( );
        //Pool = new SpritePool( ); 
        Tokens = new List<KeyValuePair<int, Transform>>( );
        Originals = new List<Transform>( );
        FillOriginals();

        FieldBase = GameObject.Find("Field");

        GF = Game.Field;
        NeedUpdate = true;
        PopulateTokens( );
        DrawField( );
    }

    private void PopulateTokens()
    {
        if (NeedUpdate)
        {
            NeedUpdate = false;
            for (int x = 0; x < GF.SizeL; x++)
                for (int y = 0; y < GF.SizeH; y++)
                {
                    GameToken t = GF[x, y];
                    Transform Tk;
                    if (Tokens.FindIndex(tk => tk.Key == t.ID) > -1)
                    {
                        Tk = Tokens.Find(tk => tk.Key == t.ID).Value;
                    }
                    else
                    {
                        Tk = AddNewToken(x, y, t);

                        NeedUpdate = true;
                    }
                    //Tk.GetComponent<TokenController>().SetPosition(ToSpaceCoordinates(x, y));
                }
        }
    }
    private void PopulateTokensOld( )
    { 
        for (int x = 0; x < GF.SizeL; x++)
            for (int y = 0; y < GF.SizeH; y++)
            {
                GameToken t = GF[x, y];
                Transform Tk;
                if (Tokens.FindIndex(tk => tk.Key == t.ID) > -1)
                {
                    Tk = Tokens.Find(tk => tk.Key == t.ID).Value;
                }
                else
                {
                    Tk = AddNewToken(x, y, t);
                }
                //Tk.GetComponent<TokenController>().SetPosition(ToSpaceCoordinates(x, y));
            }
    }

    private void CleanUpTockens()
    {
        Queue<int> todelete = new Queue<int>();
        Tokens.ForEach(delegate (KeyValuePair<int, Transform> d)
        {
            try
            {
                if (!GF.HaveID(d.Key))
                {
                    todelete.Enqueue(d.Key);
                    if (d.Value.GetComponent<TokenController>().Follower != null)
                    {
                        int idx = Tokens.FindIndex(
                            tok => tok.Value.GetComponent<TokenController>().Follower == 
                                d.Value.GetComponent<TokenController>()
                        );
                        if (idx > -1)
                        {
                            Tokens[idx].Value.GetComponent<TokenController>().Follower =
                                d.Value.GetComponent<TokenController>().Follower;
                        }
                    }
                    Destroy(d.Value.gameObject);
                }
            }
            catch (System.Exception) { }
        });
        while (todelete.Count > 0)
        {
            int key = todelete.Dequeue();
            int idx = Tokens.FindIndex(t => t.Key == key);
            //Tokens.Remove(t => t.Key == todelete.Dequeue());
            if (idx > -1)
            {
                Tokens.RemoveAt(idx);
            }
        }
        
        NeedUpdate = true;
    }

    private Transform AddNewToken(int x, int y, GameToken t)
    {
        Transform Tk = Instantiate(Originals[t.Value]).transform;
        Tk.parent = FieldBase.transform;
        Tk.GetComponent<TokenController>().Owner = GF[x, y];
            //value = GF[x, y].Value;
        //Tk.position = ToSpaceCoordinates(x, y);
        //Tk.GetComponent<TokenController>( ).SetPosition(ToSpaceCoordinates(x, y));        
        Tk.gameObject.name = string.Format("it_{0}", GF[x, y].ID);
        Tk.gameObject.SetActive(true);
        Vector3 place,step;
        switch (GF.SlideDirection)
        {
            case Directions.Up:
                place = ToSpaceCoordinates(x, GF.SizeH);
                step = ToSpaceCoordinates(x, GF.SizeH-1);
                break;
            case Directions.Down:
                place = ToSpaceCoordinates(x, -1);
                step = ToSpaceCoordinates(x, 0);
                break;
            case Directions.Left:
                place = ToSpaceCoordinates(GF.SizeL, y);
                step = ToSpaceCoordinates(GF.SizeL - 1, y);
                break;
            case Directions.Right:
                place = ToSpaceCoordinates(-1, y);
                step = ToSpaceCoordinates(0, y);
                break;
            default:
                place = ToSpaceCoordinates(0, 0);
                step = ToSpaceCoordinates(0, 0);
                break;
        }
        Tk.position = place;// (ToSpaceCoordinates(x, y + 10));        

        int idx = Tokens.FindIndex(tok => tok.Value.position == place && tok.Value.GetComponent<TokenController>().Follower == null);
        if (idx > -1)
        {
            Tokens[idx].Value.GetComponent<TokenController>().Follower = Tk.GetComponent<TokenController>();
            Tokens[idx].Value.GetComponent<TokenController>().FollowStartPosition = step;
            Tk.GetComponent<TokenController>().Paused = true ;
            Tk.GetComponent<TokenController>().gameObject.SetActive(false);
        }
        else
        {
            Tk.GetComponent<TokenController>().Paused = false;
        }
        Tokens.Add(new KeyValuePair<int, Transform>(GF[x, y].ID, Tk));

        return Tk;
    }

    private void FillOriginals( )
    {
        Originals.Clear( );
        for(int i = 0; i < 5; i++)
        {
            //Transform t = GameObject.Find(string.Format("Token{0}", i)).transform;
            Transform t = GameObject.Find("coins").transform.Find(string.Format("Token{0}", i));
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

    internal  Vector3 ToSpaceCoordinates(int x, int y)
    {
        
        return new Vector3(x * GameToken.size - 7, 4 - y * GameToken.size, 0);
    }

    internal Vector2 ToGameCoordinates(float x, float y, float z )
    {
        return ToGameCoordinates(new Vector3(x, y, z));
    }

    internal Vector2 ToGameCoordinates(Vector3 position)
    {
        return new Vector2(position.x + 7/ GameToken.size, (-4- position.y) / GameToken.size);
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
        CleanUpTockens();
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
        GF.Fill();
        CleanUpTockens();
        PopulateTokens( );
        DrawField( );
    }
}