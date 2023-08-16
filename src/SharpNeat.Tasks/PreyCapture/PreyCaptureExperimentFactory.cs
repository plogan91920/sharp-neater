﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.Experiments;
using SharpNeat.IO;
using SharpNeat.NeuralNets;
using SharpNeat.Tasks.PreyCapture.ConfigModels;

namespace SharpNeat.Tasks.PreyCapture;

/// <summary>
/// A factory for creating instances of <see cref="INeatExperiment{T}"/> for the prey capture task.
/// </summary>
public sealed class PreyCaptureExperimentFactory : INeaterExperimentFactory
{
    /// <inheritdoc/>
    public string Id => "prey-capture";

    /// <inheritdoc/>
    public INeaterExperiment<double> CreateExperiment(Stream jsonConfigStream)
    {
        // Load experiment JSON config.
        PreyCaptureExperimentConfig experimentConfig = JsonUtils.Deserialize<PreyCaptureExperimentConfig>(jsonConfigStream);

        // Get the customEvaluationSchemeConfig section.
        PreyCaptureCustomConfig customConfig = experimentConfig.CustomEvaluationSchemeConfig;

        // Create an evaluation scheme object for the prey capture task.
        var evalScheme = new PreyCaptureEvaluationScheme(
            customConfig.PreyInitMoves,
            customConfig.PreySpeed,
            customConfig.SensorRange,
            customConfig.MaxTimesteps,
            customConfig.TrialsPerEvaluation);

        // Create a NeatExperiment object with the evaluation scheme,
        // and assign some default settings (these can be overridden by config).
        var experiment = new NeaterExperiment<double>(evalScheme, Id)
        {
            IsAcyclic = false,
            CyclesPerActivation = 1,
            ActivationFnName = ActivationFunctionId.LeakyReLU.ToString()
        };

        // Apply configuration to the experiment instance.
        experiment.Configure(experimentConfig);
        return experiment;
    }

    /// <inheritdoc/>
    public INeaterExperiment<float> CreateExperimentSinglePrecision(Stream jsonConfigStream)
    {
        throw new NotImplementedException();
    }
}
