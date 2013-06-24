using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SphericalHarmonicAnalyze.Data;
using System.Threading.Tasks;
namespace SphericalHarmonicAnalyze.View
{
    class test
    {
        public static void checkMap(double[][] SGG_data, List<int>[] map, List<double[]> greed, ReferenceSystem el) {

            Parallel.For(0, map.Length, (i) =>
              {
                  if (map[i].Count > 0)
                  {
                      foreach (int p in map[i])
                      {
                          double d_phi = greed[i][0] - SGG_data[p][1];
                          double d_lam = greed[i][1] - SGG_data[p][2];
                          if (d_phi > MathFunc.deg2rad(el.gridParameters.cellSize) || d_lam > MathFunc.deg2rad(el.gridParameters.cellSize))
                          {
                              System.Windows.Forms.MessageBox.Show(string.Format("Fuck!\r\np{0},l{1}__{2}", MathFunc.rad2deg(d_phi), MathFunc.rad2deg(d_lam),i));
                          }
                      }

                  };
              });
        
        }
    }
}
