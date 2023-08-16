// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Experiments;

/// <summary>
/// <see cref="INeaterExperimentFactory"/> extension methods.
/// </summary>
public static class NeaterExperimentFactoryExtensions
{
    /// <summary>
    /// Creates a new instance of <see cref="INeaterExperiment{T}"/> using the provided NEAT experiment
    /// configuration.
    /// </summary>
    /// <param name="experimentFactory">The experiment factory instance.</param>
    /// <param name="jsonConfigFilename">The name of a file from which experiment JSON configuration can be read.</param>
    /// <returns>A new instance of <see cref="INeaterExperiment{T}"/>.</returns>
    public static INeaterExperiment<double> CreateExperiment(
        this INeaterExperimentFactory experimentFactory,
        string jsonConfigFilename)
    {
        using FileStream fs = File.OpenRead(jsonConfigFilename);
        return experimentFactory.CreateExperiment(fs);
    }
}
