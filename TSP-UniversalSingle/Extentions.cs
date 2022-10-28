using System.Numerics;

namespace TSPStandard
{
    public static class ImagingExtentions
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
            Vector2[] tempRoute = tour.ToArray();
            float output = 0;
            int L = tempRoute.Length;
            for (int i = 0; i < L; i++)
            {
                output += Vector2.Distance(tempRoute[i], tempRoute[(i + 1) % L]);
            }
            return output;
        }
        public static float CostSquared(this IEnumerable<Vector2> tour)
        {
            Vector2[] tempRoute = tour.ToArray();
            float output = 0;
            int L = tempRoute.Length;
            for (int i = 0; i < L; i++)
            {
                output += Vector2.DistanceSquared(tempRoute[i], tempRoute[(i + 1) % L]);
            }
            return output;
        }
        /// <summary>
        /// Cost that doesnt loop
        /// </summary>
        /// <param name="tour"></param>
        /// <returns></returns>
        public static float TourLength(this IEnumerable<Vector2> tour)
        {
            Vector2[] tempRoute = tour.ToArray();
            float output = 0;
            int L = tempRoute.Length - 1;
            for (int i = 0; i < L; i++)
            {
                output += Vector2.Distance(tempRoute[i], tempRoute[(i + 1) % L]);
            }
            return output;
        }
        /// <summary>
        /// CostSquared that doesnt loop
        /// </summary>
        /// <param name="tour"></param>
        /// <returns></returns>
        public static float TourLengthSquared(this IEnumerable<Vector2> tour)
        {
            Vector2[] tempRoute = tour.ToArray();
            float output = 0;
            int L = tempRoute.Length - 1;
            for (int i = 0; i < L; i++)
            {
                output += Vector2.DistanceSquared(tempRoute[i], tempRoute[(i + 1) % L]);
            }
            return output;
        }
        public static float Cost(this Vector2[] tour)
        {
            ReadOnlySpan<Vector2> span = new(tour);
            return span.Cost();
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
        // same set
        public static bool ValidateTSPSet(this IEnumerable<Vector2> set, IEnumerable<Vector2> correctSet)
        {
            bool result = set.Count() == correctSet.Count(); // Checking size
            if (!result)
            {
                List<Vector2> setList = set.Distinct().ToList(); // removing duplicates
                result = result && setList.Count() == set.Count(); // checking if duplicates where found

                if (!result)
                {
                    setList.ForEach(x => result = result && correctSet.Contains(x)); // checking that all the points are the same across both collections
                }
            }
            return result;
        }
    }
}
