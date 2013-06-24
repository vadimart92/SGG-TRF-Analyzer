using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SphericalHarmonicAnalyze
{
    class LUDenceMatrix:MathNet.Numerics.LinearAlgebra.Double.Factorization.LU
    {
        MathNet.Numerics.LinearAlgebra.Double.DenseMatrix dm { get; set; }
    }
}
