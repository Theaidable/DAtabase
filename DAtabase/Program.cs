using DAtabase.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAtabase
{
    class Program
    {
        //string som skal bruges til at forbinde til databasen
        static string connectionString = @"Server=MSI\SQLEXPRESS;Database=DAtabase;Trusted_Connection=True;";

        static void Main(string[] args)
        {
            bool exit = false;

            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine("--- Shattered Reflections ---");
                Console.WriteLine("1. New Game");
                Console.WriteLine("2. Load Game");
                Console.WriteLine("3. Quit Game");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();
                switch (choice)
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
                        Console.WriteLine("Invalid choice. Press any key to try again.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Metode til at starte et nyt spil
        /// </summary>
        static void NewGame()
        {
            GameState state = new GameState();

            //Instansiere en player
            state.Player = new Player { X = 0, Y = 0, Health = 100, SoulCoins = 0 };

            //Instansiere en NPC
            state.Npc = new NPC { X = 10, Y = 0 };

            state.Enemy = null;
            state.Inventory = new List<Item>();
            state.QuestAccepted = false;
            state.QuestCompleted = false;
            state.Objective = "Talk to NPC";

            Console.Clear();
            Console.WriteLine("Initializing Game...");

            Console.WriteLine();
            Console.WriteLine("Game initialized and saved. Press Enter to continue.");
            Console.ReadLine();
            
            DisplayGameState(state);
            GameLoop(state);
        }

        static void LoadGameAndPlay()
        {
            Console.Clear();
            Console.WriteLine("Select slot to load or type 'delete' to delete a save:");

            for (int slot = 1; slot <= 4; slot++)
            {
                bool hasSave = CheckIfSaveExists(slot);
                Console.WriteLine($"{slot}. Slot {slot}: {(hasSave ? "Save exists" : "No saved game found")}");
            }

            Console.WriteLine();
            Console.WriteLine("5: Go back to main menu");

            string choice = Console.ReadLine();

            if (choice.ToLower() == "delete")
            {
                DeleteSave();
                return;
            }
            else if (choice == "5")
            {
                return;
            }

            int selectedSlot;

            if (!int.TryParse(choice, out selectedSlot) || selectedSlot < 1 || selectedSlot > 4)
            {
                Console.WriteLine("Invalid choice. Press Enter to continue");
                Console.ReadLine();
                return;
            }

            GameState state = LoadGame(selectedSlot);

            if (state == null)
            {
                Console.WriteLine("No saved game found on this slot. Press Enter to continue.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Game loaded. Press Enter to continue playing.");
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
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        static void GameLoop(GameState state)
        {
            bool playing = true;

            while (playing == true)
            {
                Console.Clear();
                DisplayGameState(state);
                Console.WriteLine("Controls: A (left), D (right), W (jump), S (fall)");
                Console.WriteLine("E to interact, K to attack, I to view inventory, Q to save & ESC to quit to main menu");
                Console.WriteLine();

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.A:
                        state.Player.X -= 1;
                        break;
                    case ConsoleKey.D:
                        state.Player.X += 1;
                        break;
                    case ConsoleKey.W:
                        state.Player.Y += 10;
                        break;
                    case ConsoleKey.S:
                        state.Player.Y -= 10;
                        break;
                    case ConsoleKey.E:
                        Player.Interact(state);
                        break;
                    case ConsoleKey.K:
                        Player.Attack(state);
                        break;
                    case ConsoleKey.I:
                        Player.ShowInventory(state);
                        break;
                    case ConsoleKey.Q:
                        SaveGame(state);
                        break;
                    case ConsoleKey.Escape:
                        playing = false;
                        continue;
                    default:
                        break;
                }
            }
        }

        static void DisplayGameState(GameState state)
        {
            Console.WriteLine($"Player position: x: {state.Player.X}  y: {state.Player.Y}");
            Console.WriteLine($"Player health: {state.Player.Health}");
            Console.WriteLine($"Player gold: {state.Player.SoulCoins}");
            Console.WriteLine($"Current objective: {state.Objective}");
            Console.WriteLine();
            Console.WriteLine($"NPC position: x: {state.Npc.X}  y: {state.Npc.Y}");
            Console.WriteLine();


            if (state.Enemy != null)
            {
                Console.WriteLine($"Enemy position: x: {state.Enemy.X}  y: {state.Enemy.Y}");
                Console.WriteLine($"Enemy health: {state.Enemy.Health}");
                Console.WriteLine();
            }
        }

        static void SaveGame(GameState state)
        {
            Console.WriteLine("Choose save slot (1-4):");
            for (int slot = 1; slot <= 4; slot++)
            {
                bool hasSave = CheckIfSaveExists(slot);
                Console.WriteLine($"{slot}. Slot {slot}: {(hasSave ? "Save exists" : "No saved game found")}");
            }


            int saveSlot;

            if (!int.TryParse(Console.ReadLine(), out saveSlot) || saveSlot < 1 || saveSlot > 4)
            {
                Console.WriteLine("Invalid slot. Press Enter to continue.");
                Console.ReadLine();
                return;
            }

            state.SaveSlot = saveSlot;

            string queryPlayer = @"
                IF EXISTS (SELECT * FROM Player WHERE playerID = @slot)
                    UPDATE Player SET PlayerHealth=@health, PositionX=@x, PositionY=@y, Currency=@coins WHERE playerID=@slot
                ELSE
                    INSERT INTO Player(playerID,PlayerHealth,PositionX,PositionY,Currency) VALUES (@slot,@health,@x,@y,@coins);";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(queryPlayer, con))
                {
                    cmd.Parameters.AddWithValue("@slot", state.SaveSlot);
                    cmd.Parameters.AddWithValue("@health", state.Player.Health);
                    cmd.Parameters.AddWithValue("@x", state.Player.X);
                    cmd.Parameters.AddWithValue("@y", state.Player.Y);
                    cmd.Parameters.AddWithValue("@coins", state.Player.SoulCoins);
                    cmd.ExecuteNonQuery();
                }

                // Gem Enemy-state (hvis enemy findes)
                if (state.Enemy != null)
                {
                    string enemyQuery = @"
                        IF EXISTS (SELECT * FROM Enemies WHERE enemyID = @slot)
                            UPDATE Enemies SET EnemyHealth=@health,PositionX=@x,PositionY=@y WHERE enemyID=@slot
                        ELSE
                            INSERT INTO Enemies(enemyID,EnemyHealth,PositionX,PositionY) VALUES (@slot,@health,@x,@y);";

                    using (SqlCommand cmd = new SqlCommand(enemyQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@slot", state.SaveSlot);
                        cmd.Parameters.AddWithValue("@health", state.Enemy.Health);
                        cmd.Parameters.AddWithValue("@x", state.Enemy.X);
                        cmd.Parameters.AddWithValue("@y", state.Enemy.Y);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Gem Inventory-state
                string clearInventory = "DELETE FROM Inventory WHERE playerID=@slot";
                using (SqlCommand clearCmd = new SqlCommand(clearInventory, con))
                {
                    clearCmd.Parameters.AddWithValue("@slot", state.SaveSlot);
                    clearCmd.ExecuteNonQuery();
                }

                foreach (var item in state.Inventory)
                {
                    string inventoryQuery = "INSERT INTO Inventory(playerID,itemID,Amount) VALUES(@slot,@itemName,@amount);";
                    using (SqlCommand cmd = new SqlCommand(inventoryQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@slot", state.SaveSlot);
                        cmd.Parameters.AddWithValue("@itemName", item.Name); // Brug item.Name som nøgle her, ellers opret ItemID
                        cmd.Parameters.AddWithValue("@amount", item.Amount);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        static GameState LoadGame(int slot)
        {
            GameState state = new GameState();

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
                            SoulCoins = reader.GetInt32(reader.GetOrdinal("Currency"))
                        };
                    }
                }

                // Indlæs Enemy
                string enemyQuery = "SELECT * FROM Enemies WHERE enemyID = @slot";
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
                                Y = (int)reader.GetDouble(reader.GetOrdinal("PositionY"))
                            };
                        }
                        else
                            state.Enemy = null;
                    }
                }

                // Indlæs Inventory
                state.Inventory = new List<Item>();
                string inventoryQuery = "SELECT itemID,Amount FROM Inventory WHERE playerID = @slot";
                using (SqlCommand cmd = new SqlCommand(inventoryQuery, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slot);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            state.Inventory.Add(new Item
                            {
                                Name = reader.GetString(reader.GetOrdinal("itemID")),
                                Amount = reader.GetInt32(reader.GetOrdinal("Amount"))
                            });
                        }
                    }
                }

                // Indstil standardværdier for NPC osv.
                state.Npc = new NPC { X = 10, Y = 0 };
                state.Objective = state.Enemy != null ? "Remove Monster" : "Talk to NPC";
                state.QuestAccepted = state.Enemy != null;
                state.QuestCompleted = false;
                state.SaveSlot = slot;
            }

            return state;
        }

        static void DeleteSave()
        {
            Console.WriteLine("Select save slot to delete (1-4):");
            int slotToDelete;
            if (!int.TryParse(Console.ReadLine(), out slotToDelete) || slotToDelete < 1 || slotToDelete > 4)
            {
                Console.WriteLine("Invalid slot. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            string query = "DELETE FROM Player WHERE playerID = @slot";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@slot", slotToDelete);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                        Console.WriteLine("Nothing to delete.");
                    else
                        Console.WriteLine($"Save slot {slotToDelete} deleted successfully.");

                    Console.WriteLine("Press Enter to continue.");
                    Console.ReadLine();
                }
            }
        }
    }
}
