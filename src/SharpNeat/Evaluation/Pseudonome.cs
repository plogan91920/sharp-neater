using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNeat.EvolutionAlgorithm;

namespace SharpNeat.Evaluation;

/// <summary>
/// Represents a pseudo-genome, prepared to be collapsed to a Network when observed.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TPhenome"></typeparam>
public class Pseudonome<T>
    where T : struct
{
    /// <summary>
    /// The experiment's Model.
    /// </summary>
    readonly NeatModel<T> model;

    readonly IBlackBox<T> blackBox;

    /// <summary>
    /// A standard constructor.
    /// </summary>
    /// <param name="genome">The genome to be converted.</param>
    public Pseudonome(NeatGenome<T> genome, IBlackBox<T> _blackBox)
    {
        model = genome.Model;
        blackBox = _blackBox;
    }

    /// <summary>
    /// Collapses the Pseudonome into a Phenome and calculates the answers.
    /// </summary>
    /// <returns>The Answers keyed by question.</returns>
    public Dictionary<Question<T>, T> Observe(NeatObservation<T>/*[]*/ observation /*, NeatConnection<T>[] connections */)
    {
        // Temporary
        NeatConnection<T>[] connections = Array.Empty<NeatConnection<T>>();
        (Dictionary<Trait<T>, NodeKey> inputMap, Dictionary<Question<T>, NodeKey> outputMap) = model.Observe(new NeatObservation<T>[] { observation }, connections);

        // convert inputs from strings -> ids -> sequence
        List<NodeKey> inputNodeMap = new();
        List<T> inputValues = new();
        foreach ((Trait<T> trait, NodeKey inputNodeKey) in inputMap)
        {
            inputNodeMap.Add(inputNodeKey);
            inputValues.Add(trait.Value);
        }

        // convert outputs from Question -> ids -> sequence
        List<NodeKey> outputNodeMap = new();
        foreach (NodeKey outputNodeKey in outputMap.Values)
            outputNodeMap.Add(outputNodeKey);

        // serialize hidden nodes
        // TODO: include virtual nodes

        // build a digraph from the collapsed observations

        // run calculation
        blackBox.Reset();

        var inputs = blackBox.Inputs.Span;
        inputs = inputValues.ToArray();

        var outputs = blackBox.Outputs.Span;

        blackBox.Activate();

        // return output values
        Dictionary<Question<T>, T> outputValues = new();
        foreach ((Question<T> question, NodeKey outputNodeKey) in outputMap)
        {
            int outputIndex = outputNodeMap.IndexOf(outputNodeKey);
            T outputValue = outputs[outputIndex];
            outputValues[question] = outputValue;
        }

        return outputValues;
    }
}
