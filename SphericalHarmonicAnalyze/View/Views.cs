using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SphericalHarmonicAnalyze.View
{
    class Views
    {
        public static string printDoubleArray(double[] array) {
            StringBuilder sb = new StringBuilder();
            foreach (var element in array)
            {
                sb.AppendLine(element.ToString());
            }
            return sb.ToString();
        }
        public static string getDataForGreed(ref double[][] greed, ref List<int>[] map, ref double[][] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < greed.Length; i++)
            {
                sb.AppendFormat("Colat:{0}\tLong:{1}\r\n", greed[i][0], greed[i][1]);
                foreach (var v in map[i])
                {
                    sb.AppendFormat("Fi:{0},La:{1} ", data[v][1], data[v][2]);
                }
                i++;
            }
            return sb.ToString();
        }
        
    }
}
