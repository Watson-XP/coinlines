using System;
using System.Collections;
using System.Collections.Generic;

namespace coinlines
{

    public static class TheGame
    {
        public static gameField field;

    }

    public class gameField
    {
        private int fieldSizeL;
        private int fieldSizeH;
        public int SizeL => fieldSizeL;
        public int SizeH => fieldSizeH;

        private int minSolution = 3; // minimum elements in line to be collected

        private List<int> FieldData;

        public gameField(int sizeL, int sizeH = 0)
        {
            fieldSizeL = sizeL;
            fieldSizeH = ( sizeH == 0 ) ? sizeL : sizeH;
            Init( );
        }

        private void Init( )
        {
            FieldData = new List<int>(fieldSizeH * fieldSizeL);
            for(int i = 0; i < fieldSizeH * fieldSizeL; i++)
                FieldData.Add(0);
        }

        public int Get(int x, int y)
        {
            InRange(x, y);
            return FieldData[x + y * fieldSizeL];
        }

        public void Set(int x, int y, int val)
        {
            InRange(x, y);
            FieldData[ToFlatXY(x, y)] = val;
        }

        private int ToFlatXY(int x, int y) => InRange(x,y,true)?x + (y * fieldSizeL):-1;
        private int FromFlatX(int index) => index % fieldSizeL;
        private int FromFlatY(int index) => index / fieldSizeL;

        private bool InRange(int x, int y, bool onlycheck = false)
        {
            //if ((x + y * fieldSizeL) < FieldData.Count) return true;
            if(( x < fieldSizeL ) && ( y < fieldSizeH ) && (x > -1 ) && (y>-1)) return true;
            if(onlycheck) return false;
            throw new IndexOutOfRangeException(string.Format("x={0}, y={1}, size={2} ", x, y, FieldData.Count));
        }

        public void FindRows( )
        {
            int x;
            int xpos;
            int y = -1;
            int TmpVal;
            int Total = 0;
            while(++y < fieldSizeH)
            {
                xpos = 0;
                x = 0;
                Total = 1;
                TmpVal = Get(x, y);
                while(++x < fieldSizeL)
                {
                    if(TmpVal == Get(x, y))
                    {
                        Total++;
                    }
                    else
                    {
                        if(Total >= minSolution)
                        {
                            while(--Total >= 0)
                            {
                                Set(x - Total - 1, y, 0);
                            }
                        }
                        Total = 1;
                        TmpVal = Get(x, y);
                    }
                }
                if(Total >= minSolution)
                    while(--Total >= 0)
                    {
                        Set(x - 1 - Total, y, 0);
                    }
            }
        }

        public void SetFill(int seed)
        {
            Random random = new Random(seed);
            for(int x = 0; x < fieldSizeL; x++)
                for(int y = 0; y < fieldSizeH; y++)
                    Set(x, y, random.Next(1, 4));
        }

        public void Clasterize( )
        {
            Queue<int> Current = new Queue<int>( );
            List<int> unprocessed = new List<int>();
            List<int> Cluster = new List<int>();

            // process all but zero elements
            for(int i = 0; i < FieldData.Count; i++)                            
                if(FieldData[i] > 0)
                    unprocessed.Add(i);
            

            int index;
            int index2;
            

            while(unprocessed.Count > 0)
            {
                index = unprocessed[0];
                unprocessed.RemoveAt(0);
                Current.Clear( );
                Cluster.Clear( );
                Current.Enqueue(index);

                while(Current.Count > 0)
                {
                    index = Current.Dequeue( );
                    Cluster.Add(index);

                    index2 = ToFlatXY(FromFlatX(index) - 1, FromFlatY(index));
                    if(unprocessed.Contains(index2))
                        if(FieldData[index2] == FieldData[index])
                        {
                            Current.Enqueue(index2);
                            unprocessed.Remove(index2);
                        }

                    index2 = ToFlatXY(FromFlatX(index) + 1, FromFlatY(index));
                    if(unprocessed.Contains(index2))
                        if(FieldData[index2] == FieldData[index])
                        {
                            Current.Enqueue(index2);
                            unprocessed.Remove(index2);
                        }

                    index2 = ToFlatXY(FromFlatX(index), FromFlatY(index) - 1);
                    if(unprocessed.Contains(index2))
                        if(FieldData[index2] == FieldData[index])
                        {
                            Current.Enqueue(index2);
                            unprocessed.Remove(index2);
                        }

                    index2 = ToFlatXY(FromFlatX(index), FromFlatY(index) + 1);
                    if(unprocessed.Contains(index2))
                        if(FieldData[index2] == FieldData[index])
                        {
                            Current.Enqueue(index2);
                            unprocessed.Remove(index2);
                        }
                }

                int linesize;
                int maxline = 0;
                int i;
                int tidx;
                if(Cluster.Count >= minSolution)
                {
                    foreach(int idx in Cluster)
                    {
                        tidx = idx;
                        linesize = 1;
                        while(Cluster.Contains(++tidx) && ( FromFlatY(tidx) == FromFlatY(tidx - 1) ))
                            linesize++;
                        if(linesize > maxline) maxline = linesize;
                        tidx = idx;
                        linesize = 1;                        
                        while(Cluster.Contains(ToFlatXY(FromFlatX(idx), FromFlatY(idx)+ linesize)) )
                            linesize++;
                        if(linesize > maxline) maxline = linesize;
                    }

                    if(maxline >= minSolution)
                    {
                        foreach(int idx in Cluster)
                            FieldData[idx] = 0;
                        break;
                    }
                }
            }
        }
    }

}