// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using System.Diagnostics.CodeAnalysis;
using SharpNeat.NeuralNets;

namespace SharpNeat.Neat.Genome;

/// <summary>
/// NeatGenome metadata.
/// Genome related values/settings that are consistent across all genomes for the lifetime of an evolutionary algorithm run.
/// </summary>
/// <typeparam name="T">Neural net numeric data type.</typeparam>
public class NeatConcept<T>
    where T : struct
{
    public string Key { get; }

    /// <summary>
    /// Input node count.
    /// </summary>
    public int InputNodeCount { get; }

    /// <summary>
    /// Output node count.
    /// </summary>
    public int OutputNodeCount { get; }

    /// <summary>
    /// The total number of input and output nodes.
    /// </summary>
    public int InputOutputNodeCount => InputNodeCount + OutputNodeCount;

    #region Construction

    /// <summary>
    /// Construct a new instance.
    /// </summary>
    internal NeatConcept(string key)
    {
        Key = key;
    }

    public int GetTraitNode(string traitKey)
    {
        return 0;
    }

    public int GetQuestionNode(string questionKey)
    {
        return 0;
    }

    #endregion
}

/// <summary>
/// A relationship between two concepts;
/// </summary>
public struct NeatRelationship : IRelationship
{
    /// <summary>
    /// The key denoting this relationship type.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The key denoting the source Concept.
    /// </summary>
    public string SourceKey { get; }

    /// <summary>
    /// The key denoting the target Concept.
    /// </summary>
    public string TargetKey { get; }

    /// <summary>
    /// A standard constructor.
    /// </summary>
    /// <param name="key">The key denoting this relationship type.</param>
    /// <param name="sourceKey">The key denoting the source Concept.</param>
    /// <param name="targetKey">The key denoting the target Concept.</param>
    public NeatRelationship(string key, string sourceKey, string targetKey)
    {
        Key = key;
        SourceKey = sourceKey;
        TargetKey = targetKey;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj == null) return false;

        if (obj.GetType() == GetType())
            return base.Equals(obj);

        if (obj.GetType() != typeof(IRelationship)) return false;

        IRelationship other = (IRelationship) obj;

        return other.Key == Key && other.SourceKey == SourceKey && other.TargetKey == TargetKey;
    }
}

public interface IRelationship
{
    string Key { get; }
    string SourceKey { get; }
    string TargetKey { get; }
}
