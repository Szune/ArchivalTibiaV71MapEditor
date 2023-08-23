using System;

namespace ArchivalTibiaV71MapEditor
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            using var game = new Editor();
            game.Run();
        }
    }
}
