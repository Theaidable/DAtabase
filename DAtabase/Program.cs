using DAtabase.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DAtabase
{
    class Program
    {
        static string connectionString = "";

        static void Main(string[] args)
        {
            ChooseDatabase();

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("--- Shattered Reflections ---");
                Console.WriteLine("1. New Game");
                Console.WriteLine("2. Load Game");
                Console.WriteLine("3. Quit Game");
                Console.Write("\nEnter your choice: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        NewGame();
                        break;
                    case "2":
                        LoadGameAndPlay();
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to retry.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Præsenterer brugeren for to databasevalg og sætter connectionString.
        /// </summary>
        static void ChooseDatabase()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Choose database:");
                Console.WriteLine("1) Asbjørn");
                Console.WriteLine("2) David");
                Console.Write("\nEnter your choice: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    connectionString = "Server=MSI\\SQLEXPRESS;Database=DAtabase;Trusted_Connection=True;TrustServerCertificate=True;";
                    break;
                }
                else if (choice == "2")
                {
                    connectionString = "Server=DAVID\\SQLEXPRESS01;Database=Shattered Reflections;Trusted_Connection=True;TrustServerCertificate=True;";
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid choice. Press any key to try again.");
                    Console.ReadKey();
                }
            }
        }

        static void NewGame()
        {
            GameState state = new GameState
            {
                Player = new Player { X = 0, Y = 0, Health = 100, SoulCoins = 0, Objective = "Talk to NPC" },
                Npc = new NPC { X = 10, Y = 0 },
                Enemy = null,
                Inventory = new List<Item>(),
                QuestAccepted = false,
                QuestCompleted = false,
                Quests = new List<Quest>
                {
                    new Quest { QuestID = 1, Name = "Start of your Journey", Objective = "Talk to the old man", Completed = false }
                }
            };

            Console.Clear();
            DisplayGameState(state);
            GameLoop(state);
        }

        static void LoadGameAndPlay()
        {
            Console.Clear();
            Console.WriteLine("Select slot to load or type 'delete' to delete a save:");

            for (int slot = 1; slot <= 4; slot++)
                Console.WriteLine($"{slot}. Slot {slot}: {(CheckIfSaveExists(slot) ? "Save exists" : "No saved game found")}");

            Console.WriteLine("5. Back to main menu");

            string choice = Console.ReadLine();

            if (choice.ToLower() == "delete")
            {
                DeleteSave();
                return;
            }
            if (choice == "5")
                return;

            if (!int.TryParse(choice, out int selectedSlot) || selectedSlot < 1 || selectedSlot > 4)
            {
                Console.WriteLine("Invalid choice. Press Enter.");
                Console.ReadLine();
                return;
            }

            GameState state = LoadGame(selectedSlot);

            if (state == null)
            {
                Console.WriteLine("No saved game found. Press Enter.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Game loaded. Press Enter.");
            Console.ReadLine();

            DisplayGameState(state);
            GameLoop(state);
        }

        static bool CheckIfSaveExists(int slot)
        {
            string query = "SELECT COUNT(*) FROM Player WHERE playerID=@slot";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        static void GameLoop(GameState state)
        {
            bool playing = true;

            while (playing)
            {
                Console.Clear();
                DisplayGameState(state);
                Console.WriteLine("Controls: A,D,W,S (move), E (interact), K (attack), I (inventory), J (quests), Q (save), ESC (quit)");
                Console.WriteLine();

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.A: state.Player.X -= 1; break;
                    case ConsoleKey.D: state.Player.X += 1; break;
                    case ConsoleKey.W: state.Player.Y += 10; break;
                    case ConsoleKey.S: state.Player.Y -= 10; break;
                    case ConsoleKey.E: Player.Interact(state); break;
                    case ConsoleKey.K: Player.Attack(state); break;
                    case ConsoleKey.I: Player.ShowInventory(state); break;
                    case ConsoleKey.J: Player.ShowQuestLog(state); break;
                    case ConsoleKey.Q: SaveGame(state); break;
                    case ConsoleKey.Escape: playing = false; break;
                }
            }
        }

        static void DisplayGameState(GameState state)
        {
            Console.WriteLine($"Player position: x:{state.Player.X}  y:{state.Player.Y}");
            Console.WriteLine($"Health: {state.Player.Health} | Gold: {state.Player.SoulCoins}");

            // Hent objective fra den aktive (ufuldstændige) quest, hvis der er en
            var activeQuest = state.Quests.FirstOrDefault(q => !q.Completed);
            if (activeQuest != null)
                Console.WriteLine($"Current objective: {activeQuest.Objective}");
            else
                Console.WriteLine("No active objective.");

            Console.WriteLine();

            Console.WriteLine($"NPC position: x:{state.Npc.X}  y:{state.Npc.Y}");
            Console.WriteLine();

            if (state.Enemy != null)
                Console.WriteLine($"Enemy '{state.Enemy.Name}' at x:{state.Enemy.X}, y:{state.Enemy.Y}, Health: {state.Enemy.Health}\n");
        }


        static void SaveGame(GameState state)
        {
            Console.WriteLine("Choose slot (1-4):");
            for (int slot = 1; slot <= 4; slot++)
                Console.WriteLine($"{slot}. Slot {slot}: {(CheckIfSaveExists(slot) ? "Exists" : "Empty")}");

            if (!int.TryParse(Console.ReadLine(), out int saveSlot) || saveSlot < 1 || saveSlot > 4)
            {
                Console.WriteLine("Invalid slot. Press Enter.");
                Console.ReadLine();
                return;
            }

            state.SaveSlot = saveSlot;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string playerQuery = @"
                    IF EXISTS(SELECT * FROM Player WHERE playerID=@slot)
                        UPDATE Player SET PlayerHealth=@health, PositionX=@x, PositionY=@y, Currency=@coins WHERE playerID=@slot
                    ELSE
                        INSERT INTO Player VALUES (@slot,@health,@x,@y,@coins)";

                using (SqlCommand cmd = new SqlCommand(playerQuery, con))
                {
                    cmd.Parameters.AddWithValue("@slot", saveSlot);
                    cmd.Parameters.AddWithValue("@health", state.Player.Health);
                    cmd.Parameters.AddWithValue("@x", state.Player.X);
                    cmd.Parameters.AddWithValue("@y", state.Player.Y);
                    cmd.Parameters.AddWithValue("@coins", state.Player.SoulCoins);
                    cmd.ExecuteNonQuery();
                }

                new SqlCommand($"DELETE FROM Inventory WHERE playerID={saveSlot};DELETE FROM QuestLog WHERE playerID={saveSlot};", con).ExecuteNonQuery();

                foreach (var item in state.Inventory)
                    new SqlCommand($"INSERT INTO Inventory(playerID,itemID,Amount) SELECT {saveSlot},itemID,{item.Amount} FROM Items WHERE itemName='{item.Name}'", con).ExecuteNonQuery();

                foreach (var quest in state.Quests)
                    new SqlCommand($"INSERT INTO QuestLog(playerID,questID,completed) SELECT {saveSlot},questID,{(quest.Completed ? 1 : 0)} FROM Quest WHERE questName='{quest.Name}'", con).ExecuteNonQuery();
            }
            Console.WriteLine("Game saved. Press Enter.");
            Console.ReadLine();
        }

        static GameState LoadGame(int slot)
        {
            GameState state = new GameState { SaveSlot = slot };

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Indlæs Player
                string queryPlayer = "SELECT * FROM Player WHERE playerID = @slot";
                using (SqlCommand cmd = new SqlCommand(queryPlayer, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        state.Player = new Player
                        {
                            Health = reader.GetInt32(reader.GetOrdinal("PlayerHealth")),
                            X = (int)reader.GetDouble(reader.GetOrdinal("PositionX")),
                            Y = (int)reader.GetDouble(reader.GetOrdinal("PositionY")),
                            SoulCoins = reader.GetInt32(reader.GetOrdinal("Currency")),
                            Objective = "Continue your journey"
                        };
                    }
                }

                // Indlæs Enemy
                string enemyQuery = "SELECT * FROM Enemy WHERE enemyID = @slot";
                using (SqlCommand cmd = new SqlCommand(enemyQuery, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            state.Enemy = new Enemy
                            {
                                Health = reader.GetInt32(reader.GetOrdinal("EnemyHealth")),
                                X = (int)reader.GetDouble(reader.GetOrdinal("PositionX")),
                                Y = (int)reader.GetDouble(reader.GetOrdinal("PositionY")),
                                Name = reader.GetString(reader.GetOrdinal("name"))
                            };
                        }
                        else
                            state.Enemy = null;
                    }
                }

                // Sæt NPC (vi antager en fast position for NPC)
                state.Npc = new NPC { X = 10, Y = 0 };

                // Indlæs Inventory
                state.Inventory = new List<Item>();
                string inventoryQuery = @"
                    SELECT I.itemID, I.itemName, Inv.amount 
                    FROM [Inventory] Inv
                    JOIN [Items] I ON Inv.itemID = I.itemID 
                    WHERE Inv.playerID = @slot";

                using (SqlCommand cmd = new SqlCommand(inventoryQuery, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            state.Inventory.Add(new Item
                            {
                                ItemID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Amount = reader.GetInt32(2)
                            });
                        }
                    }
                }

                // Indlæs QuestLog
                state.Quests = new List<Quest>();
                string questLogQuery = @"
                    SELECT Q.questID, Q.questName, Q.questObjective, QL.completed
                    FROM [QuestLog] QL
                    JOIN [Quest] Q ON QL.questID = Q.questID 
                    WHERE QL.playerID = @slot";

                using (SqlCommand cmd = new SqlCommand(questLogQuery, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            state.Quests.Add(new Quest
                            {
                                QuestID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Objective = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Completed = reader.GetBoolean(3)
                            });
                        }
                    }
                }
            }

            return state;
        }


        static void DeleteSave()
        {
            Console.WriteLine("Enter slot (1-4):");
            if (!int.TryParse(Console.ReadLine(), out int slot) || slot < 1 || slot > 4)
            {
                Console.WriteLine("Invalid slot. Press Enter to continue.");
                Console.ReadLine();
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Slet rækker i Inventory, der refererer til spilleren
                string deleteInventory = "DELETE FROM Inventory WHERE playerID = @slot";
                using (SqlCommand cmd = new SqlCommand(deleteInventory, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    cmd.ExecuteNonQuery();
                }

                // Slet rækker i QuestLog, der refererer til spilleren
                string deleteQuestLog = "DELETE FROM QuestLog WHERE playerID = @slot";
                using (SqlCommand cmd = new SqlCommand(deleteQuestLog, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    cmd.ExecuteNonQuery();
                }

                // Nu slet spilleren
                string deletePlayer = "DELETE FROM Player WHERE playerID = @slot";
                using (SqlCommand cmd = new SqlCommand(deletePlayer, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        Console.WriteLine($"Save slot {slot} deleted successfully.");
                    else
                        Console.WriteLine("Nothing to delete.");
                }
            }

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }


    }
}