using System;
using System.Collections.Generic;
using System.Text;

namespace DDR
{
    class Ensemble
    {
        private List<double[]> _inputs = null;
        private List<double> _target = null;
        private KolmogorovModel[] km = null;

        public Ensemble(List<double[]> inputs, List<double> target)
        {
            _inputs = inputs;
            _target = target;
        }

        public void BuildModels(int NBlocks)
        {
            km = new KolmogorovModel[NBlocks];
            int inputlen = _inputs[0].Length;
            int blocksize = _inputs.Count / NBlocks;
            if (0 != _inputs.Count % NBlocks)
            {
                blocksize += 1;
            }
            List<double[]> blockinput = new List<double[]>();
            List<double> blocktarget = new List<double>();
            int counter = 0;
            int blockIndex = 0;
            for (int j = 0; j < _inputs.Count; ++j)
            {
                double[] x = new double[inputlen];
                for (int k = 0; k < inputlen; ++k)
                {
                    x[k] = _inputs[j][k];
                }
                double t = _target[j];

                blockinput.Add(x);
                blocktarget.Add(t);

                if (++counter >= blocksize || j >= _inputs.Count - 1)
                {
                    km[blockIndex] = new KolmogorovModel(blockinput, blocktarget, new int[] { 5, 5, 5, 5, 5 });
                    int NLeaves = 5;
                    int[] linearBlocksPerRootInput = new int[NLeaves];
                    for (int m = 0; m < NLeaves; ++m)
                    {
                        linearBlocksPerRootInput[m] = 7;
                    }
                    km[blockIndex].GenerateInitialOperators(NLeaves, linearBlocksPerRootInput);
                    km[blockIndex].BuildRepresentation(100, 0.02, 0.02);
                    //Console.WriteLine("Block model correlation koeff {0:0.00}", km[blockIndex].ComputeCorrelationCoeff());
 
                    blockinput.Clear();
                    blocktarget.Clear();
                    counter = 0;
                    ++blockIndex;
                }
            }
            //Console.WriteLine();
        }

        public double[] GetSortedOutput(double[] x)
        {
            List<double> y = new List<double>();
            for (int i = 1; i < km.Length - 1; ++i)
            {
                y.Add(km[i].ComputeOutput(x));
            }
            y.Sort();
            return y.ToArray();
        }

        public double EstimateTrainingAccuracy()
        {
            double RMSE = 0.0;
            for (int i = 0; i < _inputs.Count; ++i)
            {
                double[] output = GetSortedOutput(_inputs[i]);
                double mean = 0.0;
                foreach (double d in output)
                {
                    mean += d;
                }
                mean /= (double)(output.Length);
                RMSE += (_target[i] - mean) * (_target[i] - mean);
            }
            RMSE /= (double)(_inputs.Count);
            RMSE = Math.Sqrt(RMSE);
            return RMSE;
        }
    }
}
