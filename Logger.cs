using System;

namespace ArchivalTibiaV71MapEditor
{
    #if DEBUG
    public static class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
    #endif
}