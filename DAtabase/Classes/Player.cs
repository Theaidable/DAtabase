using System;
using System.Linq;

namespace DAtabase.Classes
{
    class Player
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; set; }
        public int SoulCoins { get; set; }
        public string Objective { get; set; }

        public static void Interact(GameState state)
        {
            // Hvis spilleren er ved NPC'ens position
            if (state.Player.X == state.Npc.X && state.Player.Y == state.Npc.Y)
            {
                // Hvis quest 2 (den, der gives af NPC'en) allerede er fuldført, siger NPC’en "Thank you so much."
                if (state.Quests.Any(q => q.QuestID == 2 && q.Completed))
                {
                    Console.WriteLine("Old Man: 'Thank you so much.'");
                }
                // Hvis quest endnu ikke er accepteret, tilbydes den
                else if (!state.QuestAccepted)
                {
                    Console.WriteLine("Old Man: 'Hey, wanderer! Could you help me? There's a monster ahead blocking the path. My old bones can't handle it. Please defeat it for me!'");
                    state.QuestAccepted = true;
                    state.Player.Objective = "Remove Monster";
                    state.Enemy = new Enemy { X = 15, Y = 0, Health = 100, Name = "Slime" };

                    // Tilføj quest 2 til questloggen og markér quest 1 (Start of your Journey) som fuldført
                    state.Quests.Add(new Quest { QuestID = 2, Name = "Tale of the old man 1", Objective = "Remove Monster", Completed = false });
                    var initialQuest = state.Quests.FirstOrDefault(q => q.QuestID == 1);
                    if (initialQuest != null)
                    {
                        initialQuest.Completed = true;
                    }
                }
                // Hvis quest er accepteret og nu er fuldført (for eksempel via et angreb på enemy)
                else if (state.QuestAccepted && state.QuestCompleted)
                {
                    Console.WriteLine("Old Man: 'You did it! Here, take this bloody napkin, some coins and potions as thanks.'");
                    state.Player.SoulCoins += 50;
                    AddToInventory(state, "Bloody napkin", 1);
                    AddToInventory(state, "Potion", 2);

                    // Marker quest 2 som fuldført
                    var quest2 = state.Quests.FirstOrDefault(q => q.QuestID == 2);
                    if (quest2 != null)
                        quest2.Completed = true;

                    state.QuestAccepted = false;
                    state.QuestCompleted = false;
                    state.Enemy = null;
                    state.Player.Objective = "Journey onward";
                }
                // Hvis quest er accepteret, men ikke endnu fuldført
                else
                {
                    Console.WriteLine("Old Man: 'Please hurry up and defeat that monster!'");
                }

                Console.WriteLine("\nPress Enter to continue.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Nothing here to interact with. Press Enter to continue.");
                Console.ReadLine();
            }
        }


        public static void Attack(GameState state)
        {
            int damage = 25;

            if (state.Enemy != null && state.Player.X == state.Enemy.X && state.Player.Y == state.Enemy.Y)
            {
                state.Enemy.Health -= damage;
                Console.WriteLine($"Player dealt {damage} damage to enemy '{state.Enemy.Name}'");

                if (state.Enemy.Health <= 0)
                {
                    Console.WriteLine("Player has defeated the enemy!");
                    state.QuestCompleted = true;
                    state.Enemy = null;
                    state.Player.Objective = "Go back to the Old Man";
                }
            }
            else
            {
                Console.WriteLine(state.Enemy == null ? "Player is attacking nothing." : "Player can't attack here.");
            }

            Console.WriteLine("\nPress Enter to continue.");
            Console.ReadLine();
        }

        public static void ShowInventory(GameState state)
        {
            Console.Clear();
            Console.WriteLine("--- Inventory ---");

            if (!state.Inventory.Any())
            {
                Console.WriteLine("Inventory is empty.");
            }
            else
            {
                foreach (var item in state.Inventory)
                    Console.WriteLine($"- {item.Name} x{item.Amount}");
            }

            Console.WriteLine("\nPress Enter to continue.");
            Console.ReadLine();
        }

        public static void AddToInventory(GameState state, string itemName, int amount)
        {
            var existing = state.Inventory.FirstOrDefault(i => i.Name == itemName);

            if (existing != null)
                existing.Amount += amount;
            else
                state.Inventory.Add(new Item { Name = itemName, Amount = amount });
        }

        public static void ShowQuestLog(GameState state)
        {
            Console.Clear();
            Console.WriteLine("--- Quest Log ---");

            if (!state.Quests.Any())
            {
                Console.WriteLine("No quests currently.");
            }
            else
            {
                foreach (var quest in state.Quests)
                    Console.WriteLine($"{quest.Name} --- {(quest.Completed ? "Completed" : "Active")}\nObjective: {quest.Objective}\n");
            }

            Console.WriteLine("Press Enter to return...");
            Console.ReadLine();
        }
    }
}
