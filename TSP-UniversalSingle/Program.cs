using System.Diagnostics;
using System.Numerics;
using TSPStandard;
using System.IO;
using System.Reflection;
using TSPStandard.Algorithm;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace TSP_UniversalSingle
{
    internal sealed class Program
    {
        public static void ToLog(string message)
        {
            string writeMe = DateTime.Now.ToString("G") + "\t" + message;
            Console.WriteLine(writeMe);
        }
        static string? EXERoot
        {
            get
            {
                if (Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) is not null) { return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); }
                else { return System.AppContext.BaseDirectory.ToString(); }
            }
        }

        private static TSPRoute? BestFound;
        private static float LowerBound;
        private static bool PauseWhenFinished = true;
        private static float CurrentScore
        {
            get
            {
                float score = BestFound.Cost / LowerBound;
                return score;
            }
        }
        static void Main(string[] args)
        {
            // Parsing folders and loading set file names
            List<string> argsList = new(args);
            string Folder_Assets;
            string Folder_sets;
            string Folder_setsXML;
            List<string> Files_sets;
            List<string> fullNames = new();
            try
            {
                Folder_Assets = Path.Combine(EXERoot, "Assets");
                Folder_sets = Path.Combine(Folder_Assets, "sets");
                Folder_setsXML = Path.Combine(Folder_Assets, "setsXML");
                Files_sets = new();
                fullNames.AddRange(Directory.GetFiles(Folder_sets, "*.tsp"));
                fullNames.AddRange(Directory.GetFiles(Folder_setsXML, "*.xml"));
                foreach (string file in fullNames)
                {
                    var info = new FileInfo(file);
                    Files_sets.Add(info.Name);
                }
            }
            catch (System.ArgumentNullException)
            {
                Folder_Assets = @".\Assets";
                Folder_sets = Path.Combine(Folder_Assets, "sets");
                Folder_setsXML = Path.Combine(Folder_Assets, "setsXML");
                Files_sets = new();
                fullNames.AddRange(Directory.GetFiles(Folder_sets, "*.tsp"));
                fullNames.AddRange(Directory.GetFiles(Folder_setsXML, "*.xml"));
                foreach (string file in fullNames)
                {
                    var info = new FileInfo(file);
                    Files_sets.Add(info.Name);
                }
            }
            // Parsing set file name
            string setName;
            string setParam = argsList.Find(arg => arg.Contains("-set:"));
            if (!string.IsNullOrEmpty(setParam)) { setName = setParam.Replace("-set:", "").Trim(); PauseWhenFinished = false; } // Use the specified set
            else
            {
                
                List<string> distincFileSets = Files_sets.Where(fileSet => fileSet.Contains(".xml")).ToList();
                var tspFileSets = Files_sets.Where(fileSet => fileSet.Contains(".tsp"));
                foreach (var tspFileSet in tspFileSets)
                {
                    if (!distincFileSets.Contains(tspFileSet.Replace(".tsp", ".xml")))
                    {
                        distincFileSets.Add(tspFileSet);
                    }
                }
                Regex rgx = new(@"[^0-9]");
                distincFileSets = distincFileSets.OrderBy(nameStr => int.Parse(rgx.Replace(nameStr, ""))).ToList();
                int L = distincFileSets.Count;
                int log10L = (int)Math.Log10(L);
                string format = new string('0', log10L) + '0';

                for (int i = 0; i < L; i++)
                {
                    Console.WriteLine(i.ToString(format) + ". " + distincFileSets[i]);
                }
                Console.WriteLine("\n");
                var (Left, Top) = Console.GetCursorPosition();
                int selection = -1;
                while(selection < 0 || selection >= L)
                {
                    Console.SetCursorPosition(Left,Top);
                    Console.WriteLine("Please pick a set:");
                    string eraser = new(' ', Console.BufferWidth);
                    Console.WriteLine(eraser);
                    Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
                    // Ask user about which set to use
                    string? input = Console.ReadLine();
                    if(!int.TryParse(input, out selection))
                    {
                        selection = -1;
                    }
                }
                setName = distincFileSets[selection];
            }
            Console.Clear();
            // Loading set
            ToLog("Starting loading set: " + setName);
            if (setName.Contains(".tsp"))
            {
                string setPath = Path.Combine(Folder_sets, setName);
                BestFound = TSPRoute.FromTSPFile(setPath);
                CalculateLowerBound(); // Since TSP file format doesnt include pre-computed lowerBound, i have to calc it
            }
            else if (setName.Contains(".xml"))
            {
                string setPath = Path.Combine(Folder_setsXML, setName);
                BestFound = TSPRoute.FromXMLFile(setPath,out LowerBound);
            }
            ToLog("Finished loading set: " + setName + "\n");
            // Priting set info
            ToLog("Name:        " + setName);
            ToLog("Length:      " + BestFound.Length.ToString());
            ToLog("Lower Bound: " + LowerBound.ToString("n"));
            // Parsing Level argument

            // Running sequence
            string seqParam = argsList.Find(arg => arg.Contains("-seq:"));
            if (string.IsNullOrEmpty(seqParam))
            {
                // Run default
                RunUniversal(new InsertionBuild(BestFound));
                RunUniversal(new SliceWindowBruteForce(BestFound));
            }
            else
            {
                // Parse sequence
                string[] SequenceStrings = seqParam.Replace("-seq:", "").Trim().Split(',');
                foreach(string seqString in SequenceStrings)
                {
                    string seqName = seqString.Trim().ToUpper();
                    switch(seqName)
                    {
                        default: break;
                        // Long name
                        case "INSERTIONBUILD": RunUniversal(new InsertionBuild(BestFound)); break;
                        case "NEARESTNEIGHBOR": RunUniversal(new InsertionBuild(BestFound)); break;
                        case "TWOOPT": RunUniversal(new TwoOpt(BestFound)); break;
                        case "SWAP": RunUniversal(new Swap(BestFound)); break;
                        case "SLICEWINDOWBRUTEFORCE": RunUniversal(new SliceWindowBruteForce(BestFound,false)); break;
                        case "SLICEWINDOWBRUTEFORCERECURSIVE": RunUniversal(new SliceWindowBruteForce(BestFound,true)); break;
                        // Short name
                        case "INB": goto case "INSERTIONBUILD";
                        case "NN": goto case "NEARESTNEIGHBOR";
                        case "2OP": goto case "TWOOPT";
                        case "SWP": goto case "SWAP";
                        case "SWB": goto case "SLICEWINDOWBRUTEFORCE";
                        case "SWR": goto case "SLICEWINDOWBRUTEFORCERECURSIVE";
                        //Aliases
                        case "2OPT": goto case "TWOOPT";
                        case "2-OPT": goto case "TWOOPT";
                        case "SLICEWINDOW": goto case "SLICEWINDOWBRUTEFORCE";
                        case "SLICEWINDOWRECURSE": goto case "SLICEWINDOWBRUTEFORCERECURSIVE";
                    }
                }


            }
            

            // Creating output folder
            string Path_OutputFolder;
            string fileName = DateTime.Now.ToString(@"yyyyMMdd-HHmmss") + "_" + BestFound.RouteName + ".xml";
            string Path_OutputFile;
            try
            {
                Path_OutputFolder = Path.Combine(EXERoot, "Output");
                Path_OutputFile = Path.Combine(Path_OutputFolder, fileName);
            }
            catch (System.ArgumentNullException)
            {
                Path_OutputFolder = @".\Output";
                Path_OutputFile = Path.Combine(Path_OutputFolder, fileName);
            }
            if (!Directory.Exists(Path_OutputFolder)) { Directory.CreateDirectory(Path_OutputFolder); }
            //SAVE FILE
            BestFound.SaveAsXMLFile(Path_OutputFile,LowerBound);
            Console.WriteLine("Saved output to: " + Path_OutputFile);
            if (PauseWhenFinished)
            {
                Console.WriteLine("\n\n Press any key to exit...");
                Console.ReadKey();
            }
        }
        static int MaxAlgNameLength;
        static string GetBanner(TSPAlgorithm algorithm)
        {
            if(MaxAlgNameLength < 1)
            {
                foreach(string algName in Enum.GetNames(typeof(AlgorithmType))) { MaxAlgNameLength = Math.Max(algName.Length, MaxAlgNameLength); }
            }

            int AlgNameLength = Enum.GetName(algorithm.AlgorithmType).Length;
            double DashLength = Convert.ToDouble(12 + MaxAlgNameLength - AlgNameLength) / 2.0;
            int LeftDashes = Convert.ToInt32(Math.Floor(DashLength));
            int RightDashes = Convert.ToInt32(Math.Ceiling(DashLength));
            return (new string('-', LeftDashes)) + ' ' + Enum.GetName(algorithm.AlgorithmType) + ' ' + (new string('-', RightDashes));
        }
        static void RunUniversal(TSPAlgorithm algorithm)
        {

            Console.WriteLine("\n" + GetBanner(algorithm));
            decimal cost_pre = Convert.ToDecimal(BestFound.Cost);
            DateTime startTime = DateTime.Now;
            Console.WriteLine("TimeStamp:     " + startTime.ToString());
            Console.WriteLine("Current Cost:  " + BestFound.Cost.ToString("n"));
            Console.WriteLine("Current Score: " + CurrentScore.ToString("n"));
            algorithm.Run();
            DateTime endTime = DateTime.Now;
            if (!algorithm.Route.ValidateTSPSet(BestFound)) { throw new Exception("ALGORITHM WAS NOT VALID."); }
            if (algorithm.Route.Cost < BestFound.Cost)
            {
                BestFound = new(algorithm.Route);
            }
            TimeSpan runTime = endTime - startTime;
            decimal cost_post = Convert.ToDecimal(BestFound.Cost);
            decimal costDiff = (decimal)1.0 - (cost_post / cost_pre);
            Console.WriteLine("--- RESULTS ---");
            Console.WriteLine("TimeStamp:     " + endTime.ToString());
            Console.WriteLine("Current Cost:  " + BestFound.Cost.ToString("n"));
            Console.WriteLine("Improvement:   " + costDiff.ToString("P"));
            Console.WriteLine("Run Time:      " + runTime);
            Console.WriteLine("Current Score: " + CurrentScore.ToString("n"));
            Console.WriteLine(GetBanner(algorithm) + "\n");
        }
        static void CalculateLowerBound()
        {
            // Calculating Lower Bound for this set
            OneTree MST = new(BestFound);
            MST.Generate();
            LowerBound = MST.Cost;
        }
    }
}