

namespace TSPStandard.Algorithm
{
    public enum WorkingSeeds
    {
        NearestNeighbor,
        InsertionBuild
    }
    public enum WorkingImprovers
    {
        TwoOpt,
        Swap,
        SliceWindowBruteForce,
        SliceWindowBruteForceRecurse
    }
    public enum AlgorithmType
    {
        unknown,
        TwoOpt,
        SegmentBruteForce,
        NearestNeighbor,
        SegmentationSearch,
        Swap,
        AntColony,
        FarthestNeighbor,
        LocalChunkOptimiser,
        BruteSeed,
        SliceWindowBruteForce,
        BestEdge,
        InsertionBuild
    }
    public class TSPAlgorithm
    {
        // TODO ADD SOME SORT OF UNIVERSIAL STATUS OBJECT THAT CAN BE READ AND THEN OUTPUTTET CORRECTLY
        public TSPRoute Route;
        public AlgorithmType AlgorithmType { get; protected set; } = AlgorithmType.unknown;
        public virtual void Run()
        {
            throw new InvalidOperationException("Cant run a base algorithm");
        }
        public TSPAlgorithm(TSPRoute route)
        {
            this.Route = new(route);
        }

    }
}
