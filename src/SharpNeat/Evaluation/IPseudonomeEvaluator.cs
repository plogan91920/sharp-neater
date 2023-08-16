// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Evaluation;

/// <summary>
/// Represents an evaluator of <typeparamref name="T"/> instances.
/// </summary>
/// <typeparam name="T">Phenome input/output signal data type.</typeparam>
public interface IPseudonomeEvaluator<T>
    where T : struct
{
    /// <summary>
    /// Evaluate a single phenome and return its fitness score or scores.
    /// </summary>
    /// <param name="pseudonome">The pseudonome to evaluate.</param>
    /// <returns>An instance of <see cref="FitnessInfo"/> that conveys the phenome's fitness scores/data.</returns>
    FitnessInfo Evaluate(Pseudonome<T> pseudonome);
}
