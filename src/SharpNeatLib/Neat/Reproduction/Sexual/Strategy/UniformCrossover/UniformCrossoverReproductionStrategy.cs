﻿/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2019 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using Redzen;
using Redzen.Numerics.Distributions;
using Redzen.Random;
using Redzen.Structures;
using SharpNeat.Neat.Genome;
using static SharpNeat.Neat.Reproduction.Sexual.Strategy.UniformCrossover.UniformCrossoverReproductionStrategyUtils;

namespace SharpNeat.Neat.Reproduction.Sexual.Strategy.UniformCrossover
{
    /// <summary>
    /// Uniform crossover.
    /// 
    /// The genes of the two parent genomes are aligned by innovation ID. The new child genome
    /// takes genes from each of the parents with a given probability (e.g. 50%).
    /// </summary>
    public class UniformCrossoverReproductionStrategy<T> : ISexualReproductionStrategy<T>
        where T : struct
    {
        readonly bool _isAcyclic;
        readonly double _secondaryParentGeneProbability;
        readonly INeatGenomeBuilder<T> _genomeBuilder;
        readonly Int32Sequence _genomeIdSeq;
        readonly Int32Sequence _generationSeq;
        readonly ConnectionGeneListBuilder<T> _connGeneListBuilder;

        #region Constructor

        /// <summary>
        /// Construct with the given strategy arguments.
        /// </summary>
        /// <param name="isAcyclic">Indicates that the strategy will be operating on acyclic graphs/genomes.</param>
        /// <param name="secondaryParentGeneProbability">The probability that a gene that exists only on the secondary parent is copied into the child genome.</param>
        /// <param name="genomeBuilder">A neat genome builder.</param>
        /// <param name="genomeIdSeq">Genome ID sequence; for obtaining new genome IDs.</param>
        /// <param name="generationSeq">A sequence that provides the current generation number.</param>
        public UniformCrossoverReproductionStrategy(
            bool isAcyclic,
            double secondaryParentGeneProbability,
            INeatGenomeBuilder<T> genomeBuilder,
            Int32Sequence genomeIdSeq,
            Int32Sequence generationSeq)
        {
            _isAcyclic = isAcyclic;
            _secondaryParentGeneProbability = secondaryParentGeneProbability;
            _genomeBuilder = genomeBuilder;
            _genomeIdSeq = genomeIdSeq;
            _generationSeq = generationSeq;

            _connGeneListBuilder = new ConnectionGeneListBuilder<T>(_isAcyclic, 1024);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a new child genome based on the genetic content of two parent genome.
        /// </summary>
        /// <param name="parent1">Parent 1.</param>
        /// <param name="parent2">Parent 2.</param>
        /// <param name="rng">Random source.</param>
        /// <returns>A new child genome.</returns>
        public NeatGenome<T> CreateGenome(NeatGenome<T> parent1, NeatGenome<T> parent2, IRandomSource rng)
        {
            try
            {
                return CreateGenomeInner(parent1, parent2, rng);
            }
            finally
            {
                // Clear down ready for re-use of the builder on the next call to CreateGenome().
                // Re-using in this way avoids having to de-alloc and re-alloc memory, thus reducing garbage collection overhead.
                _connGeneListBuilder.Clear();
            }
        }

        #endregion

        #region Private Methods

        private NeatGenome<T> CreateGenomeInner(NeatGenome<T> parent1, NeatGenome<T> parent2, IRandomSource rng)
        {
            // Randomly select one parent as being the primary parent.
            if(rng.NextBool()) {
                VariableUtils.Swap(ref parent1, ref parent2);
            }

            // Enumerate over the connection genes in both parents.
            foreach(var geneIndexPair in EnumerateParentGenes(parent1.ConnectionGenes, parent2.ConnectionGenes))
            {
                // Create a connection gene based on the current position in both parents.
                ConnectionGene<T>? connGene = CreateConnectionGene(
                    parent1.ConnectionGenes, parent2.ConnectionGenes,
                    geneIndexPair.Item1, geneIndexPair.Item2,
                    rng,
                    out bool isSecondaryGene);

                if(connGene.HasValue)
                {
                    // Attempt to add the gene to the child genome we are building.
                    _connGeneListBuilder.TryAddGene(connGene.Value, isSecondaryGene);
                }
            }

            // Convert the genes to the structure required by NeatGenome.
            ConnectionGenes<T> connGenes = _connGeneListBuilder.ToConnectionGenes();

            // Create and return a new genome.
            return _genomeBuilder.Create(
                _genomeIdSeq.Next(), 
                _generationSeq.Peek,
                connGenes);
        }

        #endregion

        #region Private Methods [Low Level]

        private ConnectionGene<T>? CreateConnectionGene(
            ConnectionGenes<T> connGenes1,
            ConnectionGenes<T> connGenes2,
            int idx1, int idx2,
            IRandomSource rng,
            out bool isSecondaryGene)
        {
            // Select gene at random if it is present on both parents.
            if(-1 != idx1 && -1 != idx2)
            {
                if(rng.NextBool())
                {
                    isSecondaryGene = false;
                    return CreateConnectionGene(connGenes1, idx1);
                }
                else
                {
                    isSecondaryGene = true;
                    return CreateConnectionGene(connGenes2, idx2);
                }
            }

            // Use the primary parent's gene if it has one.
            if(-1 != idx1) 
            {
                isSecondaryGene = false;
                return CreateConnectionGene(connGenes1, idx1);
            }

            // Otherwise use the secondary parent's gene stochastically.
            if(DiscreteDistribution.SampleBernoulli(rng, _secondaryParentGeneProbability))
            {
                isSecondaryGene = true;
                return CreateConnectionGene(connGenes2, idx2);
            }
            
            isSecondaryGene = false;
            return null;
        }

        private ConnectionGene<T> CreateConnectionGene(ConnectionGenes<T> connGenes, int idx)
        {
            return new ConnectionGene<T>(
                connGenes._connArr[idx],
                connGenes._weightArr[idx]);
        }

        #endregion
    }
}
