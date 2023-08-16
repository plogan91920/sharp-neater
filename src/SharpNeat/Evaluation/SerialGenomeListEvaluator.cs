// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Evaluation;

/// <summary>
/// An implementation of <see cref="IGenomeListEvaluator{TGenome}"/> that evaluates genomes in series on a single CPU thread.
/// </summary>
/// <typeparam name="TGenome">The genome type that is decoded.</typeparam>
/// <typeparam name="TPhenome">The phenome type that is decoded to and then evaluated.</typeparam>
/// <remarks>
/// Single threaded evaluation can be useful in various scenarios e.g. when debugging.
///
/// Genome decoding is performed by a provided IGenomeDecoder.
/// Phenome evaluation is performed by a provided IPhenomeEvaluator.
/// </remarks>
public class SerialGenomeListEvaluator<T> : IGenomeListEvaluator<NeatGenome<T>>
    where T : struct
{
    readonly IGenomeDecoder<NeatGenome<T>,IBlackBox<T>> _genomeDecoder;
    readonly IPseudonomeEvaluationScheme<T> _pseudonomeEvaluationScheme;
    readonly IPseudonomeEvaluator<T> _pseudonomeEvaluator;

    #region Constructor

    /// <summary>
    /// Construct with the provided genome decoder and phenome evaluator.
    /// </summary>
    /// <param name="genomeDecoder">Genome decoder.</param>
    /// <param name="phenomeEvaluationScheme">Phenome evaluation scheme.</param>
    public SerialGenomeListEvaluator(
        IGenomeDecoder<NeatGenome<T>,IBlackBox<T>> genomeDecoder,
        IPseudonomeEvaluationScheme<T> pseudonomeEvaluationScheme)
    {
        _genomeDecoder = genomeDecoder;
        _pseudonomeEvaluationScheme = pseudonomeEvaluationScheme;

        // Note. SerialGenomeListEvaluator will only evaluate on one thread therefore only ever requires a single evaluator.
        _pseudonomeEvaluator = pseudonomeEvaluationScheme.CreateEvaluator();
    }

    #endregion

    #region IGenomeListEvaluator

    /// <summary>
    /// Indicates if the evaluation scheme is deterministic, i.e. will always return the same fitness score for a given genome.
    /// </summary>
    /// <remarks>
    /// An evaluation scheme that has some random/stochastic characteristics may give a different fitness score at each invocation
    /// for the same genome. Such a scheme is non-deterministic.
    /// </remarks>
    public bool IsDeterministic => _pseudonomeEvaluationScheme.IsDeterministic;

    /// <summary>
    /// Gets a fitness comparer.
    /// </summary>
    /// <remarks>
    /// Typically there is a single fitness score whereby a higher score is better, however if there are multiple fitness scores
    /// per genome then we need a more general purpose comparer to determine an ordering on FitnessInfo(s), i.e. to be able to
    /// determine which is the better FitenssInfo between any two.
    /// </remarks>
    public IComparer<FitnessInfo> FitnessComparer => _pseudonomeEvaluationScheme.FitnessComparer;

    /// <summary>
    /// Evaluates a list of genomes, assigning fitness info to each.
    /// </summary>
    /// <param name="genomeList">The list of genomes to evaluate.</param>
    public void Evaluate(IList<NeatGenome<T>> genomeList)
    {
        // Decode and evaluate each genome in turn.
        foreach(NeatGenome<T> genome in genomeList)
        {
            // TODO: Implement phenome caching (to avoid decode cost when re-evaluating with a non-deterministic evaluation scheme).
            using IBlackBox<T> phenome = _genomeDecoder.Decode(genome);
            if(phenome is null)
            {   // Non-viable genome.
                genome.FitnessInfo = _pseudonomeEvaluationScheme.NullFitness;
            }
            else
            {
                genome.FitnessInfo = _pseudonomeEvaluator.Evaluate(new Pseudonome<T>(genome, _genomeDecoder.Decode(genome)));
            }
        }
    }

    /// <summary>
    /// Accepts a <see cref="FitnessInfo"/>, which is intended to be from the fittest genome in the population, and returns a boolean
    /// that indicates if the evolution algorithm can stop, i.e. because the fitness is the best that can be achieved (or good enough).
    /// </summary>
    /// <param name="fitnessInfo">The fitness info object to test.</param>
    /// <returns>Returns true if the fitness is good enough to signal the evolution algorithm to stop.</returns>
    public bool TestForStopCondition(FitnessInfo fitnessInfo)
    {
        return _pseudonomeEvaluationScheme.TestForStopCondition(fitnessInfo);
    }

    #endregion
}
