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
            Vector2[] bestFoundArr = this.Route.ToArray();
            float bestFoundCost = bestFoundArr.Cost();
            int bestStartIndex = 0;
            Parallel.For(0, this.Route.Length, startIndex =>
            {
                // Setting up for making this route
                List<Vector2> noVisit = new(bestFoundArr);
                Vector2 CurrentVector = noVisit[startIndex];
                float tourCost = 0;
                noVisit.RemoveAt(startIndex);
                // Calculating cost of building this tour
                while (noVisit.Count > 0)
                {
                    Vector2 NextVector = CurrentVector;
                    float bestCost = float.PositiveInfinity;
                    foreach (Vector2 vec in CollectionsMarshal.AsSpan(noVisit))
                    {
                        if (Vector2.Distance(vec, CurrentVector) < bestCost)
                        {
                            bestCost = Vector2.Distance(CurrentVector, vec);
                            NextVector = vec;
                        }
                    }
                    tourCost += Vector2.Distance(CurrentVector,NextVector);
                    noVisit.Remove(NextVector);
                    CurrentVector = NextVector;
                }
                tourCost += Vector2.Distance(bestFoundArr[startIndex],CurrentVector);
                if (tourCost < bestFoundCost)
                {
                    bestFoundCost = tourCost;
                    bestStartIndex = startIndex;
                }
            });

            // Building new bestFoundArr
            List<Vector2> noVisit = new(bestFoundArr);
            Vector2[] tour = new Vector2[noVisit.Count];
            Span<Vector2> tourSpan = new(tour);
            tourSpan[0] = noVisit[bestStartIndex];
            noVisit.Remove(tourSpan[0]);
            // building this route
            for (int i = 1; i < tourSpan.Length; i++)
            {
                Vector2 currentVec = tour[i - 1];
                float bestDist = float.MaxValue;
                Vector2 bestVector = Vector2.One;
                // finding closest vector
                foreach (Vector2 nextVec in noVisit)
                {
                    if (Vector2.Distance(nextVec, currentVec) < bestDist)
                    {
                        bestDist = Vector2.Distance(nextVec, currentVec);
                        bestVector = new(nextVec.X, nextVec.Y);
                    }
                }
                // adding closest vector
                tourSpan[i] = bestVector;
                noVisit.Remove(bestVector);
            }

            // Checking if i found a better route
            TSPRoute bestFound = new(this.Route.RouteName, tour);
            if (bestFound.Cost < this.Route.Cost)
            {
                this.Route = new(bestFound);
            }
        }
    }
}
