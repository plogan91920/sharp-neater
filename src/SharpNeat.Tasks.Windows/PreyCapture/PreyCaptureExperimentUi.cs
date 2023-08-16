// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.Experiments;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Tasks.PreyCapture;
using SharpNeat.Tasks.PreyCapture.ConfigModels;
using SharpNeat.Windows;
using SharpNeat.Windows.Neat;

namespace SharpNeat.Tasks.Windows.PreyCapture;

/// <summary>
/// Implementation of <see cref="IExperimentUi"/> for the Prey Capture task.
/// </summary>
public sealed class PreyCaptureExperimentUi : NeatExperimentUi
{
    readonly INeaterExperiment<double> _neaterExperiment;
    readonly PreyCaptureCustomConfig _customConfig;

    public PreyCaptureExperimentUi(
        INeaterExperiment<double> neaterExperiment,
        PreyCaptureCustomConfig customConfig)
    {
        _neaterExperiment = neaterExperiment ?? throw new ArgumentNullException(nameof(neaterExperiment));
        _customConfig = customConfig ?? throw new ArgumentNullException(nameof(customConfig));
    }

    /// <inheritdoc/>
    public override GenomeControl CreateTaskControl()
    {
        PreyCaptureWorld world = new(
            _customConfig.PreyInitMoves,
            _customConfig.PreySpeed,
            _customConfig.SensorRange,
            _customConfig.MaxTimesteps);

        var genomeDecoder = NeatGenomeDecoderFactory.CreateGenomeDecoder(
            _neaterExperiment.IsAcyclic,
            _neaterExperiment.EnableHardwareAcceleratedNeuralNets);

        return new PreyCaptureControl(genomeDecoder, world);
    }
}
