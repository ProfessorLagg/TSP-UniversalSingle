using System.Diagnostics;
using System.Numerics;
using TSPStandard;
using System.IO;
using System.Reflection;
using TSPStandard.Algorithm;

namespace TSP_UniversalSingle
{
    internal sealed class Program
    {
        public static void ToLog(string message)
        {
            string writeMe = DateTime.Now.ToString("G") + "\t" + message;
            Console.WriteLine(writeMe);
        }
        static readonly string? EXERoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
            string Folder_Assets;
            string Folder_sets;
            List<string> Files_sets;
            string[] fullNames;
            try
            {
                Folder_Assets = Path.Combine(EXERoot, "Assets");
                Folder_sets = Path.Combine(Folder_Assets, "sets");
                Files_sets = new();
                fullNames = Directory.GetFiles(Folder_sets, "*.tsp");
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
                Files_sets = new();
                fullNames = Directory.GetFiles(Folder_sets, "*.tsp");
                foreach (string file in fullNames)
                {
                    var info = new FileInfo(file);
                    Files_sets.Add(info.Name);
                }
            }

            // Parsing set file name
            string setName;
            if (args.Length > 0 && Files_sets.Contains(args[0])) { setName = args[0]; PauseWhenFinished = false; } // Use the specified set
            else
            {
                // Ask user about which set to use
                int L = Files_sets.Count;
                int log10L = (int)Math.Log10(L);
                string format = new string('0', log10L) + '0';

                for (int i = 0; i < L; i++)
                {
                    Console.WriteLine(i.ToString(format) + ". " + Files_sets[i]);
                }
                Console.WriteLine("\n");
                var baseLine = Console.GetCursorPosition();
                int selection = -1;
                while(selection < 0 || selection >= L)
                {
                    Console.SetCursorPosition(baseLine.Left,baseLine.Top);
                    Console.WriteLine("Please pick a set:");
                    string eraser = new(' ', Console.BufferWidth);
                    Console.WriteLine(eraser);
                    Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
                    string? input = Console.ReadLine();
                    if(!int.TryParse(input, out selection))
                    {
                        selection = -1;
                    }
                }
                setName = Files_sets[selection];
            }

            
            Console.Clear();
            // Loading set
            Console.WriteLine("Loading set: " + setName);
            string setPath = Path.Combine(Folder_sets, setName);
            BestFound = TSPRoute.FromFile(setPath);
            // Calculating Lower Bound for this set
            OneTree MST = new(BestFound);
            MST.Generate();
            LowerBound = MST.Cost;
            Console.WriteLine("Finished loading set\n");
            // Priting set info
            Console.WriteLine("Name:        " + setName);
            Console.WriteLine("Length:      " + BestFound.Length.ToString());
            Console.WriteLine("Lower Bound: " + LowerBound.ToString("n"));
            // Running sequence
            RunNearestNeighbor();
            RunTwoOpt();
            RunSwap();

            // Creating output folder
            string Path_OutputFolder;
            string fileName = DateTime.Now.ToString(@"yyyyMMdd-HHmmss") + "_" + BestFound.RouteName + ".tsp";
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
            BestFound.SaveAsTSPFile(Path_OutputFile);
            Console.WriteLine("Saved output to: " + Path_OutputFile);
            if (PauseWhenFinished)
            {
                Console.WriteLine("\n\n Press any key to exit...");
                Console.ReadKey();
            }
            

        }
        static void RunNearestNeighbor()
        {
            NearestNeighbor algorithm = new(BestFound);
            Console.WriteLine("\n----------- NEAREST NEIGHBOR -----------");
            decimal cost_pre = Convert.ToDecimal(BestFound.Cost);
            DateTime startTime = DateTime.Now;
            Console.WriteLine("TimeStamp:     " + startTime.ToString());
            Console.WriteLine("Current Cost:  " + BestFound.Cost.ToString("n"));
            Console.WriteLine("Current Score: " + CurrentScore.ToString("n"));
            algorithm.Run();
            DateTime endTime = DateTime.Now;
            if (algorithm.Route.Cost < BestFound.Cost)
            {
                BestFound = new(algorithm.Route);
            }
            TimeSpan runTime = endTime - startTime;
            decimal cost_post = Convert.ToDecimal(BestFound.Cost);
            decimal costDiff = (decimal)1 - (cost_post / cost_pre);
            Console.WriteLine("--- RESULTS ---");
            Console.WriteLine("TimeStamp:     " + endTime.ToString());
            Console.WriteLine("Current Cost:  " + BestFound.Cost.ToString("n"));
            Console.WriteLine("Improvement:   " + costDiff.ToString("P"));
            Console.WriteLine("Run Time:      " + runTime);
            Console.WriteLine("Current Score: " + CurrentScore.ToString("n"));
            Console.WriteLine("----------- NEAREST NEIGHBOR -----------\n");
        }
        static void RunTwoOpt()
        {
            TwoOpt algorithm = new(BestFound);
            Console.WriteLine("\n----------- TWO-OPT -------------------");
            decimal cost_pre = Convert.ToDecimal(BestFound.Cost);
            DateTime startTime = DateTime.Now;
            Console.WriteLine("TimeStamp:     " + startTime.ToString());
            Console.WriteLine("Current Cost:  " + BestFound.Cost.ToString("n"));
            Console.WriteLine("Current Score: " + CurrentScore.ToString("n"));
            algorithm.Run();
            DateTime endTime = DateTime.Now;
            if (algorithm.Route.Cost < BestFound.Cost)
            {
                BestFound = new(algorithm.Route);
            }
            TimeSpan runTime = endTime - startTime;
            decimal cost_post = Convert.ToDecimal(BestFound.Cost);
            decimal costDiff = (decimal)1 - (cost_post / cost_pre);
            Console.WriteLine("--- RESULTS ---");
            Console.WriteLine("TimeStamp:     " + endTime.ToString());
            Console.WriteLine("Current Cost:  " + BestFound.Cost.ToString("n"));
            Console.WriteLine("Improvement:   " + costDiff.ToString("P"));
            Console.WriteLine("Run Time:      " + runTime);
            Console.WriteLine("Current Score: " + CurrentScore.ToString("n"));
            Console.WriteLine("----------- TWO-OPT -------------------\n");
        }

        static void RunSwap()
        {
            Swap algorithm = new(BestFound);
            Console.WriteLine("\n----------- SWAP ----------------------");
            decimal cost_pre = Convert.ToDecimal(BestFound.Cost);
            DateTime startTime = DateTime.Now;
            Console.WriteLine("TimeStamp:     " + startTime.ToString());
            Console.WriteLine("Current Cost:  " + BestFound.Cost.ToString("n"));
            Console.WriteLine("Current Score: " + CurrentScore.ToString("n"));
            algorithm.Run();
            DateTime endTime = DateTime.Now;
            if (algorithm.Route.Cost < BestFound.Cost)
            {
                BestFound = new(algorithm.Route);
            }
            TimeSpan runTime = endTime - startTime;
            decimal cost_post = Convert.ToDecimal(BestFound.Cost);
            decimal costDiff = (decimal)1 - (cost_post / cost_pre);
            Console.WriteLine("--- RESULTS ---");
            Console.WriteLine("TimeStamp:     " + endTime.ToString());
            Console.WriteLine("Current Cost:  " + BestFound.Cost.ToString("n"));
            Console.WriteLine("Improvement:   " + costDiff.ToString("P"));
            Console.WriteLine("Run Time:      " + runTime);
            Console.WriteLine("Current Score: " + CurrentScore.ToString("n"));
            Console.WriteLine("----------- SWAP ----------------------\n");
        }



    }
}