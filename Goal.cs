using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanceBalls
{
    internal class Goal
    {
        public RectangleF Bounds { get; set; }

        public Goal(RectangleF bounds)
        {
            Bounds = bounds;
        }
        public bool IsInGoal(Bogey bogey)
        {
            return Bounds.ScaleIntersects(bogey.Bounds);
        }

    }
}
