﻿using System;
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
            SlideDirection = Directions.Up;
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
            int idx;
            bool linefound = false;

            for (int x = 0; x < sizeL; x++)
                for (int y = 0; y < sizeH; y++)
                    if (Data[x, y].Value > 0)
                        Everything.Add(new ItemData() { x = x, y = y, check = false, Token = Data[x, y] });

            while (Everything.Count() > 0 && !linefound)
            {
                Candidate.Clear();
                Cluster.Clear();
                Candidate.Enqueue(Everything[0]);
                Everything.RemoveAt(0);
                do
                {
                    ItemData tmp = Candidate.Dequeue();
                    if (tmp.x > 0)
                        if (Data[tmp.x - 1, tmp.y].Value == tmp.Token.Value)
                        {
                            idx = Everything.FindIndex(t => ((t.x == tmp.x - 1) && (t.y == tmp.y)));
                            if (idx > -1)
                            {
                                Candidate.Enqueue(Everything[idx]);
                                Everything.RemoveAt(idx);
                                tmp.check = true;
                            }
                        }
                    if (tmp.x < sizeL - 1)
                        if (Data[tmp.x + 1, tmp.y].Value == tmp.Token.Value)
                        {
                            idx = Everything.FindIndex(t => ((t.x == tmp.x + 1) && (t.y == tmp.y)));
                            if (idx > -1)
                            {
                                Candidate.Enqueue(Everything[idx]);
                                Everything.RemoveAt(idx);
                                tmp.check = true;
                            }
                        }
                    if (tmp.y < sizeH - 1)
                        if (Data[tmp.x, tmp.y + 1].Value == tmp.Token.Value)
                        {
                            idx = Everything.FindIndex(t => ((t.x == tmp.x) && (t.y == tmp.y + 1)));
                            if (idx > -1)
                            {

                                Candidate.Enqueue(Everything[idx]);
                                Everything.RemoveAt(idx);
                                tmp.check = true;
                            }
                        }
                    if (tmp.y > 0)
                        if (Data[tmp.x, tmp.y - 1].Value == tmp.Token.Value)
                        {
                            idx = Everything.FindIndex(t => ((t.x == tmp.x) && (t.y == tmp.y - 1)));
                            if (idx > -1)
                            {
                                Candidate.Enqueue(Everything[idx]);
                                Everything.RemoveAt(idx);
                                tmp.check = true;
                            }
                        }
                    Cluster.Add(tmp);
                } while (Candidate.Count > 0);

                if (Cluster.Count >= minLine)
                {
                    Cluster.Sort(CompareBySum);
                    int x = 1;
                    while (Cluster.FindIndex(t => t.x == Cluster[0].x + x && t.Token.Value == Cluster[0].Token.Value) > -1) x++;
                    if (x < minLine)
                    {
                        x = 1;
                        while (Cluster.FindIndex(t => t.y == Cluster[0].y + x && t.Token.Value == Cluster[0].Token.Value) > -1) x++;
                    }
                    if (x >= minLine)
                    {
                        Cluster.ForEach(delegate (ItemData d) { Data[d.x, d.y] = new GameToken(0); });
                        //Everything.Clear( );
                        linefound = true;
                    }
                }

            }

            return linefound;// (Cluster.Count >= minLine);
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
                            if (Data[x, y].Value != 0)
                                if (Data[x + dx, y + dy].Value == 0)
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
                            if (Data[x, y].Value == 0)
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
            if (InRange(x,y))
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