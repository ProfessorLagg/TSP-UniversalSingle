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
            int Runs = 0;
            ConcurrentDictionary<float, (int A, int B)> routes = new();
            int L = this.Route.Length;
        GenNew:
            routes.Clear();
            Vector2[] tempBase = Route.ToArray();
            Parallel.For(1, L, A =>
            //for (int A = 2; A < L; A++)
            {
                Span<Vector2> workerSpan = (Vector2[])tempBase.Clone();
                for (int B = 0; B < A; B++)
                {
                    workerSpan.Slice(B, A - B).Reverse();
                    routes.TryAdd(workerSpan.Cost(), (A, B));
                    workerSpan.Slice(B, A - B).Reverse();
                }
            });
            (int A, int B) bestIndex = routes[routes.Keys.Min()];

            Span<Vector2> bestRoute = Route.ToArray();
            bestRoute.Slice(bestIndex.B, bestIndex.A - bestIndex.B).Reverse();
            TSPRoute bestFound = new(this.Route.RouteName, bestRoute.ToArray());
            Runs += routes.Count;
            if (bestFound.Cost < this.Route.Cost)
            {
                this.Route = new(bestFound);
                goto GenNew;
            }
        }
    }
}
