using System;
using System.Collections.Generic;
using System.Text;

using Popolo.FluidNetwork;

namespace Popolo
{
    public class LatentHeatNode : Node
    {
        public override double Capacity
        {
            get
            {
                if (Potential < 7.0) return 4.2;
                else if (7.1 < Potential) return 4.2;
                //7度前後で顕熱量の1000倍の潜熱量があると仮定
                else return 4.2 * (1000d + 0.1);
            }
        }

        public LatentHeatNode()
            : base() { }

        public LatentHeatNode(string name)
            : base(name) { }

    }
}
