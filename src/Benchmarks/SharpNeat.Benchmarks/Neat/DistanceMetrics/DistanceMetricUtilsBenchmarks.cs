﻿using BenchmarkDotNet.Attributes;
using SharpNeat.Neat.DistanceMetrics.Double;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNets.Double.ActivationFunctions;

namespace SharpNeat.Neat.DistanceMetrics;

public class DistanceMetricUtilsBenchmarks
{
    readonly ConnectionGenes<double>[] _genomeArr;

    public DistanceMetricUtilsBenchmarks()
    {
        var metaNeatGenome = NeaterModel<double>.CreateAcyclic(12, 1, new LeakyReLU());
        var popLoader = new NeatPopulationLoader<double>(metaNeatGenome);
        List<NeatGenome<double>> genomeList = popLoader.LoadFromZipArchive("data/binary11.pop");
        _genomeArr = genomeList.Select(x => x.ConnectionGenes).ToArray();
    }

    [Benchmark]
    public void CalculateEuclideanCentroid()
    {
        _ = DistanceMetricUtils.CalculateEuclideanCentroid((ReadOnlySpan<ConnectionGenes<double>>)_genomeArr);
    }
}
