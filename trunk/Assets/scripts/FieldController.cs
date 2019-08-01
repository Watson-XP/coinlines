using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using conilines.engine;
using conilines.unity;
using WSTools;
using UnityEngine.UI;
using System;

namespace conilines.unity
{
    public enum FieldStates
    {
        Ready, Slide,
        Removing,
        Cleanup,
        Hold
    }
    public class FieldController : MonoBehaviour
    {
        public GameField FieldData;
        private List<TokenController> Tokens;
        private TokenController[] TokenOriginals;
        private List<TokenController> KilledTokens;
        public FieldStates State { get; private set; }
        private List<TokenController> SlidingTokens;

        private void Awake()
        {
            State = FieldStates.Hold;            
        }

        // unity events
        private void Start()
        {
            Tokens = new List<TokenController>();
            LoadOriginals();            
            State = FieldStates.Ready;
        }

        public void OnGameFieldInit()
        {
            FieldData = TheGame.Me.Field;
            foreach (TokenController tk in Tokens)
                Destroy(tk.gameObject);
            
            Tokens.Clear();
            AddTokens();
            TeleportTokensInPlace();
        }

        private void Update()
        {
            switch (State)
            {
                case FieldStates.Hold:
                    break;
                case FieldStates.Ready:
                    break;
                case FieldStates.Slide:
                    if (SlidingTokens.Count > 0)
                    {
                        SlidingTokens.RemoveAll(tk => !tk.InMotion);

                        if (SlidingTokens.FindIndex(tk=>tk.InMotion && tk.Created == false ) == -1)
                        {
                            UpdateCreatedSlidingTokens();
                        }/*
                        if (SlidingTokens.Count > 0)
                        {
                            foreach (TokenController tk in SlidingTokens)
                                tk.Created = false;
                        }*/
                    }
                    else
                        State = FieldStates.Ready;
                    break;
                case FieldStates.Removing:
                    break;
                case FieldStates.Cleanup:
                    if (KilledTokens.Count == 0) State = FieldStates.Ready;
                    else
                    {
                        if (KilledTokens[0].death)
                        {
                            Destroy(KilledTokens[0].gameObject);
                            Tokens.Remove(KilledTokens[0]);
                            KilledTokens.RemoveAt(0);
                        }/*
                        if (KilledTokens[0] == null)
                        {
                            Tokens.Remove(KilledTokens[0]);
                            KilledTokens.RemoveAt(0);
                        }*/
                    }
                    break;
                default:
                    break;
            }
        }


        //  External functions :)
        private void AddTokens()
        {
            if (State != FieldStates.Ready) return;

            for (int x = 0; x < FieldData.FieldLength; x++)
                for (int y = 0; y < FieldData.FieldHeight; y++)
                {
                    if (Tokens.FindIndex(tc => tc.item.ID == FieldData[x, y].ID) == -1)
                        SpawnNewToken(x, y);
                }
            GenocideTokens();
        }

        public void TeleportTokensInPlace()
        {
            if (State != FieldStates.Ready) return;
            foreach (TokenController tk in Tokens)
            {
                tk.Created = false;
                tk.Teleport();
            }
        }

        private void RemoveSolution()
        {
            if (State != FieldStates.Ready) return;
            bool removed = FieldData.GetLines();
            //if (!removed) return;
            GenocideTokens();
        }

        public void Slide()
        {
            if (State != FieldStates.Ready) return;
            FillSlideList();
            int dx;
            int dy;
            int sx;
            int sy;
            int ex;
            int ey;
            switch (FieldData.SlideDirection)
            {
                case Directions.Up:
                    dx = 1;
                    dy = 1;
                    sx = 0;
                    sy = 0;
                    ex = FieldData.FieldLength - 1;
                    ey = FieldData.FieldHeight - 1;
                    break;
                case Directions.Down:
                    dx = 1;
                    dy = -1;
                    sx = 0;
                    sy = FieldData.FieldHeight - 1;
                    ex = FieldData.FieldLength - 1;
                    ey = 0;
                    break;
                case Directions.Left:
                    dx = 1;
                    dy = 1;
                    sx = 0;
                    sy = 0;
                    ex = FieldData.FieldLength - 1;
                    ey = FieldData.FieldHeight - 1;

                    break;
                case Directions.Right:
                    dx = -1;
                    dy = 1;
                    sx = FieldData.FieldLength - 1;
                    sy = 0;
                    ex = 0;
                    ey = FieldData.FieldHeight - 1;
                    break;
                default:
                    dx = 0;
                    dy = 0;
                    sx = 0;
                    sy = 0;
                    ex = 0;
                    ey = 0;
                    break;
            }
            for (int x = sx; x != ex; x += dx)
                for (int y = sy; y != ey; y += dy)
                {

                }

            UpdateCreatedSlidingTokens();
        }

        private void UpdateCreatedSlidingTokens()
        {
            int x = 0;
            int y = 0;
            while (x < FieldData.FieldLength)
            {
                if (FieldData.SlideDirection == Directions.Up) y = 0;
                    else y = FieldData.FieldHeight - 1;

                while (y < FieldData.FieldHeight && y >= 0 && SlidingTokens.FindIndex(tx => tx.Created && tx.item.ID == FieldData[x, y].ID) == -1)
                    if (FieldData.SlideDirection == Directions.Up) y++; else y--;
                if (y < FieldData.FieldHeight && y >= 0)
                    SlidingTokens.Find(tx => tx.Created && tx.item.ID == FieldData[x, y].ID).Created = false;
                x++;
            }
        }

        public void FillSlideList()
        {

            //FieldData.Slide(); //       debug only  !!!

            SlidingTokens = new List<TokenController>();
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
                State = FieldStates.Slide;
            else
                State = FieldStates.Ready;
        }

        public void RemoveAllSolutions()
        {
            if (State != FieldStates.Ready) return;
            AddTokens();
            TeleportTokensInPlace();
            bool loop = false;
            do
            {
                RemoveSolution();
                loop = (KilledTokens.Count > 0);
                Slide();

            } while (loop);
        }

        public static Vector3 LocalPosition(int x, int y)
        {
            int size = 1;
            int sx = 1;
            int sy = 1;
            return new Vector3(x: (x * size) + sx, y: (y * size) + sy, z: 0);
        }

        //  Internal functions :D

        private void LoadOriginals()
        {
            TokenOriginals = new TokenController[GameToken.maxIndex];
            for (int i = 0; i < GameToken.maxIndex; i++)
            {

                TokenOriginals[i] = GameObject.Find("coins").transform.Find(string.Format("Token{0}", i)).GetComponent<TokenController>();
            }
        }
        private void SpawnNewToken(int x, int y)
        {
            Transform fieldParent = GameObject.Find("Field/current").GetComponent<Transform>();
            TokenController newToken = GameObject.Instantiate<Transform>(TokenOriginals[FieldData[x, y].Value].GetComponent<Transform>(), fieldParent).GetComponent<TokenController>();
            newToken.SetPosition(LocalPosition(x, y));
            newToken.Associate(FieldData[x, y]);
            Vector3 SpawnPosition;
            switch (FieldData.SlideDirection)
            {
                case Directions.Up:
                    SpawnPosition = LocalPosition(x, FieldData.FieldHeight);
                    break;
                case Directions.Down:
                    SpawnPosition = LocalPosition(x, -1);
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
            Tokens.Add(newToken);
            newToken.Created = true;
        }

        private void Syncronize()
        {
            AddTokens();
            TeleportTokensInPlace();
        }

        private void GenocideTokens()
        {
/*            List<int> ids = new List<int>();
            for (int x = 0; x < FieldData.FieldLength; x++)
                for (int y = 0; y < FieldData.FieldHeight; y++)
                {
                    ids.Add(FieldData[x, y].ID);
                }
            foreach (TokenController tk in Tokens)
                if (!ids.Contains(tk.item.ID))
                    //foreach (int idx in ids)
                    //Tokens.Find(tk => tk.item.ID == udx).Kill();           
                    tk.Kill();
            */
            KilledTokens = Tokens.FindAll(tk => tk.perish == true && tk.created == false);
            int i = 0;
            /*
            foreach (TokenController tk in KilledTokens)
            {
                (GameObject.Find(string.Format("txxDelete ({0})", i++))).GetComponent<Text>().text = tk.name;
            }
            while (i < 21)
                (GameObject.Find(string.Format("txxDelete ({0})", i++))).GetComponent<Text>().text = "";
                */
            if (KilledTokens.Count > 0)
                State = FieldStates.Cleanup;
            else
                State = FieldStates.Ready;
        }
    }
}
