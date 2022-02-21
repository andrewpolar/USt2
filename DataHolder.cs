using System;
using System.Collections.Generic;
using System.Text;

namespace USt2
{
    interface IDataGenerator
    {
        public double Formula(double[] x);
        public double[] GetRandomInput();
        public double[] AddNoise(double[] x, double magnitude, out double datanorm, out double errornorm);
    }

    class DataGenerator1 : IDataGenerator
    {
        Random _rnd = new Random();

        public double Formula(double[] x)
        {
            //y = (1/pi)*(2+2*x3)*(1/3)*(atan(20*exp(x5)*(x1-0.5+x2/6))+pi/2) + (1/pi)*(2+2*x4)*(1/3)*(atan(20*exp(x5)*(x1-0.5-x2/6))+pi/2);
            double pi = 3.14159265359;
            if (5 != x.Length)
            {
                Console.WriteLine("Formala error");
                Environment.Exit(0);
            }
            double y = (1.0 / pi);
            y *= (2.0 + 2.0 * x[2]);
            y *= (1.0 / 3.0);
            y *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 + x[1] / 6.0)) + pi / 2.0;

            double z = (1.0 / pi);
            z *= (2.0 + 2.0 * x[3]);
            z *= (1.0 / 3.0);
            z *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 - x[1] / 6.0)) + pi / 2.0;

            return y + z;
        }

        public double[] GetRandomInput()
        {
            double[] x = new double[5];
            x[0] = (_rnd.Next() % 100) / 100.0;
            x[1] = (_rnd.Next() % 100) / 100.0;
            x[2] = (_rnd.Next() % 100) / 100.0;
            x[3] = (_rnd.Next() % 100) / 100.0;
            x[4] = (_rnd.Next() % 100) / 100.0;
            return x;
        }

        public double[] AddNoise(double[] x, double magnitude, out double datanorm, out double errornorm)
        {
            double[] z = new double[5];
            z[0] = x[0] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[1] = x[1] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[2] = x[2] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[3] = x[3] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[4] = x[4] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);

            datanorm = 0.0;
            foreach (double d in x)
            {
                datanorm += d * d;
            }
            datanorm /= x.Length;
            datanorm = Math.Sqrt(datanorm);

            errornorm = 0.0;
            for (int i = 0; i < x.Length; ++i)
            {
                errornorm += (x[i] - z[i]) * (x[i] - z[i]);
            }
            errornorm /= x.Length;
            errornorm = Math.Sqrt(errornorm);

            return z;
        }
    }

    class DataHolder
    {
        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        public double _noise = 0.0;
        private IDataGenerator iData = new DataGenerator1(); //this is switch between different datasets

        public double[] GetRandomInput()
        {
            return iData.GetRandomInput();
        }

        public double GetExactOutput(double[] input)
        {
            return iData.Formula(input);
        }

        public void ShowInputs(int category)
        {
            int N = _inputs.Count;
            for (int i = 0; i < N; ++i)
            {
                foreach (double d in _inputs[i])
                {
                    Console.Write("{0:0.00} ", d);
                }
                Console.Write(" | {0:0.00} ", _target[i]);
                Console.WriteLine();
            }
        }

        public double[] GetStatData(double[] x, int N)
        {
            List<double> y = new List<double>();
            for (int i = 0; i < N; ++i)
            {
                double datanorm;
                double errornorm;
                y.Add(iData.Formula(iData.AddNoise(x, _noise, out datanorm, out errornorm)));
            }
            return y.ToArray();
        }

        public void BuildFormulaData(double noise, int N)
        {
            _noise = noise;
            _inputs.Clear();
            _target.Clear();

            double alldatanorm = 0.0;
            double allerrornorm = 0.0;

            for (int i = 0; i < N; ++i)
            {
                double datanorm;
                double errornorm;
                double[] x = GetRandomInput();
                double y = iData.Formula(iData.AddNoise(x, noise, out datanorm, out errornorm));

                alldatanorm += datanorm;
                allerrornorm += errornorm;

                _inputs.Add(x);
                _target.Add(y);
            }

            Console.WriteLine("Data is generated, average relative error {0:0.0000}", allerrornorm / alldatanorm);
        }
    }
}

