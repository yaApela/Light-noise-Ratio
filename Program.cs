using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace OSH
{
    class Program
    {
        static void calcSNR(List<double> A, List<double> phi, List<double> tau, double Tp, double Ti, int Q, double PSD, double deltat, out double[] kappaP, out double[] kappaI, out double[] delay, out double q)
        {
            double Tmin = tau.Min();
            double Tmax = tau.Max();
            double LDouble = (Tmax - Tmin) / deltat + 1;
            long L = checked((long)Math.Floor(LDouble));
            int j = 0;
            double[] B = new double[L];
            double[] psi = new double[L];
            kappaP = new double[L];
            kappaI = new double[L];
            delay = new double[L];

            for (int i = 0; i < L; i++)
            {
                int[] RaysInd = tau.Select((t, index) => new { Index = index, Value = t }).Where(x => x.Value >= Tmin + i * deltat && x.Value < Tmin + (i + 1) * deltat).Select(x => x.Index).ToArray();
                if (RaysInd.Length > 0)
                {
                    j++;
                    B[j - 1] = Math.Sqrt(Math.Pow(A.Where((a, index) => RaysInd.Contains(index)).Select((a, index) => a * Math.Cos(phi[index])).Sum(), 2) + Math.Pow(A.Where((a, index) => RaysInd.Contains(index)).Select((a, index) => a * Math.Sin(phi[index])).Sum(), 2));
                    psi[j - 1] = Math.Atan(-A.Where((a, index) => RaysInd.Contains(index)).Select((a, index) => a * Math.Sin(phi[index])).Sum() / A.Where((a, index) => RaysInd.Contains(index)).Select((a, index) => a * Math.Cos(phi[index])).Sum());
                    kappaP[j - 1] = Math.Pow(B[j - 1], 2) * Tp / (2 * Math.Pow(PSD, 2));
                    delay[j - 1] = tau.Where((t, index) => RaysInd.Contains(index)).Average();
                }
            }

            int[] mq = Enumerable.Range(1, j).ToArray();
            for (int i = 0; i < j; i++)
            {
                int[] nq = mq.Where(x => x != i + 1).ToArray();
                double PSDadded = nq.Select(x => Math.Pow(B[x - 1], 2)).Sum() / 2 * deltat;
                kappaI[i] = Math.Pow(B[i], 2) * Ti / (2 * (Math.Pow(PSD, 2) + PSDadded));
            }

            L = j;
            Array.Sort(B);
            Array.Reverse(B);
            if (Q > B.Length) Q = B.Length;
            mq = Enumerable.Range(1, Q).ToArray();
            double[] Interf = new double[L];
            for (int i = 0; i < L; i++)
            {
                int[] nq = mq.Where(x => x != i + 1).ToArray();
                Interf[i] = nq.Select(x => Math.Pow(B[x - 1], 2)).Sum() * Math.Pow(B[i], 2) * Ti / (2 * (1 / deltat));
            }

            q = Math.Pow(B.Where((b, index) => mq.Contains(index + 1)).Select(b => Math.Pow(b, 2)).Sum() * Ti, 2) / (B.Where((b, index) => mq.Contains(index + 1)).Select(b => Math.Pow(b, 2)).Sum() * Ti * Math.Pow(PSD, 2) + Interf.Sum());

        }
    }
}

