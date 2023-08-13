using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DanceBalls
{
    internal class Bumper : Ball
    {
        public bool IsActive { get; set; }
        public Vector2 RequestedPosition { get; set; }

        public Bumper(Game game, float x, float y, float radius)
            : base(game, new Vector2(x, y), radius, Path.Combine(Game.AppRoot, "blue orb.png"))
        {
        }

        public void RequestMoveTo(Point clientLocation, Point clientOffset)
        {
            //var origPosition = Position;
            var newPoint = Game.ClientToScale(clientLocation.Subtract(clientOffset));
            var newPosition = new Vector2(newPoint.X, newPoint.Y);
            //var newPosition = Game.ClientToScale(clientLocation);
            if (newPosition.X < Game.ScaleLeft + Radius) newPosition.X = Game.ScaleLeft + Radius;
            if (newPosition.X > Game.ScaleRight - Radius) newPosition.X = Game.ScaleRight - Radius;
            if (newPosition.Y < Game.ScaleBottom + Radius) newPosition.Y = Game.ScaleBottom + Radius;
            if (newPosition.Y > Game.ScaleTop - Radius) newPosition.Y = Game.ScaleTop - Radius;

            //foreach (var bogey in Game.Bogeys.Values)
            //{
            //    if (WouldCollideWith(newPoint, Radius, bogey))
            //    {
            //        var dir = (newPosition - origPosition).Normalize();
            //        var allowed = dir * (Radius * 0.99f);
            //        newPosition = Position + allowed;
            //        break;
            //    }
            //}
            RequestedPosition = newPosition;
        }

    }
}
