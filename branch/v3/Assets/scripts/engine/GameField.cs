using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conilines.engine
{
    public class GameField
    {
        //public delegate void DoInitField();


        private readonly int minLine = 3;
        private GameToken[,] Data;
        public GameToken this[int x, int y]
        {
            get { return Data[x, y]; }
        }

        private int sizeL;
        private int sizeH;
        public Directions SlideDirection { get; private set; }

        private readonly int Seed;
        public int FieldLength => sizeL;
        public int FieldHeight => sizeH;
        Random rnd;

        public int nextSeed
        {
            get
            {
                return rnd.Next(1, GameToken.maxIndex);
            }
        }

        public GameField(int sizeL = 10, int sizeH = 10, int seed = 0)
        {
            this.sizeL = sizeL;
            this.sizeH = sizeH;
            Data = new GameToken[sizeL, sizeH];
            Seed = seed;
            SlideDirection = Directions.Down;
            InitField();
        }

        public void InitField()
        {
            rnd = new Random(Seed);

            for (int x = 0; x < sizeL; x++)
                for (int y = 0; y < sizeH; y++)
                    Data[x, y] = new GameToken(rnd.Next(1, GameToken.maxIndex));
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Array:{0}x{1}", sizeL, sizeH);
        }
        private static int CompareBySum(ItemData x, ItemData y)
        {
            int diff = (x.x + x.y - y.x - y.y);
            return Math.Sign(diff);
        }

        public bool GetLines()
        {
            List<ItemData> Cluster = new List<ItemData>();
            Queue<ItemData> Candidate = new Queue<ItemData>();
            List<ItemData> Everything = new List<ItemData>();

            int y = -1;
            int x = 0;
            bool linefound = false;
            int xmax = 0;
            int xlen = 0;
            int xmaxstart = 0;
            int xlenstart = 0;
            while ((++y < sizeH) && !linefound) // scan lines
            {
                x = 0;
                xlen = 0;
                while (++x < sizeL)
                {
                    if (Data[x, y].Alive && Data[x - 1, y].Alive)
                    {
                        if (Data[x, y].Value == Data[x - 1, y].Value)
                        {
                            if (xlen == 0)
                            {
                                xlenstart = x - 1;
                                xlen++;
                            }
                            xlen++;
                        }
                        else
                        {
                            if (xmax < xlen)
                            {
                                xmax = xlen;
                                xmaxstart = xlenstart;
                            }
                            xlen = 0;
                        }
                        if (xmax >= minLine)
                            linefound = true;
                    }
                }
            }

            if (linefound)
            {
                for (int ix = xmaxstart; ix < xmaxstart + xmax; ix++)
                    Cluster.Add(new ItemData() { x = ix, y = y - 1, check = false, Token = Data[ix, y - 1] });

                BuildCluster(ref Cluster, x: xmaxstart, y: y - 1);

                foreach (ItemData itd in Cluster)
                {
                    Data[itd.x, itd.y].Kill();
                 //   Data[itd.x, itd.y] = new GameToken(0);
                }

                if (Cluster.Count > 9)
                    return true;                
            }

            return false;
        }

        private void BuildCluster(ref List<ItemData> Cluster, int x, int y)
        {
            int clusterValue = Data[x, y].Value;
            bool keepup = true;
            int maxcluster = 40;            
            while (keepup)
            {
                keepup = false;
                for (int cIndex = 0; cIndex < Cluster.Count; cIndex++)
                {
                    ItemData itd = Cluster[cIndex];
                    if (itd.check) continue;

                    if (InRange(itd.x - 1, itd.y))
                        if (Data[itd.x - 1, itd.y].Value == clusterValue)
                        {
                            bool test = TestAddCluster(Cluster,
                                new ItemData() { x = itd.x - 1, y = itd.y, check = false, Token = Data[itd.x - 1, itd.y] }
                                );
                            keepup = keepup || test;
                        }
                    if (InRange(itd.x + 1, itd.y))
                        if (Data[itd.x + 1, itd.y].Value == clusterValue)
                        {
                            bool test = TestAddCluster(Cluster,
                                new ItemData() { x = itd.x + 1, y = itd.y, check = false, Token = Data[itd.x + 1, itd.y] }
                                );
                            keepup = keepup || test;
                        }

                    if (InRange(itd.x, itd.y - 1))
                        if (Data[itd.x, itd.y - 1].Value == clusterValue)
                        {
                            bool test = TestAddCluster(Cluster,
                                  new ItemData() { x = itd.x, y = itd.y - 1, check = false, Token = Data[itd.x, itd.y - 1] }
                                 );
                            keepup = keepup || test;
                        }

                    if (InRange(itd.x, itd.y + 1))
                        if (Data[itd.x, itd.y + 1].Value == clusterValue)
                        {
                            bool test = TestAddCluster(Cluster,
                                 new ItemData() { x = itd.x, y = itd.y + 1, check = false, Token = Data[itd.x, itd.y + 1] }
                                );
                            keepup = keepup || test;
                        }
                    itd.check = true;
                    Cluster[cIndex] = itd;
                }
                keepup = keepup && (maxcluster <= Cluster.Count);
            }
            if (maxcluster <= Cluster.Count)
                Cluster.Clear();
        }

        private bool TestAddCluster(List<ItemData> Cluster, ItemData itd)
        {
            if (InRange(itd.x, itd.y))
                if (Cluster.FindIndex(t => t.x == itd.x && t.y == itd.y) == -1)
                {
                    Cluster.Add(itd);
                    return true;
                }
            return false;
        }

        public bool Slide()
        {
            int dx = 0;
            int dy = 0;
            switch (SlideDirection)
            {
                case Directions.Left: dx = -1; break;
                case Directions.Right: dx = 1; break;
                case Directions.Up: dy = -1; break;
                case Directions.Down: dy = 1; break;
            }
            bool reslide;
            bool result = false;
            do
            {
                reslide = false;
                for (int x = 0; x < FieldLength; x++)
                    for (int y = 0; y < sizeH; y++)
                    {
                        if (InRange(x + dx, y + dy))
                            if (Data[x, y].Alive)
                                if (!Data[x + dx, y + dy].Alive)
                                {
                                    Swap(x, y, x + dx, y + dy);
                                    reslide = true;
                                    result = true;
                                }
                    }
            } while (reslide);
            return result;
        }

        public void Fill(bool nolines = false)
        {
            bool loop = false;
            do
            {
                loop = false;
                for (int x = 0; x < FieldLength; x++)
                    for (int y = 0; y < sizeH; y++)
                    {
                        if (InRange(x, y))
                            if (!Data[x, y].Alive)
                            {
                                //GameToken t = ;
                                Data[x, y] = new GameToken(nextSeed);
                            }
                    }
                if (nolines)
                    loop = GetLines();
                if (loop) Slide();
                while (nolines && GetLines()) Slide();

            } while (loop);
        }

        private void Swap(int x, int y, int dx, int dy)
        {
            GameToken tmp = Data[x, y];
            Data[x, y] = Data[dx, dy];
            Data[dx, dy] = tmp;
        }

        public void SwapTokens(int id, Directions where)
        {            
            int x = -1;
            int y = -1;
            while (++x < FieldLength)
            {
                while (++y < FieldHeight)
                {
                    if (Data[x, y].ID == id)
                        break;
                }
                if (y < FieldHeight)
                    if (Data[x, y].ID == id)
                        break;
                y = -1;
            }
            if (InRange(x, y))
                if (Data[x, y].ID == id)
                {
                    switch (where)
                    {
                        case Directions.Up:
                            Swap(x, y, x, y + 1);
                            break;
                        case Directions.Down:
                            Swap(x, y, x, y - 1);
                            break;
                        case Directions.Left:
                            Swap(x, y, x - 1, y);
                            break;
                        case Directions.Right:
                            Swap(x, y, x + 1, y);
                            break;
                    }
                }
        }

        private bool InRange(int v1, int v2)
        {
            return (v1 >= 0) && (v1 < sizeL) && (v2 >= 0) && (v2 < sizeH);
        }

        public int[] Coordinates(int id)
        {
            int x = -1;
            int y = 0;
            int[] res = new int[2];
            res[0] = -1;
            res[1] = -1;
            while (++x < FieldLength)
            {
                y = -1;
                while (++y < FieldHeight)
                {
                    if (Data[x, y].ID == id)
                    {
                        res[0] = x;
                        res[1] = y;
                        x = FieldLength + 1;
                        y = FieldHeight + 1;
                    }
                }
            }
            return res;
        }

        public bool HaveID(int id)
        {
            foreach (GameToken t in Data)
            {
                if (t.ID == id) return true;
            }
            return false;

        }
    }
}