using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanceBalls
{
    internal class BogeyGenerator
    {
        private System.Timers.Timer Timer { get; set; }
        private Game Game { get; set; }
        private Random rand = new();

        public BogeyGenerator(Game game, int interval = 2000) 
        { 
            Game = game;
            Timer = new(interval);
            Timer.Elapsed += Timer_Elapsed;

        }

        public void Start()
        {
            Timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (rand.NextDouble() > 0.2)
            {
                Game.AddNewBogey();
            }
        }
    }
}
