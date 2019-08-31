﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSTools;

namespace conilines.engine
{
    public class GameToken
    {
        private readonly int id;
        public int ID => id;
        private int value;
        

        public int Value => value;
        public bool Alive { get; private set; }

        public GameToken(int value)
        {
            this.id = IDFactory.GetID();
            this.value = value;
            Alive = true;
        }

        public static bool operator ==(GameToken a, GameToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GameToken a, GameToken b)
        {
            return !a.Equals(b);
        }

        public static bool operator >(GameToken a, GameToken b)
        {
            return a.Value > b.Value;
        }
        public static bool operator <(GameToken a, GameToken b)
        {
            return a.Value < b.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if(obj.GetType( ) == typeof(int)) return this.value == (int)obj;
            if(obj.GetType( ) != typeof(GameToken)) return false;
            return  ((GameToken)obj).Value == value ;
        }

        public override int GetHashCode( )
        {
            return base.GetHashCode( );
        }

        public override string ToString( )
        {
            return string.Format("[{0}] {1}", id, value);
        }

        public void Kill()
        {
            Alive = false;
        }        
    }
}
