using System.Numerics;

namespace TSPStandard
{
    public static class TSPExtentions
    {
        // Numbers
        // To map a value x from range: [a..b] to range [a'..b']
        // x' = ((x - a) / (b - a)) * (b' - a') + a'
        public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
        public static double Map(this double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
        public static float Map(this float value, float rangeFromStart, float rangeFromEnd, float rangeToStart, float rangeToEnd)
        {
            float xn = ((value - rangeFromStart) / (rangeFromEnd - rangeFromStart)) * (rangeToEnd - rangeToStart) + rangeToStart;
            return xn;
            //return ((value - rangeFromStart) / (rangeFromEnd - rangeFromStart)) * ((rangeToEnd - rangeToStart) + rangeToStart;
        }
        public static float Sign(this float value)
        {
            return value / Math.Abs(value);
        }
        // Collections
        public static int[] CircularNormalize(this IEnumerable<int> integers)
        {
            List<int> nodes = new(integers);
            int[] output = new int[nodes.Count];
            int minIndex = nodes.IndexOf(nodes.Min());
            for (int i = 0; i < nodes.Count; i++)
            {
                output[i] = nodes[(i + minIndex) % nodes.Count];
            }
            return output;
        }
        public static float Wieght(this (Vector2 from, Vector2 to) edge)
        {
            return Vector2.Distance(edge.from, edge.to);
        }
        public static List<float> X(this IEnumerable<Vector2> vectors)
        {
            List<float> output = new();
            foreach (Vector2 vec in vectors)
            {
                output.Add(vec.X);
            }
            return output;
        }
        public static List<float> Y(this IEnumerable<Vector2> vectors)
        {
            List<float> output = new();
            foreach (Vector2 vec in vectors)
            {
                output.Add(vec.Y);
            }
            return output;
        }
        // Cost
        public static float Cost(this IEnumerable<Vector2> tour)
        {
            return new ReadOnlySpan<Vector2>(tour.ToArray()).Cost();
        }
        public static float Cost(this Span<Vector2> tour)
        {
            float output = 0;
            for (int i = 0; i < tour.Length; i++)
            {
                output += Vector2.Distance(tour[i], tour[(i + 1) % tour.Length]);
            }
            return output;
        }
        public static float Cost(this Memory<Vector2> tour)
        {
            Span<Vector2> temp = tour.ToArray();
            float output = 0;
            for (int i = 0; i < tour.Length; i++)
            {
                output += Vector2.Distance(temp[i], temp[(i + 1) % temp.Length]);
            }
            return output;
        }
        public static float Cost(this ReadOnlySpan<Vector2> tour)
        {
            float output = 0;
            for (int i = 0; i < tour.Length; i++)
            {
                output += Vector2.Distance(tour[i], tour[(i + 1) % tour.Length]);
            }
            return output;
        }
        public static float CostSquared(this IEnumerable<Vector2> tour)
        {
            return new ReadOnlySpan<Vector2>(tour.ToArray()).CostSquared();
        }
        public static float CostSquared(this Span<Vector2> tour)
        {
            float output = 0;
            for (int i = 0; i < tour.Length; i++)
            {
                output += Vector2.DistanceSquared(tour[i], tour[(i + 1) % tour.Length]);
            }
            return output;
        }
        public static float CostSquared(this Memory<Vector2> tour)
        {
            Span<Vector2> temp = tour.ToArray();
            float output = 0;
            for (int i = 0; i < tour.Length; i++)
            {
                output += Vector2.DistanceSquared(temp[i], temp[(i + 1) % temp.Length]);
            }
            return output;
        }
        public static float CostSquared(this ReadOnlySpan<Vector2> tour)
        {
            float output = 0;
            for (int i = 0; i < tour.Length; i++)
            {
                output += Vector2.DistanceSquared(tour[i], tour[(i + 1) % tour.Length]);
            }
            return output;
        }
        public static float TourLength(this IEnumerable<Vector2> tour)
        {
            return new ReadOnlySpan<Vector2>(tour.ToArray()).TourLength();
        }
        public static float TourLength(this Span<Vector2> tour)
        {
            return new ReadOnlySpan<Vector2>(tour.ToArray()).TourLength();
        }
        public static float TourLength(this Memory<Vector2> tour)
        {
            return new ReadOnlySpan<Vector2>(tour.ToArray()).TourLength();
        }
        public static float TourLength(this ReadOnlySpan<Vector2> tour)
        {
            float output = 0;
            for (int i = 0; i < tour.Length; i++)
            {
                output += Vector2.Distance(tour[i], tour[(i + 1) % tour.Length]);
            }
            return output;
        }
        public static float TourLengthSquared(this IEnumerable<Vector2> tour)
        {
            return new ReadOnlySpan<Vector2>(tour.ToArray()).TourLengthSquared();
        }
        public static float TourLengthSquared(this Span<Vector2> tour)
        {
            return new ReadOnlySpan<Vector2>(tour.ToArray()).TourLengthSquared();
        }
        public static float TourLengthSquared(this Memory<Vector2> tour)
        {
            return new ReadOnlySpan<Vector2>(tour.ToArray()).TourLengthSquared();
        }
        public static float TourLengthSquared(this ReadOnlySpan<Vector2> tour)
        {
            float output = 0;
            for (int i = 0; i < tour.Length; i++)
            {
                output += Vector2.DistanceSquared(tour[i], tour[(i + 1) % tour.Length]);
            }
            return output;
        }
        // same set
        /// <summary>
        /// Validates a permutation of a TSP set against another permutaion
        /// </summary>
        /// <param name="set">The set to be tested</param>
        /// <param name="correctSet">A known valid set</param>
        /// <returns>true if set is valid, else false</returns>
        public static bool ValidateTSPSet(this IEnumerable<Vector2> set, IEnumerable<Vector2> correctSet)
        {
            if (set.Count() == correctSet.Count())
            {
                var sortedSet = set.OrderBy(v => v.X).ThenBy(v => v.Y).ToArray();
                var sortedCorrectSet = correctSet.OrderBy(v => v.X).ThenBy(v => v.Y).ToArray();
                for (int i = 0; i < sortedSet.Length; i++)
                {
                    if (sortedSet[i] != sortedCorrectSet[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }


        }
    }
}
