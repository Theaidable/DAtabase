using DAtabase.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAtabase
{
    class Program
    {
        //string som skal bruges til at forbinde til databasen
        /*static string connectionString = 
         * "Server=servernavn;
         * Database=databasenavn;
         * Trusted_Connection=True;
         * TrustServerCertificate=True;
        */

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

            // Gem initial spiltilstand i databasen
            SaveGame(state);

            Console.WriteLine();
            Console.WriteLine("Game initialized and saved. Press Enter to continue.");
            Console.ReadLine();
            
            DisplayGameState(state);
            GameLoop(state);
        }

        static void LoadGameAndPlay()
        {
            GameState state = LoadGame();

            //Hvis ikke der er nogen saved games, så skal man gå tilbage
            if (state == null)
            {
                Console.WriteLine("No saved game found. Press Enter to return to main menu.");
                Console.ReadLine();
                return;
            }

            Console.Clear();
            Console.WriteLine("Loaded Game:");
            DisplayGameState(state);
            Console.WriteLine("Press Enter to continue playing.");
            Console.ReadLine();
            GameLoop(state);
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

                // Auto-save efter hvert input
                SaveGame(state);
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
        }

        static GameState LoadGame()
        {
            return null;
        }
    }
}
