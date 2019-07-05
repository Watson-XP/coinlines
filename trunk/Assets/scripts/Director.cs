using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using conilines.engine;
using UnityEngine;
using UnityEngine.UI;

namespace conilines.unity
{
    public enum DirectorState { Ready, Start, Busy, Wait }

    public class Director : MonoBehaviour
    {
        static Director Me = null;
        public static TheGame Game;
        private Queue<GameAction> Actions;
        private GameAction ActiveAction;
        public static FieldController Field;
        public DirectorState State { get; private set; }

        public int WaitTimer;
        ProgressBar TimerBar;
        private List<TokenController> Tokens;
        public string ActionInProgress;

        void Awake()
        {
            if (Me == null)
            {
                Game = new TheGame();
                Actions = new Queue<GameAction>();
                State = DirectorState.Ready;
                ActiveAction = null;
                Field = null;
                Tokens = new List<TokenController>();
            }
            else
            {
                Destroy(this);
            }
        }
        private void Start()
        {
            if (Field == null)
                throw new NullReferenceException("GameField not created properly.");
            CreateField(123);
            TimerBar = GameObject.Find("TimerBar").GetComponent<ProgressBar>();

        }

        private void Update()
        {
            //DebugPanelCheck();
            switch (State)
            {
                case DirectorState.Ready:
                    if (Actions.Count == 0)
                    {
                        Actions.Enqueue(new GameAction("GetControl"));
                        DebugPanelCheck();
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
                case DirectorState.Wait:
                    DebugPanelCheck();
                    break;

            }
        }

        private void InitAction()
        {
            if (!(ActiveAction is GameAction)) return;

            ActionInProgress = string.Format("{0}: init", ActiveAction.name);
            switch (ActiveAction.name)
            {
                case "PopulateField":
                {
                    Field.PopulateTokens();
                    Tokens.AddRange(Field.NewTokens);
                    State = DirectorState.Busy;
                }
                break;
                case "DrawField":
                {
                    Field.DrawField();
                    FinishCurrentAction();
                }
                break;
                case "GetControl":
                    TokenController tk = Field.ClickHandle();
                    if (tk != null)
                    {
                        tk.updateValue();
                        State = DirectorState.Busy;
                    }
                    break;
                case "RemoveTokens":
                    Game.Field.GetLines();
                    State = DirectorState.Busy;
                    break;
                case "SlideField":
                    if (Game.Field.Slide())
                    {
                        Tokens.AddRange(Field.GetMovingTokens());
                        State = DirectorState.Busy;
                    }
                    else FinishCurrentAction();
                    break;
                case "CleanSolution":
                    bool redo = Game.Field.Slide();
                    if (redo)
                    {
                        Actions.Enqueue(new GameAction("RemoveTokens"));
                        Actions.Enqueue(GameAction.Slide);// new GASlide());//GameAction("SlideField"));
                        Actions.Enqueue(new GameAction("CleanSolution"));
                        FinishCurrentAction();
                    }
                    else {
                        Game.Field.Fill(true);
                        Actions.Enqueue(new GameAction("PopulateField"));
                        //Game.Field.Slide();                        
                        Actions.Enqueue(new GameAction("SlideField"));
                    }
                    State = DirectorState.Busy;
                    break;
                default:
                    FinishCurrentAction();
                    break;

            }
        }

        private void ProcessAction()
        {
            if (!(ActiveAction is GameAction))
            {
                FinishCurrentAction();
                return;
            }
            ActionInProgress = string.Format("{0}: run", ActiveAction.name);
            switch (ActiveAction.name)
            {
                case "RemoveTokens":
                    Field.ClearDestroyedTokens();
                    FinishCurrentAction();
                    break;
                case "GetControl":
                    Actions.Enqueue(new GameAction("CleanSolution"));
                    FinishCurrentAction();
                    break;
                case "SlideField":
                    if (Tokens.Count == 0)
                    {
                        FinishCurrentAction();
                        return;
                    }

                    int sx = 0;
                    bool keepup = true;
                    do // cycle through columns 
                    {
                        TokenController tk = null;
                        while ((tk == null) && (sx < Game.Field.SizeL))
                        {
                            List<TokenController> alls =
                                Tokens.FindAll(x => x.GamePosition.x == Field.ToSpaceCoordinates(sx, 0).x);

                            alls.Sort(delegate (TokenController a, TokenController b)
                            {
                                if (a.GamePosition.y < b.GamePosition.y) return 1;
                                if (a.GamePosition.y == b.GamePosition.y) return 0;
                                return -1;
                            });

                            if (alls.Count > 0) // this comumn have new tokens
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
                                        if (tk == null) sx++;
                                    }
                                }

                            }
                            else sx++;// not a single token in this column found :(
                        }

                        if (tk == null) // no token to release in this column
                        {
                            if (sx >= Game.Field.SizeL)
                            {
                                keepup = false;
                                continue;
                            }
                        }
                        else // release token
                        {
                            if (tk.Created)
                            {
                                tk.Created = false;
                                keepup = false;
                            }
                            else// token in place => stop monitoring
                            {
                                if (!tk.InMotion)
                                {
                                    Tokens.Remove(tk);
                                    sx--;
                                }
                            }
                        }
                    } while (keepup);
                    break;
                case "CleanSolution":
                    FinishCurrentAction();
                    break;
                default:
                {
                    FinishCurrentAction();
                }
                break;
            }
        }

        private void DebugPanelCheck()
        {
            //            State = DirectorState.Ready;
            //return;

            Text nm = GameObject.Find("DirectorPanel/ActionName").GetComponent<Text>();
            if (nm == null) return;

            GameAction act = Actions.ElementAtOrDefault<GameAction>(0);
            if (act is null)
            {
                nm.text = "Empty";
                if (State == DirectorState.Wait)
                    Step();
            }
            else
            {
                if (State == DirectorState.Start) nm.color = new Color(1.0f, 1.0f, 0.0f);
                else
                if (State == DirectorState.Busy) nm.color = new Color(0.0f, 1.0f, 1.0f); else
                    nm.color = new Color(1.0f, 1.0f, 1.0f);
                nm.text = act.name;
                WaitTimer += Convert.ToInt32(Time.deltaTime * 100);
                if (TimerBar)
                    TimerBar.BarValue = WaitTimer;
                if (WaitTimer > 100)
                {
                    WaitTimer = 0;
                    switch (State)
                    {
                        case DirectorState.Ready:
                            break;
                        case DirectorState.Start:
                            break;
                        case DirectorState.Busy:
                            break;
                        case DirectorState.Wait:
                            Step();
                            break;
                    }

                }
            }
        }


        public void Step()
        {
            State = DirectorState.Ready;
        }

        private void FinishCurrentAction()
        {
            State = DirectorState.Wait;
            WaitTimer = 0;
            ActiveAction = null;
        }

        public void CreateField(int seed = -1)
        {
            if (seed > -1)
            {
                Game.CreateField(seed);
            }
            Actions.Enqueue(new GameAction("PopulateField"));
            Actions.Enqueue(new GameAction("DrawField"));
        }

        public void Populate()
        {

            Actions.Enqueue(new GameAction("PopulateField"));
        }

        public void Slide()
        {

            Actions.Enqueue(new GameAction("SlideField"));
            //Actions.Enqueue(new GameAction("PopulateField"));
        }

        public void ClearSolutions()
        {

            Actions.Enqueue(new GameAction("CleanSolution"));
            //Actions.Enqueue(new GameAction("PopulateField"));
        }

        public void RemoveTokens()
        {

            Actions.Enqueue(new GameAction("RemoveTokens"));
        }
    }


    public enum ControllerState { Ready, Start, Run}

    public class ControllerActions
    {
        private List<GameAction> items;
        internal GameAction this[string index]
        {
            get {
                GameAction g = items.Find(act => act.name == index);
                if (g is null)
                    return items[0];
                else return g;
            }
            set {
                GameAction g = items.Find(act => act.name == index);
                if (g is null)
                {
                    items.Add(value);
                }
            }
        }
        public ControllerActions()
        {
            items = new List<GameAction>();

            this["none"] = new GameAction("none");
            this["Slide"] = new GameAction("SlideField");
            this["Populate"] = new GameAction("PopulaeFiled");
            this["Draw"] = new GameAction("DrawField");
            this["Clean"] = new GameAction("CleanUpRemoved");
            this["Solve"] = new GameAction("CleanUpSolutions");
        }
    }
    public class Controller : MonoBehaviour
    {            
        private ControllerActions MyActions;
        private Queue<GameAction> Actions;
        internal GameAction MyAction;
        public ControllerState State { get; private set; }

        private FieldController FieldView;
        private GameField FieldData;

        private void Awake()
        {
            Actions = new Queue<GameAction>();
            MyActions = new ControllerActions();
            Actions.Enqueue(MyActions["none"]);
        }

        private void Start()
        {
            FieldView = GetComponent<FieldController>();
            if (FieldView == null)
                throw new ArgumentNullException("Field not assigned");
        }

        private void Update()
        {
            switch (MyAction.name)
            {
                case "PopulaeFiled":
                    PopulaeFiled();
                    break;
                case "DrawField":
                    DrawField();
                    break;
                default:
                    CheckControls();
                    FinishAction();
                    break;
            }

        }

        private void CheckControls()
        {
            throw new NotImplementedException();
        }

        private void PopulaeFiled()
        {
            if (FieldView.State == FieldStates.Ready)
            {
                FieldView.PopulateTokens();
                FinishAction();
            }
        }

        private void DrawField()
        {
            if (FieldView.State == FieldStates.Ready)
            {
                FieldView.DrawField();
                FinishAction();
            }
        }

        private void FinishAction()
        {
            MyAction = Actions.Dequeue();
            if (MyAction is null)
            {
                MyAction = MyActions["none"];
            }
            State = ControllerState.Ready;
        }

    }

}