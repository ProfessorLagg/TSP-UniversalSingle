﻿using System.Diagnostics;
using System.Numerics;
using TSPStandard;
using System.IO;
using System.Reflection;
using TSPStandard.Algorithm;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
                var (Left, Top) = Console.GetCursorPosition();
                int selection = -1;
                while(selection < 0 || selection >= L)
                {
                    Console.SetCursorPosition(Left,Top);
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
            ToLog("Starting loading set: " + setName);
            string setPath = Path.Combine(Folder_sets, setName);
            BestFound = TSPRoute.FromFile(setPath);
            // Calculating Lower Bound for this set
            CalculateLowerBound();
            ToLog("Finished loading set: " + setName + "\n");
            // Priting set info
            ToLog("Name:        " + setName);
            ToLog("Length:      " + BestFound.Length.ToString());
            ToLog("Lower Bound: " + LowerBound.ToString("n"));
            // Running sequence
            RunUniversal(new NearestNeighbor(BestFound));
            RunUniversal(new TwoOpt(BestFound));
            RunUniversal(new Swap(BestFound));

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