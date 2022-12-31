using System;
using System.Collections.Generic;
using System.Text;


namespace TSPStandard.Algorithm
{
    public sealed class TwoOpt : TSPAlgorithm
    {
        public TwoOpt(TSPRoute route) : base(route)
        {
            this.AlgorithmType = AlgorithmType.TwoOpt;
        }
        public override void Run()
        { // V4_2
            int L = this.Route.Length;
            bool changed = false;
            Vector2[] localRoute = this.Route.ToArray();
            Span<Vector2> localSpan = new(localRoute);
            float bestCost = localSpan.Cost();
            int min_i = 1;
            do
            {
                changed = false;
                for (int i = min_i; i < L; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        localSpan.Slice(j, i - j).Reverse();
                        if (localSpan.Cost() < bestCost)
                        {
                            changed = true;
                            bestCost = localSpan.Cost();
                            min_i = j - 1;
                            break;
                        }
                        else
                        {
                            localSpan.Slice(j, i - j).Reverse();
                        }
                    }
                    if (changed) { break; }
                }
            } while (changed);

            TSPRoute bestFound = new(this.Route.RouteName, localRoute.ToArray());
            if (bestFound.Cost < this.Route.Cost)
            {
                this.Route = new(bestFound);
            }
        }
    }
}
