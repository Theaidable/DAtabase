using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAtabase.Classes
{
    class Enemy
    {
        //Public properties

        //X position
        public int X { get; set; }

        //Y position
        public int Y { get; set; }

        //Enemy's liv
        public int Health { get; set; }

        //Enemy's navn / type
        public string Name { get; set; }
    }
}
