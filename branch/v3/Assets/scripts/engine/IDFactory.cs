using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSTools
{
    public static class IDFactory
    {
        private static int id = 0;

        public static int GetID( )
        {
            return id++;
        }

    }
}
