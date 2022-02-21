using System;
using System.Collections.Generic;
using DDR;

//This code is written by Andrew Polar and Mike Poluektov. It demos building of stochastic artificial intelligence
//models based on Kolmogorov-Arnold representation.

namespace USt2
{
    class Program
    {
        static void Main(string[] args)
        {
            int TrainingSize = 10000;
            double DataError = 0.5;
            DataHolder dhTraining = new DataHolder();
            dhTraining.BuildFormulaData(DataError, TrainingSize);

            Resorter resorter = new Resorter(dhTraining._inputs, dhTraining._target);
            resorter.Resort(1);
            resorter.Resort(2);
            resorter.Resort(3);
            resorter.Resort(5);
            resorter.Resort(7);
            resorter.Resort(11);

            Ensemble ensemble = new Ensemble(Resorter._inputs, Resorter._target);
            ensemble.BuildModels(12);

            //Test accuracy
            int KSTRejected = 0;
            List<double> meanForEachMonteCarlo = new List<double>();
            List<double> meanForEachDDR = new List<double>();
            List<double> singleOutput = new List<double>();
            List<double> lower50PercentDDR = new List<double>();
            List<double> upper50PercentDDR = new List<double>();
            List<double> lower50PercentMonteCarlo = new List<double>();
            List<double> upper50PercentMonteCarlo = new List<double>();

            int N = 100;
            for (int i = 0; i < N; ++i)
            {
                double[] randomInput = dhTraining.GetRandomInput();
                double[] MonteCarloOutput = dhTraining.GetStatData(randomInput, 200);

                singleOutput.Add(MonteCarloOutput[0]);
                Array.Sort(MonteCarloOutput);

                meanForEachMonteCarlo.Add(Static.GetMean(MonteCarloOutput));
 
                double[] votes = ensemble.GetSortedOutput(randomInput);
                lower50PercentDDR.Add(votes[3]);
                upper50PercentDDR.Add(votes[8]);
                lower50PercentMonteCarlo.Add(MonteCarloOutput[50]);
                upper50PercentMonteCarlo.Add(MonteCarloOutput[150]);

                meanForEachDDR.Add(Static.GetMean(votes));

                if (true == Static.KSTRejected005(MonteCarloOutput, votes))
                {
                    ++KSTRejected;
                }
            }
            Console.WriteLine("Pearson for DDR expectation and target = {0:0.0000}", Static.PearsonCorrelation(meanForEachDDR.ToArray(), 
                singleOutput.ToArray()));

            Console.WriteLine("Pearson for DDR expectation and Monte-Carlo expectation {0:0.0000}", 
                Static.PearsonCorrelation(meanForEachMonteCarlo.ToArray(), meanForEachDDR.ToArray()));
            Console.WriteLine("Kolmogorov-Smirnov rejected data sets {0} out of {1}", KSTRejected, N);

            Console.WriteLine("Pearson for lower border of 50% of DDR and Monte-Carlo {0:0.0000}",
                Static.PearsonCorrelation(lower50PercentDDR.ToArray(), lower50PercentMonteCarlo.ToArray()));

            Console.WriteLine("Pearson for upper border of 50% of DDR and Monte-Carlo {0:0.0000}",
                Static.PearsonCorrelation(upper50PercentDDR.ToArray(), upper50PercentMonteCarlo.ToArray()));
        }
    }
}
