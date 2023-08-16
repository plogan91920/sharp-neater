// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Evaluation;

/// <summary>
/// Static factory class for creating instances of <see cref="IGenomeListEvaluator{TGenome}"/>.
/// </summary>
public static class GenomeListEvaluatorFactory
{
    /// <summary>
    /// Create a new genome list evaluator.
    /// </summary>
    /// <typeparam name="TGenome">Genome type.</typeparam>
    /// <typeparam name="TPhenome">Phenome type.</typeparam>
    /// <param name="genomeDecoder">Genome decoder, for decoding a genome to a phenome.</param>
    /// <param name="phenomeEvaluationScheme">Phenome evaluation scheme.</param>
    /// <param name="degreeOfParallelism">The number of CPU threads to distribute work to.</param>
    /// <returns>A new instance of <see cref="IGenomeListEvaluator{TGenome}"/>.</returns>
    public static IGenomeListEvaluator<NeatGenome<T>> CreateEvaluator<T>(
        IGenomeDecoder<NeatGenome<T>,IBlackBox<T>> genomeDecoder,
        IPseudonomeEvaluationScheme<T> pseudonomeEvaluationScheme,
        int degreeOfParallelism)
        where T : struct
    {
        // Reject nonsensical values for degreeOfParallelism.
        if(degreeOfParallelism < 1)
            throw new ArgumentException("Must be 1 or above.", nameof(degreeOfParallelism));

        // Create a serial (single threaded) evaluator if degreeOfParallelism is one.
        if(degreeOfParallelism == 1)
        {
            return new SerialGenomeListEvaluator<T>(
                genomeDecoder,
                pseudonomeEvaluationScheme);
        }

        // Create a parallel (multi-threaded) evaluator for degreeOfParallelism > 1.
        if(pseudonomeEvaluationScheme.EvaluatorsHaveState)
        {
            return new ParallelGenomeListEvaluator<T>(
                genomeDecoder,
                pseudonomeEvaluationScheme,
                degreeOfParallelism);
        }

        // else
        return new ParallelGenomeListEvaluatorStateless<T>(
            genomeDecoder,
            pseudonomeEvaluationScheme,
            degreeOfParallelism);
    }
}
