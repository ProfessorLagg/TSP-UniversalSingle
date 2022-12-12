using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TSPStandard.Algorithm
{
    public class InsertionBuild : TSPAlgorithm
    {
        public InsertionBuild(TSPRoute route) : base(route)
        {
            this.AlgorithmType = AlgorithmType.InsertionBuild;
        }
        public override void Run()
        {
            // RunV2
            ReadOnlySpan<Vector2> localSpan = new(this.Route.ToArray());
            List<Vector2> noVisit = this.Route.ToList();
            Vector2 best_vA = Vector2.Zero;
            Vector2 best_vB = Vector2.Zero;
            float dist_vAvB = float.NegativeInfinity;
            foreach (Vector2 vA in localSpan)
            {
                foreach (Vector2 vB in localSpan)
                {
                    if (Vector2.Distance(vA, vB) > dist_vAvB)
                    {
                        best_vA = vA;
                        best_vB = vB;
                        dist_vAvB = Vector2.Distance(vA, vB);
                    }
                }
            }
            List<Vector2> bestFoundTour = new List<Vector2>() { best_vA, best_vB };
            noVisit.Remove(best_vA);
            noVisit.Remove(best_vB);
            while (bestFoundTour.Count < localSpan.Length)
            {
                // Getting the furthest apart pair of points in bestFound
                Vector2 bestA = Vector2.Zero;
                Vector2 bestB = Vector2.Zero;
                int insertAt = -1;
                float bestDist = float.NegativeInfinity;
                for (int i = 0; i < bestFoundTour.Count; i++)
                {
                    int nex = (i + 1) % bestFoundTour.Count;
                    if (Vector2.Distance(bestFoundTour[i],bestFoundTour[nex]) > bestDist)
                    {
                        bestDist = Vector2.Distance(bestFoundTour[i], bestFoundTour[nex]);
                        bestA = bestFoundTour[i];
                        bestB = bestFoundTour[nex];
                        insertAt = nex;
                    }
                }
                //Vector2 bestNext = noVisit.MinBy(vec => vec.Dist(bestA) + vec.Dist(bestB));
                Vector2 bestNext = Vector2.Zero;
                float minDist = float.PositiveInfinity;
                foreach (Vector2 vec in CollectionsMarshal.AsSpan(noVisit))
                {
                    float dSum = Vector2.Distance(vec,bestA) + Vector2.Distance(vec, bestB);
                    if (dSum < minDist)
                    {
                        minDist = dSum;
                        bestNext = vec;
                    }
                }
                bestFoundTour.Insert(insertAt, bestNext);
                noVisit.Remove(bestNext);
            }
            TSPRoute bestFound = new(this.Route.RouteName, bestFoundTour);
            if (bestFound.Cost < this.Route.Cost)
            {
                this.Route = new(bestFound);
            }
        }
    }
}
