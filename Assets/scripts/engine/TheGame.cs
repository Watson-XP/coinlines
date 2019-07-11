using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace conilines.engine
{
    public class TheGame
    {
        public static TheGame Me;
        private List<GameToken> Tokens;
        private List<GameField> Fields;
        private int currentfield = 0;
        public GameField Field
        {
            get
            {
                if(Fields.Count == 0) return null;
                if(currentfield < Fields.Count) return Fields[currentfield];
                return null;
            }
        }

        public TheGame( )
        {
            if (TheGame.Me is null)
            {
                Tokens = new List<GameToken>();
                Fields = new List<GameField>();
                currentfield = -1;
                CreateField(100);
                Me = this;
            }
            else
            {
                throw new Exception("Game already created");
            }
        }

        public void CreateField(int seed)
        {
            GameField gf = new GameField(seed: seed);
            Fields.Add(gf);
            currentfield = Fields.Count - 1;
            ///gf.Fill(true);
        }
    }
}
