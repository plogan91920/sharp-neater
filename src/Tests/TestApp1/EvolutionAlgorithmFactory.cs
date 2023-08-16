using SharpNeat.Neat.EvolutionAlgorithm;
using SharpNeat.Tasks.BinaryElevenMultiplexer;
using SharpNeat.Tasks.BinarySixMultiplexer;
using SharpNeat.Tasks.CartPole.DoublePole;
using SharpNeat.Tasks.CartPole.SinglePole;
using SharpNeat.Tasks.GenerativeFunctionRegression;
using SharpNeat.Tasks.PreyCapture;
using SharpNeat.Tasks.Xor;

namespace TestApp1;

public static class EvolutionAlgorithmFactory
{
    public static NeaterEvolutionAlgorithm<double> CreateNeatEvolutionAlgorithm_Xor()
    {
        var experimentFactory = new XorExperimentFactory();
        return Utils.CreateNeatEvolutionAlgorithm(experimentFactory);
    }

    public static NeaterEvolutionAlgorithm<double> CreateNeatEvolutionAlgorithm_Binary6()
    {
        var experimentFactory = new BinarySixMultiplexerExperimentFactory();
        return Utils.CreateNeatEvolutionAlgorithm(experimentFactory);
    }

    public static NeaterEvolutionAlgorithm<double> CreateNeatEvolutionAlgorithm_Binary11()
    {
        var experimentFactory = new BinaryElevenMultiplexerExperimentFactory();
        return Utils.CreateNeatEvolutionAlgorithm(experimentFactory);
    }

    public static NeaterEvolutionAlgorithm<double> CreateNeatEvolutionAlgorithm_GenerativeSinewave()
    {
        var experimentFactory = new GenerativeFnRegressionExperimentFactory();
        return Utils.CreateNeatEvolutionAlgorithm(experimentFactory);
    }

    public static NeaterEvolutionAlgorithm<double> CreateNeatEvolutionAlgorithm_CartSinglePole()
    {
        var experimentFactory = new CartSinglePoleExperimentFactory();
        return Utils.CreateNeatEvolutionAlgorithm(experimentFactory);
    }

    public static NeaterEvolutionAlgorithm<double> CreateNeatEvolutionAlgorithm_CartDoublePole()
    {
        var experimentFactory = new CartDoublePoleExperimentFactory();
        return Utils.CreateNeatEvolutionAlgorithm(experimentFactory);
    }

    public static NeaterEvolutionAlgorithm<double> CreateNeatEvolutionAlgorithm_PreyCapture()
    {
        var experimentFactory = new PreyCaptureExperimentFactory();
        return Utils.CreateNeatEvolutionAlgorithm(experimentFactory);
    }
}
