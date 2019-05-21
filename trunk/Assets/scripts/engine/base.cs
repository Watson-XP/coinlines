using System;
using System.Collections;
using System.Collections.Generic;

namespace coinlines {

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

        private void Init()
        {
            FieldData = new List<int>(fieldSizeH*fieldSizeL );
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
            FieldData[x + y * fieldSizeL] = val;
        }

        private bool InRange(int x, int y, bool onlycheck=false)
        {
            //if ((x + y * fieldSizeL) < FieldData.Count) return true;
            if(( x < fieldSizeL ) && ( y < fieldSizeH )) return true;
            if(onlycheck) return false;
            throw new IndexOutOfRangeException(string.Format("x={0}, y={1}, size={2} ", x, y, FieldData.Count ));
        }

        public void FindRows( )
        {
            int x;
            int xpos;
            int y = 0;
            int TmpVal;
            int Total = 0;
            while(y++ < fieldSizeH)
            {
                xpos = 0;
                x = 0;
                Total = 0;
                TmpVal = Get(x, y);
                while(++x < fieldSizeL)
                {
                    if(TmpVal == Get(x, y))
                    {
                        Total++;                        
                    }
                    else
                    {
                        if (Total >= minSolution)
                        {
                            x--;
                            break;
                        }
                        Total = 0;
                        TmpVal = Get(x, y);
                    }
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
    }  

}