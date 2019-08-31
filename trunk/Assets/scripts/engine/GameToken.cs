using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSTools;

namespace conilines.engine
{
    /// <summary>
    /// Token and it's properties
    /// </summary>
    public class GameToken
    {
        private readonly int id;
        /// <summary>
        /// global identifier 
        /// </summary>
        public int ID => id;
        private readonly int _value;       

        /// <summary>
        /// token value
        /// </summary>
        public int Value => _value;

        /// <summary>
        /// alive tokens are playable
        /// </summary>
        public bool Alive { get; private set; }

        /// <summary>
        /// initialize value
        /// </summary>
        /// <param name="value"></param>
        public GameToken(int value)
        {
            this.id = IDFactory.GetID();
            this._value = value;
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
            if(obj.GetType( ) == typeof(int)) return this._value == (int)obj;
            if(obj.GetType( ) != typeof(GameToken)) return false;
            return  ((GameToken)obj).Value == _value ;
        }

        public override int GetHashCode( )
        {
            return base.GetHashCode( );
        }

        public override string ToString( )
        {
            return string.Format("[{0}] {1}", id, _value);
        }

        public void Kill()
        {
            Alive = false;
        }        
    }
}
