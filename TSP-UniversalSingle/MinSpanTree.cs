using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TSPStandard
{
    public class MinSpanTree
    {
        public Vector2[] NodeBase { get; protected set; }
        public float Cost
        {
            get
            {
                return edges.Sum(e => Vector2.Distance(e.from, e.to));
            }
        }
        public List<(Vector2 from, Vector2 to)> edges = new();
        public MinSpanTree(IEnumerable<Vector2> nodeBase)
        {
            NodeBase = nodeBase.CircularNormalize();
        }
        public virtual void Generate()
        {
            List<Vector2> noVisit = new(NodeBase);
            List<Vector2> MST = new();
            List<(Vector2 from, Vector2 to)> output = new();
            MST.Add(noVisit[0]);
            noVisit.RemoveAt(0);

            while (noVisit.Count > 0)
            {
                (Vector2 from, Vector2 to) bestEdge = new();
                float bestWeight = float.PositiveInfinity;
                foreach (Vector2 Unvisited in CollectionsMarshal.AsSpan(noVisit))
                {
                    foreach (Vector2 Visited in CollectionsMarshal.AsSpan(MST))
                    {
                        float weight = Vector2.DistanceSquared(Visited, Unvisited);
                        if (weight < bestWeight)
                        {
                            bestEdge = new(Visited, Unvisited);
                            bestWeight = weight;
                        }
                    }
                }
                int fromIndex = MST.IndexOf(bestEdge.from);
                MST.Insert(fromIndex + 1, bestEdge.to);
                foreach (Vector2 visit in MST)
                {
                    noVisit.Remove(visit);
                }
                output.Add(bestEdge);
            }
            this.edges = new(output);
        }
        internal static MinSpanTree FromTSPRoute(TSPRoute route)
        {
            return new(route);
        }
    }
    public sealed class OneTree : MinSpanTree
    {
        public OneTree(IEnumerable<Vector2> nodeBase) : base(nodeBase)
        {
        }
        public override void Generate()
        {
            base.Generate();
            ConcurrentDictionary<float, List<(Vector2 from, Vector2 to)>> options = new();
            Parallel.For(0, this.NodeBase.Length, i =>
            {
                List<(Vector2 from, Vector2 to)> option = GenTree(i);
                options.TryAdd(option.Sum(e => Vector2.Distance(e.from, e.to)), option);
            });
            this.edges = new(options[options.Keys.Max()]);

        }
        private List<(Vector2 from, Vector2 to)> GenTree(int exclude)
        {
            List<(Vector2 from, Vector2 to)> output = new(this.edges);
            Vector2 excludedNode = this.NodeBase[exclude];
            output.Remove(output.Find(a => a.from == excludedNode));

            List<Vector2> ByDistToExlcude = new(NodeBase.OrderBy(a => Vector2.Distance(a, excludedNode)));
            output.Add((ByDistToExlcude[0], excludedNode));
            output.Add((excludedNode, ByDistToExlcude[1]));
            return output;
        }
    }
}
