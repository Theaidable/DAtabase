using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAtabase.Classes
{
    class Quest
    {
        public int QuestID { get; set; }
        public string Name { get; set; }
        public string Objective { get; set; }
        public bool Completed { get; set; }
    }
}
