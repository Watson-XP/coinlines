using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using conilines.engine;
using UnityEngine.UI;

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
            State = DirectorState.Ready;
            ActiveAction = new GameAction("none");
            FieldView.AddTokens();
            FieldView.TeleportTokensInPlace();
        }

        // Update is called once per frame
        void Update()
        {
            debugModeText.text = string.Format("{0}[{1}]",  State.ToString(), ActiveAction.Stage);
            if (State == DirectorState.Ready) return;
            if (ActiveAction.name == "CleanUpSolutions")
            {
                switch (ActiveAction.Stage)
                {
                    case 0: // remove solved tokens
                        if (FieldView.State == FieldStates.Ready)
                        {
                            FieldView.RemoveSolution();
                            if (FieldView.State == FieldStates.Ready)
                            {
                                ActiveAction.Stage = 2;                                
                            }else
                                ActiveAction.Stage = 1;
                        }
                        break;

                    case 1: // Slide existing tokens to fill gaps
                        if (FieldView.State == FieldStates.Ready)
                        {
                            Game.Field.Slide();
                            FieldView.Slide();
                            ActiveAction.Stage = 0;
                        }
                        break;

                    case 2: // Everything is clear and nothing to slide => add new tokens to fill up
                        if (FieldView.State == FieldStates.Ready)
                        {
                            Game.Field.Fill();
                            FieldView.AddTokens();
                            ActiveAction.Stage = 10;
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

    }
}