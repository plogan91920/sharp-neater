using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNeat.Neat.Genome;

/// <summary>
/// A class representing the observations in a scene.
/// </summary>
public class NeatObservation<T>
    where T : struct
{
    public string Key { get; }

    public Trait<T>[] Traits { get; }

    public Question<T>[] Questions { get; }

    public NeatObservation(string key, Trait<T>[] traits, Question<T>[] questions)
    {
        Key = key;
        Traits = traits;
        Questions = questions;
    }
}

public struct Trait<T>
{
    public string Key { get; }

    public T Value { get; }

    public Trait(string key, T value)
    {
        Key = key;
        Value = value;
    }
}

public struct Question<T>
{
    public string Key;

    // TODO: add offset

    public Question(string key)
    {
        Key = key;
    }
}

public struct NeatConnection<T> : IRelationship
    where T : struct
{
    /// <summary>
    /// The key representing the Relationship demonstrated by this connection.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The source observation.
    /// </summary>
    public NeatObservation<T> Source { get; }

    public string SourceKey { get => Source.Key; }

    /// <summary>
    /// The target observation.
    /// </summary>
    public NeatObservation<T> Target { get; }

    public string TargetKey { get => Target.Key; }

    /// <summary>
    /// A standard constructor.
    /// </summary>
    /// <param name="key">The key representing the Relationship demonstrated by this connection.</param>
    /// <param name="source">The source observation.</param>
    /// <param name="target">The target observation.</param>
    public NeatConnection(string key, NeatObservation<T> source, NeatObservation<T> target)
    {
        Key = key;
        Source = source;
        Target = target;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj == null || obj.GetType() != GetType()) return false;

        NeatConnection<T> other = (NeatConnection<T>) obj;

        return other.Key == Key && other.Source == Source && other.Target == Target;
    }
}
