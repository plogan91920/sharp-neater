// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.NeuralNets;

namespace SharpNeat.Neat.Genome;

/// <summary>
/// NeatGenome metadata.
/// Genome related values/settings that are consistent across all genomes for the lifetime of an evolutionary algorithm run.
/// </summary>
/// <typeparam name="T">Neural net numeric data type.</typeparam>
public class NeaterModel<T>
    where T : struct
{
    /// <summary>
    /// Unique concepts observed in the simulation.
    /// </summary>
    Dictionary<string, NeaterConcept<T>> concepts = new Dictionary<string, NeaterConcept<T>>();

    /// <summary>
    /// Input node keys from all concepts.
    /// </summary>
    public NodeKey[] InputNodes
    {
        get
        {
            List<NodeKey> inputNodes = new List<NodeKey>();
            foreach ((string conceptKey, NeaterConcept<T> concept) in concepts)
            {
                foreach ((string _, int id) in concept.InputNodes)
                    inputNodes.Add(new NodeKey(conceptKey, id));
            }
            return inputNodes.ToArray();
        }
    }

    /// <summary>
    /// Input node keys from all concepts.
    /// </summary>
    public NodeKey[] OutputNodes
    {
        get
        {
            List<NodeKey> outputNodes = new List<NodeKey>();
            foreach ((string conceptKey, NeaterConcept<T> concept) in concepts)
            {
                foreach ((string _, int id) in concept.OutputNodes)
                    outputNodes.Add(new NodeKey(conceptKey, id));
            }
            return outputNodes.ToArray();
        }
    }

    /// <summary>
    /// Indicates if the genomes that are evolved are acyclic, i.e. they should have no recurrent/cyclic
    /// connection paths.
    /// </summary>
    public bool IsAcyclic { get; }

    /// <summary>
    /// For cyclic neural networks (i.e. if <see cref="IsAcyclic"/> = false) this defines how many timesteps to
    /// run the neural net per call to Activate().
    /// </summary>
    public int CyclesPerActivation { get; }

    /// <summary>
    /// The neuron activation function to use in evolved networks. NEAT uses the same activation
    /// function at each node.
    /// </summary>
    public IActivationFunction<T> ActivationFn { get; }

    /// <summary>
    /// Maximum connection weight scale/magnitude.
    /// E.g. a value of 5 defines a weight range of -5 to 5.
    /// The weight range is strictly enforced, e.g. when creating new connections and mutating existing ones.
    /// </summary>
    public double ConnectionWeightScale { get; }

    #region Construction

    /// <summary>
    /// Construct a new instance.
    /// </summary>
    /// <param name="isAcyclic">Indicates if the genomes that are evolved are acyclic, i.e. they should have no
    /// recurrent/cyclic connection paths.</param>
    /// <param name="cyclesPerActivation">For cyclic neural networks (i.e. if <see cref="IsAcyclic"/> = false)
    /// this defines how many timesteps to run the neural net per call to Activate().</param>
    /// <param name="activationFn">The neuron activation function to use in evolved networks. NEAT uses the same
    /// activation function at each node.</param>
    /// <param name="connectionWeightScale">Maximum connection weight scale/magnitude.</param>
    internal NeaterModel(
        bool isAcyclic, int cyclesPerActivation,
        IActivationFunction<T> activationFn,
        double connectionWeightScale = 5.0)
    {
        if(!isAcyclic && cyclesPerActivation < 1) throw new ArgumentOutOfRangeException(nameof(cyclesPerActivation));

        IsAcyclic = isAcyclic;
        CyclesPerActivation = cyclesPerActivation;
        ActivationFn = activationFn;
        ConnectionWeightScale = connectionWeightScale;
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

/// <summary>
/// A complex key demonstrating the nodes position.
/// </summary>
public struct NodeKey
{
    /// <summary>
    /// The key of the concept that owns this node.
    /// </summary>
    public string Key;

    /// <summary>
    /// The Id of this node within it's concept.
    /// </summary>
    public int Id;

    /// <summary>
    /// A basic Constructor.
    /// </summary>
    /// <param name="key">/// The key of the concept that owns this node.</param>
    /// <param name="id">/// The Id of this node within it's concept.</param>
    public NodeKey(string key, int id)
    {
        Key = key;
        Id = id;
    }
}
