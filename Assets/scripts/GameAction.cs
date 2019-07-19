using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conilines.unity
{
    internal class GameAction
    {
        public string name;
        public object p1;
        public int Step;

        public GameAction(string name, object p1)
        {
            init(name, p1);
        }

        public GameAction(string name)
        {
            init(name, null);
        }

        private void init(string name, object p1)
        {
            this.name = name;
            this.p1 = p1;
            Step = 0;
        }

        public static GameAction Slide
        {
            get { return new GameAction("SlideField"); }
        }
    }

    class GASlide : GameAction
    {
        public GASlide() : base("SlideField")
        {
            
        }
    }
}