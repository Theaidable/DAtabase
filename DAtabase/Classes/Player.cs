using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Hvis spilleren er ved NPC'en
            if (state.Player.X == state.Npc.X && state.Player.Y == state.Npc.Y)
            {
                //Første gang man snakker med NPC'en
                if (state.QuestAccepted == false)
                {
                    Console.WriteLine("NPC: 'Hey, you, wanderer, come here for a moment, I want you to help me out with something. " +
                        "You see that monster over there, he is blocking the path to move forward, could you help me by defeating it. " +
                        "My old bones has gotten the better of me...'");

                    state.QuestAccepted = true;
                    state.Player.Objective = "Remove Monster";

                    //Spawn enemy
                    state.Enemy = new Enemy { X = 15, Y = 0, Health = 100 };
                }

                //Hvis man klaret questen
                else if (state.QuestAccepted && state.QuestCompleted)
                {
                    Console.WriteLine("NPC: 'Well done! Thank you for defeating the monster. " +
                        "Here, take this napkin, 50 soulcoins and potions and be on your way.'");

                    //Giv spilleren deres reward
                    state.Player.SoulCoins += 50;
                    AddToInventory(state, "Bloody napkin", 1);
                    AddToInventory(state, "Potion", 2);

                    //Set booleans
                    state.QuestAccepted = false;
                    state.QuestCompleted = false;
                    state.Enemy = null;
                }

                //Hvis man snakker med ham igen efter man har accepteret quest, men ikke klaret quest
                else
                {
                    Console.WriteLine("NPC: 'Take your time, but please hurry and KILL IT!'");
                }

                Console.WriteLine();
                Console.WriteLine("Press Enter to continue.");
                Console.ReadLine();
            }

            else
            {
                Console.WriteLine("There is nothing to interact with here.");

                Console.WriteLine("Press Enter to continue.");
                Console.ReadLine();
            }
        }

        public static void Attack(GameState state)
        {
            int damage = 25;

            // Hvis enemy findes og spilleren er på samme position som enemy
            if (state.Enemy != null && state.Player.X == state.Enemy.X && state.Player.Y == state.Enemy.Y)
            {
                state.Enemy.Health -= damage;
                Console.WriteLine($"Player dealt {damage} damage to enemy");

                if (state.Enemy.Health <= 0)
                {
                    Console.WriteLine("Player has defeated the enemy!");
                    state.QuestCompleted = true;
                    state.Enemy = null;
                    state.Player.Objective = "Talk to NPC";
                }

                Console.WriteLine("Press Enter to continue.");
                Console.ReadLine();
            }

            else
            {
                if (state.Enemy == null)
                {
                    Console.WriteLine("Player is attacking nothing.");
                }
                else
                {
                    Console.WriteLine("Player can't attack here.");
                }

                Console.WriteLine("Press Enter to continue.");
                Console.ReadLine();
            }
        }

        public static void ShowInventory(GameState state)
        {
            Console.WriteLine("--- Inventory ---");

            //Hvis man ikke har nogen items
            if (state.Inventory.Count == 0)
            {
                Console.WriteLine("Inventory is empty.");
            }

            //Else vis alle ens items
            else
            {
                foreach (var item in state.Inventory)
                {
                    Console.WriteLine($"- {item.Name} x{item.Amount}");
                }
            }

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }

        public static void AddToInventory(GameState state, string itemName, int amount)
        {
            var existing = state.Inventory.Find(i => i.Name == itemName);

            if (existing != null)
            {
                existing.Amount += amount;
            }

            else
            {
                state.Inventory.Add(new Item { Name = itemName, Amount = amount });
            }
        }

        public static void ShowQuestLog(GameState state)
        {
            Console.Clear();
            Console.WriteLine("--- Quest Log ---");

            if (state.Quests.Count == 0)
            {
                Console.WriteLine("No quests currently.");
            }
            else
            {
                foreach (var quest in state.Quests)
                {
                    string status = quest.Completed ? "Completed" : "Active";
                    Console.WriteLine($"{quest.Name} --- {status}");
                    Console.WriteLine($"Objective: {quest.Objective}\n");
                }
            }
            Console.WriteLine("Press Enter to return...");
            Console.ReadLine();
        }

    }
}
