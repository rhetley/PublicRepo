using System;

namespace DataDrivenCave
{
    // Demonstrating the Model-View-Controller design pattern, here is the V in MVC
    public class View
    {
        private static View uniqueInstance = new View(); // Make it a singleton
        public static View GetInstance()
        {
            return uniqueInstance;
        }

        public void DisplayText(string textInput)
        {
            Console.WriteLine(textInput);
        }
    }
}