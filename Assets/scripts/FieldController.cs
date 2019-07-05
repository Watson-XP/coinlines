using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using conilines.engine;
using conilines.unity;
using WSTools;
using UnityEngine.UI;
public enum FieldStates
{
    Ready, Slide,
    Removing
}
public class FieldController : MonoBehaviour
{
    //      Well, on a second thought, maybe pool is not required: items addede to a board
    //  only when field created and sometime when new required. At most 1/s for a 2-3 sec at all.
    //  private SpritePool Pool;
    private List<KeyValuePair<int, Transform>> Tokens;
    private List<Transform> Originals;
    private GameField GF;
    public FieldStates State;
    private bool autofill;
    public GameObject FieldBase;

    public TheGame Game;
    public List<TokenController> NewTokens;

    private void Awake()
    {
        Game = Director.Game;
        if (FieldBase == null)
        {
            Director.Field = this;
            autofill = false;
        }
        else
            autofill = true;
    }

    // Start is called before the first frame update
    void Start()
    {

        //Pool = new SpritePool( ); 
        Tokens = new List<KeyValuePair<int, Transform>>();
        Originals = new List<Transform>();
        FillOriginals();

        if (FieldBase == null)
            FieldBase = GameObject.Find("Field/Pool");

        GF = Game.Field;
        State = FieldStates.Ready;
    }

    // Update is called once per frame
    void Update()
    {
        if (autofill)  // nonmanageable mode always show 'map' :)
        {
            ClearDestroyedTokens();
            AddNewTokens();
            DrawField();
            return;
        }

        switch (State)
        {
            case FieldStates.Removing:
                if (Tokens.FindIndex(tk => tk.Value.GetComponent<TokenController>().perish) == -1)
                    State = FieldStates.Ready;
                break;
        }

    }

    private void FillOriginals() //init arrray of token prefabs
    {
        Originals.Clear();
        for (int i = 0; i < 5; i++)
        {
            Transform t = GameObject.Find(string.Format("Token{0}", i)).transform;
            Originals.Add(t);
        }
    }

    public void DrawField()// Place all known tokens to their respective places
    {
        Transform tmp;
        for (int i = 0; i < GF.SizeH; i++)
            for (int j = 0; j < GF.SizeL; j++)
            {
                //SpriteRenderer s = Pool[GF[j, i].ID].GetObject( );
                int id = GF[j, i].ID;
                tmp = Tokens.Find(x => x.Key == id).Value;
                if (!(tmp is null))
                {
                    //tmp.GetComponent<TokenController>( ).SetPosition(ToSpaceCoordinates(j, i));
                    tmp.localPosition = ToSpaceCoordinates(j, i);
                    tmp.GetComponent<TokenController>().Created = false;
                }
            }
    }


    public void PopulateTokens()
    {
        ClearDestroyedTokens();
        AddNewTokens();
    }

    public void AddNewTokens() // Add tokens created on field
    {
        NewTokens = new List<TokenController>();

        for (int x = 0; x < GF.SizeL; x++)
            for (int y = 0; y < GF.SizeH; y++)
            {
                GameToken t = GF[x, y];
                Transform Tk;
                if (Tokens.FindIndex(tk => tk.Key == t.ID) > -1)
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
                    Tk.GetComponent<TokenController>().Associate(GF[x, y]);

                    Tk.localPosition = ToSpaceCoordinates(x, GF.SizeH + 1);
                    Tk.GetComponent<TokenController>().SetPosition(ToSpaceCoordinates(x, y));
                    NewTokens.Add(Tk.GetComponent<TokenController>());

                    Tokens.Add(new KeyValuePair<int, Transform>(GF[x, y].ID, Tk));
                    Tk.gameObject.name = string.Format("it_{0}[{1},{2}]", GF[x, y].ID, x, y);

                }
                //Tk.GetComponent<TokenController>().SetPosition(ToSpaceCoordinates(x, y));
            }
    }

    public void ClearDestroyedTokens() // Send kill command to Tokens removed from field 
    {
        Tokens.ForEach(delegate (KeyValuePair<int, Transform> d)
        {
            try
            {
                if (!GF.HaveID(d.Key))
                {
                    d.Value.GetComponent<TokenController>().DoDestroy();
                }
            }
            catch (System.Exception) { }
        });
        State = FieldStates.Removing;
    }

    public void SlideTokens()
    {
        State = FieldStates.Slide;
        List<TokenController> tks = new List<TokenController>();
        Tokens.FindAll(tk => tk.Value.GetComponent<TokenController>().InMotion).
            ForEach(delegate (KeyValuePair<int, Transform> kv) {
                tks.Add(kv.Value.GetComponent<TokenController>());                
            });

    }

    public Vector3 ToSpaceCoordinates(int x, int y)
    {
        int size = 1;
        return new Vector3(x * size - 8, 4 - y * size, 0);
    }

    private int GetSeedFromControl()
    {
        GameObject go = GameObject.Find("interface_debug/inp_seed/Text");

        if (go == null)
            return -1;

        Text If = go.GetComponent<Text>();
        try
        {
            int newseed = System.Convert.ToInt32(If.text);
            return newseed;
        }

        catch (System.FormatException) { return -1; }
        //return -2;
    }

    public void ClearSolutions()
    {
        //        GF.Clasterize( );
        //GF.GetLines( );
        //if (GF.Slide( ))
        GF.Fill(true);
        PopulateTokens();
        DrawField();
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

    public List<TokenController> GetMovingTokens()
    {
        List<TokenController> res = new List<TokenController>();
        foreach (KeyValuePair<int, Transform> kvp in Tokens)
        {
            if (kvp.Value)
                if (kvp.Value.GetComponent<TokenController>().InMotion)
                {
                    res.Add(kvp.Value.GetComponent<TokenController>());
                }
        }
        return res;
    }
}