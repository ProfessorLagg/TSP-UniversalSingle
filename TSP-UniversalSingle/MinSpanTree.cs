using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

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
            ConcurrentDictionary<(Vector2 from, Vector2 to),float> DistanceMatrix = new();
            Func<(Vector2 from, Vector2 to),float> edgeWeight = a => DistanceMatrix.GetOrAdd(a,Vector2.Distance(a.from,a.to));
            ConcurrentDictionary<float, (Vector2 from, Vector2 to)> edgeOptions = new();
            while(noVisit.Count > 0)
            {
                edgeOptions.Clear();
                (Vector2 from, Vector2 to) bestEdge = new();
                float bestWeight = float.PositiveInfinity;
                foreach (Vector2 Unvisited in noVisit)
                {
                    foreach(Vector2 Visited in MST)
                    {

                        (Vector2 from, Vector2 to) edgeA = new(Visited, Unvisited);
                        float weight = DistanceMatrix.GetOrAdd(edgeA, Vector2.Distance(edgeA.from, edgeA.to));
                        if(weight < bestWeight)
                        {
                            bestEdge = edgeA;
                            bestWeight = weight;
                        }
                    }
                }
                

                int fromIndex = MST.IndexOf(bestEdge.from);
                MST.Insert(fromIndex + 1, bestEdge.to);
                foreach(Vector2 visit in MST)
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
        private List<(Vector2 from, Vector2 to)> GenTreeV0(int exclude)
        {
            List<Vector2> newBase = new(NodeBase);
            Vector2 excluded = NodeBase[0];            
            newBase.RemoveAt(exclude);
            GenMST(newBase);
            List<(Vector2 from, Vector2 to)> edgesOutput = new();

            // get the from -> excluded point edge
            float bestFromWeight = float.PositiveInfinity;
            (Vector2 from, Vector2 to) bestFromEdge = new();
            List<Vector2> noCheck = new(newBase);
            foreach (Vector2 node in noCheck)
            {
                float nodeWeight = Vector2.Distance(node, excluded);
                if (nodeWeight < bestFromWeight)
                {
                    bestFromWeight = nodeWeight; bestFromEdge = new(node, excluded);
                }
            }
            noCheck.Remove(bestFromEdge.from);
            edgesOutput.Add(bestFromEdge);

            // Get the excluded point -> to edge
            float bestToWeight = float.PositiveInfinity;
            (Vector2 from, Vector2 to) bestToEdge = new();
            foreach (Vector2 node in noCheck)
            {
                float nodeWeight = Vector2.Distance(node, excluded);
                if (nodeWeight < bestToWeight)
                {
                    bestToWeight = nodeWeight; bestToEdge = new(excluded, node);
                }
            }
            edgesOutput.Add(bestToEdge);

            return edgesOutput;
        }
        private List<(Vector2 from, Vector2 to)> GenTreeV1(int exclude)
        {
            if(this.edges.Count == 0)
            {
                base.Generate();
            }

            List<(Vector2 from, Vector2 to)> output = new(this.edges);
            Vector2 excludedEdge = output[exclude].from; // getting the from point, which is unique for the edges, from the excluded vertex
            output.RemoveAt(exclude);
            Dictionary<int, Vector2> temp = new();
            output.ForEach(a => temp.TryAdd(a.from.GetHashCode(), a.from));
            output.ForEach(a => temp.TryAdd(a.to.GetHashCode(), a.to));
            List<Vector2> localNodeBase = new(temp.Values);
            

            // getting (vectorInOutput -> excludedEdge)
            Vector2 nearestVector = new();
            float nearestWeight = float.NegativeInfinity;
            foreach(Vector2 edge in localNodeBase)
            {
                float thisWeight = (edge, excludedEdge).Wieght();
                if(thisWeight < nearestWeight)
                {
                    nearestWeight = thisWeight;
                    nearestVector = edge;
                }
            }
            output.Add((nearestVector, excludedEdge));
            localNodeBase.Remove(nearestVector);

            // getting (excludedEdge -> vectorInOutput)
            nearestVector = new();
            nearestWeight = float.NegativeInfinity;
            foreach (Vector2 edge in localNodeBase)
            {
                float thisWeight = (edge, excludedEdge).Wieght();
                if (thisWeight < nearestWeight)
                {
                    nearestWeight = thisWeight;
                    nearestVector = edge;
                }
            }
            output.Add((excludedEdge, nearestVector));
            return output;
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
        private List<(Vector2 from, Vector2 to)> GenMST(IEnumerable<Vector2> vectors)
        {
            List<Vector2> noVisit = new(vectors);
            List<Vector2> MST = new();
            List<(Vector2 from, Vector2 to)> output = new();
            MST.Add(noVisit[0]);
            noVisit.RemoveAt(0);
            ConcurrentDictionary<(Vector2 from, Vector2 to), float> DistanceMatrix = new();
            Func<(Vector2 from, Vector2 to), float> edgeWeight = a => DistanceMatrix.GetOrAdd(a, Vector2.Distance(a.from, a.to));
            ConcurrentDictionary<float, (Vector2 from, Vector2 to)> edgeOptions = new();
            while (noVisit.Count > 0)
            {
                edgeOptions.Clear();
                (Vector2 from, Vector2 to) bestEdge = new();
                float bestWeight = float.PositiveInfinity;
                foreach (Vector2 Unvisited in noVisit)
                {
                    foreach (Vector2 Visited in MST)
                    {

                        (Vector2 from, Vector2 to) edgeA = new(Visited, Unvisited);
                        float weight = DistanceMatrix.GetOrAdd(edgeA, Vector2.Distance(edgeA.from, edgeA.to));
                        if (weight < bestWeight)
                        {
                            bestEdge = edgeA;
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
            return output;
        }
    }
}
