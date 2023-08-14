// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.NeuralNets;

namespace SharpNeat.Neat.Genome;

/// <summary>
/// NeatGenome metadata.
/// Genome related values/settings that are consistent across all genomes for the lifetime of an evolutionary algorithm run.
/// </summary>
/// <typeparam name="T">Neural net numeric data type.</typeparam>
public class NeaterConcept<T>
    where T : struct
{
    /// <summary>
    /// Reserved Input node IDs.
    /// </summary>
    public Dictionary<string, int> InputNodes = new Dictionary<string, int>();

    /// <summary>
    /// Reserved Output node IDs.
    /// </summary>
    public Dictionary<string, int> OutputNodes = new Dictionary<string, int>();

    #region Construction

    /// <summary>
    /// Construct a new instance.
    /// </summary>
    internal NeaterConcept()
    {
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Create a new instance of <see cref="NeaterModel{T}"/>, with <see cref="IsAcyclic"/> set to true, i.e.,
    /// for evolving acyclic neural networks.
    /// </summary>
    /// <param name="inputNodeCount">Input node count.</param>
    /// <param name="outputNodeCount">Output node count.</param>
    /// <param name="activationFn">The neuron activation function to use in evolved networks. NEAT uses the same
    /// activation function at each node.</param>
    /// <param name="connectionWeightScale">Maximum connection weight scale/magnitude.</param>
    /// <returns>A new instance of <see cref="NeaterModel{T}"/>.</returns>
    public static NeaterModel<T> CreateAcyclic(
        int inputNodeCount, int outputNodeCount,
        IActivationFunction<T> activationFn,
        double connectionWeightScale = 5.0)
    {
        return new NeaterModel<T>(
            true, 0,
            activationFn,
            connectionWeightScale);
    }

    /// <summary>
    /// Create a new instance of <see cref="NeaterModel{T}"/>, with <see cref="IsAcyclic"/> set to false, i.e.,
    /// for evolving cyclic neural networks.
    /// </summary>
    /// <param name="inputNodeCount">Input node count.</param>
    /// <param name="outputNodeCount">Output node count.</param>
    /// <param name="cyclesPerActivation">For cyclic neural networks (i.e. if <see cref="IsAcyclic"/> = false)
    /// this defines how many timesteps to run the neural net per call to Activate().</param>
    /// <param name="activationFn">The neuron activation function to use in evolved networks. NEAT uses the same
    /// activation function at each node.</param>
    /// <param name="connectionWeightScale">Maximum connection weight scale/magnitude.</param>
    /// <returns>A new instance of <see cref="NeaterModel{T}"/>.</returns>
    public static NeaterModel<T> CreateCyclic(
        int inputNodeCount, int outputNodeCount,
        int cyclesPerActivation,
        IActivationFunction<T> activationFn,
        double connectionWeightScale = 5.0)
    {
        return new NeaterModel<T>(
            false, cyclesPerActivation,
            activationFn,
            connectionWeightScale);
    }

    #endregion
}
