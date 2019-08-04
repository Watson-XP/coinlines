using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conilines.engine
{
    public struct GameTokenData
    {
        public int x;
        public int y;
        public int id;
        public int value;

        internal GameTokenData(ItemData itd) : this()
        {
            x = itd.x;
            y = itd.y;
            id = itd.Token.ID;
            value = itd.Token.Value;
        }

        internal GameTokenData(int x, int y, GameToken t) : this()
        {
            this.x = x;
            this.y = y;
            id = t.ID;
            value = t.Value;
        }
    }

    public class TokenEventArgs : EventArgs
    {
        public List<GameTokenData> Tokens;

        public TokenEventArgs()
        {
            Tokens = new List<GameTokenData>();
        }
    }

    public class GameField
    {
        //public delegate void DoInitField();

        public event EventHandler<TokenEventArgs> TokensAdded;
        public event EventHandler<TokenEventArgs> TokensKilled;
        public event EventHandler<TokenEventArgs> FieldChanged;

        private readonly int minLine = 3;
        private GameToken[,] Data;
        public GameToken this[int x, int y]
        {
            get { return Data[x, y]; }
        }

        private int sizeL;
        private int sizeH;
        public Directions SlideDirection { get; set; }

        private readonly int Seed;
        public int FieldLength => sizeL;
        public int FieldHeight => sizeH;
        Random rnd;
        private int TotalTokens;

        public int nextSeed
        {
            get
            {
                return rnd.Next(1, GameToken.maxIndex);
            }
        }

        public bool Complete { get { return (TotalTokens == sizeL * sizeH); } }

        public GameField(int sizeL = 10, int sizeH = 10, int seed = 0)
        {
            this.sizeL = sizeL;
            this.sizeH = sizeH;
            Data = new GameToken[sizeL, sizeH];
            Seed = seed;
            SlideDirection = Directions.Up;
            InitField();
        }

        public void InitField()
        {
            rnd = new Random(Seed);

            for (int x = 0; x < sizeL; x++)
                for (int y = 0; y < sizeH; y++)
                    Data[x, y] = new GameToken(rnd.Next(1, GameToken.maxIndex));
            TotalTokens = sizeH * sizeL;

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

        public bool GetLines(bool FindAndKill = true)
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
                        }else
                            xlen = 0;

                        if (xmax < xlen)
                        {
                            xmax = xlen;
                            xmaxstart = xlenstart;
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

                TokenEventArgs ea = new TokenEventArgs();

                if (Cluster.Count > 0)
                {
                    if (FindAndKill)
                    {
                        foreach (ItemData itd in Cluster)
                        {
                            Data[itd.x, itd.y].Kill();
                            TotalTokens--;
                            ea.Tokens.Add(new GameTokenData(itd));
                        }
                        OnTokensKilled(ea);
                    }
                    return true;
                }


            }
            x = -1;
            while ((++x < sizeL) && !linefound) // scan columns
            {
                y = 0;
                xlen = 0;
                while (++y < sizeH)
                {
                    if (Data[x, y].Alive && Data[x, y-1].Alive)
                    {
                        if (Data[x, y].Value == Data[x, y-1].Value)
                        {
                            if (xlen == 0)
                            {
                                xlenstart = y - 1;
                                xlen++;
                            }
                            xlen++;
                        }
                        else
                            xlen = 0;

                        if (xmax < xlen)
                        {
                            xmax = xlen;
                            xmaxstart = xlenstart;
                        }
                        if (xmax >= minLine)
                            linefound = true;
                    }
                }
            }

            if (linefound)
            {
                for (int iy = xmaxstart; iy < xmaxstart + xmax; iy++)
                    Cluster.Add(new ItemData() { x = x-1, y = iy, check = false, Token = Data[x-1, iy] });

                BuildCluster(ref Cluster, x: x-1, y: xmaxstart);

                TokenEventArgs ea = new TokenEventArgs();

                if (Cluster.Count > 0)
                {
                    if (FindAndKill)
                    {
                        foreach (ItemData itd in Cluster)
                        {
                            Data[itd.x, itd.y].Kill();
                            TotalTokens--;
                            ea.Tokens.Add(new GameTokenData(itd));
                        }
                        OnTokensKilled(ea);
                    }
                    return true;
                }
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

        internal GameToken NextToken(int x, int y)
        {
            switch (SlideDirection)
            {
                case Directions.Up:
                    if (InRange(x, y - 1))
                        return Data[x, y - 1];
                    break;
                case Directions.Down:
                    if (InRange(x, y + 1))
                        return Data[x, y + 1];
                    break;
                case Directions.Left:
                    if (InRange(x - 1, y))
                        return Data[x - 1, y];
                    break;
                case Directions.Right:
                    if (InRange(x + 1, y))
                        return Data[x + 1, y];
                    break;
            }
            return null;
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
                case Directions.Up: dy = 1; break;
                case Directions.Down: dy = -1; break;
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
            if (result)
            {
                TokenEventArgs ea = new TokenEventArgs();
                OnFieldChanged(ea);
            }
            return result;
        }

        private void AddToken(int x, int y, TokenEventArgs ea)
        {
            Data[x, y] = new GameToken(nextSeed);
            ea.Tokens.Add(new GameTokenData(x, y, Data[x, y]));
            TotalTokens++;
        }

        public void Fill(bool nolines = false)
        {
            TokenEventArgs ea = new TokenEventArgs();

            bool loop = false;
            do
            {
                loop = false;
                for (int x = 0; x < sizeL; x++)
                    for (int y = 0; y < sizeH; y++)
                    {
                        if (InRange(x, y))
                            if (!Data[x, y].Alive)
                            {
                                //GameToken t = ;
                                Data[x, y] = new GameToken(nextSeed);
                                ea.Tokens.Add(new GameTokenData(x, y, Data[x, y]));
                                TotalTokens++;
                            }
                    }
                if (nolines)
                    loop = GetLines();
                if (loop) Slide();
                while (nolines && GetLines()) Slide();

            } while (loop);
            if (ea.Tokens.Count > 0)
                OnTokensAdded(ea);
        }

        public void FillOneLine(bool nolines = false)
        {
            TokenEventArgs ea = new TokenEventArgs();


            //int x, y, sx, sy, ex, ey, dx, dy, steps;
            //switch (SlideDirection)
            //{
            //    case Directions.Up:
            //        sx = 0; sy = sizeH - 1; ex = sizeL; ey = 0; steps = sizeH;
            //        break;
            //    case Directions.Down:
            //        sx = 0; sy = 0; ex = sizeL; ey = sizeH - 1; steps = sizeH;
            //        break;
            //    case Directions.Left:
            //        sx = sizeL - 1; sy = 0; ex = 0; ey = sizeH - 1; steps = sizeL;
            //        break;
            //    case Directions.Right:
            //        sx = 0; sy = 0; ex = 0; ey = sizeH - 1; steps = sizeL;
            //        break;
            //}
            //dx = Math.Sign(ex - sx); dy = Math.Sign(ey - sy);

            switch (SlideDirection)
            {
                case Directions.Up:

                    for (int x = 0; x < sizeH; x++)
                    {
                        int y = sizeH - 1;
                        while ((y >= 0) && (Data[x, y].Alive)) y--;
                        if (y >= 0)
                        {
                            AddToken(x, y, ea);
                        }
                    }
                    break;
                case Directions.Down:
                    for (int x = 0; x < sizeH; x++)
                    {
                        int y = 0;
                        while ((y < sizeH) && (Data[x, y].Alive)) y++;
                        if (y < sizeH)
                        {
                            AddToken(x, y, ea);
                        }
                    }
                    break;
                case Directions.Right:
                    for (int y = 0; y < sizeH; y++)
                    {
                        int x = sizeH - 1;
                        while ((x >= 0) && (Data[x, y].Alive)) x--;
                        if (x >= 0)
                        {
                            AddToken(x, y, ea);
                        }
                    }
                    break;
                case Directions.Left:
                    for (int y = 0; y < sizeH; y++)
                    {
                        int x = 0;
                        while ((x < sizeH) && (Data[x, y].Alive)) x++;
                        if (x < sizeL)
                        {
                            AddToken(x, y, ea);
                        }
                    }
                    break;
            }
            if (ea.Tokens.Count > 0)
                OnTokensAdded(ea);
            else
                TotalTokens = sizeH * sizeL;
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


        protected virtual void OnTokensAdded(TokenEventArgs e)
        {
            EventHandler<TokenEventArgs> handler = TokensAdded;
            handler?.Invoke(this, e);
        }

        protected virtual void OnTokensKilled(TokenEventArgs e)
        {
            EventHandler<TokenEventArgs> handler = TokensKilled;
            handler?.Invoke(this, e);
        }

        protected virtual void OnFieldChanged(TokenEventArgs e)
        {
            EventHandler<TokenEventArgs> handler = FieldChanged;
            handler?.Invoke(this, e);
        }


    }
}