// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Evaluation;

/// <summary>
/// A pool of phenome evaluators.
/// </summary>
/// <remarks>
/// Used when the phenome evaluators have state, which is expensive to allocate, therefore we wish to
/// maintain a pool or re-usable evaluators instead of constructing and discarding a new evaluator for
/// each evaluation (or more accurately, each Parallel.For partition).
/// </remarks>
/// <typeparam name="T">Phenome type.</typeparam>
public interface IPseudonomeEvaluatorPool<T>
    where T : struct
{
    /// <summary>
    /// Get an evaluator from the pool.
    /// </summary>
    /// <returns>An evaluator instance.</returns>
    IPseudonomeEvaluator<T> GetEvaluator();

    /// <summary>
    /// Releases an evaluator back into the pool.
    /// </summary>
    /// <param name="evaluator">The evaluator to release.</param>
    void ReleaseEvaluator(IPseudonomeEvaluator<T> evaluator);
}
