// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Experiments;

/// <summary>
/// Represents a factory of <see cref="INeaterExperiment{T}"/>.
/// </summary>
public interface INeaterExperimentFactory
{
    /// <summary>
    /// Gets a unique human-readable ID for the experiment, e.g. 'binary-11-multiplexer'.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Creates a new instance of <see cref="INeatExperiment{T}"/> using the provided NEAT experiment
    /// configuration.
    /// </summary>
    /// <param name="jsonConfigStream">A stream from which experiment JSON configuration can be read.</param>
    /// <returns>A new instance of <see cref="INeatExperiment{T}"/>.</returns>
    INeaterExperiment<double> CreateExperiment(Stream jsonConfigStream);

    /// <summary>
    ///  Creates a new instance of <see cref="INeatExperiment{T}"/> using the provided NEAT experiment
    /// configuration, and using single-precision floating-point number format for the
    /// genome and neural-net connection weights.
    /// </summary>
    /// <param name="jsonConfigStream">A stream from which experiment JSON configuration can be read.</param>
    /// <returns>A new instance of <see cref="INeatExperiment{T}"/>.</returns>
    INeaterExperiment<float> CreateExperimentSinglePrecision(Stream jsonConfigStream);
}
