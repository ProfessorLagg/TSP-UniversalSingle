using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TSPStandard
{
    public sealed class TSPRoute : IEnumerable<Vector2>
    {
        // Type Data
        private static ConcurrentDictionary<int, float> TourCostMatrix = new();
        private static ConcurrentDictionary<int, float> DistanceMatrix = new();
        // Constructors
        public TSPRoute(string routeName)
        {
            this.Vectors = new();
            this.RouteName = routeName;
        }
        public TSPRoute(string routeName, IEnumerable<Vector2> vectors)
        {
            this.Vectors = new(vectors.ToArray());
            this.RouteName = routeName;
        }
        public TSPRoute(TSPRoute route)
        {
            this.Vectors = new(route.ToArray());
            this.RouteName = route.RouteName;
        }
        // Instance Data
        public string RouteName;
        private List<Vector2> Vectors;
        public int Length
        {
            get
            {
                return Vectors.Count;
            }
        }
        public int TourHash
        {
            get
            {
                return Vectors.CircularNormalize().GetHashCode();
            }
        }
        private int lastHash = 0;
        private float _Cost;
        public float Cost
        {
            get
            {
                if (TourHash != lastHash)
                {
                    return GetCost();
                }
                else
                {
                    return _Cost;
                }

            }
        }
        private float _CostSquared;
        public float CostSquared
        {
            get
            {
                if (TourHash != lastHash)
                {
                    return GetCostSquared();
                }
                else
                {
                    return _CostSquared;
                }

            }
        }
        // Instance Methods
        public void Add(Vector2 element)
        {
            Vectors.Add(element);
        }
        public void AddRange(IEnumerable<Vector2> element)
        {
            Vectors.AddRange(element);
        }
        public void Reverse()
        {
            this.Vectors.Reverse();
        }
        public void CopyFrom(IEnumerable<Vector2> vectors)
        {
            this.Vectors = new(vectors);
        }
        public Vector2 Get(int index)
        {
            return Vectors[index];
        }
        public void Set(int index, Vector2 value)
        {
            this.Vectors[index] = value;
        }
        public float GetCost()
        {
            float output = 0f;
            for (int i = 0; i < Length; i++)
            {
                output += Vector2.Distance(this[i], this[i + 1]);
            }
            _Cost = output;
            lastHash = TourHash;
            return _Cost;
        }
        public float GetCostSquared()
        {
            float output = 0f;
            for (int i = 0; i < Length; i++)
            {
                output += Vector2.DistanceSquared(this[i], this[i + 1]);
            }
            _CostSquared = output;
            lastHash = TourHash;
            return _CostSquared;
        }
        public static TSPRoute FromTSPFile(string path)
        {
            char delim = Convert.ToChar(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
            List<string> lines = File.ReadAllLines(path).ToList();

            string name = lines.Find(x => x.Contains("NAME :")).Replace("NAME :", "").Trim();
            TSPRoute output = new(name);
            int currentIndex = lines.FindIndex(x => x.Contains("NODE_COORD_SECTION")) + 1;
            string currentLine = lines[currentIndex];
            do
            {
                string[] split = currentLine.Split(" ");
                string xString = split[split.Length - 2].Replace('.', delim);
                string yString = split[split.Length - 1].Replace('.', delim);
                output.Add(new Vector2(float.Parse(xString), float.Parse(yString)));
                currentIndex++;
                currentLine = lines[currentIndex].Trim();
            } while (!currentLine.Contains("EOF"));
            return output;
        }
        public void SaveAsXMLFile(string path)
        {
            SaveAsXMLFile(path, this.Vectors.GetLowerBound());
        }
        public void SaveAsXMLFile(string path, float lowerBound)
        {
            XDocument xml = new(new XElement("TSPSet"));
            xml.Root.Add(new XElement("name", this.RouteName));
            xml.Root.Add(new XElement("dimension", this.Length.ToString()));
            xml.Root.Add(new XElement("lowerBound", lowerBound));
            xml.Root.Add(new XElement("permutationCost", this.Cost));
            XElement pointsElement = new("points");
            for (int i = 0; i < this.Length; i++)
            {
                Vector2 vec = Vectors[i];
                pointsElement.Add(new XElement("point",
                    new XAttribute("index", (i + 1).ToString()),
                    new XAttribute("x", vec.X),
                    new XAttribute("y", vec.Y)
                    ));
            }
            xml.Root.Add(pointsElement);

            xml.Save(path);
        }
        public static TSPRoute FromXMLFile(string path)
        {
            float trashcan = 0;
            return FromXMLFile(path, out trashcan);
        }
        public static TSPRoute FromXMLFile(string path, out float lowerBound)
        {
            char delim = Convert.ToChar(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
            XDocument xml = XDocument.Load(path);
            var root = xml.Root;

            string RouteName = root.XPathSelectElement("//name").Value;
            string lowBoundString = root.XPathSelectElement("//lowerBound").Value;
            lowerBound = float.Parse(lowBoundString.Replace('.', delim));
            List<Vector2> Points = new();
            var pointElements = root.XPathSelectElements("//points/point");
            foreach (var pointElement in pointElements)
            {
                float x = float.Parse(pointElement.Attribute("x").Value.Replace('.', delim));
                float y = float.Parse(pointElement.Attribute("y").Value.Replace('.', delim));
                Points.Add(new Vector2(x, y));
            }
            return new TSPRoute(RouteName, Points);
        }
        // Overrides, indexers and operators
        public Vector2 this[int i]
        {

            get { return Vectors[i % Length]; }
            set { Vectors[i % Length] = value; }
        }
        public IEnumerator<Vector2> GetEnumerator()
        {
            foreach (Vector2 val in this.Vectors)
            {
                yield return val;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public static class Extentions
    {
        public static Vector2[] CircularNormalize(this IEnumerable<Vector2> points)
        {
            List<Vector2> nodes = new(points);
            Vector2[] output = new Vector2[nodes.Count];
            int minIndex = nodes.FindIndex(a => a.LengthSquared() == nodes.Min(x => x.LengthSquared()));
            for (int i = 0; i < nodes.Count; i++)
            {
                //int findIndex = (i+ minLengthIndex) % nodes.Count;
                output[i] = nodes[(i + minIndex) % nodes.Count];
            }
            return output;
        }
        public static int GetHashCodePair(this Vector2 pointA, Vector2 pointB)
        {
            int hashA = pointA.GetHashCode();
            int hashB = pointB.GetHashCode();
            return HashCode.Combine(Math.Max(hashA, hashB), Math.Min(hashA, hashB));
        }
    }
}