using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TSPStandard.Algorithm
{
    public sealed class NearestNeighbor : TSPAlgorithm
    {
        public NearestNeighbor(TSPRoute route) : base(route)
        {
            this.AlgorithmType = AlgorithmType.NearestNeighbor;
        }

        public override void Run()
        {
            Vector2[] bestTour = this.Route.ToArray();
            Parallel.For(0, this.Route.Length, startIndex =>
            {
                List<Vector2> noVisit = new(this.Route);
                Vector2[] tour = this.Route.ToArray();
                Span<Vector2> tourSpan = new(tour);
                tourSpan[0] = this.Route[startIndex];
                // building tour
                for (int i = 1; i < this.Route.Length; i++)
                {
                    Vector2 currentVector = tourSpan[i - 1];
                    Vector2 bestVector = Vector2.One;
                    float bestDistance = float.PositiveInfinity;
                    foreach (Vector2 item in CollectionsMarshal.AsSpan(noVisit))
                    {
                        if (Vector2.DistanceSquared(item, currentVector) < bestDistance)
                        {
                            bestDistance = Vector2.DistanceSquared(item, currentVector);
                            bestVector = item;
                        }
                    }
                    tour[i] = bestVector;
                    noVisit.Remove(bestVector);
                }
                lock (bestTour)
                {
                    if (tour.Cost() < bestTour.Cost())
                    {
                        bestTour = tour.ToArray();
                    }
                }
            });
            TSPRoute bestFound = new(this.Route.RouteName, bestTour);
            if (bestFound.Cost < this.Route.Cost)
            {
                this.Route = new(bestFound);
            }
        }
    }
}
