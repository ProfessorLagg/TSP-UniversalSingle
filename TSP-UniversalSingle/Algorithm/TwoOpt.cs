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
        {
            int L = this.Route.Length;
            bool changed = false;
            do
            {
                changed = false;
                Vector2[] tempBase = Route.ToArray();
                Vector2[] bestTour = tempBase.ToArray();
                float bestCost = bestTour.Cost();
                Parallel.For(1, L, A =>
                {
                    Vector2[] workerArray = tempBase.ToArray();
                    Span<Vector2> workerSpan = new(workerArray);
                    for (int B = 0; B < A; B++)
                    {
                        workerSpan.Slice(B, A - B).Reverse();
                        if (workerSpan.Cost() < bestCost)
                        {
                            lock (bestTour)
                            {
                                bestTour = workerArray.ToArray();
                                bestCost = bestTour.Cost();
                            }
                        }
                        workerSpan.Slice(B, A - B).Reverse();
                    }
                });
                TSPRoute bestFound = new(this.Route.RouteName, bestTour.ToArray());
                if (bestFound.Cost < this.Route.Cost)
                {
                    this.Route = new(bestFound);
                    changed = true;
                }
            } while (changed);
        }
    }
}
