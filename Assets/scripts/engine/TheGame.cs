using System;
using System.Collections.Generic;


namespace conilines.engine
{
    public class TheGame
    {                
        public event EventHandler<EventArgs> FieldSet;

        public static TheGame Me;

        internal static readonly int maxIndex = 6; // total number of different tokens
        internal static Directions Direction = Directions.Down; // Default "Gravitation"

        private List<GameToken> Tokens;
        private List<GameField> Fields;
        private int currentfield = 0;
        

        public GameField Field
        {
            get
            {
                if ((currentfield > -1 ) && (currentfield < Fields.Count)) return Fields[currentfield];
                throw new ArgumentOutOfRangeException("Game have no fields");               
            }
        }

        public TheGame( )
        {
            if (TheGame.Me is null)
            {
                Tokens = new List<GameToken>();
                Fields = new List<GameField>();
                currentfield = -1;
                //CreateField(100);
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
            OnFieldSet(new EventArgs());
        }
        protected virtual void OnFieldSet(EventArgs e)
        {
            EventHandler<EventArgs> handler = FieldSet;
            handler?.Invoke(this, e);
        }

    }
}
