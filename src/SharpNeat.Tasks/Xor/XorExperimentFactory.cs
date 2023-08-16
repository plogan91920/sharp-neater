﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.Experiments;
using SharpNeat.Experiments.ConfigModels;
using SharpNeat.IO;
using SharpNeat.NeuralNets;

namespace SharpNeat.Tasks.Xor;

/// <summary>
/// A factory for creating instances of <see cref="INeatExperiment{T}"/> for the XOR task.
/// </summary>
public sealed class XorExperimentFactory : INeaterExperimentFactory
{
    /// <inheritdoc/>
    public string Id => "xor";

    /// <inheritdoc/>
    public INeaterExperiment<double> CreateExperiment(Stream jsonConfigStream)
    {
        // Load experiment JSON config.
        ExperimentConfig experimentConfig = JsonUtils.Deserialize<ExperimentConfig>(jsonConfigStream);

        // Create an evaluation scheme object for the XOR task.
        var evalScheme = new XorEvaluationScheme();

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
