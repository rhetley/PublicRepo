using System;
using System.IO;
using System.Linq;

namespace DataDrivenCave
{
    // The rooms class, intended to fill its arrays from an external file
    public class Rooms
    {
        // All values at the same index are for the same room
        private string[] names;
        private string[] descriptions;
        private string[][] connectedRooms;
        private int[][] affordances;
        private int totalNumberOfRooms = 0;

        // Indices for several important targets that can be affected long-distance by the user
        // These are identified while reading the external file
        // Only one index is allowed, so if multiple are found in the file then only the final one is kept
        // It is impossible to win the game without all five placed somewhere
        private int sourceOfOreIndex = -1;
        private int pathmakerIndex = -1;
        private int furnaceIndex = -1;
        private int generatorIndex = -1;
        private int truePathIndex = -1;

        // This is only used once the Activator is generated
        // It accommodates the possibility of the player dropping it somewhere then resetting the puzzle
        private int activatorIndex;

        private static Rooms uniqueInstance = new Rooms(); // Make it a singleton
        public static Rooms GetInstance()
        {
            return uniqueInstance;
        }

        public string ReportName(int roomIndex)
        {
            return names[roomIndex];
        }

        public string ReportDescription(int roomIndex)
        {
            return descriptions[roomIndex];
        }

        public string[] ReportConnections(int roomIndex)
        {
            return connectedRooms[roomIndex];
        }

        public int[] ReportAffordances(int roomIndex)
        {
            return affordances[roomIndex];
        }

        public void ChangeAffordance(int roomIndex, int elementIndex, int newValue)
        {
            affordances[roomIndex][elementIndex] = newValue;
        }

        public int ReportNumberOfRooms()
        {
            return totalNumberOfRooms;
        }

        public int ReportOreIndex()
        {
            return sourceOfOreIndex;
        }

        public int ReportPathmakerIndex()
        {
            return pathmakerIndex;
        }

        public int ReportFurnaceIndex()
        {
            return furnaceIndex;
        }

        public int ReportGeneratorIndex()
        {
            return generatorIndex;
        }

        public int ReportTruePathIndex()
        {
            return truePathIndex;
        }
        public int ReportActivatorIndex()
        {
            return activatorIndex;
        }
        public void ChangeActivatorIndex(int newValue)
        {
            activatorIndex = newValue;
        }

        public string InitializeRoomsFromFile()
        {
            // This function will return either a success statement or an explanation of why initialization failed
            // The format expected for an input text file is as follows:
            // 
            // STARTDATA
            // ROOMNAME
            // Text To Be Written in the Game
            // ROOMDESCRIPTION
            // Text with a triple&&&where there is to be a line break upon display in the game
            // ROOMEXITS
            // lowercase direction
            // Exact Room Name of Destination in that Direction
            // any more lowercase directions
            // Same Pattern
            // ROOMAFFORDANCES
            // lowercase item or functionality name
            // any more items or functionality
            // ROOMNAME
            // Et cetera
            // ROOMNAME
            // Et cetera
            // ENDDATA
            // 
            // If a room is empty, there cannot be blank space between ROOMAFFORDANCES and the next ROOMNAME or the ENDDATA footer
            // Instead there must be "none," "null" (the word written out), or "empty."
            // 
            // Otherwise, the code looks for these headers and ignores some lines of text
            // Many bits of erroneous text in the middle, such as in ROOMAFFORDANCES, will generate error strings instead
            // These catch cases of incorrect formating or ordering of information
            // 
            // Comments belong only before the STARTDATA header or after the ENDDATA footer
            // 
            // The player begins in the first room listed
            // A map must include all five targets noted with special indices (sourceOfOreIndex, et cetera)
            // If the room reached by following the True Path is in a normal direction (e.g., "north"),
            // there must be a duplicate route for that room in the "other" direction or it will not be blocked properly

            bool fileInputSuccessful = new bool();
            string initializationResult = "Success";
            try
            {
                File.ReadAllText("roomdata.txt");
                fileInputSuccessful = true;
            }
            catch (Exception thrownException)
            {
                fileInputSuccessful = false;
                initializationResult = "Failed to read file with this message: " + thrownException.Message;
            }

            if (fileInputSuccessful) // Only if the try statement succeeded should this continue
            {
                string fullReadFile = File.ReadAllText("roomdata.txt");
                string[] fileSeparated = fullReadFile.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // First check to see what the size of all arrays should be
                for (int i = 0; i < fileSeparated.Length; i++)
                {
                    if (fileSeparated[i] == "ROOMNAME")
                    {
                        totalNumberOfRooms++;
                    }
                }
                names = new string[totalNumberOfRooms];
                descriptions = new string[totalNumberOfRooms];
                connectedRooms = new string[totalNumberOfRooms][];
                affordances = new int[totalNumberOfRooms][];

                // Ignoring the OFFSETFORARRAY value, this is how many alternatives are allowed by DIRECTION and TARGET
                int sizeOfDirectionEnum = Enum.GetNames(typeof(Controller.DIRECTION)).Length - 1;
                int sizeOfTargetEnum = Enum.GetNames(typeof(Controller.TARGET)).Length - 1;

                int positionToFill = -1; // This starts at -1 and is incremented as rooms are found for filling the arrays
                int generalCounter = 0; // This iterates through the entire input file

                bool lookingForStart = true; // Used to search for the organization inside the input file
                bool lookingForName;
                bool lookingForDescription;
                bool lookingForExits;
                bool lookingForAffordances;
                bool lookingForEnd = true; // STARTDATA and ENDDATA should only happen once, but the others will keep iterating

                bool cyclingExits = true; // Exit and affordance subsections can be of arbitrary length, so these are used to cycle
                bool cyclingAffordances = true;

                while (lookingForStart) // First skip any comments at the start of the file before the word STARTDATA
                {
                    if (fileSeparated[generalCounter] == "STARTDATA")
                    {
                        lookingForStart = false;
                    }
                    generalCounter++; // Whether STARTDATA is found or not, look to the next element
                    if (generalCounter == fileSeparated.Length)
                    {
                        lookingForStart = false;
                        initializationResult = "Failed to find STARTDATA header combined with data, cannot load rooms from file.";
                    }
                } // End of lookingForStart loop

                while (lookingForEnd && initializationResult == "Success") // Always stop if an initialization error happens
                {
                    lookingForName = true; // This loop may be performed many times, so reset checks other than STARTDATA or ENDDATA
                    lookingForDescription = true;
                    lookingForExits = true;
                    lookingForAffordances = true;
                    if (generalCounter == fileSeparated.Length)
                    { // At this point, the code must have run through other elements and gotten to the end without ENDDATA
                        initializationResult = "End of file found without ENDDATA footer, cannot load rooms from file.";
                        lookingForName = false; // Prevent all further checks
                        lookingForDescription = false;
                        lookingForExits = false;
                        lookingForAffordances = false;
                        lookingForEnd = false;
                    }
                    if (fileSeparated[generalCounter] == "ENDDATA")
                    { // At this point, either there are no rooms in the file, or the code just finished an intact room
                        lookingForName = false; // Prevent all further checks, but at least it's not a file error
                        lookingForDescription = false;
                        lookingForExits = false;
                        lookingForAffordances = false;
                        lookingForEnd = false;
                    }

                    while (lookingForName) // This is the first of the repeatable subloops, done once for each room
                    {
                        if (generalCounter == fileSeparated.Length)
                        { // At this point, the code must have run through other elements and not found a ROOMDATA
                            initializationResult = "Failed to find ROOMNAME header combined with data, cannot load rooms from file.";
                            lookingForName = false; // Prevent all further checks
                            lookingForDescription = false;
                            lookingForExits = false;
                            lookingForAffordances = false;
                            lookingForEnd = false;
                        }
                        else if (fileSeparated[generalCounter] == "ROOMNAME")
                        {
                            lookingForName = false;
                            generalCounter++; // The next element should be a name
                            positionToFill++; // Increment the index to fill in all the arrays, as a room has been found
                            if (generalCounter != fileSeparated.Length) // Check for the end of the file
                            { // If the next element is not actually a name, it will be seen as strange text during gameplay
                                names[positionToFill] = fileSeparated[generalCounter]; // Store the file's data in the proper array location
                            }
                            else
                            {
                                initializationResult = "Failed to find ROOMNAME header combined with data, cannot load rooms from file.";
                                lookingForDescription = false; // Prevent all further checks
                                lookingForExits = false;
                                lookingForAffordances = false;
                                lookingForEnd = false;
                            }
                        }
                        else
                        {
                            generalCounter++;
                        }

                    } // End of lookingForName subloop

                    while (lookingForDescription) // There should only be one name per room, and next should be a description
                    {
                        if (generalCounter == fileSeparated.Length)
                        { // At this point, the code must have run through elements after ROOMDATA but not found ROOMDESCRIPTION
                            initializationResult = "Failed to find ROOMDESCRIPTION header combined with data, cannot load rooms from file.";
                            lookingForDescription = false; // Prevent all further checks
                            lookingForExits = false;
                            lookingForAffordances = false;
                            lookingForEnd = false;
                        }
                        else if(fileSeparated[generalCounter] == "ROOMDESCRIPTION")
                        {
                            lookingForDescription = false;
                            generalCounter++; // The next element should be a description on one line with "&&&" for line breaks
                            if (generalCounter != fileSeparated.Length) // Check for the end of the file
                            { // If the next element is not actually a description, it will be seen as strange text during gameplay
                                descriptions[positionToFill] = fileSeparated[generalCounter];  // Store the file's data in the proper array location
                            }
                            else
                            {
                                initializationResult = "Failed to find ROOMDESCRIPTION header combined with data, cannot load rooms from file.";
                                lookingForExits = false; // Prevent all further checks
                                lookingForAffordances = false;
                                lookingForEnd = false;
                            }
                        }
                        else
                        {
                            generalCounter++;
                        }
                    } // End of lookingForDescription subloop

                    while (lookingForExits) // There should only be one description per room, and next should be exits
                    {
                        if (generalCounter == fileSeparated.Length)
                        { // At this point, the code must have run through elements after ROOMDATA but not found ROOMEXITS
                            initializationResult = "Failed to find ROOMEXITS header combined with data, cannot load rooms from file.";
                            lookingForExits = false; // Prevent all further checks
                            lookingForAffordances = false;
                            lookingForEnd = false;
                        }
                        else if (fileSeparated[generalCounter] == "ROOMEXITS")
                        {
                            lookingForExits = false; // The next elements should begin a list of exits on two lines each
                            string[] foundExits = new string[sizeOfDirectionEnum];
                            while (cyclingExits) // Cycle through all elements in this subsection
                            {
                                generalCounter++; // Increment once when entering this code, for a total of twice before restarting the loop
                                if (generalCounter == fileSeparated.Length || generalCounter + 1 == fileSeparated.Length)
                                {
                                    initializationResult = "End of file found in ROOMEXITS data, cannot load rooms from file.";
                                    cyclingExits = false; // Prevent all further checks
                                    lookingForAffordances = false;
                                    lookingForEnd = false;
                                }
                                else
                                {
                                    switch (new string(fileSeparated[generalCounter].ToLower().Take(6).ToArray()))
                                    {
                                        // The Controller has non-repeating values for enums COMMAND, DIRECTION, and TARGET to avoid collisions
                                        // To translate these into locations in an array, DIRECTION and TARGET have an OFFSETFORARRAY value
                                        // Once subtracted, an array of the right size (sizeOfDirectionEnum or sizeOfTargetEnum) can be mapped
                                        case "north":
                                        case "n":
                                            foundExits[(int)Controller.DIRECTION.NORTH - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "northeast": // Only the first six characters are checked, but these longer cases are listed for readability
                                        case "northe":
                                        case "ne":
                                            foundExits[(int)Controller.DIRECTION.NORTHEAST - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "east":
                                        case "e":
                                            foundExits[(int)Controller.DIRECTION.EAST - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "southeast":
                                        case "southe":
                                        case "se":
                                            foundExits[(int)Controller.DIRECTION.SOUTHEAST - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "south":
                                        case "s":
                                            foundExits[(int)Controller.DIRECTION.SOUTH - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "southwest":
                                        case "southw":
                                        case "sw":
                                            foundExits[(int)Controller.DIRECTION.SOUTHWEST - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "west":
                                        case "w":
                                            foundExits[(int)Controller.DIRECTION.WEST - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "northwest":
                                        case "northw":
                                        case "nw":
                                            foundExits[(int)Controller.DIRECTION.NORTHWEST - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "up":
                                        case "u":
                                            foundExits[(int)Controller.DIRECTION.UP - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "down":
                                        case "d":
                                            foundExits[(int)Controller.DIRECTION.DOWN - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        case "other":
                                            foundExits[(int)Controller.DIRECTION.OTHER - (int)Controller.DIRECTION.OFFSETFORARRAY]
                                                = fileSeparated[generalCounter + 1];
                                            break;
                                        default:
                                            initializationResult = "Non-recognized direction found in ROOMEXITS data, cannot load rooms from file.";
                                            cyclingExits = false; // Prevent all further checks
                                            lookingForAffordances = false;
                                            lookingForEnd = false;
                                            break;
                                    }
                                    generalCounter++; // This loop increments a total of twice before restarting, and here is the second
                                    if (generalCounter != fileSeparated.Length)
                                    {
                                        if (fileSeparated[generalCounter + 1] == "ROOMAFFORDANCES")
                                        {
                                            cyclingExits = false; // Once exits end, next should be affordances
                                            generalCounter++; // Advance out of this two-line, one-step-ahead subsection
                                        }
                                    }
                                }
                            }
                            connectedRooms[positionToFill] = foundExits; // Store the file's data in the proper array location
                            cyclingExits = true; // Reset this for any further room
                        }
                        else
                        {
                            generalCounter++;
                        }
                    } // End of lookingForExits subloop

                    while (lookingForAffordances) // After all the exits, next should be affordances
                    { // The first check for ROOMAFFORDANCES was already done in the two-line, one-step-ahead ROOMEXITS cycling
                        lookingForAffordances = false; // The next elements should begin a list of affordances on single lines each
                        int[] foundAffordances = new int[sizeOfTargetEnum];
                        while (cyclingAffordances)  // Begin cycling through all elements in this subsection
                        {
                            generalCounter++; // Increment once when entering this loop
                            if (generalCounter == fileSeparated.Length)
                            { 
                              // At this point, the only remaining way to reach the end of the list is if it ends without ENDDATA
                                initializationResult = "End of file found in ROOMAFFORDANCES data, cannot load rooms from file.";
                                cyclingAffordances = false; // Prevent all further checks
                                lookingForEnd = false;
                            }
                            else
                            {
                                switch (new string(fileSeparated[generalCounter].ToLower().Take(6).ToArray()))
                                {
                                    // The Controller has non-repeating values for enums COMMAND, DIRECTION, and TARGET to avoid collisions
                                    // To translate these into locations in an array, DIRECTION and TARGET have an OFFSETFORARRAY value
                                    // Once subtracted, an array of the right size (sizeOfDirectionEnum or sizeOfTargetEnum) can be mapped
                                    case "compass": // Only the first six characters are checked, but these longer cases are listed for readability
                                    case "compas":
                                        foundAffordances[(int)Controller.TARGET.COMPASS - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        break;
                                    case "activator":
                                    case "activa": // The Activator should only be created by the Activator Generator
                                        initializationResult = "Game-breaking target \"activator\" found in ROOMAFFORDANCES data, cannot play with these data.";
                                        cyclingAffordances = false; // Prevent all further checks
                                        lookingForEnd = false;
                                        break;
                                    case "bubble":
                                    case "plotadvancium":
                                    case "plotad":
                                    case "ore":
                                        foundAffordances[(int)Controller.TARGET.ORE - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        sourceOfOreIndex = positionToFill; // The original source provides an infinite amount of ore
                                        break;
                                    case "lunch":
                                    case "food":
                                    case "meal":
                                        foundAffordances[(int)Controller.TARGET.LUNCH - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        break;
                                    case "pathmaker":
                                    case "pathma":
                                    case "machine":
                                    case "machin":
                                        foundAffordances[(int)Controller.TARGET.PATHMAKER - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        pathmakerIndex = positionToFill; // This can be can be affected long-distance by the user
                                        break;
                                    case "furnace":
                                    case "furnac":
                                        foundAffordances[(int)Controller.TARGET.FURNACE - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        furnaceIndex = positionToFill; // This can be can be affected long-distance by the user
                                        break;
                                    case "generator":
                                    case "genera":
                                    case "slot":
                                        foundAffordances[(int)Controller.TARGET.GENERATOR - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        generatorIndex = positionToFill; // This can be can be affected long-distance by the user
                                        break;
                                    case "lever":
                                        foundAffordances[(int)Controller.TARGET.LEVER - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        break;
                                    case "path":
                                        foundAffordances[(int)Controller.TARGET.PATH - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        truePathIndex = positionToFill; // This can be can be affected long-distance by the user
                                        break;
                                    case "rest":
                                    case "restfulness":
                                    case "restfu":
                                        foundAffordances[(int)Controller.TARGET.RESTFULNESS - (int)Controller.TARGET.OFFSETFORARRAY] = 1;
                                        break;
                                    case "none":
                                    case "null":
                                    case "empty":
                                        // These are the proper ways to end a room's entry if the room has nothing in it
                                        break;
                                    default:
                                        initializationResult = "Non-recognized target found in ROOMAFFORDANCES data, cannot load rooms from file.";
                                        cyclingAffordances = false; // Prevent all further checks
                                        lookingForEnd = false;
                                        break;
                                }
                                if (generalCounter + 1 != fileSeparated.Length)
                                {
                                    if (fileSeparated[generalCounter + 1] == "ROOMNAME" || fileSeparated[generalCounter + 1] == "ENDDATA")
                                    {
                                        cyclingAffordances = false; // Once affordances end, either another room or the file's end could be next
                                        generalCounter++; // Advance out of this single-line subsection
                                    }
                                }
                            }
                        }
                        affordances[positionToFill] = foundAffordances; // Store the file's data in the proper array location
                        cyclingAffordances = true; // Reset this for any further room
                    } // End of lookingForAffordances subloop

                } // End of lookingForEnd loop
                // At this point, it will skip any comments after the word ENDDATA

                // Check whether all five target indices have been filled from the file, as -1 can't be an array location
                if (initializationResult == "Success") // Don't overwrite any more urgent error message, though
                {
                    if (sourceOfOreIndex == -1 || pathmakerIndex == -1 || furnaceIndex == -1 || generatorIndex == -1 || truePathIndex == -1)
                    {
                        initializationResult = "Data file missing one or more critical gameplay targets, cannot play with these data.  Missing:";
                        if (sourceOfOreIndex == -1)
                        {
                            initializationResult = initializationResult + "\n\rplotadvancium";
                        }
                        if (pathmakerIndex == -1)
                        {
                            initializationResult = initializationResult + "\n\rpathmaker";
                        }
                        if (furnaceIndex == -1)
                        {
                            initializationResult = initializationResult + "\n\rfurnace";
                        }
                        if (generatorIndex == -1)
                        {
                            initializationResult = initializationResult + "\n\rgenerator";
                        }
                        if (truePathIndex == -1)
                        {
                            initializationResult = initializationResult + "\n\rpath";
                        }
                    }
                }

            } // End of fileInputSuccessful check

            ChangeActivatorIndex(-1); // This is the only value not read from the file: the Activator cannot exist at the start of play

            return initializationResult;
        } // InitializeRoomsFromFile()
    }
}