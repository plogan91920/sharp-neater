// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.Domains.FunctionRegression;
using SharpNeat.Experiments;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Tasks.FunctionRegression;
using SharpNeat.Windows;
using SharpNeat.Windows.Neat;

namespace SharpNeat.Tasks.Windows.GenerativeFunctionRegression;

public sealed class GenerativeFnRegressionUi : NeatExperimentUi
{
    readonly INeaterExperiment<double> _neaterExperiment;
    readonly Func<double, double> _fn;
    readonly ParamSamplingInfo _paramSamplingInfo;

    public GenerativeFnRegressionUi(
        INeaterExperiment<double> neaterExperiment,
        Func<double, double> fn,
        ParamSamplingInfo paramSamplingInfo)
    {
        _neaterExperiment = neaterExperiment ?? throw new ArgumentNullException(nameof(neaterExperiment));
        _fn = fn;
        _paramSamplingInfo = paramSamplingInfo;
    }

    /// <inheritdoc/>
    public override GenomeControl CreateTaskControl()
    {
        var genomeDecoder = NeatGenomeDecoderFactory.CreateGenomeDecoder(
            _neaterExperiment.IsAcyclic,
            _neaterExperiment.EnableHardwareAcceleratedNeuralNets);

        return new FnRegressionControl(
            _fn,
            _paramSamplingInfo,
            true,
            genomeDecoder);
    }
}
