using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conilines.engine
{
    internal struct ItemData
    {
        public int x;
        public int y;
        public bool check;
        public GameToken Token;
    }


    public class GameField
    {
        private readonly int minLine = 3;
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
        private static int CompareBySum(ItemData x, ItemData y)
        {
            int diff = ( x.x + x.y - y.x - y.y );
            return Math.Sign(diff);
        }

        public bool GetLines( )
        {
            List<ItemData> Cluster = new List<ItemData>( );
            Queue<ItemData> Candidate = new Queue<ItemData>( );
            List<ItemData> Everything = new List<ItemData>( );            
            int idx;

            for(int x = 0; x < sizeL; x++)
                for(int y = 0; y < sizeH; y++)
                    if(Data[x, y].Value > 0)
                        Everything.Add( new ItemData( ) { x = x, y = y, check = false, Token = Data[x, y] });

            while(Everything.Count() > 0)
            {
                Candidate.Clear( );
                Cluster.Clear( );
                Candidate.Enqueue(Everything[0]);
                Everything.RemoveAt(0);
                do
                {
                    ItemData tmp = Candidate.Dequeue( );                    
                    if(tmp.x > 0)
                    if (Data[tmp.x-1, tmp.y].Value == tmp.Token.Value)
                    {
                        idx =  Everything.FindIndex(t => ( ( t.x == tmp.x - 1 ) &&(t.y == tmp.y)));
                        if(idx > -1)
                        {
                            Candidate.Enqueue(Everything[idx]);
                            Everything.RemoveAt(idx);
                            tmp.check = true;
                        }
                    }
                    if(tmp.x < sizeL-1)
                        if(Data[tmp.x + 1, tmp.y].Value == tmp.Token.Value)
                    {
                        idx = Everything.FindIndex(t => ( ( t.x == tmp.x + 1 ) && ( t.y == tmp.y ) ));
                        if(idx > -1)
                        {                            
                            Candidate.Enqueue(Everything[idx]);
                            Everything.RemoveAt(idx);
                            tmp.check = true;
                        }
                    }
                    if (tmp.y < sizeH-1)
                    if(Data[tmp.x , tmp.y+1].Value == tmp.Token.Value)
                    {
                        idx = Everything.FindIndex(t => ( ( t.x == tmp.x  ) && ( t.y == tmp.y+1 ) ));
                        if(idx > -1)
                        {

                            Candidate.Enqueue(Everything[idx]);
                            Everything.RemoveAt(idx);
                            tmp.check = true;
                        }
                    }
                    if ( tmp.y > 0)
                    if(Data[tmp.x , tmp.y-1].Value == tmp.Token.Value)
                    {
                        idx = Everything.FindIndex(t => ( ( t.x == tmp.x ) && ( t.y == tmp.y-1 ) ));
                        if(idx > -1)
                        {
                            Candidate.Enqueue(Everything[idx]);
                            Everything.RemoveAt(idx);
                            tmp.check = true;
                        }
                    }                    
                    Cluster.Add(tmp);
                } while(Candidate.Count > 0);

                if (Cluster.Count >= minLine)
                {
                    Cluster.Sort(CompareBySum);
                    int x = 1;
                    while(Cluster.FindIndex(t => t.x == Cluster[0].x + x && t.Token.Value == Cluster[0].Token.Value) > -1) x++;
                    if (x < minLine)
                    {
                        x = 1;
                        while(Cluster.FindIndex(t => t.y == Cluster[0].y + x && t.Token.Value == Cluster[0].Token.Value) > -1) x++;
                    }
                    if(x >= minLine)
                    {
                        Cluster.ForEach(delegate (ItemData d) { Data[d.x, d.y] = new GameToken(0); });                        
                        //Everything.Clear( );
                    }                    
                }

            }

            return ( Cluster.Count > 0 );
        }

        public void Slide()
        {
            int dx = 1;
            int dy = 0;
            bool reslide;
            do
            {
                reslide = false;
                for(int x = 0; x < SizeL; x++)
                    for(int y = 0; y < sizeH; y++)
                    {
                        if(InRange(x + dx, y + dy))
                            if(Data[x, y].Value != 0)
                                if(Data[x + dx, y + dy].Value == 0)
                            {
                                Swap(x, y, x+dx, y+dy);
                                reslide = true;
                            }
                    }
            } while(reslide) ;
        }

        private void Swap(int x, int y, int dx, int dy)
        {
            GameToken tmp = Data[x, y];
            Data[x, y] = Data[dx, dy];
            Data[dx, dy] = tmp;
        }

        private bool InRange(int v1, int v2)
        {
            return ( v1 >= 0 ) && ( v1 < sizeL ) && ( v2 >= 0 ) && ( v2 < sizeH );
        }

        public bool HaveID(int id)
        {
            foreach(GameToken t in Data)
            {
                if(t.ID == id) return true;
            }
            return false;
        }

    }
}
