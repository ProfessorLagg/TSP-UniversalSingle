using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Reflection;

using static System.Net.Mime.MediaTypeNames;

namespace TSPStandard
{
    public static class TSPExtentions
    {
        // =========== NUMBERS ===========
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
        public static float Pow(this float value, float power)
        {
            double vd = Convert.ToDouble(value);
            double pd = Convert.ToDouble(power);
            double rd = Math.Pow(vd, pd);
            return Convert.ToSingle(rd);
        }
        // =========== COLLECTIONS ===========
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
        private static string AsString<T>(this T[,] arr)
        {
            List<T[]> values = new List<T[]>();
            int L0 = arr.GetLength(0);
            int L1 = arr.GetLength(1);
            for (int i = 0; i < L0; i++)
            {
                T[] addMe = new T[L1];
                for (int j = 0; j < L1; j++)
                {
                    addMe[j] = arr[i, j];
                }
                values.Add(addMe);
            }

            return values.ToString();
        }
        public static string AsString<T>(this IEnumerable<T[]> enumerable)
        {
            string result = "";
            List<T[]> values = enumerable.ToList();
            int L0 = values.Count;
            int L1 = values.First().Length;
            List<string[]> fields = new();
            int maxStringLength = 0;
            foreach (T[] arr in values)
            {
                string[] addMe = new string[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    string str = arr[i].ToString();
                    addMe[i] = str;
                    maxStringLength = Math.Max(str.Length, maxStringLength);
                }
                fields.Add(addMe);
            }
            List<string> lines = new();
            foreach (string[] strArr in fields)
            {
                string line = "";
                for (int i = 0; i < strArr.Length; i++)
                {
                    int spacerLength = maxStringLength - strArr[i].Length;
                    string spacer = new string(' ', spacerLength + 1);
                    line += strArr[i] + spacer;
                }
                lines.Add(line);
            }

            result = string.Join("\n", lines);
            return result;
        }
        // =========== COST ===========
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
        // =========== Lower Bound and MST ===========
        public static HashSet<(Vector2 v1, Vector2 v2)> GetMST(this IEnumerable<Vector2> vectors)
        {
            Span<Vector2> localvectors = new(vectors.ToArray());
            HashSet<(Vector2 v1, Vector2 v2)> MST = new();
            List<Vector2> noVisit = localvectors.ToArray().ToList();
            List<Vector2> visit = new() { localvectors[0] };
            noVisit.Remove(localvectors[0]);
            while (noVisit.Count > 0)
            {
                Vector2 best_v1 = Vector2.Zero;
                Vector2 best_v2 = Vector2.Zero;
                float bestDist = float.PositiveInfinity;

                Parallel.ForEach(visit, v1 =>
                {
                    foreach (Vector2 v2 in noVisit)
                    {
                        if (Vector2.Distance(v1, v2) < bestDist)
                        {
                            best_v1 = v1;
                            best_v2 = v2;
                            bestDist = Vector2.Distance(v1, v2);
                        }
                    }
                });
                MST.Add((best_v1, best_v2));
                noVisit.Remove(best_v2);
                visit.Add(best_v2);
            }
            return MST;
        }
        public static float GetMSTCost(this IEnumerable<Vector2> vectors)
        {
            float MST_cost = 0;
            Span<Vector2> localVectors = new(vectors.ToArray());
            List<Vector2> noVisit = vectors.ToList();

            Vector2 firstvector = noVisit[0];
            List<Vector2> visit = new() { firstvector };
            noVisit.Remove(firstvector);
            List<float> noVisit_bestDist = noVisit.Select(v => Vector2.Distance(v, firstvector)).ToList();

            Vector2 nextV2 = Vector2.Zero;

            while (noVisit.Count > 0)
            {
                // finding the best edge
                float bestDist = noVisit_bestDist.Min();
                int noVisitIndex = noVisit_bestDist.IndexOf(bestDist);
                nextV2 = noVisit[noVisitIndex];
                MST_cost += bestDist;
                visit.Add(nextV2);
                noVisit.RemoveAt(noVisitIndex);
                noVisit_bestDist.RemoveAt(noVisitIndex);
                for (int i = 0; i < noVisit.Count; i++)
                {
                    if (Vector2.Distance(noVisit[i], nextV2) < noVisit_bestDist[i]) { noVisit_bestDist[i] = Vector2.Distance(noVisit[i], nextV2); }
                }
            }
            return MST_cost;
        }
        public static float GetLowerBound(this IEnumerable<Vector2> vectors)
        {// V4
            Vector2[] localRoute = vectors.ToArray();
            int L = localRoute.Length;
            float[] vertexCosts = new float[L];
            // making ranges
            IEnumerable<Vector2>[] Ranges = Enumerable.Range(0, L).Select(index => Enumerable.Range(0, L).Where(i => i != index).Select(i => localRoute[i])).ToArray();
            // Getting MST Costs
            Task<float>[] tasks_MSTCost = Enumerable.Range(0, L)
                .Select(index =>
                    Task<float>.Run(async () =>
                        Ranges[index].GetMSTCost()
                        )
                    ).ToArray();


            Parallel.For(0, L, index =>
            {
                var range = Ranges[index];
                Vector2 removedVertex = localRoute[index];
                Vector2 firstEdgeEnd = range.MinBy(v => Vector2.Distance(v, removedVertex));
                float vertexCost = Vector2.Distance(removedVertex, firstEdgeEnd);
                Vector2 secondEdgeEnd = range.Where(v => v != firstEdgeEnd).MinBy(v => Vector2.Distance(v, removedVertex));
                vertexCost += Vector2.Distance(removedVertex, secondEdgeEnd);
                vertexCosts[index] = vertexCost;
            });
            float[] oneTreeCosts = new float[L];
            for (int i = 0; i < L; i++)
            {
                tasks_MSTCost[i].Wait();
                oneTreeCosts[i] = tasks_MSTCost[i].Result + vertexCosts[i];

            }

            return oneTreeCosts.Max();
        }
        // =========== Validation ===========
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
        // =========== Assembly ===========
        /// <summary>
        /// Tries to get the build time of the assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="target"></param>
        /// <returns>DateTime of when this assembly was built</returns>
        public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }
    }
}
