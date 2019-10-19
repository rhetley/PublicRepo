namespace DataDrivenCave
{

    class Adventure
    {
        private static Adventure uniqueInstance = new Adventure(); // Make it a singleton
        public static Adventure GetInstance()
        {
            return uniqueInstance;
        }

        static void Main(string[] args)
        {
            // Prepare the adventure
            string initialDisplayText = GameLogic.GetInstance().InitializeGameState();
            View.GetInstance().DisplayText(initialDisplayText);

            while (GameLogic.GetInstance().CheckGameState()) // Main loop, controlled by the runGame bool
            {
                int[] playerInput = Controller.GetInstance().GetPlayerCommand();
                string textOutput = GameLogic.GetInstance().ResolvePlayerCommand(playerInput); // During debugging only
                View.GetInstance().DisplayText(textOutput);
            }

            // End the adventure
            string finalDisplayText = GameLogic.GetInstance().CompleteGame();
            View.GetInstance().DisplayText(finalDisplayText);
            Controller.GetInstance().WaitForPlayer();
        }
    }
}