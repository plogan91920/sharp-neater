﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.Experiments;
using SharpNeat.Experiments.ConfigModels;
using SharpNeat.IO;
using SharpNeat.NeuralNets;

namespace SharpNeat.Tasks.BinarySixMultiplexer;

/// <summary>
/// A factory for creating instances of <see cref="INeaterExperiment{T}"/> for the Binary 6-multiplexer task.
/// </summary>
public sealed class BinarySixMultiplexerExperimentFactory : INeaterExperimentFactory
{
    /// <inheritdoc/>
    public string Id => "binary-6-multiplexer";

    /// <inheritdoc/>
    public INeaterExperiment<double> CreateExperiment(Stream jsonConfigStream)
    {
        // Load experiment JSON config.
        ExperimentConfig experimentConfig = JsonUtils.Deserialize<ExperimentConfig>(jsonConfigStream);

        // Create an evaluation scheme object for the binary 6-multiplexer task.
        var evalScheme = new BinarySixMultiplexerEvaluationScheme();

        // Create a NeatExperiment object with the evaluation scheme,
        // and assign some default settings (these can be overridden by config).
        var experiment = new NeaterExperiment<double>(evalScheme, Id)
        {
            IsAcyclic = true,
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
