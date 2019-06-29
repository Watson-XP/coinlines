using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using conilines.engine;
using UnityEngine;

namespace conilines.unity
{

    public enum DirectorState { Ready, Start, Busy }

    public class Director : MonoBehaviour
    {
        public static TheGame Game;
        private Queue<GameAction> Actions;
        private GameAction ActiveAction;
        public static FieldController Field;
        public DirectorState State { get; private set; }
        private List<TokenController> Tokens;

        void Awake()
        {
            Game = new TheGame();
            Actions = new Queue<GameAction>();
            State = DirectorState.Ready;
            ActiveAction = null;
            Field = null;
            Tokens = new List<TokenController>();
        }
        private void Start()
        {
            if (Field is null)
                throw new NullReferenceException("GameField not created properly.");
            CreateField(123);
        }

        private void Update()
        {
            switch (State)
            {
                case DirectorState.Ready:
                    if (Actions.Count == 0)
                    {
                        Actions.Enqueue(new GameAction("GetControl"));
                        return;
                    }
                    ActiveAction = Actions.Dequeue();
                    if (ActiveAction is null) return;
                    State = DirectorState.Start;
                    break;
                case DirectorState.Start:
                    InitAction();
                    break;
                case DirectorState.Busy:
                    ProcessAction();
                    break;
            }
        }

        private void InitAction()
        {
            if (!(ActiveAction is GameAction)) return;

            switch (ActiveAction.name)
            {
                case "PopulateField":
                {
                    Field.PopulateTokens();
                    Tokens = Field.NewTokens;
                    State = DirectorState.Busy;
                }
                break;
                case "DrawField":
                {
                    Field.DrawField();
                    State = DirectorState.Ready;
                }
                break;
                case "GetControl":
                    TokenController tk = Field.ClickHandle();
                    if (tk != null)
                    {
                        tk.updateValue();
                        Slide();
                        State = DirectorState.Busy;
                    }
                    break;
                case "SlideField":
                    if (Game.Field.GetLines())
                    {
//                        Game.Field.Slide();
                        Populate();
                        Slide();
                    }
                    else if (Game.Field.Slide())
                    {
                        Game.Field.Fill();
                        Populate();
                        Slide();
                    }

                    State = DirectorState.Busy;
                    break;
                default:
                    State = DirectorState.Busy;
                    break;

            }
        }

        private void ProcessAction()
        {
            switch (ActiveAction.name)
            {
                case "PopulateField":
                {
                    if (Tokens.Count == 0)
                    {
                        State = DirectorState.Ready;
                        ActiveAction = null;
                        return;
                    }

                    int sx = 0;
                    bool keepup = true;
                    do
                    {
                        TokenController tk = null;
                        while ((tk is null) && (sx < Game.Field.SizeL))
                        {
                            List<TokenController> alls = Tokens.FindAll(x => x.GamePosition.x == Field.ToSpaceCoordinates(sx, 0).x);
                            alls.Sort(delegate (TokenController a, TokenController b)
                            {
                                if (a.GamePosition.y < b.GamePosition.y) return 1;
                                if (a.GamePosition.y == b.GamePosition.y) return 0;
                                return -1;
                            });
                            if (alls.Count > 0)
                            {
                                if (alls[0].Created)
                                    tk = alls[0];
                                else
                                {
                                    if (alls[0].InMotion)
                                        sx++;
                                    else
                                    {
                                        Tokens.Remove(alls[0]);
                                        tk = (alls.Count > 1) ? alls[1] : null;
                                        if (tk is null) sx++;
                                    }
                                }

                            }
                            else sx++;// not a single token in this column found :(
                        }
                        if (tk is null)
                            if (sx == Game.Field.SizeL) { keepup = false; continue; }

                        if (tk.Created)
                        {
                            tk.Created = false;
                            keepup = false;
                        }
                        else
                        {
                            if (!tk.InMotion)
                            {
                                Tokens.Remove(tk);
                                sx--;
                            }
                        }
                    } while (keepup);
                }
                break;
                case "GetControl":
                    Game.Field.GetLines();
                    Game.Field.Fill();
                    State = DirectorState.Ready;
                    break;
                case "SlideField":
                    Populate();
                    State = DirectorState.Ready;
                    break;
                default:
                {
                    State = DirectorState.Ready;
                    ActiveAction = null;
                }
                break;
            }

        }

        public void CreateField(int seed = -1)
        {
            if (seed > -1)
            {
                Game.CreateField(seed);
            }
            Actions.Enqueue(new GameAction("PopulateField"));
            //Actions.Enqueue(new GameAction("DrawField"));
        }

        public void Populate()
        {
            Actions.Enqueue(new GameAction("PopulateField"));
        }

        public void Slide()
        {
            Actions.Enqueue(new GameAction("SlideField"));
        }
    }
}