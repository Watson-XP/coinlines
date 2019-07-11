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
    public enum DirectorState { Ready, Start, Busy, Wait,
        Cleaning,
        Slide,
        CLeanUpComplete
    }

    public enum ControllerState { Ready, Start, Run }

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
        public FieldController FieldView;
        private void Start()
        {
            
        }



    }
}