using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace DanceBalls
{
    internal class Bogey : Ball
    {
        public bool IsBeingBumped { get; set; }

        public Bogey(Game game, float x, float y, float radius)
            : this(game, new Vector2(x, y), radius, new Vector2(0, 0))
        { }

        public Bogey(Game game, Vector2 position, float radius, Vector2 speed)
            : base(game, position, radius, speed, Path.Combine(Game.AppRoot, "orb.png"))
        {
        }
    }
}
