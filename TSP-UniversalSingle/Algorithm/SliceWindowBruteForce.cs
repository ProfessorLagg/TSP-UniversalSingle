using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TSPStandard.Algorithm
{
    public class SliceWindowBruteForce : TSPAlgorithm
    {
        public SliceWindowBruteForce(TSPRoute route) : base(route)
        {
            this.AlgorithmType = AlgorithmType.SliceWindowBruteForce;
        }
        public SliceWindowBruteForce(TSPRoute route, bool recursive) : base(route)
        {
            this.AlgorithmType = AlgorithmType.SliceWindowBruteForce;
            this.Recursive = recursive;
        }
        private static PermutationGenerator permGen = new();
        public int PermLength = 8;
        public bool Recursive = false;
        public override void Run()
        {
            List<int[]> IndexPerms = permGen.GetIndexPermutations(PermLength);
        Start:
            Span<Vector2> localSpan = new(this.Route.ToArray());
            int L = localSpan.Length;

            for (int i = 0; i < (L - PermLength); i++)
            {
                int preIndex = i - 1;
                if (preIndex < 0) { preIndex += L; }
                Vector2 preVector = localSpan[preIndex];
                Vector2 postVector = localSpan[(i + 1 + PermLength) % L];

                ReadOnlySpan<Vector2> originalSlice = new(localSpan.Slice(i, PermLength).ToArray());
                float BestCost = originalSlice.TourLength() + Vector2.Distance(preVector,originalSlice[0]) + Vector2.Distance(originalSlice[originalSlice.Length - 1],postVector);
                foreach (int[] indexPerm in CollectionsMarshal.AsSpan(IndexPerms))
                {
                    // calculating cost of slice if perm was applied
                    float cost = Vector2.Distance(postVector,originalSlice[indexPerm[indexPerm.Length - 1]]);
                    Vector2 currVec = preVector;
                    for (int j = 0; j < indexPerm.Length; j++)
                    {
                        cost += Vector2.Distance(currVec,originalSlice[indexPerm[j]]);
                        currVec = originalSlice[indexPerm[j]];
                    }
                    // Applying perm if cost of slice was better
                    if (cost < BestCost)
                    {
                        var destSlice = localSpan.Slice(i, PermLength);
                        for (int k = 0; k < destSlice.Length; k++)
                        {
                            destSlice[k] = originalSlice[indexPerm[k]];
                        }
                        BestCost = cost;
                    }
                }
            }
            TSPRoute bestFound = new(this.Route.RouteName, localSpan.ToArray());
            if (bestFound.Cost < this.Route.Cost)
            {
                this.Route = new(bestFound);
                if (Recursive) { goto Start; }
            }
        }
    }
}
