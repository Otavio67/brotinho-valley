using UnityEngine;
namespace Brotinho
{
    public static class DebugMenu
    {
        public static bool Enabled
        {
            get
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
                #else
                return false;
                #endif
            }
        }
        public static void AddCoins(GameSession game)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            game.State.coins += 10000; game.Bus.Refresh();
            #endif
        }
        public static void SkipTime(GameSession game)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Simulation.Advance(game, 600); game.Bus.Refresh();
            #endif
        }
    }
}
