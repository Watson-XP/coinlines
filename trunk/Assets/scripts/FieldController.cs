using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using conilines.engine;
using conilines.unity;
using WSTools;
using UnityEngine.UI;
public enum FieldStates { Ready, Slide }
public class FieldController: MonoBehaviour
{
    //      Well, on a second thought, maybe pool is not required: items addede to a board
    //  only when field created and sometime when new required. At most 1/s for a 2-3 sec at all.
    //  private SpritePool Pool;
    private List<KeyValuePair<int, Transform>> Tokens;
    private List<Transform> Originals;
    private GameField GF;
    public  FieldStates State;

    public GameObject FieldBase;

    public TheGame Game;
    public List<TokenController> NewTokens;

    private void Awake()
    {
        Game = Director.Game;
        Director.Field = this;
    }

    // Start is called before the first frame update
    void Start( )
    {

        //Pool = new SpritePool( ); 
        Tokens = new List<KeyValuePair<int, Transform>>( );
        Originals = new List<Transform>( );
        FillOriginals( );

        FieldBase = GameObject.Find("Field/Pool");

        GF = Game.Field;
        
        State = FieldStates.Ready;
    }

    public void PopulateTokens( )
    {
        Tokens.ForEach(delegate (KeyValuePair<int, Transform> d) {
            try
            {
                if (!GF.HaveID(d.Key))
                {
                    d.Value.GetComponent<TokenController>().DoDestroy();
                }
            }
            catch(System.Exception){ }
        });
        //Tokens.Clear( );
        NewTokens = new List<TokenController>();

        for (int x = 0; x < GF.SizeL; x++)
            for(int y = 0; y < GF.SizeH; y++)
            {
                GameToken t = GF[x, y];
                Transform Tk;
                if(Tokens.FindIndex(tk => tk.Key == t.ID) > -1)
                {
                     Tk = Tokens.Find(tk => tk.Key == t.ID).Value;
                    Tk.GetComponent<TokenController>().updateValue();
                    Tk.gameObject.name = string.Format("!it_{0}[{1},{2}]", GF[x, y].ID, x, y);
                    Tk.GetComponent<TokenController>().SetPosition(ToSpaceCoordinates(x, y));
                }
                else
                {
                    Tk = Instantiate(Originals[t.Value]).transform;
                    Tk.parent = FieldBase.transform;
                    Tk.GetComponent<TokenController>( ).Associate(GF[x, y]);                    
                    
                    Tk.position = ToSpaceCoordinates(x, GF.SizeH+1);
                    Tk.GetComponent<TokenController>( ).SetPosition(ToSpaceCoordinates(x, y));
                    NewTokens.Add(Tk.GetComponent<TokenController>());

                    Tokens.Add(new KeyValuePair<int, Transform>(GF[x, y].ID, Tk));
                    Tk.gameObject.name = string.Format("it_{0}[{1},{2}]", GF[x, y].ID,x,y);

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

    public void DrawField( )
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

    public Vector3 ToSpaceCoordinates(int x, int y)
    {
        int size = 1;
        return new Vector3(x * size - 8, 4 - y * size, 0);
    }

    // Update is called once per frame
    void Update( )
    {
        if (State == FieldStates.Slide)
        {
            GF.Fill(true);            
            PopulateTokens();
            DrawField();
            State = FieldStates.Ready;
            return;
        }
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

    public void ClearSolutions( )
    {
        //        GF.Clasterize( );
        //GF.GetLines( );
        //if (GF.Slide( ))
        GF.Fill( true );
        PopulateTokens( );
        DrawField( );
    }

    public TokenController ClickHandle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                TokenController tk = hit.collider.gameObject.GetComponent<TokenController>();
                if (tk != null)
                {
                    tk.Clicked = true;
                    tk.item.NextValue();
                }
                
                Text nm = GameObject.Find("SelectedItemName").GetComponent<Text>();
                if (nm != null)
                {
                    nm.text = string.Format("[{0}] {1}", tk.name, tk.value);
                }
                return tk;
            }
        }
        return null;
    }
}