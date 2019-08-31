using conilines.engine;
using System.Collections.Generic;
using UnityEngine;

namespace conilines.unity
{
    /// <summary>
    /// Possible state of Field Renderer
    /// </summary>
    public enum FieldStates
    {
        Ready, Slide,
        Cleanup,
        Hold,
        Init
    }
    public class FieldController : MonoBehaviour
    {
        public GameField FieldData;
        private List<TokenController> Tokens;
        private TokenController[] TokenOriginals;
        private FieldStates state;
        private List<TokenController> SlidingTokens;
        private bool associated;
        public Transform cursor;

        UnityEngine.UI.Text DebugText2;
        

        public FieldStates State => state;// { get { return state; } set { StateQueue.Enqueue(value); } }


        private void Awake()
        {
            state = FieldStates.Hold;
            SlidingTokens = new List<TokenController>();
        }

        // unity events
        private void Start()
        {
            Tokens = new List<TokenController>();
            LoadOriginals();
            associated = false;
            GameObject cObj = GameObject.Find("SelectCursor");
            Debug.LogFormat("cursor:", cObj.name);
            cursor = cObj.GetComponent<Transform>();
            //cursor = GameObject.Find("SelectCursor").transform;

            DebugText2 = GameObject.Find("Debugtext2").GetComponent<UnityEngine.UI.Text>();
        }

        public void OnGameFieldInit()
        {
            FieldData = TheGame.Me.Field;
            CleanUpTokens();

            FieldData.TokensAdded += OnTokensAdd;
            FieldData.TokensKilled += OnTokensKilled;
            FieldData.FieldChanged += OnFieldChanged;
            associated = true;
        }
        private void Update()
        {
            switch (State)
            {
                case FieldStates.Init:
                    OnGameFieldInit();
                    break;
                case FieldStates.Hold:
                    break;
                case FieldStates.Slide:
                    ReleaseNextSlide();
                    break;
                case FieldStates.Cleanup:                    
                    foreach (TokenController tc in SlidingTokens.FindAll(tk => tk.death))
                        Destroy(tc.gameObject);
                    Tokens.RemoveAll(tk => tk.death);
                    SlidingTokens.RemoveAll(tk => tk.death);
                    if (SlidingTokens.Count == 0) state = FieldStates.Ready;
                    break;
            }
        }


        //  External functions :)
        public void FillSlideList()
        {
            
            for (int x = 0; x < FieldData.FieldLength; x++)
                for (int y = 0; y < FieldData.FieldHeight; y++)
                {
                    int idx = Tokens.FindIndex(tc => tc.item.ID == FieldData[x, y].ID);
                    if (idx > -1)
                    {
                        Tokens[idx].SetPosition(LocalPosition(x, y));
                        if (Tokens[idx].InMotion)
                            SlidingTokens.Add(Tokens[idx]);
                    }
                }
            foreach (TokenController tk in Tokens)
                if (tk.Created) SlidingTokens.Add(tk);

            if (SlidingTokens.Count > 0)
                state = FieldStates.Slide;
            else
                state = FieldStates.Ready;
        }
        public void TeleportTokensInPlace()
        {            
            foreach (TokenController tk in Tokens)
            {
                tk.Created = false;
                tk.Teleport();
            }
        }
        public static Vector3 LocalPosition(int x, int y)
        {
            int size = 1;
            int sx = 1;
            int sy = 1;
            return new Vector3(x: (x * size) + sx, y: (y * size) + sy, z: 0);
        }
        public void SyncField()
        {
            if (associated)
            {
                FieldData.TokensAdded -= OnTokensAdd;
                FieldData.TokensKilled -= OnTokensKilled;
                FieldData.FieldChanged -= OnFieldChanged;
            }
            OnGameFieldInit();            
            RespawnTokens();
            state = FieldStates.Ready;
        }

        private void RespawnTokens()
        {
            for (int x = 0; x<FieldData.FieldLength; x++)
                for (int y = 0; y < FieldData.FieldHeight; y++)                
                    SpawnNewToken(x, y, true);
        }

        private void CleanUpTokens()
        {
            foreach (TokenController tk in Tokens)
                GameObject.Destroy(tk?.gameObject);
            Tokens.Clear();            
        }

        //  Internal functions :D
        private void LoadOriginals()
        {
            TokenOriginals = new TokenController[TheGame.maxIndex];
            for (int i = 0; i < TheGame.maxIndex; i++)
            {
                TokenOriginals[i] = GameObject.Find("coins").transform.Find(string.Format("Token{0}", i)).GetComponent<TokenController>();
            }
        }
        private void SpawnNewToken(int x, int y, bool teleport=false)
        {
            int idx = Tokens.FindIndex(tc => tc.id == FieldData[x, y].ID);
            if (idx > -1)
            {
                Tokens[idx].Teleport();
                return;
            }
            Transform fieldParent = GameObject.Find("Field/current").GetComponent<Transform>();
            Transform go = GameObject.Instantiate<Transform>(TokenOriginals[FieldData[x, y].Value].GetComponent<Transform>(), fieldParent);
            go.gameObject.SetActive(true);
            TokenController newToken = go.GetComponent<TokenController>();            
            newToken.SetPosition(LocalPosition(x, y));
            newToken.Associate(FieldData[x, y]);
            newToken.parent = this;
            if (teleport)
            {
                newToken.Teleport();
                newToken.Created = false;
            }
            else
            {
                Vector3 SpawnPosition;
                switch (FieldData.SlideDirection)
                {
                    case Directions.Up:
                        SpawnPosition = LocalPosition(x, -1);
                        break;
                    case Directions.Down:
                        SpawnPosition = LocalPosition(x, FieldData.FieldHeight);
                        break;
                    case Directions.Left:
                        SpawnPosition = LocalPosition(FieldData.FieldLength, y);
                        break;
                    case Directions.Right:
                        SpawnPosition = LocalPosition(-1, y);
                        break;
                    default:
                        SpawnPosition = LocalPosition(-1, -1);
                        break;
                }
                newToken.transform.localPosition = SpawnPosition;
                newToken.created = true;
            }
            Tokens.Add(newToken);
        }
        private void ReleaseNextSlide()
        {
            if (SlidingTokens.Count == 0)
            {
                state = FieldStates.Ready;
                return;
            }
            foreach (TokenController tk in SlidingTokens)
                tk.Created = false;
            SlidingTokens.RemoveAll(tk => !tk.created && !tk.InMotion);

            foreach(TokenController tk in SlidingTokens)
            {
                GameToken t = FieldData.NextToken(Mathf.CeilToInt(tk.GamePosition.x), Mathf.CeilToInt(tk.GamePosition.y));
                if (t is null) continue;

                if (SlidingTokens.FindIndex(tkn=>tkn.item.ID == t.ID) == -1)
                {
                    //tk.gameObject.SetActive(true);
                    tk.Created = false;
                }                
            }
        }
        private void OnFieldChanged(object sender, TokenEventArgs e)
        {
            Tokens.ForEach(
                delegate (TokenController tc) 
                {
                    if (tc.perish) Destroy(tc.gameObject);
                });

            Tokens.RemoveAll(tc => tc.perish);
            FillSlideList();

            DebugText2.text = FieldData.GetLines(FindAndKill: false).ToString();
        }

        private void OnTokensKilled(object sender, TokenEventArgs e)
        {
            state = FieldStates.Cleanup;
            SlidingTokens.ForEach(delegate (TokenController tk) { tk.Teleport(); });
            SlidingTokens.Clear();
            SlidingTokens.AddRange(Tokens.FindAll(tk => tk.perish));
            foreach (TokenController tc in SlidingTokens)
                tc.gameObject.name = "[killed]" + tc.gameObject.name;

            DebugText2.text = FieldData.GetLines(FindAndKill: false).ToString();
        }

        private void OnTokensAdd(object sender, TokenEventArgs e)
        {
            foreach (GameTokenData td in e.Tokens)
            {
                SpawnNewToken(td.x, td.y);
                SlidingTokens.Add(Tokens[Tokens.Count - 1]);
            }

            //SlidingTokens.RemoveAll(tk=>!tk.perish);

            state = FieldStates.Slide;

            DebugText2.text = FieldData.GetLines(FindAndKill: false).ToString();
        }

        public void OnCursorShow(Transform target)
        {
            cursor.position = target.position;
            cursor.gameObject.SetActive(true);
        }
        public void OnCursorHide()
        {
            cursor.gameObject.SetActive(false);
        }
    }
}
