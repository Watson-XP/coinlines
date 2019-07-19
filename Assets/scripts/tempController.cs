using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using conilines.engine;
using UnityEngine.UI;
using System;

namespace conilines.unity
{
    public class tempController : MonoBehaviour
    {
        TheGame Game;
        public FieldController FieldView;
        public DirectorState State;

        internal GameAction ActiveAction;
        public Text debugModeText;

        // Start is called before the first frame update
        void Start()
        {
            Game = new TheGame();
            State = DirectorState.Init;
            ActiveAction = new GameAction("InitGame");
        }

        // Update is called once per frame
        void Update()
        {
            debugModeText.text = string.Format("{0}[{1}]",  State.ToString(), ActiveAction.Step);
            if (State == DirectorState.Ready) return;
            if (ActiveAction.name == "InitGame") 
            {
                DoInitField();
            }
            else if (ActiveAction.name == "CleanUpSolutions")
            {
                CleanupSolutions();
            }
        }

        private void CleanupSolutions()
        {
            switch (ActiveAction.Step)
            {
                case 0: // remove solved tokens
                    if (FieldView.State == FieldStates.Ready)
                    {
                        FieldView.RemoveSolution();
                        if (FieldView.State == FieldStates.Ready)
                        {
                            ActiveAction.Step = 2;
                        }
                        else
                            ActiveAction.Step = 1;
                    }
                    break;

                case 1: // Slide existing tokens to fill gaps
                    if (FieldView.State == FieldStates.Ready)
                    {
                        Game.Field.Slide();
                        FieldView.Slide();
                        ActiveAction.Step = 0;
                    }
                    break;

                case 2: // Everything is clear and nothing to slide => add new tokens to fill up
                    if (FieldView.State == FieldStates.Ready)
                    {
                        //FieldView.RemoveSolution();
                        Game.Field.Fill(true);                        
                        FieldView.AddTokens();                        
                        ActiveAction.Step = 10;
                    }
                    break;
                case 10: // Finished
                    if (FieldView.State == FieldStates.Ready)
                    {
                        FieldView.Slide();
                        ActiveAction = new GameAction("none");
                        State = DirectorState.Ready;
                    }
                    break;
            }
        }

        public void DoInitField()
        {
            Debug.Log("on init");
            State = DirectorState.Ready;
            
            TheGame.Me.CreateField((new System.Random()).Next(150));
            FieldView.OnGameFieldInit();
            ActiveAction = new GameAction("none");
        }

        public void ClearAllSolutions()
        {

            if (State != DirectorState.Ready) return;

            State = DirectorState.Cleaning;
            ActiveAction = new GameAction("CleanUpSolutions");

        }

        public void SlideField()
        {
            Game.Field.Slide();
            FieldView.Slide();
        }

        private void OnSwapTokens(SwapTokensData data)
        {
            if (State != DirectorState.Ready) return;
            TheGame.Me.Field.SwapTokens(data.iD, data.directions);
            FieldView.FillSlideList();
            ClearAllSolutions();            
        }
    }
}