using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Security.Cryptography;
using System.Diagnostics;

namespace TSPStandard.Algorithm
{
    public sealed class Swap : TSPAlgorithm
    {
        public Swap(TSPRoute route) : base(route)
        {
            this.AlgorithmType = AlgorithmType.Swap;
        }
        public override void Run()
        {
            TSPRoute bestFound = new(this.Route);
            int L = this.Route.Length;
            bool changed = false;
            do
            {
                Vector2[] tmpRoute = this.Route.ToArray();
                float bestCost = float.PositiveInfinity;
                (int A, int B) bestIndex = (-1, -1);
                changed = false;

                Parallel.For(1, L, B =>
                //for (int B = 1; B < L; B++)
                {
                    Parallel.For(0, B, A =>
                    //for (int A = 0; A < B; A++)
                    {
                        int PreA = A - 1;
                        int PreB = B - 1;
                        if (PreA < 0) { PreA = L + PreA; }
                        if (PreB < 0) { PreB = L + PreB; }
                        int PostA = (A + 1) % L;
                        int PostB = (B + 1) % L;
                        float newCost = -1;
                        if (PostA == B || PreA == B)
                        {
                            if (PostB < B && PostB == A)
                            {
                                newCost = tmpRoute.Cost() -
                                Vector2.Distance(tmpRoute[A], tmpRoute[PostA]) -
                                Vector2.Distance(tmpRoute[PreB], tmpRoute[B]) -
                                Vector2.Distance(tmpRoute[B], tmpRoute[A]) +
                                Vector2.Distance(tmpRoute[B], tmpRoute[PostA]) +
                                Vector2.Distance(tmpRoute[PreB], tmpRoute[A]) +
                                Vector2.Distance(tmpRoute[A], tmpRoute[B]);
                            }
                            else
                            {
                                newCost = tmpRoute.Cost() -
                                Vector2.Distance(tmpRoute[PreA], tmpRoute[A]) -
                                Vector2.Distance(tmpRoute[A], tmpRoute[B]) -
                                Vector2.Distance(tmpRoute[B], tmpRoute[PostB]) +
                                Vector2.Distance(tmpRoute[PreA], tmpRoute[B]) +
                                Vector2.Distance(tmpRoute[B], tmpRoute[A]) +
                                Vector2.Distance(tmpRoute[A], tmpRoute[PostB]);
                            }
                        }
                        else
                        {
                            newCost = tmpRoute.Cost() -
                            Vector2.Distance(tmpRoute[PreA], tmpRoute[A]) -
                            Vector2.Distance(tmpRoute[A], tmpRoute[PostA]) -
                            Vector2.Distance(tmpRoute[PreB], tmpRoute[B]) -
                            Vector2.Distance(tmpRoute[B], tmpRoute[PostB]) +
                            Vector2.Distance(tmpRoute[PreA], tmpRoute[B]) +
                            Vector2.Distance(tmpRoute[B], tmpRoute[PostA]) +
                            Vector2.Distance(tmpRoute[PreB], tmpRoute[A]) +
                            Vector2.Distance(tmpRoute[A], tmpRoute[PostB]);
                        }
                        if (newCost > 0 && newCost < bestCost)
                        {
                            bestIndex = (A, B);
                            bestCost = newCost;
                        }
                    });
                });
                bestFound = new(this.Route);
                bestFound[bestIndex.A] = Route[bestIndex.B];
                bestFound[bestIndex.B] = Route[bestIndex.A];
                if (bestFound.Cost < Route.Cost)
                {

                    Route = new(bestFound);
                    changed = true;
                }
            } while (changed);

        }
    }
}
