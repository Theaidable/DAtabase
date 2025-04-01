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
        public List<Quest> Quests { get; set; }
        public bool QuestAccepted { get; set; }
        public bool QuestCompleted { get; set; }
        public int SaveSlot { get; set; }

        /// <summary>
        /// Constructor til at oprette en ny gamestate
        /// </summary>
        public GameState()
        {
            Inventory = new List<Item>();
            Quests = new List<Quest>();
        }
    }
}
