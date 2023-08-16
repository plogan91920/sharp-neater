﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using System.Diagnostics;
using SharpNeat.Evaluation;

namespace SharpNeat.Tasks.Xor;

#pragma warning disable CA1725 // Parameter names should match base declaration.

/// <summary>
/// Evaluator for the logical XOR task.
///
/// Two inputs supply the two XOR input values.
///
/// The correct response for the single output is input1 XOR input2.
///
/// Evaluation consists of querying the provided black box for all possible input combinations (2^2 = 4).
/// </summary>
public sealed class XorEvaluator : IPseudonomeEvaluator<double>
{
    /// <summary>
    /// Evaluate the provided black box against the logical XOR task,
    /// and return its fitness score.
    /// </summary>
    /// <param name="pseudonome">The pseudonome to evaluate.</param>
    /// <returns>A new instance of <see cref="FitnessInfo"/>.</returns>
    public FitnessInfo Evaluate(Pseudonome<double> pseudonome)
    {
        double fitness = 0.0;
        bool success = true;
        IBlackBox<double> box = pseudonome.BlackBox;

        // Test case 0, 0.
        double output = Activate(box, 0.0, 0.0);
        success &= output <= 0.5;
        fitness += 1.0 - (output * output);

        // Test case 1, 1.
        box.Reset();
        output = Activate(box, 1.0, 1.0);
        success &= output <= 0.5;
        fitness += 1.0 - (output * output);

        // Test case 0, 1.
        box.Reset();
        output = Activate(box, 0.0, 1.0);
        success &= output > 0.5;
        fitness += 1.0 - ((1.0 - output) * (1.0 - output));

        // Test case 1, 0.
        box.Reset();
        output = Activate(box, 1.0, 0.0);
        success &= output > 0.5;
        fitness += 1.0 - ((1.0 - output) * (1.0 - output));

        // If all four responses were correct then we add 10 to the fitness.
        if(success)
            fitness += 10.0;

        return new FitnessInfo(fitness);
    }

    #region Private Static Methods

    private static double Activate(
        IBlackBox<double> box,
        double in1, double in2)
    {
        var inputs = box.Inputs.Span;
        var outputs = box.Outputs.Span;

        // Bias input.
        inputs[0] = 1.0;

        // XOR inputs.
        inputs[1] = in1;
        inputs[2] = in2;

        // Activate the black box.
        box.Activate();

        // Read output signal.
        double output = outputs[0];
        Clip(ref output);
        Debug.Assert(output >= 0.0, "Unexpected negative output.");
        return output;
    }

    private static void Clip(ref double x)
    {
        if(x < 0.0) x = 0.0;
        else if(x > 1.0) x = 1.0;
    }

    #endregion
}
