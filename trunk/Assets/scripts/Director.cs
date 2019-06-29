using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using conilines.engine;
using UnityEngine;

namespace conilines.unity {
    public class Director : MonoBehaviour
    {
        public static TheGame Game;
        private Queue<GameAction> Actions;
        public Director()
        {
            Game = new TheGame();
            Actions = new Queue<GameAction>();
        }
    }

}