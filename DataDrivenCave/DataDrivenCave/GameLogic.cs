using System;
using System.Linq;

namespace DataDrivenCave
{
    // Demonstrating the Model-View-Controller design pattern, here is the M in MVC
    public class GameLogic
    {
        private static GameLogic uniqueInstance = new GameLogic(); // Make it a singleton
        public static GameLogic GetInstance()
        {
            return uniqueInstance;
        }

        private int currentLocation; // Location ID numbers are used as an index across arrays in Rooms
        private bool runGame; // Controls the game loop
        private int[] inventory;
        private int filledInventory; // A maximum of two allowed
        // Player inventory is like another set of room affordances, but only some items can be picked up
        // These are at the following indices:
        // Controller.TARGET.COMPASS - Helpful Tell-You-Where-To-Go Compass
        // Controller.TARGET.ACTIVATOR - Activator
        // Controller.TARGET.ORE - Bubble of Plotadvancium Ore
        // Controller.TARGET.LUNCH - Packed Lunch
        // All are set to 0 if absent, 1 if present, or, in the case of the Lunch, 2 if dropped in the Furnace and lost forever
        // The Lunch requires special attention as getting to end of the game without it causes the bad ending

        public string InitializeGameState() // Get the basic setup
        {
            string initialReport = Rooms.GetInstance().InitializeRoomsFromFile();

            if (initialReport == "Success") // If all worked, get ready for the game proper
            {
                currentLocation = 0;
                runGame = true;
                inventory = new int[Enum.GetNames(typeof(Controller.TARGET)).Length - 1];
                filledInventory = 0;
                initialReport = "     Welcome to Predictable Cave Adventure, Data-Driven Edition!\n\r" +
                    "This edition is \"doubly enhanced\" from the original, first by adding incongruous puzzle-solving elements on\n\r" +
                    "top of what had otherwise been a straightforward ambient game experience, second by improving the breadth\n\r" +
                    "and versatility of the game itself.\n\r\n\r" +
                    "Play by typing single words, or, at most, two-word commands.  For instance, you can \"go east\" or \"go e\" or\n\r" +
                    "just type \"e.\"  You can \"get\" and \"use\" certain things, and any objects you collect can be seen in your\n\r" +
                    "\"inventory\", \"i.\"  Type \"help\" for more, and type \"quit\" to exit the game.\n\r\n\r" +
                    "For now, you are in a deep cave.  You should probably \"look\" around.";
            }
            else // If it didn't work, keep the error message in the string and return it, but tell the game to exit therafter
            {
                runGame = false;
            }
            
            return initialReport;
        }

        public bool CheckGameState() // Is the game still supposed to run?
        {
            return runGame;
        }

        public string ResolvePlayerCommand(int[] commandInput)
        {
            string description = "Error in resolving player command";
            bool needsAffordanceDescription = false; // Used if either a look or successful movement command were detected

            switch (commandInput[0]) // First int is the commmand, second int is empty or is the target
            {
                case (int)Controller.COMMAND.EMPTY:
                    description = "Speak up, speak up!";
                    break;
                case (int)Controller.COMMAND.ERROR:
                    description = "I'm sorry, I did not understand you.";
                    break;
                case (int)Controller.COMMAND.OVERSIZE:
                    description = "A bit much!  Please use commands of one or two words.";
                    break;
                case (int)Controller.COMMAND.LOOK:
                    description = Rooms.GetInstance().ReportName(currentLocation); // More description will be added to this below, for efficiency
                    needsAffordanceDescription = true;
                    break;
                case (int)Controller.COMMAND.GO:
                    bool pathIsOpen = true; // Make sure there is not a specific impediment to the path, namely the True Path

                    if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY]
                        == 1)
                    { // Here, the True Path is present (value greater than 0) but closed (value of 1)
                        if (commandInput[1] == (int)Controller.DIRECTION.OTHER)
                        { // The player is trying to go in the "other" (True Path) direction
                            pathIsOpen = false;
                        }
                        if (Rooms.GetInstance().ReportConnections(currentLocation)[(int)Controller.DIRECTION.OTHER - (int)Controller.DIRECTION.OFFSETFORARRAY]
                            == Rooms.GetInstance().ReportConnections(currentLocation)[commandInput[1] - (int)Controller.DIRECTION.OFFSETFORARRAY])
                        { // Going in an ordinary direction which is also identified as the "other" (True Path) direction
                            pathIsOpen = false;
                        }
                    }

                    if (pathIsOpen)
                    {
                        if (commandInput[1] == (int)Controller.COMMAND.EMPTY) // Start with checking for errors in the go command
                        {
                            description = "You do not seem to have finished your thought.";
                        }
                        else if (commandInput[1] == (int)Controller.COMMAND.ERROR)
                        {
                            description = "I do not understand where you want to go.";
                        }
                        else if (Rooms.GetInstance().ReportConnections(currentLocation)[commandInput[1] - (int)Controller.DIRECTION.OFFSETFORARRAY] != null)
                        {
                            bool lookingForMatch = true; // This prevents getting multiple matches and doing, e.g., a "supereast" command
                            int currentRoomToCheck = 0;
                            while (lookingForMatch && currentRoomToCheck < Rooms.GetInstance().ReportNumberOfRooms())
                            {
                                if (Rooms.GetInstance().ReportConnections(currentLocation)[commandInput[1] - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                    == Rooms.GetInstance().ReportName(currentRoomToCheck))
                                {
                                    currentLocation = currentRoomToCheck;
                                    lookingForMatch = false;
                                    switch (commandInput[1])
                                    {
                                        case (int)Controller.DIRECTION.NORTH:
                                            description = "You move north.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.NORTHEAST:
                                            description = "You move northeast.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.EAST:
                                            description = "You move east.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.SOUTHEAST:
                                            description = "You move southeast.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.SOUTH:
                                            description = "You move south.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.SOUTHWEST:
                                            description = "You move southwest.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.WEST:
                                            description = "You move west.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.NORTHWEST:
                                            description = "You move northwest.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.UP:
                                            description = "You move up.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.DOWN:
                                            description = "You move down.\n\r\n\r";
                                            break;
                                        case (int)Controller.DIRECTION.OTHER:
                                            description = "You move out of this area.\n\r\n\r";
                                            break;
                                        default:
                                            description = "Impossible move action detected, but you succeeded.  Good for you.\n\r";
                                            break;
                                    }
                                    description = description + Rooms.GetInstance().ReportName(currentLocation); // More description will be added to this below, for efficiency
                                    needsAffordanceDescription = true;
                                }
                                else
                                {
                                    currentRoomToCheck++;
                                }
                            }
                        }
                        else // Any invalid move is blocked
                        {
                            description = "You cannot go that way!";
                        }
                    }
                    else // When there is a problem with the True Path, a "valid" move towards it is blocked
                    {
                        description = "You cannot go that way!";
                    }
                    break;
                case (int)Controller.COMMAND.GET:
                    // At most, the player can carry two items, one for each hand
                    // The crumbs from the Packed Lunch don't count as an item and will remain in inventory forever if obtained
                    if (filledInventory > 1)
                    {
                        description = "You only have two hands!  Before you think of picking up anything, you should drop an inventory item.";
                    }
                    else
                    {
                        switch (commandInput[1])
                        {
                            case (int)Controller.TARGET.COMPASS:
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                {
                                    inventory[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                    filledInventory++;
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                            (int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY, 0);
                                    description = "What a helpful little thing!  Ooh, and it has an LCD readout!";
                                }
                                else
                                {
                                    description = "I do not see that here.";
                                }
                                break;
                            case (int)Controller.TARGET.ACTIVATOR:
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                {
                                    Rooms.GetInstance().ChangeActivatorIndex(-1); // The Activator will be reset from inventory if the puzzle resets
                                    inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                    filledInventory++;
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                            (int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY, 0);
                                    description = "You pick up the Activator.  Now you just have to figure out where to use it.";
                                    if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                                    { // Ensure that the Activator Generator's description stays logical as the Activator is manipulated
                                        Rooms.GetInstance().ChangeAffordance(currentLocation,
                                                (int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY, 3);
                                    }
                                }
                                else
                                {
                                    description = "I do not see that here.";
                                }
                                break;
                            case (int)Controller.TARGET.ORE:
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                { // An infinite amount of Plotadvancium is available, so the checks are different from other inventory objects
                                    if (inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                    {
                                        description = "Why bother?  You already carry all the Ore that you could need.";
                                    }
                                    else
                                    {
                                        inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        filledInventory++;
                                        description = "You scoop up some Plotadvancium Ore.  It is light and filled with potential.";
                                        if (currentLocation != Rooms.GetInstance().ReportOreIndex())
                                        { // Only change the affordances if this is not the original source of all Plotadvancium Ore
                                            Rooms.GetInstance().ChangeAffordance(currentLocation,
                                                (int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY, 0);
                                        }
                                    }
                                }
                                else
                                {
                                    description = "I do not see that here.";
                                }
                                break;
                            case (int)Controller.TARGET.LUNCH:
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                {
                                    inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                    filledInventory++;
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                            (int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY, 0);
                                    description = "Well, this has a little of everything.  It should be just what you need at the end of the day.";
                                }
                                else
                                {
                                    description = "I do not see that here.";
                                }
                                break;
                            case (int)Controller.COMMAND.EMPTY:
                                description = "You do not seem to have finished your thought.";
                                break;
                            case (int)Controller.COMMAND.ERROR:
                                description = "I do not understand what you want to get.";
                                break;
                            default:
                                description = "You can only get what you can lift with your hands!";
                                break;
                        }
                    }
                    break;
                case (int)Controller.COMMAND.DROP:
                    description = "You can only drop what is in your inventory!"; // This will be a default text across various conditionals
                    switch (commandInput[1])
                    {
                        case (int)Controller.TARGET.COMPASS:
                            if (inventory[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                inventory[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] = 0;
                                filledInventory--;
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                                {
                                    description = "You drop the Compass into the Omnidirectional Furnace.  It is incinerated!\n\r" +
                                        "Amazingly, it has no helpful effect.";
                                }
                                else
                                {
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY, 1);
                                    description = "You drop the Compass and try to pretend you have never before seen its helpful LCD readout.";
                                }
                            }
                            break;
                        case (int)Controller.TARGET.ACTIVATOR:
                            if (inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] = 0;
                                filledInventory--;
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                                {
                                    description = "You drop the Activator into the Omnidirectional Furnace.  It is incinerated!\n\r" +
                                        "It doesn't seem to \"activate\" anything this way, though.";
                                }
                                else
                                {
                                    Rooms.GetInstance().ChangeActivatorIndex(currentLocation); // The Activator may need to be reset if the puzzle resets
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY, 1);
                                    description = "You set the Activator down.";
                                }
                            }
                            break;
                        case (int)Controller.TARGET.ORE:
                            if (inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                // An infinite amount of Plotadvancium is available, but it also advances the plot if dropped correctly,
                                // so the checks are different from other inventory objects
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                                {
                                    inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] = 0;
                                    filledInventory--;
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY, 2); // Power the furnace
                                    if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportPathmakerIndex())[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                    { // If unpowered, find and power the Pathmaker Machine
                                        Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportPathmakerIndex(),
                                        (int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY, 2);
                                    }
                                    description = "You drop the Plotadvancium Ore into the Omnidirectional Furnace.  It is incinerated!\n\r" +
                                        "With a great \"whoosh,\" the powerful energy contained in the Ore flows out across the depths!";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                { // Keep the count in any location at one, instead of counting up forever
                                    description = "No, there is already plenty of Plotadvancium Ore here.  Stop littering!";
                                }
                                else
                                {
                                    inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] = 0;
                                    filledInventory--;
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY, 1);
                                    description = "You drop the Bubble of Plotadvancium Ore.  It eventually settles to the ground.";
                                }
                            }
                            break;
                        case (int)Controller.TARGET.LUNCH:
                            if (inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                // The Packed Lunch is unique and important to the plot, so the checks are slightly different
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                                {
                                    inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] = 2; // Leave crumbs
                                    filledInventory--; // Still free up a hand for inventory, as crumbs don't take up space
                                    description = "You drop the Packed Lunch into the Omnidirectional Furnace.  It is incinerated!\n\r" +
                                        "You are left with nothing but regret.  And sparse, unfulfilling crumbs on your hands.";
                                }
                                else
                                {
                                    inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] = 0;
                                    filledInventory--;
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY, 1);
                                    description = "With great care, you leave the Packed Lunch on a flat surface.";
                                }
                            }
                            break;
                        case (int)Controller.COMMAND.EMPTY:
                            description = "You do not seem to have finished your thought.";
                            break;
                        case (int)Controller.COMMAND.ERROR:
                            description = "I do not understand what you want to drop.";
                            break;
                    }
                    break;
                case (int)Controller.COMMAND.USE:
                    description = "You can only use what is present!"; // This will be a default text across various conditionals
                    switch (commandInput[1])
                    {
                        case (int)Controller.TARGET.COMPASS:
                            if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                            {
                                description = "It is on the ground.  Might you consider getting it in hand first?";
                            }
                            else if (inventory[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                description = "The helpful LCD readout reads:\n\r";
                                // Check all possible puzzle states, starting from the last and going to the first
                                if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportTruePathIndex())[(int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY] == 2
                                    && inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                { // The True Path is waiting, as is rest with a Packed Lunch
                                    description = description + "\"Your have your Lunch and can follow the True Path to a place of rest at the end of the beach.\"";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportTruePathIndex())[(int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY] == 2
                                    && inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                                { // The True Path is waiting, as is rest, but the player only has crumbs
                                    description = description + "\"You can follow the True Path to a place of rest at the end of the beach!  But what happened to\n\r" +
                                        "your Packed Lunch?\"";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportTruePathIndex())[(int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY] == 2
                                    && inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 0)
                                { // The True Path is waiting, but the player needs the Packed Lunch
                                    description = description + "\"You can follow the True Path to the end of the beach!  But wait: shouldn't you bring a Packed Lunch?\n\r" +
                                        "Is it still back home?\"";
                                }
                                else if (inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1
                                    && Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportPathmakerIndex())[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                                { // The player has the Activator and the Pathmaker Machine is powered
                                    description = description + "\"The powered Pathmaker Machine awaits!  Why not activate it?  It is in the farthest west.\"";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportGeneratorIndex())[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 3
                                    && Rooms.GetInstance().ReportActivatorIndex() != -1
                                    && Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportPathmakerIndex())[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                                { // The player has seen and dropped the Activator somewhere safe, and the Pathmaker Machine is powered
                                    description = description + "\"You have powered the Pathmaker Machine!  Go get your Activator back, then activate it!\"";
                                }
                                else if (inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1
                                    && Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportPathmakerIndex())[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 1
                                    && inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                { // The player has the Activator, the Pathmaker Machine is unpowered, but the player has Plotadvancium Ore (only possible with three inventory space)
                                    description = description + "\"The Ore goes in the Omnidirectional Furnace, at the deepest depths of the caves.  Drop it in!\"";
                                }
                                else if (inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1
                                    && Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportPathmakerIndex())[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 1
                                    && inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 0)
                                { // The player has the Activator, the Pathmaker Machine is unpowered, and the player does not have Plotadvancium Ore
                                    description = description + "\"You have made many advances!  But you can't use the Activator on what's really important without\n\r" +
                                        "a Bubble of Plotadvancium Ore.  The source hides in the north caves where work is done.\"";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportGeneratorIndex())[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 3
                                    && Rooms.GetInstance().ReportActivatorIndex() != -1
                                    && Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportPathmakerIndex())[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 1
                                    && inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                { // The player has seen and dropped the Activator somewhere safe, the Pathmaker Machine is unpowered, but the player has Plotadvancium Ore
                                    description = description + "\"The Ore goes in the Omnidirectional Furnace, at the deepest depths of the caves.  Drop it in!\n\r" +
                                        "Then go get your Activator.\"";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportGeneratorIndex())[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 3
                                    && Rooms.GetInstance().ReportActivatorIndex() == -1
                                    && inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 0)
                                { // The Activator Generator is working, but the player dropped the Activator in the Furnace
                                    description = description + "\"Erm, why did you drop your Activator in the Furnace?  Try switching the Lever off and on again.\n\r" +
                                        "That will probably help.\"";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportGeneratorIndex())[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 3)
                                { // The Activator Generator is working, but the player dropped the Activator somewhere safe
                                    description = description + "\"You have made many advances!  You will need your Activator back eventually, but you should find a\n\r" +
                                        "Bubble of Plotadvancium Ore.  The source hides in the north caves where work is done.\"";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportGeneratorIndex())[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                                { // The Activator Generator is waiting
                                    description = description + "\"The Lever started an Activator Generator somewhere nearby.  See what it generated!\"";
                                }
                                else // Start of game
                                {
                                    description = description + "\"A magical adventure awaits!  To start, why not look for something magical?\n\r" +
                                        "To be specific: find the Magical Make-Something-Happen Lever.  It is in the farthest east.\"";
                                }
                            }
                            break;
                        case (int)Controller.TARGET.ACTIVATOR:
                            if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                            {
                                description = "It is waiting in front of you.  Might you consider getting it in hand first?";
                            }
                            else if (inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                description = "You use the Activator.";
                                if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                {
                                    description = description + "\n\rThe Pathmaker Machine has no power and cannot be activated.  Odd.  Have you seen a source of power?";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                                {
                                    description = description + "\n\rThe Pathmaker Machine has been activated!  Wonderful!  Now you need only find the \"path\" it made.";
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY, 3); // Change the Pathmaker Machine
                                    Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportTruePathIndex(),
                                        (int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY, 2); // Find and change the True Path
                                }
                                else if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 3)
                                {
                                    description = description + "\n\rAh, it seems that \"activating\" an active Pathmaker Machine turns it back off again.  Well.";
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY, 2); // Change the Pathmaker Machine
                                    Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportTruePathIndex(),
                                        (int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY, 1); // Find and change the True Path
                                }
                                else if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                                {
                                    description = description + "\n\rThe Omnidirectional Furnace seems plenty active already, thank you very much!";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                                {
                                    description = description + "\n\rThe Activator Generator is not affected by its generated Activators, as that obviously would be a\n\r" +
                                        "terrible idea.";
                                }
                                else if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                                {
                                    description = description + "\n\rIt \"activates\" the Lever.  It is now \"off!\"  And then a complex pattern of events transpires.\n\r" +
                                        "In the end, the Activator vanishes from your hand!  You should probably look about for anything else\n\r" +
                                        "that changed.";
                                    // Wipe out all relevant puzzle progress
                                    Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY, 1);  // Reset the Lever
                                    inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] = 0;
                                    filledInventory--; // Remove the Activator from inventory
                                    Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportGeneratorIndex(),
                                        (int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY, 1); // Find and reset the Activator Generator
                                    if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportPathmakerIndex())[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 3)
                                    { // If the Pathmaker Machine was fueled and activated, find and partially reset the Pathmaker Machine
                                        Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportPathmakerIndex(),
                                            (int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY, 2);
                                    }
                                    Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportTruePathIndex(),
                                        (int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY, 1); // Find and reset the True Path
                                }
                                else
                                {
                                    description = description + "  Apparently, nothing around here needs activating.";
                                }
                            }
                            break;
                        case (int)Controller.TARGET.ORE:
                            if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] > 0
                                && inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 0)
                            {
                                description = "The Plotadvancium Ore drifts about.  Might you consider getting it in hand first?";
                            }
                            else if (inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                description = "Rare and important it may be, it is nothing to you unless you find a way to unleash its power.";
                            }
                            break;
                        case (int)Controller.TARGET.LUNCH:
                            if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                            {
                                description = "It is right there, looking delicious.  Might you consider getting it in hand first?";
                            }
                            else if (inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                description = "You think you should find a nice place and rest before you begin to eat.";
                            }
                            else if (inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                            {
                                description = "You have nothing left but crumbs.  All is lost.";
                            }
                            break;
                        case (int)Controller.TARGET.PATHMAKER:
                            if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                            {
                                description = "It seems that you do not get to use the Pathmaker Machine.  Something else does.";
                            }
                            break;
                        case (int)Controller.TARGET.FURNACE:
                            if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                            {
                                description = "The only way you can \"use\" a blazing furnace is by dropping things into it!";
                            }
                            break;
                        case (int)Controller.TARGET.GENERATOR:
                            if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                description = "The Activator Generator does not have any clear means of interaction from here.";
                            }
                            else if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] > 1)
                            {
                                description = "The Activator Generator is already in use!";
                            }
                            break;
                        case (int)Controller.TARGET.LEVER:
                            if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                            {
                                description = "You pull the Lever.  It is now \"on\"!  Something magical has happened!  Somewhere!";
                                Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY, 2);  // Change the Lever
                                Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportGeneratorIndex(),
                                    (int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY, 2); // Find and turn on the Activator Generator
                                Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportGeneratorIndex(),
                                    (int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY, 1); // Create the Activator at that location
                                Rooms.GetInstance().ChangeActivatorIndex(Rooms.GetInstance().ReportGeneratorIndex()); // The Activator may need to be reset if the puzzle resets
                            }
                            else if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                            {
                                description = "You pull the Lever.  It is now \"off!\"";
                                Rooms.GetInstance().ChangeAffordance(currentLocation,
                                        (int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY, 1);  // Change the Lever
                                if (inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1
                                    || Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] > 0)
                                {
                                    description = description + "  And then a complex pattern of events transpires.\n\r" +
                                        "In the end, the Activator vanishes!  You should probably look about for anything else that changed.";
                                }
                                // Wipe out all relevant puzzle progress
                                Rooms.GetInstance().ChangeAffordance(currentLocation,
                                    (int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY, 1);  // Change the Lever
                                if (inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                                {
                                    inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] = 0;
                                    filledInventory--; // Remove the Activator if in inventory
                                }
                                if (Rooms.GetInstance().ReportActivatorIndex() != -1)
                                {
                                    Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportActivatorIndex(),
                                    (int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY, 0); // Find and remove the Activator if not in inventory
                                    Rooms.GetInstance().ChangeActivatorIndex(-1); // Reset the Activator if not in inventory
                                }
                                Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportGeneratorIndex(),
                                    (int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY, 1); // Find and reset the Activator Generator
                                if (Rooms.GetInstance().ReportAffordances(Rooms.GetInstance().ReportPathmakerIndex())[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] == 3)
                                { // If the Pathmaker Machine was fueled and activated, find and partially reset the Pathmaker Machine
                                    Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportPathmakerIndex(),
                                        (int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY, 2);
                                }
                                Rooms.GetInstance().ChangeAffordance(Rooms.GetInstance().ReportTruePathIndex(),
                                    (int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY, 1); // Find and reset the True Path
                            }
                            break;
                        case (int)Controller.COMMAND.EMPTY:
                            description = "You do not seem to have finished your thought.";
                            break;
                        case (int)Controller.COMMAND.ERROR:
                            description = "I do not understand what you want to use.";
                            break;
                    }
                    break;
                case (int)Controller.COMMAND.INVENTORY:
                    // At most, the player can carry two items, one for each hand
                    // The crumbs from the Packed Lunch don't count as an item and could be listed as a third entry here
                    if (inventory[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] == 0
                        && inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 0
                        && inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 0
                        && inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 0)
                    {
                        description = "You carry nothing in either hand.";
                    }
                    else
                    {
                        description = "You carry:";
                        if (inventory[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                        {
                            description = description + "\n\rA Helpful Tell-You-Where-To-Go Compass";
                        }
                        if (inventory[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                        {
                            description = description + "\n\rAn Activator generated by an Activator Generator";
                        }
                        if (inventory[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                        {
                            description = description + "\n\rA Bubble of Plotadvancium Ore";
                        }
                        if (inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                        {
                            description = description + "\n\rA Packed Lunch, presumed delicious";
                        }
                        if (inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] == 2)
                        {
                            description = description + "\n\rThe crumbs remaining from a Packed Lunch, stuck to your hands";
                        }
                    }
                    break;
                case (int)Controller.COMMAND.REST:
                    if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.RESTFULNESS - (int)Controller.TARGET.OFFSETFORARRAY] == 1)
                    { // The conditions for resting are that the player be in the correct location and have the Packed Lunch
                        switch (inventory[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY])
                        {
                            case 0:
                                description = "The beach is marvelous.  This would be a good place to rest, but aren't you hungry?  Perhaps you should\n\r" +
                                    "pick up something to eat before you settle here.";
                                break;
                            case 1:
                                description = "The beach is marvelous.  Yes, here is a good place to rest.  You get out your food, settle down, and\n\r" +
                                    "enjoy a relaxing meal as the day comes to a close.  An unusual day, today, and you are glad to have this\n\r" +
                                    "moment to yourself.  This is a true ending of satisfaction.";
                                runGame = false; // Good ending
                                break;
                            case 2:
                                description = "The beach is marvelous.  Yes, here would is a good place to rest, but you cannot quiet your mind: the\n\r" +
                                    "ill fate of your Packed Lunch still plays before your eyes, the crumbs on your hands a leaden reminder.\n\r" +
                                    "This bad ending brings you no satisfaction.";
                                runGame = false; // Bad ending
                                break;
                            default:
                                description = "The ability to rest has been initialized to an impossible value.  Fine job on getting here.";
                                break;
                        }
                    }
                    else
                    {
                        description = "No, you do not think this is a good place to rest.";
                    }
                    break;
                case (int)Controller.COMMAND.HELP:
                    description = "You call for help!  It turns out that help is available.  Here is a mostly-complete command list:\n\rlook\n\rgo\n\rget\n\rdrop\n\ruse\n\r" +
                            "inventory\n\rrest\n\rhelp\n\rinitialize\n\rquit\n\r" +
                            "(Don't type the last two unless you mean it.)\n\r" +
                            "Many of these are actually two-word commands, like \"get,\" and a few synonyms are allowed.  Some can be\n\r" +
                            "abbreviated, like you don't need to type \"go east\" if you want to \"go e\" or \"east\" or \"e.\"  When using\n\r" +
                            "long commands, only the first six characters are read, so \"northeast,\" \"northeas,\" \"northea,\" and \"northe\"\n\r" +
                            "are all interpreted the same.";
                    break;
                case (int)Controller.COMMAND.INITIALIZE:
                    Console.Clear();
                    description = InitializeGameState();
                    break;
                case (int)Controller.COMMAND.QUIT:
                    description = "You begin the journey away from here.";
                    runGame = false;
                    break;
            }

            if (needsAffordanceDescription) // Add affordance text for either a look or move command
            { // The room name was already put in the description string to wipe out its default text

                // Parse any lines in the description divided by triple&&&
                string[] toAppend = Rooms.GetInstance().ReportDescription(currentLocation).Split("&&&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < toAppend.Length; i++)
                {
                    description = description + "\n\r" + toAppend[i];
                }

                // First, add most stationary objects
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY])
                { // Pathmaker Machine
                    case 1:
                        description = description + "\n\rThere is a half-hidden device here called the Pathmaker Machine.  It is unactivated and unpowered.";
                        break;
                    case 2:
                        description = description + "\n\rHalf-hidden but now full of energy, the Pathmaker Machine has been powered by the Plotadvancium Ore you\n\r" +
                            "put in the Omnidirectional Furnace!  Wow, that's a lot of words.  It's still unactivated.";
                        break;
                    case 3:
                        description = description + "\n\rThe Pathmaker Machine has been activated and is doing everything it can!";
                        break;
                }
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY])
                { // Omnidirectional Furnace
                    case 1:
                        description = description + "\n\rRumbling beneath you is the vast mouth of the Omnidirectional Furnace.  Anything you dropped here would\n\r" +
                            "be incinerated for energy!  Omnidirectionally.";
                        break;
                    case 2:
                        description = description + "\n\rRumbling beneath you is the vast mouth of the Omnidirectional Furnace, now loudly transmitting the energy\n\r" +
                            "of the Plotadvancium Ore out to everything that could conceivably be powered by it.";
                        break;
                }
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY])
                { // Activator Generator which can generate an Activator
                    case 1:
                        description = description + "\n\rThere is, in addition, a metal slot in the rock.  This is the output slot of the Activator Generator.\n\r" +
                            "It doesn't seem to be generating anything.";
                        break;
                    case 2:
                        description = description + "\n\rThere is, in addition, a metal slot in the rock.  The metal slot glows!  The Activator Generator is\n\r" +
                            "generating with all its might.  An Activator rests in the slot, waiting for some needful person.";
                        break;
                    case 3:
                        if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY]
                            == 0)
                        {
                            description = description + "\n\rThe Activator Generator glows with the effort of generating an Activator.\n\r" +
                                "You, however, already took the Activator, so there is nothing here for you.";
                        }
                        else // If the Activator is also present, allow for the later description
                        {
                            description = description + "\n\rThe Activator Generator glows with the effort of generating an Activator.";
                        }
                        break;
                }
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY])
                { // Magical Make-Something-Happen Lever
                    case 1:
                        description = description + "\n\rIn the last length of mountain rock, someone has installed a Magical Make-Something-Happen Lever.\n\r" +
                            "It is set to \"off.\"";
                        break;
                    case 2:
                        description = description + "\n\rIn the last length of mountain rock, someone has installed a Magical Make-Something-Happen Lever.\n\r" +
                            "Fortunately for present purposes, it is set to \"on.\"";
                        break;
                }
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY])
                { // The True Path
                    case 1:
                        description = description + "\n\rDespite all reason, there is no actual path along the beach.  You cannot go further from here.";
                        break;
                    case 2:
                        description = description + "\n\rThe True Path has opened.  You may traverse the beach.";
                        break;
                }
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.RESTFULNESS - (int)Controller.TARGET.OFFSETFORARRAY])
                { // Ability To Rest
                    case 1:
                        description = description + "\n\rOr you could rest here.";
                        break;
                }

                // Second, add items that the player can pick up
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY])
                {  // Bubble of Plotadvancium Ore
                    case 1:
                        if (currentLocation == Rooms.GetInstance().ReportOreIndex())
                        { // If this is the original source of all Plotadvancium Ore, give a unique description
                            description = description + "\n\rBubbling from the pit are units of raw Plotadvancium Ore!  A rare element that can be found only when\n\r" +
                                "you need a rare element.  They float and cool in the air; you could grab a Bubble from here.";
                        }
                        else
                        {
                            description = description + "\n\rYou left a Bubble of Plotadvancium Ore here, for some reason.";
                        }
                        break;
                }
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY])
                {  // Helpful Tell-You-Where-To-Go Compass
                    case 1:
                        description = description + "\n\rOn the ground you see someone has dropped a Helpful Tell-You-Where-To-Go Compass.";
                        break; // Some items such as this have no more cases, but are written to parallel the form of other items
                }
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.ACTIVATOR - (int)Controller.TARGET.OFFSETFORARRAY])
                { // Activator from the Activator Generator
                    case 1: // Do not describe the Activator if it is still in its initial slot where it was generated
                        if (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] != 2)
                        {
                            description = description + "\n\rAn Activator waits where you left it.  It's still good.";
                        }
                        break;
                }
                switch (Rooms.GetInstance().ReportAffordances(currentLocation)[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY])
                {  // Packed Lunch
                    case 1:
                        description = description + "\n\rAn unassuming Packed Lunch sits here, wrapped in checkered cloth.";
                        break;
                }
            }

            return description;
        }

        public string CompleteGame() // Acknowledge that the player has completed all conditions to win the game
        {
            string description = ". . . You are exiting the game.  Press any key to end.";
            return description;
        }
    }
}