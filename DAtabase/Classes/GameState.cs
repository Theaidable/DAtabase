using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAtabase.Classes
{
    class GameState
    {
        //Public properties
        public Player Player { get; set; }
        public NPC Npc { get; set; }
        public Enemy Enemy { get; set; }
        public List<Item> Inventory { get; set; }
        public bool QuestAccepted { get; set; }
        public bool QuestCompleted { get; set; }
        public string Objective { get; set; }
    }
}
