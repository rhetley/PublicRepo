using System;
using System.Linq;

namespace DataDrivenCave
{
    // Demonstrating the Model-View-Controller design pattern, here is the C in MVC
    public class Controller
    {
        public enum COMMAND // To be used in the first int in playerCommand
        {
            EMPTY = 0, ERROR = 1, OVERSIZE = 2, LOOK = 3, GO = 4, GET = 5, DROP = 6, USE = 7,
            INVENTORY = 8, REST = 9, HELP = 10, INITIALIZE = 11, QUIT = 12,
        }; // OVERSIZE is used if the player types more words than currently allowed by the parser
        // Some variant words are accepted for legitimate commands as shown in GetPlayerCommand

        public enum DIRECTION // To be used in the second int in playerCommand
        {
            OFFSETFORARRAY = 13, NORTH = 13, NORTHEAST = 14, EAST = 15, SOUTHEAST = 16,
            SOUTH = 17, SOUTHWEST = 18, WEST = 19, NORTHWEST = 20, UP = 21, DOWN = 22, OTHER = 23
        }; // Error values are left to the enum COMMAND for readability
        // OFFSETFORARRAY is used to compare these numbers directly to an array of room exits
        // Otherwise, numbering continues from the end of COMMAND so there can be no collisions due to misread ints

        public enum TARGET // Also for the second int: the use of this or a direction is determined by the first int
        {
            OFFSETFORARRAY = 24, COMPASS = 24, ACTIVATOR = 25, ORE = 26, LUNCH = 27,
            PATHMAKER = 28, FURNACE = 29, GENERATOR = 30, LEVER = 31, PATH = 32, RESTFULNESS = 33
        }; // Error values are left to the enum COMMAND for readability
        // OFFSETFORARRAY is used to compare these numbers directly to an array of interactable or collectable objects
        // Otherwise, numbering continues from the end of DIRECTION so there can be no collisions due to misread ints

        private static Controller uniqueInstance = new Controller(); // Make it a singleton
        public static Controller GetInstance()
        {
            return uniqueInstance;
        }

        public void WaitForPlayer() // So the C in MVC even controls "press any key to continue"
        {
            Console.ReadKey();
        }

        public int[] GetPlayerCommand()
        {
            // Not all commands (first int) require a target (second int), so initialize with empty values
            int[] playerCommand = new int[2] { (int)COMMAND.EMPTY, (int)COMMAND.EMPTY };

            string commandInput = Console.ReadLine();
            commandInput = commandInput.ToLower();

            string[] inputSeparated = commandInput.Split(" ".ToCharArray(), StringSplitOptions.None);

            if (inputSeparated.Length > 2) // Catch if too many words were identified for this parser
            {
                playerCommand[0] = (int)COMMAND.OVERSIZE;
            }
            else if (inputSeparated[0] != "") // Completely empty commands are handled already
            {
                    switch (new string (inputSeparated[0].Take(6).ToArray()))
                {
                    case "look":
                    case "l":
                        playerCommand[0] = (int)COMMAND.LOOK;
                        break;
                    case "go":
                    case "walk":
                        playerCommand[0] = (int)COMMAND.GO;
                        break;
                    case "north":
                    case "n":
                    case "northeast": // Only the first six characters are checked, but these longer cases are listed for readability
                    case "northe":
                    case "ne":
                    case "east":
                    case "e":
                    case "southeast":
                    case "southe":
                    case "se":
                    case "south":
                    case "s":
                    case "southwest":
                    case "southw":
                    case "sw":
                    case "west":
                    case "w":
                    case "northwest":
                    case "northw":
                    case "nw":
                    case "up":
                    case "u":
                    case "down":
                    case "d":
                        playerCommand[0] = (int)COMMAND.GO;
                        // Duplicate what is commonly a single-word command for the direction identification code
                        inputSeparated = new string[2] { inputSeparated[0], inputSeparated[0] };
                        break;
                    case "get":
                        playerCommand[0] = (int)COMMAND.GET;
                        break;
                    case "drop":
                        playerCommand[0] = (int)COMMAND.DROP;
                        break;
                    case "use":
                        playerCommand[0] = (int)COMMAND.USE;
                        break;
                    case "read": // This word is allowed for exactly one target, but means the same as "use"
                        if (inputSeparated[1] == "compass")
                        {
                            playerCommand[0] = (int)COMMAND.USE;
                        }
                        else
                        {
                            playerCommand[0] = (int)COMMAND.ERROR;
                        }
                        break;
                    case "eat": // This word is allowed for exactly one target, but means the same as "use"
                        if (inputSeparated[1] == "lunch" || inputSeparated[1] == "food")
                        {
                            playerCommand[0] = (int)COMMAND.USE;
                        }
                        else
                        {
                            playerCommand[0] = (int)COMMAND.ERROR;
                        }
                        break;
                    case "inventory":
                    case "invent":
                    case "i":
                        playerCommand[0] = (int)COMMAND.INVENTORY;
                        break;
                    case "rest":
                        playerCommand[0] = (int)COMMAND.REST;
                        break;
                    case "help":
                        playerCommand[0] = (int)COMMAND.HELP;
                        break;
                    case "initialize":
                    case "initia":
                        playerCommand[0] = (int)COMMAND.INITIALIZE;
                        break;
                    case "q":
                    case "quit":
                    case "exit":
                        playerCommand[0] = (int)COMMAND.QUIT;
                        break;
                    default:
                        playerCommand[0] = (int)COMMAND.ERROR;
                        break;
                }
            }

            if (inputSeparated.Length > 1) // Again, empty commands are handled already
            {
                switch (new string(inputSeparated[1].Take(6).ToArray()))
                {
                    // A nonsense command like "get north" will still generate a get command with a "direction"
                    // But then, it will generate refusal text in GameLogic because the second int's value will be out of range
                    case "north":
                    case "n":
                        playerCommand[1] = (int)DIRECTION.NORTH;
                        break;
                    case "northeast": // Only the first six characters are checked, but these longer cases are listed for readability
                    case "northe":
                    case "ne":
                        playerCommand[1] = (int)DIRECTION.NORTHEAST;
                        break;
                    case "east":
                    case "e":
                        playerCommand[1] = (int)DIRECTION.EAST;
                        break;
                    case "southeast":
                    case "southe":
                    case "se":
                        playerCommand[1] = (int)DIRECTION.SOUTHEAST;
                        break;
                    case "south":
                    case "s":
                        playerCommand[1] = (int)DIRECTION.SOUTH;
                        break;
                    case "southwest":
                    case "southw":
                    case "sw":
                        playerCommand[1] = (int)DIRECTION.SOUTHWEST;
                        break;
                    case "west":
                    case "w":
                        playerCommand[1] = (int)DIRECTION.WEST;
                        break;
                    case "northwest":
                    case "northw":
                    case "nw":
                        playerCommand[1] = (int)DIRECTION.NORTHWEST;
                        break;
                    case "up":
                    case "u":
                        playerCommand[1] = (int)DIRECTION.UP;
                        break;
                    case "down":
                    case "d":
                        playerCommand[1] = (int)DIRECTION.DOWN;
                        break;
                    // A nonsense command like "go compass" will still generate a go command with a "target"
                    // But then, it will generate refusal text in GameLogic because the second int's value will be out of range
                    case "compass":
                    case "compas":
                        playerCommand[1] = (int)TARGET.COMPASS;
                        break;
                    case "activator":
                    case "activa":
                        playerCommand[1] = (int)TARGET.ACTIVATOR;
                        break;
                    case "bubble":
                    case "plotadvancium":
                    case "plotad":
                    case "ore":
                        playerCommand[1] = (int)TARGET.ORE;
                        break;
                    case "lunch":
                    case "food":
                    case "meal":
                        playerCommand[1] = (int)TARGET.LUNCH;
                        break;
                    case "pathmaker":
                    case "pathma":
                    case "machine":
                    case "machin":
                        playerCommand[1] = (int)TARGET.PATHMAKER;
                        break;
                    case "furnace":
                    case "furnac":
                        playerCommand[1] = (int)TARGET.FURNACE;
                        break;
                    case "generator":
                    case "genera":
                    case "slot":
                        playerCommand[1] = (int)TARGET.GENERATOR;
                        break;
                    case "lever":
                        playerCommand[1] = (int)TARGET.LEVER;
                        break;
                    case "path": // Not likely to be used directly, but the ungrammatical command "go path" is possible
                        if (inputSeparated[0] == "go" || inputSeparated[0] == "use") // Grammatically, "use path" is valid
                        {
                            playerCommand[0] = (int)COMMAND.GO; // Adjust the first int in case it wasn't already "go"
                            playerCommand[1] = (int)DIRECTION.OTHER; // This command is valid in an appropriate room
                        }
                        else // Other things like an attempt to "get" the path will generate refusal text in GameLogic
                        {
                            playerCommand[1] = (int)TARGET.PATH;
                        }
                        break;
                    case "rest":
                    case "restfulness":
                    case "restfu":
                        playerCommand[1] = (int)TARGET.RESTFULNESS;
                        if (inputSeparated[0] == "go" || inputSeparated[0] == "get") // Grammatically, "get rest" is valid
                        { // The ungrammatical command "go rest" is like "go to sleep," so it is valid
                            playerCommand[0] = (int)COMMAND.REST; // Adjust the first int to fit the expected command
                        }
                        break;
                    default:
                        playerCommand[1] = (int)COMMAND.ERROR;
                        break;
                }
            }
            return playerCommand;
        } // GetPlayerCommand()
    }
}