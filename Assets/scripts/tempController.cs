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

        private void Awake()
        {
            Game = new TheGame();
        }
        // Start is called before the first frame update
        void Start()
        {
            
            State = DirectorState.Init;
            ActiveAction = new GameAction("InitGame");
        }

        // Update is called once per frame
        void Update()
        {
            debugModeText.text = string.Format("{0}[{1}]", State.ToString(), ActiveAction.Step);
            if (State == DirectorState.Ready)
            {
                KeyboardCommands();
                return;
            }
            if (ActiveAction.name == "InitGame")
            {
                DoInitField();
            }
            else if (ActiveAction.name == "CleanUpSolutions")
            {
                CleanupSolutions();
            }

            
        }

        private void KeyboardCommands()
        {
            if (FieldView.State != FieldStates.Ready) return;

            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                Game.Field.SlideDirection = Directions.Up;
            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                Game.Field.SlideDirection = Directions.Down;
            }
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                Game.Field.SlideDirection = Directions.Left;
            }
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                Game.Field.SlideDirection = Directions.Right;
            }
        }

        private void CleanupSolutions()
        {
            if (FieldView.State == FieldStates.Ready)
                switch (ActiveAction.Step)
            {
                case 0: // remove solved tokens
                    
                        TheGame.Me.Field.GetLines();
                        ActiveAction.Step = 1;                    
                    break;

                case 1: // Slide existing tokens to fill gaps                    
                        Game.Field.Slide();
                        if (Game.Field.GetLines(FindAndKill: false))
                            ActiveAction.Step = 0;
                        else
                            ActiveAction.Step = 2;
                    break;

                case 2: // Everything is clear and nothing to slide => add new tokens to fill up
                        //FieldView.RemoveSolution();
                        Game.Field.FillOneLine();
                        if (!Game.Field.Complete)
                            return;

                        if (Game.Field.GetLines(FindAndKill: false))
                            ActiveAction.Step = 0;
                        else
                        {
                            State = DirectorState.Ready;
                            ActiveAction = new GameAction("none");
                        }
                    break;
            }
        }

        public void DoInitField()
        {
            State = DirectorState.Ready;

            TheGame.Me.CreateField((new System.Random()).Next(150));
            FieldView.SyncField();
            ActiveAction = new GameAction("none");
        }

        public void ClearSolution()
        {
            if (State != DirectorState.Ready) return;
            TheGame.Me.Field.GetLines();
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
        }

        public void FillField()
        {
            Game.Field.Fill();
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