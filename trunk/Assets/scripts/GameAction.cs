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

        public GameAction(string name, object p1)
        {
            this.name = name;
            this.p1 = p1;
        }
        public GameAction(string name)
        {
            this.name = name;
            p1 = null;
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