// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using System.Diagnostics;
using SharpNeat.NeuralNets;

namespace SharpNeat.Neat.Genome;

/// <summary>
/// NeatGenome metadata.
/// Genome related values/settings that are consistent across all genomes for the lifetime of an evolutionary algorithm run.
/// </summary>
/// <typeparam name="T">Neural net numeric data type.</typeparam>
public class NeatModel<T>
    where T : struct
{
    /// <summary>
    /// The list of Concepts observed.
    /// </summary>
    Dictionary<string, NeatConcept<T>> Concepts { get; } = new();

    /// <summary>
    /// The list of Relationships observed between Concepts.
    /// </summary>
    List<NeatRelationship> Relationships { get; } = new();

    /// <summary>
    /// Input node count.
    /// </summary>
    public int InputNodeCount { get; }

    /// <summary>
    /// Output node count.
    /// </summary>
    public int OutputNodeCount { get; }

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

    /// <summary>
    /// The total number of input and output nodes.
    /// </summary>
    public int InputOutputNodeCount => InputNodeCount + OutputNodeCount;

    #region Construction

    /// <summary>
    /// Construct a new instance.
    /// </summary>
    /// <param name="inputNodeCount">Input node count.</param>
    /// <param name="outputNodeCount">Output node count.</param>
    /// <param name="isAcyclic">Indicates if the genomes that are evolved are acyclic, i.e. they should have no
    /// recurrent/cyclic connection paths.</param>
    /// <param name="cyclesPerActivation">For cyclic neural networks (i.e. if <see cref="IsAcyclic"/> = false)
    /// this defines how many timesteps to run the neural net per call to Activate().</param>
    /// <param name="activationFn">The neuron activation function to use in evolved networks. NEAT uses the same
    /// activation function at each node.</param>
    /// <param name="connectionWeightScale">Maximum connection weight scale/magnitude.</param>
    internal NeatModel(
        int inputNodeCount, int outputNodeCount,
        bool isAcyclic, int cyclesPerActivation,
        IActivationFunction<T> activationFn,
        double connectionWeightScale = 5.0)
    {
        // Note. Zero input nodes is allowed, but zero output nodes is nonsensical.
        if(inputNodeCount < 0) throw new ArgumentOutOfRangeException(nameof(inputNodeCount));
        if(outputNodeCount < 1) throw new ArgumentOutOfRangeException(nameof(outputNodeCount));
        if(!isAcyclic && cyclesPerActivation < 1) throw new ArgumentOutOfRangeException(nameof(cyclesPerActivation));

        InputNodeCount = inputNodeCount;
        OutputNodeCount = outputNodeCount;
        IsAcyclic = isAcyclic;
        CyclesPerActivation = cyclesPerActivation;
        ActivationFn = activationFn;
        ConnectionWeightScale = connectionWeightScale;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Create a new instance of <see cref="NeatModel{T}"/>, with <see cref="IsAcyclic"/> set to true, i.e.,
    /// for evolving acyclic neural networks.
    /// </summary>
    /// <param name="inputNodeCount">Input node count.</param>
    /// <param name="outputNodeCount">Output node count.</param>
    /// <param name="activationFn">The neuron activation function to use in evolved networks. NEAT uses the same
    /// activation function at each node.</param>
    /// <param name="connectionWeightScale">Maximum connection weight scale/magnitude.</param>
    /// <returns>A new instance of <see cref="NeatModel{T}"/>.</returns>
    public static NeatModel<T> CreateAcyclic(
        int inputNodeCount, int outputNodeCount,
        IActivationFunction<T> activationFn,
        double connectionWeightScale = 5.0)
    {
        return new NeatModel<T>(
            inputNodeCount, outputNodeCount,
            true, 0,
            activationFn,
            connectionWeightScale);
    }

    /// <summary>
    /// Create a new instance of <see cref="NeatModel{T}"/>, with <see cref="IsAcyclic"/> set to false, i.e.,
    /// for evolving cyclic neural networks.
    /// </summary>
    /// <param name="inputNodeCount">Input node count.</param>
    /// <param name="outputNodeCount">Output node count.</param>
    /// <param name="cyclesPerActivation">For cyclic neural networks (i.e. if <see cref="IsAcyclic"/> = false)
    /// this defines how many timesteps to run the neural net per call to Activate().</param>
    /// <param name="activationFn">The neuron activation function to use in evolved networks. NEAT uses the same
    /// activation function at each node.</param>
    /// <param name="connectionWeightScale">Maximum connection weight scale/magnitude.</param>
    /// <returns>A new instance of <see cref="NeatModel{T}"/>.</returns>
    public static NeatModel<T> CreateCyclic(
        int inputNodeCount, int outputNodeCount,
        int cyclesPerActivation,
        IActivationFunction<T> activationFn,
        double connectionWeightScale = 5.0)
    {
        return new NeatModel<T>(
            inputNodeCount, outputNodeCount,
            false, cyclesPerActivation,
            activationFn,
            connectionWeightScale);
    }

    #endregion

    #region Public Methods

    public ObservationInformation<T> Observe(NeatObservation<T>[] observations, NeatConnection<T>[] connections)
    {
        // Add observed concepts
        foreach (NeatObservation<T> observation in observations)
        {
            if (Concepts.ContainsKey(observation.Key)) continue;

            Concepts[observation.Key] = new NeatConcept<T>(observation.Key);
        }

        // Add observed relationships
        foreach (NeatConnection<T> connection in connections)
        {
            if (Relationships.Any((r) => r.Equals(connection))) continue;

            Relationships.Add(new NeatRelationship(connection.Key, connection.SourceKey, connection.TargetKey));
        }

        // Add observed traits and questions
        Dictionary<Trait<T>, NodeKey> inputMap = new();
        Dictionary<Question<T>, NodeKey> outputMap = new();
        foreach (NeatObservation<T> observation in observations)
        {
            foreach (Trait<T> trait in observation.Traits)
            {
                int traitNodeId = Concepts[observation.Key].GetTraitNode(trait.Key);
                inputMap[trait] = new NodeKey(observation.Key, traitNodeId);
            }

            foreach (Question<T> question in observation.Questions)
            {
                int questionNodeId = Concepts[observation.Key].GetQuestionNode(question.Key);
                outputMap[question] = new NodeKey(observation.Key, questionNodeId);
            }
        }

        return new ObservationInformation<T>(inputMap, outputMap);
    }

    #endregion
}

public struct ObservationInformation<T>
{
    /// <summary>
    /// 
    /// </summary>
    public Dictionary<Trait<T>, NodeKey> InputMap;

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<Question<T>, NodeKey> OutputMap;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputMap"></param>
    /// <param name="outputMap"></param>
    public ObservationInformation(Dictionary<Trait<T>, NodeKey> inputMap, Dictionary<Question<T>, NodeKey> outputMap)
    {
        InputMap = inputMap;
        OutputMap = outputMap;
    }

    public void Deconstruct(out Dictionary<Trait<T>, NodeKey> inputMap, out Dictionary<Question<T>, NodeKey> outputMap)
    {
        inputMap = InputMap;
        outputMap = OutputMap;
    }
}

public struct NodeKey
{
    public string ConceptKey;
    public int Id;

    public NodeKey(string conceptKey, int id)
    {
        ConceptKey = conceptKey;
        Id = id;
    }
}
