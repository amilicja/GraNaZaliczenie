using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraNaZaliczenie
{
    class Program
    {
        static int worldWidth = 40;
        static int worldHeight = 20;

        static char[,] world = new char[worldHeight, worldWidth];
        static char player = '@';
        static char wall = '#';
        static char item = '*';
        static char npc = 'N';
        static char emptySpace = ' ';
        static char house = 'H';
        static char keyItem = 'K';
        static char basement = 'B';

        static int playerX = worldWidth / 2;
        static int playerY = worldHeight / 2;

        static int playerHealth = 12;
        static int score = 0;
        static Random random = new Random();

        static Dictionary<Tuple<int, int>, int> npcHealth = new Dictionary<Tuple<int, int>, int>();
        static List<string> activeMissions = new List<string>();
        static List<string> inventory = new List<string>();

        static int level = 1;
        static Dictionary<int, string> levelNames = new Dictionary<int, string>() // Dictionary to hold level names
        {
            { 1, "Market" },
            { 2, "Residential area" },
            { 3, "House" },
            { 4, "Basement" }
        };

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            InitializeWorld();

            while (true)
            {
                DrawWorld();
                var key = Console.ReadKey(true).Key;
                MovePlayer(key);
                UpdateNPCs(); // Update NPC positions after player moves

                // Check if the player has completed the mission to reach 100 gold on level 1
                if (score >= 100 && level == 1 && activeMissions.Contains("Reach 100 gold"))
                {
                    Console.Clear();
                    Console.WriteLine("Congratulations! You've reached 100 gold and completed the mission!");
                    Console.ReadKey(true);
                    level++;
                    InitializeWorld();
                    score = 0; // Reset score for the new level
                    playerHealth = 10; // Reset player health for the new level
                    activeMissions.Remove("Reach 100 gold"); // Remove the mission after completing it
                }

                if (playerHealth <= 0)
                {
                    Console.Clear();
                    Console.WriteLine("You have been defeated! Game Over.");
                    Console.WriteLine($"Your final score: {score}");
                    Thread.Sleep(1200);
                    break;
                }
            }
        }

        static void InitializeWorld()
        {
            npcHealth.Clear();
            activeMissions.Clear();

            ShowLevelIntro(level);

            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    if (x == 0 || x == worldWidth - 1 || y == 0 || y == worldHeight - 1)
                    {
                        world[y, x] = wall;
                    }
                    else
                    {
                        world[y, x] = emptySpace;
                    }
                }
            }

            // Add mission to reach 100 gold on level 1
            if (level == 1 && !activeMissions.Contains("Reach 100 gold"))
            {
                activeMissions.Add("Reach 100 gold");
            }

            bool inHouse = false;

            if (level == 2)
            {
                AddHouse();
                AddKey();
                inHouse = false;
            }
            else if (level == 3) // Level 3 for the basement
            {
                AddBasement();
                inHouse = true;
                activeMissions.Add("Clean house than enter the basement"); // Add mission to clean house
                AddRandomObjects(item, 10); // Add items for the mission
            }
            else if (level == 4) // Level 4 - Special NPC only
            {
                AddFinalNPC();
                activeMissions.Add("Go to PC in a Corner"); // Add mission to talk to the final NPC/ npc placeholder do kompa XD
            }

            // Add items only if not in level 4
            if (level != 4 && level != 3)
            {
                AddRandomObjects(item, 10);
            }

            // Add NPCs only if not in house and not in level 4
            if (!inHouse && level != 4)
            {
                AddRandomNPCs(5);
            }
        }

        static void ShowLevelIntro(int level)
        {
            Console.Clear();
            string introText = "";
            switch (level)
            {
                case 1:
                    introText = "One day your father sent you to the market to buy vegetables, but you had holes in your pockets and you lost the 100 zloty you got from him. You need to find a way to earn money and get back home.\nUse arrows to move around.";
                    break;
                case 2:
                    introText = "After a tough shopping trip, you went home but just before arriving, you remembered that you had the key in the same (holey) pocket, which of course you no longer have. Find the house key.";
                    break;
                case 3:
                    introText = "Weary, you returned home, and you were about to head to your room, but your father wouldn't let you off the hook and made you clean the entire house. Clean the house.";
                    break;
                case 4:
                    introText = "You finally made it to your den. Turn on the PC and start Fortnite.";
                    break;
            }
            Console.WriteLine(introText);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        static void AddHouse()
        {
            int houseStartX = random.Next(5, worldWidth - 10);
            int houseStartY = random.Next(5, worldHeight - 10);

            for (int y = houseStartY; y < houseStartY + 4; y++)
            {
                for (int x = houseStartX; x < houseStartX + 6; x++)
                {
                    world[y, x] = house;
                }
            }

            // Creating door for the house
            world[houseStartY + 1, houseStartX] = emptySpace;

            // Automatically add mission to find the key to the house
            activeMissions.Add("Find Key to House");
        }

        static void AddBasement()
        {
            int basementStartX = random.Next(5, worldWidth - 10);
            int basementStartY = random.Next(5, worldHeight - 10);

            for (int y = basementStartY; y < basementStartY + 4; y++)
            {
                for (int x = basementStartX; x < basementStartX + 6; x++)
                {
                    world[y, x] = basement;
                }
            }

            // Creating door for the basement
            world[basementStartY + 1, basementStartX] = emptySpace;
        }

        static void AddKey()
        {
            int x, y;
            do
            {
                x = random.Next(1, worldWidth - 1);
                y = random.Next(1, worldHeight - 1);
            } while (world[y, x] != emptySpace);

            world[y, x] = keyItem;
        }

        static void AddRandomObjects(char obj, int count)
        {
            int added = 0;
            while (added < count)
            {
                int x = random.Next(1, worldWidth - 1);
                int y = random.Next(1, worldHeight - 1);
                if (world[y, x] == emptySpace)
                {
                    world[y, x] = obj;
                    added++;
                }
            }
        }

        static void AddFinalNPC()
        {
            int npcX = worldWidth - 2; // Fixed position: bottom-right corner
            int npcY = worldHeight - 2;

            world[npcY, npcX] = npc;
        }

        static void AddRandomNPCs(int count)
        {
            int added = 0;
            while (added < count)
            {
                int x = random.Next(1, worldWidth - 1);
                int y = random.Next(1, worldHeight - 1);

                // Check if the position is not inside the basement area
                if (!(level == 3 && IsInBasementArea(x, y)))
                {
                    if (world[y, x] == emptySpace)
                    {
                        world[y, x] = npc;
                        npcHealth[new Tuple<int, int>(x, y)] = 3; // NPC start with 3 health points
                        added++;
                    }
                }
            }
        }

        static bool IsInBasementArea(int x, int y)
        {
            // Define the boundaries of the basement area (example)
            int basementStartX = worldWidth / 2 - 3; // Start X coordinate of basement
            int basementStartY = worldHeight / 2 - 2; // Start Y coordinate of basement
            int basementWidth = 6; // Width of basement
            int basementHeight = 4; // Height of basement

            // Check if the position is within the basement area
            return x >= basementStartX && x < basementStartX + basementWidth &&
                   y >= basementStartY && y < basementStartY + basementHeight;
        }

        static void DrawWorld()
        {
            Console.Clear();
            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    if (x == playerX && y == playerY)
                    {
                        Console.Write(player);
                    }
                    else
                    {
                        Console.Write(world[y, x]);
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Level: {levelNames[level]}"); // Display level name
            Console.WriteLine($"Health: {playerHealth}  Gold: {score}"); // Display health and gold
            Console.WriteLine("Inventory: " + string.Join(", ", inventory)); // Display inventory
            Console.WriteLine("Active Missions:");
            foreach (var mission in activeMissions)
            {
                Console.WriteLine($"- {mission}");
            }
        }

        static void MovePlayer(ConsoleKey key)
        {
            int newX = playerX;
            int newY = playerY;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    newY--;
                    break;
                case ConsoleKey.DownArrow:
                    newY++;
                    break;
                case ConsoleKey.LeftArrow:
                    newX--;
                    break;
                case ConsoleKey.RightArrow:
                    newX++;
                    break;
                case ConsoleKey.Q:
                    Environment.Exit(0);
                    break;
            }

            if (world[newY, newX] != wall)
            {
                if (world[newY, newX] == house && level == 2 && inventory.Contains("Key"))
                {
                    Console.Clear();
                    Console.WriteLine("You used the key and entered the house!");
                    Console.ReadKey(true);
                    level++;
                    InitializeWorld();
                    playerHealth = 10; // Reset player health for the new level
                    inventory.Remove("Key"); // Remove the key from inventory after entering the house
                }
                else if (world[newY, newX] == basement && level == 3 && !world.Cast<char>().Contains(item))
                {
                    Console.Clear();
                    Console.WriteLine("You entered the basement!");
                    Console.ReadKey(true);
                    level++; // Move to the basement level
                    InitializeWorld();
                    playerHealth = 10; // Reset player health for the new level
                }
                else if (world[newY, newX] != house && world[newY, newX] != basement)
                {
                    playerX = newX;
                    playerY = newY;

                    if (world[playerY, playerX] == item)
                    {
                        world[playerY, playerX] = emptySpace;
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("You picked up an item!");
                        CheckMissions("Collect Item");
                    }
                    else if (world[playerY, playerX] == keyItem)
                    {
                        world[playerY, playerX] = emptySpace;
                        inventory.Add("Key");
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("You picked up a key!");
                        CheckMissions("Find Key to House"); // Complete the mission to find the key
                        activeMissions.Add("Enter the House"); // Start the mission to enter the house
                    }
                    else if (world[playerY, playerX] == npc)
                    {
                        if (level == 4)
                        {
                            InteractWithFinalNPC();
                            return; // Return to prevent further movement after interaction
                        }
                        else
                        {
                            InteractWithNPC(playerX, playerY);
                        }
                    }
                }
            }
        }

        static void InteractWithNPC(int x, int y)
        {
            var npcPosition = new Tuple<int, int>(x, y);
            if (npcHealth.ContainsKey(npcPosition))
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("You meet an NPC! (press T to talk, A to attack, any other key to ignore him)");
                var action = Console.ReadKey(true).Key;

                if (action == ConsoleKey.T)
                {
                    GiveMission();
                }
                else if (action == ConsoleKey.A)
                {
                    if (MiniGame())
                    {
                        npcHealth[npcPosition]--;
                        if (npcHealth[npcPosition] <= 0)
                        {
                            Console.WriteLine("You defeated the NPC!");
                            Thread.Sleep(1200);
                            world[y, x] = emptySpace;
                            npcHealth.Remove(npcPosition);
                            score += 10; // Add points for defeating an NPC
                            CheckMissions("Defeat NPC");
                        }
                        else
                        {
                            Console.WriteLine("You hit the NPC!");
                            Thread.Sleep(1200);
                            playerHealth--; // NPC attacks back
                            if (playerHealth <= 0)
                            {
                                Console.WriteLine("You have been defeated by the NPC!");
                                Thread.Sleep(1200);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("You missed!");
                        Thread.Sleep(1200);
                        playerHealth--; // NPC attacks back
                        if (playerHealth <= 0)
                        {
                            Console.WriteLine("You have been defeated by the NPC!");
                            Thread.Sleep(1200);
                        }
                    }
                }
            }
        }

        static void InteractWithFinalNPC()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("You got to the PC! (press T to start playing)");
            var action = Console.ReadKey(true).Key;

            if (action == ConsoleKey.T)
            {
                Console.WriteLine("Thank you for playing! Game by Alicja Maliszewska, Wojciech Kołaczek, Albert Gałecki.");
                Console.WriteLine("Press random key to end game.");
                Console.ReadLine();
                Console.ReadKey(true);
                CheckMissions("Go to PC in a Corner"); // Complete the mission to go to the PC in the corner
                Environment.Exit(0); // End the game after interacting with PC
            }
        }

        static void GiveMission()
        {
            string mission = random.Next(2) == 0 ? "Collect Item" : "Defeat NPC";
            activeMissions.Add(mission);
            Console.WriteLine($"You received a mission: {mission}");
        }

        static void CheckMissions(string completedTask)
        {
            if (activeMissions.Contains(completedTask))
            {
                activeMissions.Remove(completedTask);
                score += 20; // Add gold for completing a mission
                Console.WriteLine($"Mission completed: {completedTask}");

                // Check if all items have been collected on level 3
                if (level == 3 && completedTask == "Clean house then enter the basement" && !world.Cast<char>().Contains(item))
                {
                    activeMissions.Remove("Clean house then enter the basement"); // Complete the mission to clean the house
                    activeMissions.Add("Enter the Basement"); // Add new mission to enter the basement
                }
            }
        }

        static void UpdateNPCs()
        {
            List<Tuple<int, int>> npcPositions = npcHealth.Keys.ToList();
            foreach (var position in npcPositions)
            {
                int x = position.Item1;
                int y = position.Item2;

                // Skip updating NPCs on level 4 to make them stationary
                if (level == 4)
                    continue;

                // Example AI: NPCs move randomly
                int direction = random.Next(4);
                int newX = x;
                int newY = y;

                switch (direction)
                {
                    case 0: // Move up
                        newY--;
                        break;
                    case 1: // Move down
                        newY++;
                        break;
                    case 2: // Move left
                        newX--;
                        break;
                    case 3: // Move right
                        newX++;
                        break;
                }

                // Check if the new position is within bounds and empty space
                if (newX >= 0 && newX < worldWidth && newY >= 0 && newY < worldHeight &&
                    world[newY, newX] == emptySpace)
                {
                    // Move NPC to the new position
                    world[y, x] = emptySpace;
                    world[newY, newX] = npc;
                    npcHealth[new Tuple<int, int>(newX, newY)] = npcHealth[position];
                    npcHealth.Remove(position);
                }
            }
        }

        static bool MiniGame()
        {
            Console.Clear();
            Console.WriteLine("Press the correct key sequence to attack the NPC!");

            string[] keys = { "A", "S", "D", "W" };
            Random rnd = new Random();
            string keySequence = string.Join("", Enumerable.Range(0, 5).Select(i => keys[rnd.Next(keys.Length)]));

            Console.WriteLine($"Sequence: {keySequence}");
            Console.WriteLine("You have 5 seconds to type the sequence!");

            DateTime startTime = DateTime.Now;

            string playerInput = "";
            while ((DateTime.Now - startTime).TotalSeconds < 5)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).KeyChar.ToString().ToUpper();
                    playerInput += key;

                    if (playerInput.Length == keySequence.Length)
                    {
                        break;
                    }
                }
            }

            return playerInput == keySequence;
        }
    }
}