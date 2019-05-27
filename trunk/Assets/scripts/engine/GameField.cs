using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conilines.engine
{
    public class GameField
    {
        private GameToken[,] Data;
        public GameToken this[int x, int y]
        {
            get { return Data[x, y]; }
        }

        private int sizeL;
        private int sizeH;
        
        private readonly int Seed;
        public int SizeL => sizeL;
        public int SizeH => sizeH;

        public GameField(int sizeL = 10, int sizeH = 10, int seed = 0)
        {
            this.sizeL = sizeL;
            this.sizeH = sizeH;
            Data = new GameToken[sizeL, sizeH];
            Seed = seed;
            InitField();
        }

        private void InitField( )
        {
            Random rnd = new Random(Seed);

            for(int x = 0; x < sizeL; x++)
                for(int y = 0; y < sizeH; y++)
                    Data[x, y] = new GameToken(rnd.Next(1, GameToken.maxIndex));
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode( )
        {
            return base.GetHashCode( );
        }

        public override string ToString( )
        {
            return string.Format("Array:{0}x{1}", sizeL, sizeH);
        }

        

    }
}
