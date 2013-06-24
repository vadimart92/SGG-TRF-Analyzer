using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cudafy.Compilers;
using Cudafy;
using Cudafy.Translator;
using Cudafy.Host;
using Cudafy.Maths.BLAS;

namespace SphericalHarmonicAnalyze.Data
{
    class CUDA_funcs
    {
        #region cuda
        public static void cudaTransposeAndMultiply(ref MathNet.Numerics.LinearAlgebra.Double.DenseMatrix dm)
        {
            Cudafy.CudafyModule km = Cudafy.Translator.CudafyTranslator.Cudafy();
            km.Serialize();
            GPGPU gpu = CudafyHost.GetDevice(eGPUType.Cuda);
            int cols = dm.ColumnCount, rows = dm.RowCount;
            dm.Storage.ToColumnMajorArray();
            double[] a = dm.ToColumnWiseArray();
            dm = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(1, 1);
            double[] dev_a = gpu.Allocate<double>(a.Length);
            GPGPUBLAS blas = GPGPUBLAS.Create(gpu);
            double[] a_d = gpu.CopyToDevice<double>(a);
            double[] c_d = gpu.Allocate<double>(cols * cols);
            gpu.StartTimer();
            blas.GEMM(cols, rows, cols, 1, a_d, a_d, 0, c_d, Cudafy.Maths.BLAS.Types.cublasOperation.T);
            a = new double[cols * cols];
            gpu.CopyFromDevice<double>(c_d, a);
            gpu.FreeAll();
            dm = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(cols, cols, a);
        }
        public static void cudaTranspose(ref MathNet.Numerics.LinearAlgebra.Double.DenseMatrix dm)
        {
            
            GPGPU gpu = CudafyHost.GetDevice(eGPUType.Cuda);
            
            GPGPUBLAS blas = GPGPUBLAS.Create(gpu);
            
            int cols = dm.ColumnCount, rows = dm.RowCount;
            int restRows = rows - cols; 
            //double[] a = dm.Storage.ToColumnMajorArray();
            double[] a = dm.SubMatrix(0, cols, 0, cols).Storage.ToColumnMajorArray();
            double[] b = dm.SubMatrix(cols, restRows, 0, cols).Storage.ToColumnMajorArray();
            dm = null;

            double[] a_d = gpu.CopyToDevice<double>(a);
            a = null;
            double[] c_d = gpu.Allocate<double>(cols * cols);
            double[] x_d = gpu.CopyToDevice<double>(new double[] { 1 });
            blas.GEMV(cols, cols, 1, c_d, x_d, 0, x_d, Cudafy.Maths.BLAS.Types.cublasOperation.T);
            a = new double[cols * rows];
            gpu.CopyFromDevice<double>(c_d, 0, a, 0, cols * cols);
            gpu.FreeAll();
            a_d = gpu.CopyToDevice<double>(b);
            b = null;
            c_d = gpu.Allocate<double>(restRows * cols);
            x_d = gpu.CopyToDevice<double>(new double[] { 1 });
            blas.GEMV(restRows, cols, 1, c_d, x_d, 0, x_d, Cudafy.Maths.BLAS.Types.cublasOperation.T);
            gpu.CopyFromDevice<double>(c_d, 0, a, cols * cols, restRows * cols);
            gpu.FreeAll();
            dm = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(cols, rows, a);
        }
      public double[] transpose(double[] inputArray) {
          GPGPU gpu = CudafyHost.GetDevice(eGPUType.Cuda);
          CudafyModule km = CudafyTranslator.Cudafy(eArchitecture.sm_35);
          gpu.LoadModule(km);
          dim3 grid = new dim3(1000);
          gpu.Launch();
          return new double[1];
      }
        [Cudafy]
        public void transposeNative(GThread thread, double[] odata, double[] idata, int rows, int cols)
        {
            odata[thread.threadIdx.x] = (double)thread.gridDim.x+ (double)thread.threadIdx.x/100;
        }
        #endregion
    }
}
