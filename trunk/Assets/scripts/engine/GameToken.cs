using System;
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
        internal static readonly int maxIndex = 5;

        public int Value => value;

        public GameToken(int value)
        {
            this.id = IDFactory.GetID();
            this.value = value;
            
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

        internal void NextValue()
        {
            value++;
            if (value >= maxIndex) value = 1;            
        }
    }
}
