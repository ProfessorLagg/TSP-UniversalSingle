using System.Diagnostics;

namespace TSPStandard
{
    public sealed class PermutationGenerator
    {
        private static string _ExePath;
        private static string ExePath
        {
            get
            {
                if (string.IsNullOrEmpty(_ExePath))
                {
                    if(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) is not null) { _ExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); }
                    else if(System.AppContext.BaseDirectory is not null) { _ExePath = System.AppContext.BaseDirectory.ToString(); }
                }
                return _ExePath;
            }
        }
        //System.AppContext.BaseDirectory
        private string PermFolderPath = Path.Combine(ExePath, "Assets", "permutations");
        private Dictionary<int, string> PermutationSetFilePaths;
        private static ConcurrentDictionary<int, List<int[]>> PermutationSetRamCache = new();
        public static bool UseRamCache = false;
        public bool BuildFileCache = false;
        private void EnumeratePermutationSets()
        {
            PermutationSetFilePaths = new();
            if (!Directory.Exists(PermFolderPath)) { Directory.CreateDirectory(PermFolderPath); }
            List<string> filePaths = new(Directory.GetFiles(PermFolderPath));
            var permFiles = filePaths.Where(p => Path.GetExtension(p) == ".perm");

            foreach (string filePath in permFiles)
            {
                int length = Convert.ToInt32(Path.GetFileNameWithoutExtension(filePath));
                PermutationSetFilePaths.TryAdd(length, filePath);
            }
            var csvFiles = filePaths.Where(p => Path.GetExtension(p) == ".csv");
            foreach (string filePath in csvFiles)
            {
                int length = Convert.ToInt32(Path.GetFileNameWithoutExtension(filePath));
                PermutationSetFilePaths.TryAdd(length, filePath);
            }
        }
        public PermutationGenerator()
        {
            EnumeratePermutationSets();
        }
        public List<int[]> GetIndexPermutations(int permutationLength)
        {
            List<int[]> LoadOrCalcPerms()
            {
                if (permutationLength < 6 || !PermutationSetFilePaths.Keys.Contains(permutationLength))
                {
                    return GenerateIndexPermutations(permutationLength);
                }
                else
                {
                    return LoadIndexPermutations(permutationLength);
                }
            }

            if (UseRamCache)
            {
                return PermutationSetRamCache.GetOrAdd(permutationLength, LoadOrCalcPerms());
            }
            else
            {
                return LoadOrCalcPerms();
            }
        }
        private void SavePermutations(List<int[]> permutations)
        {
            // saving CSV
            List<string> lines = new();
            foreach (int[] perm in permutations)
            {
                string line = perm[0].ToString();
                for (int i = 1; i < perm.Length; i++)
                {
                    line += ";" + perm[i].ToString();
                }
                lines.Add(line);
            }
            string fileNameCSV = permutations[0].Length.ToString() + ".csv";
            string savePathCSV = Path.Combine(PermFolderPath, fileNameCSV);
            File.WriteAllLines(savePathCSV, lines);
            // saving .perm
            string fileNamePerm = permutations[0].Length.ToString() + ".perm";
            string savePathPerm = Path.Combine(PermFolderPath, fileNamePerm);
            using (var fs = new FileStream(savePathPerm, FileMode.Create, FileAccess.Write))
            {
                foreach (int[] perm in permutations)
                {
                    for (int i = 0; i < perm.Length; i++)
                    {
                        byte[] bi = new byte[] { Convert.ToByte(perm[i]) };

                        fs.Write(bi);
                    }
                }
            }
        }
        private List<int[]> LoadIndexPermutations(int permutationLength)
        {
            List<int[]> result = new();
            string loadPath = PermutationSetFilePaths[permutationLength];
            string extention = Path.GetExtension(loadPath).ToLower();
            switch (extention)
            {
                default:
                    throw new ArgumentException("Cannot load file with type: " + extention);
                case ".csv":
                    LoadCSV();
                    goto Exit;
                case ".perm":
                    LoadPerm();
                    goto Exit;
            }
            void LoadCSV()
            {
                string[] lines = File.ReadAllLines(loadPath);
                foreach (string line in new Span<string>(lines))
                {
                    string[] split = line.Split(';');
                    int[] newPerm = new int[permutationLength];
                    for (int i = 0; i < permutationLength; i++)
                    {
                        newPerm[i] = int.Parse(split[i]);
                    }
                    result.Add(newPerm);
                }
                if (BuildFileCache)
                {
                    string permPath = Path.GetFileNameWithoutExtension(loadPath) + ".perm";
                    if (!File.Exists(permPath)) { SavePermutations(result); }
                }
            }
            void LoadPerm()
            {
                byte[] bytes = File.ReadAllBytes(loadPath);
                List<byte[]> chunks = bytes.Chunk(permutationLength).ToList(); // Splitting into permutation byte sized chunks
                foreach (var chunk in chunks)
                {
                    List<int> newPerm = new();
                    foreach (byte intByte in chunk)
                    {
                        newPerm.Add(Convert.ToInt32(intByte));
                    }
                    result.Add(newPerm.ToArray());
                }
                if (BuildFileCache)
                {
                    string csvPath = Path.GetFileNameWithoutExtension(loadPath) + ".csv";
                    if (!File.Exists(csvPath)) { SavePermutations(result); }
                }
            }
        Exit:
            return result;
        }
        private List<int[]> GenerateIndexPermutations(int permutationLength)
        {
            List<int[]> result = new();
            List<int> rootPerm = new(); for (int i = 0; i < permutationLength; i++) { rootPerm.Add(i); }
            List<List<int>> tempResult = new();
            for (int i = 0; i < permutationLength; i++) { tempResult.Add(new() { i }); }
            while (tempResult[0].Count < permutationLength)
            {
                List<List<int>> oldResults = new List<List<int>>(tempResult);
                tempResult.Clear();
                foreach (List<int> oldResult in oldResults)
                {
                    var missing = rootPerm.Where(x => !oldResult.Contains(x));
                    foreach (var miss in missing)
                    {
                        var newResult = new List<int>(oldResult);
                        newResult.Add(miss);
                        tempResult.Add(newResult);
                    }
                }
            }
            foreach (var temp in tempResult)
            {
                result.Add(temp.ToArray());
            }
            if (BuildFileCache) { SavePermutations(result); }

            return result;
        }
    }
}
