﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Evaluation;

/// <summary>
/// An implementation of <see cref="IGenomeListEvaluator{NeatGenome<T>}"/> that evaluates genomes in parallel on multiple CPU threads.
/// </summary>
/// <typeparam name="NeatGenome<T>">The genome type that is decoded.</typeparam>
/// <typeparam name="IBlackBox<T>">The phenome type that is decoded to and then evaluated.</typeparam>
/// <remarks>
/// Genome decoding to a phenome is performed by a <see cref="IGenomeDecoder{NeatGenome<T>, IBlackBox<T>}"/>.
/// Phenome fitness evaluation is performed by a <see cref="IPhenomeEvaluator{IBlackBox<T>}"/>.
///
/// This class is for use with a stateless (and therefore thread safe) phenome evaluator, i.e. one phenome evaluator is created
/// and the is used concurrently by multiple threads.
/// </remarks>
public class ParallelGenomeListEvaluator<T> : IGenomeListEvaluator<NeatGenome<T>>
    where T : struct
{
    readonly IGenomeDecoder<NeatGenome<T>,IBlackBox<T>> _genomeDecoder;
    readonly IPseudonomeEvaluationScheme<T> _pseudonomeEvaluationScheme;
    readonly ParallelOptions _parallelOptions;
    readonly IPseudonomeEvaluatorPool<T> _evaluatorPool;

    #region Constructor

    /// <summary>
    /// Construct with the provided genome decoder and phenome evaluator.
    /// </summary>
    /// <param name="genomeDecoder">Genome decoder.</param>
    /// <param name="pseudonomeEvaluationScheme">Phenome evaluation scheme.</param>
    /// <param name="degreeOfParallelism">The desired degree of parallelism.</param>
    public ParallelGenomeListEvaluator(
        IGenomeDecoder<NeatGenome<T>,IBlackBox<T>> genomeDecoder,
        IPseudonomeEvaluationScheme<T> pseudonomeEvaluationScheme,
        int degreeOfParallelism)
    {
        // This class should only be used with evaluation schemes that use evaluators with state,
        // otherwise ParallelGenomeListEvaluatorStateless should be used.
        if(!pseudonomeEvaluationScheme.EvaluatorsHaveState) throw new ArgumentException("Evaluation scheme must have state.", nameof(pseudonomeEvaluationScheme));

        // Reject degreeOfParallelism values less than 2. -1 should have been resolved to an actual number by the time
        // this constructor is invoked, and 1 is nonsensical for a parallel evaluator.
        if(degreeOfParallelism < 2) throw new ArgumentException("Must be 2 or above.", nameof(degreeOfParallelism));

        _genomeDecoder = genomeDecoder;
        _pseudonomeEvaluationScheme = pseudonomeEvaluationScheme;
        _parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = degreeOfParallelism
        };

        // Create a pool of phenome evaluators.
        // Note. the pool is initialised with a number of pre-constructed evaluators that matches
        // degreeOfParallelism. We don't expect the pool to be asked for more than this number of
        // evaluators at any given point in time.
        _evaluatorPool = new PseudonomeEvaluatorStackPool<T>(
            pseudonomeEvaluationScheme,
            degreeOfParallelism);
    }

    #endregion

    #region IGenomeListEvaluator

    /// <summary>
    /// Indicates if the evaluation scheme is deterministic, i.e. will always return the same fitness score for a given genome.
    /// </summary>
    /// <remarks>
    /// An evaluation scheme that has some random/stochastic characteristics may give a different fitness score at each invocation
    /// for the same genome, such a scheme is non-deterministic.
    /// </remarks>
    public bool IsDeterministic => _pseudonomeEvaluationScheme.IsDeterministic;

    /// <summary>
    /// The evaluation scheme's fitness comparer.
    /// </summary>
    /// <remarks>
    /// Typically there is a single fitness score and a higher score is considered better/fitter. However, if there are multiple
    /// fitness values assigned to a genome (e.g. where multiple measures of fitness are in use) then we need a task specific
    /// comparer to determine the relative fitness between two instances of <see cref="FitnessInfo"/>.
    /// </remarks>
    public IComparer<FitnessInfo> FitnessComparer => _pseudonomeEvaluationScheme.FitnessComparer;

    /// <summary>
    /// Evaluates a collection of genomes and assigns fitness info to each.
    /// </summary>
    /// <param name="genomeList">The list of genomes to evaluate.</param>
    public void Evaluate(IList<NeatGenome<T>> genomeList)
    {
        // Decode and evaluate genomes in parallel.
        // Notes.
        // This overload of Parallel.ForEach accepts a factory function for obtaining an object that represents some state
        // that can re-used within a partition, here we return a phenome evaluator as that partition state object.
        //
        // Here a partition is a group genomes from genomeList that will be evaluated by a single thread, i.e.
        // partitions may be executed in parallel, but genomes within a partition are evaluated sequentially and
        // therefore require only one phenome evaluator between them, we just need to ensure evaluator state is reset between
        // evaluations.
        Parallel.ForEach(
            genomeList,
            _parallelOptions,
            () => _evaluatorPool.GetEvaluator(),    // Get a phenome evaluator from the pool to use for the current partition.
            (genome, loopState, evaluator) =>       // Evaluate a single genome.
            {
                using IBlackBox<T> phenome = _genomeDecoder.Decode(genome);
                if(phenome is null)
                {   // Non-viable genome.
                    genome.FitnessInfo = _pseudonomeEvaluationScheme.NullFitness;
                }
                else
                {
                    genome.FitnessInfo = evaluator.Evaluate(new Pseudonome<T>(genome, phenome));
                }

                // The {evaluator} param for the next call of this anonymous method comes from what we return here,
                // this is useful for struct based partition state, but here we're just passing an object ref around.
                return evaluator;
            },
            (evaluator) => _evaluatorPool.ReleaseEvaluator(evaluator)); // Release this partition's phenome evaluator back into pool.
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
