using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SphericalHarmonicAnalyze.Data
{
    /// <summary>
    /// Клас реалізує зчитування і вивід інформації в різні формати файлів:
    /// текстові - для наочності, бінарні для оптимізації швидкості
    /// </summary>
   public class IOFunc
    {
        public static void writeMatrixToFile(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix matrix, string file, System.Windows.Forms.RichTextBox tb1, MainForm frm){
            System.IO.Stream fileStream = new System.IO.FileStream(file,System.IO.FileMode.Create);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fileStream, System.Text.Encoding.Default);
            double[] arr =  matrix.ToColumnWiseArray();
            System.IO.TextWriter tr = new System.IO.StreamWriter(file + ".metainfo");
            tr.WriteLine("Column count");
            tr.WriteLine(matrix.ColumnCount);
            tr.WriteLine("Row count");
            tr.WriteLine(matrix.RowCount);
            tr.Flush();
            tr.Close();
            int len = arr.Length;
            matrix = null;
            int index=0;
            byte[] bytes = new byte[arr.Length*8];
            foreach (double item in arr)
            {
                BitConverter.GetBytes(item).CopyTo(bytes,index);
                if (index / 8 / 500D == Math.Round(index / 8 / 500D)) { tb1.BeginInvoke(new MainForm.setProgressDel(frm.addVal), new object[] { (int)index / 8, len, "Записано" }); }
                index+=8;
            }
            bw.Write(bytes);
            bw.Flush(); 
            tb1.BeginInvoke(new MainForm.setProgressDel(frm.addVal), new object[] { (int)0, len, "" });
            bw.Flush();
            bw.Close();
            fileStream.Close();
        }
        public static void writeVectorToFile(MathNet.Numerics.LinearAlgebra.Double.DenseVector vector, string file, System.Windows.Forms.RichTextBox tb1, MainForm frm)
        {
            System.IO.Stream fileStream = new System.IO.FileStream(file, System.IO.FileMode.Create);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fileStream, System.Text.Encoding.Default);
            double[] arr = vector.Values;
            System.IO.TextWriter tr = new System.IO.StreamWriter(file + ".metainfo");
            tr.WriteLine("Items count");
            tr.WriteLine(vector.Count);
            tr.Flush();
            tr.Close();
            int len = arr.Length;
            vector = null;
            int index = 0;
            byte[] bytes = new byte[arr.Length * 8];
            foreach (double item in arr)
            {
                BitConverter.GetBytes(item).CopyTo(bytes, index);
                if (index / 8 / 500D == Math.Round(index / 8 / 500D)) { tb1.BeginInvoke(new MainForm.setProgressDel(frm.addVal), new object[] { (int)index / 8, len, "Записано" }); }
                index += 8;
            }
            bw.Write(bytes);
            bw.Flush();
            tb1.BeginInvoke(new MainForm.setProgressDel(frm.addVal), new object[] { (int)0, len, "" });
            bw.Flush();
            bw.Close();
            fileStream.Close();
        }
       public static MathNet.Numerics.LinearAlgebra.Double.DenseMatrix readDenceMatrixFromBinFile(string file,MainForm.setProgressDel d) {
            string[] meta = System.IO.File.ReadAllLines(file + ".metainfo");
            System.IO.FileStream fs = new System.IO.FileStream(file,System.IO.FileMode.Open);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs,System.Text.Encoding.Default);
            int cols = int.Parse(meta[1]);
            int rows = int.Parse(meta[3]);
            MathNet.Numerics.LinearAlgebra.Double.DenseMatrix dm = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(rows,cols);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    dm[i, j] = br.ReadDouble();
                }
                d.Invoke(br.BaseStream.Position / br.BaseStream.Length, 1, "Завантаження");
            }
            br.Close();
            fs.Close();
            return dm;
        }
        public static MathNet.Numerics.LinearAlgebra.Double.DenseVector readDenceVectorFromBinFile(string file, MainForm.setProgressDel d)
        {
            string[] meta = System.IO.File.ReadAllLines(file + ".metainfo");
            System.IO.FileStream fs = new System.IO.FileStream(file, System.IO.FileMode.Open);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs, System.Text.Encoding.Default);
            int cols = int.Parse(meta[1]);
            MathNet.Numerics.LinearAlgebra.Double.DenseVector dv = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(cols);
                for (int j = 0; j < cols; j++)
                {
                    dv[j] = br.ReadDouble();
                    d.Invoke(br.BaseStream.Position / br.BaseStream.Length, 1, "Завантаження");
                }   
            br.Close();
            fs.Close();
            fs.Dispose(); 
            return dv;
        }
       //Зчитування даних з файлу .SGG
        public static double[][] read_SGG_data(string folder, MainForm.setProgressDel d) {
            System.IO.FileInfo fi = new FileInfo(folder);
            folder = fi.DirectoryName;
            List<string> files = new List<string>();
           foreach (var file in System.IO.Directory.GetFiles(folder))
            {
               var f = new FileInfo(file);
               if (f.Extension.Equals(".SGG",System.StringComparison.CurrentCultureIgnoreCase)) { files.Add(file); };  
            }
           List<double[]> myList = new List<double[]>(getLines(files[0]));
           if (files.Count > 0) { 
            foreach (string filename in files)
            {
              string[] lines = System.IO.File.ReadAllLines(filename);
            var x= Parallel.For (0,lines.Length,(i)=>{
                double[] temp = null;
                double lat = 0; int l = 0;
                string[] line = null; 
                lock (lines)
                {
                    line = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
                if (line.Length > 20)
                {
                    lat = double.Parse(line[3]);
                   //if (lat < 0) lat += 360;
                    lat = MathFunc.deg2rad(lat);
                    temp = new double[] { double.Parse(line[1]), MathFunc.deg2rad(90 - double.Parse(line[2])), lat, double.Parse(line[6]) };
                   
                        if (Math.Abs( temp[1]) < Math.PI && Math.Abs(temp[2]) < 2D * Math.PI)
                        {
                             lock (myList) {myList.Add(temp);}
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Помилка в лінії номер " + i.ToString());
                        }
                    l++;
                    if (Math.Round(l / 20000D) == (double)l/20000D) { d.Invoke(l / lines.Length, 1, "Обробка файлу вимірюваннь"); };
                }
            });  
            }           
           };
           return myList.ToArray();
        }
        public static void write_SGG_data(string fn, double[][] data) {
            System.IO.TextWriter tr = new System.IO.StreamWriter(fn,false,System.Text.Encoding.Default);
            foreach (double[] item in data)
            {
                tr.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}",item[0],item[1],item[2],item[3]));
            }
            tr.Flush();
            tr.Close();
        }
        public static void binwrite_SGG_data(string fn, double[][] data) {
            System.IO.Stream file = System.IO.File.Open(fn, System.IO.FileMode.Create);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(file);
            foreach (var line in data)
            {
                foreach (var item in line)
                {
                    file.Write(BitConverter.GetBytes(item), 0, 8);
                }
                
            }
            bw.Flush();
            bw.Close();
            file.Close();

        }
        public static double[][] binLoad_SGG_data(string fn)
        {
            System.IO.FileInfo fi =new System.IO.FileInfo(fn);
            long size = fi.Length/8/4;
            System.IO.Stream file = System.IO.File.Open(fn, System.IO.FileMode.Open);
            System.IO.BinaryReader br = new System.IO.BinaryReader(file);
            double[][] d = new double[size][];
           int nsize = 0;
            System.Text.DecoderReplacementFallbackBuffer fb = new DecoderReplacementFallbackBuffer(new DecoderReplacementFallback("x"));
            while ((br.BaseStream.Length - br.BaseStream.Position) > 8)
            {
                double d1=br.ReadDouble(),d2=br.ReadDouble(),d3=br.ReadDouble(),d4=br.ReadDouble();
                d[nsize] = new double[] { d1,d2,d3,d4};
                nsize++;
            }
            br.Close();
            file.Close();
            return d;
        }
        private static int getLines(string fn) {
            System.IO.TextReader tr = new System.IO.StreamReader(fn);
            int c = 0;
            while (tr.Peek() != (-1) && tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length < 15)
            {
                c++;
            }
          return  System.IO.File.ReadAllLines(fn).Length - c;
        }
        public static double[][] read_SGG_data_cad(string[] filenames)
        {
            List<double[]> myList = new List<double[]>();
            Parallel.ForEach<string>(filenames, (f) =>
            {
            System.IO.TextReader tr = new System.IO.StreamReader(f);
            string[] line = null;
            double[] temp = null;
            do
            {
                line = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (line.Length > 20)
                {
                    temp = new double[] { double.Parse(line[1]), double.Parse(line[2]), double.Parse(line[3]), double.Parse(line[6]) };
                    lock (myList)
                    {
                        myList.Add(temp);
                    } 
                }
            } while (tr.Peek() != (-1));
            tr.Close();
            });
            return myList.ToArray();
        }
        public static string matrixToString(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix dm, ReferenceSystem rs) {
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 21; i++)
            {
                sb.AppendLine();
            }
            for (int i = 2; i <= rs.maxDegree; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (j != 0)
                    {
                        sb.AppendLine(string.Format("{0}\t{1}\t{2:0.000000000000e+00}\t{3:0.000000000000e+00}", i, j, dm.Values[index], dm.Values[index + 1]));
                        index += 2;
                    }
                    else {
                        sb.AppendLine(string.Format("{0}\t{1}\t{2:0.000000000000e+00}\t{3:0.000000000000e+00}", i, j, dm.Values[index], 0));
                        index += 1;
                    }
                }
            }
            return sb.ToString();
        }
        public static void writeModeVectorlToTxtFile(MathNet.Numerics.LinearAlgebra.Double.DenseVector dm, ReferenceSystem rs, string file) {
            System.IO.File.WriteAllText(file, matrixToString((MathNet.Numerics.LinearAlgebra.Double.DenseMatrix)dm.ToColumnMatrix(), rs), System.Text.Encoding.Default);
        }
        public static void writeMatrixToTxtFile(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix dm, string file) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dm.RowCount; i++)
            {
                foreach (var item in dm.SubMatrix(i,1,0,dm.ColumnCount).ToColumnWiseArray())
	                {
                        sb.AppendFormat("{0,5:0.0000000000000e+00};",item);
	                }
                sb.Append("\r\n");
            }
            System.IO.File.WriteAllText(file, sb.ToString());
        }
        public static void writeVectorToTxtFile(MathNet.Numerics.LinearAlgebra.Double.DenseVector dm, string file)
        {
            StringBuilder sb = new StringBuilder();
            
                foreach (var item in dm.Values)
                {
                    sb.AppendFormat("{0,5:0.0000000000000e+00}\r\n", item);
                }
            System.IO.File.WriteAllText(file, sb.ToString());
        }
        public static void writeGreedToCsvFile(List<double[]> greed, string file) {
            System.IO.TextWriter tw = new System.IO.StreamWriter(file,false);
            int i=0;
            foreach (var item in greed)    
            {
                tw.WriteLine("{2:0.000};{1:0.000};{0}", i, MathFunc.rad2deg((Math.PI / 2D) - item[0]), MathFunc.rad2deg((item[1] < Math.PI) ? item[1] : item[1] - Math.PI * 2D));
                i++;
            }
            tw.Flush();
            tw.Close();
        }
        //Записує кількість вимірів для кожної клітинки сітки
        public static void writeGreedToCsvFileWithMeasureCount(List<double[]> greed, List<int>[] map, string file)
        {
            System.IO.TextWriter tw = new System.IO.StreamWriter(file, false);
            int i = 0;
            int ii = 0;
            foreach (var item in greed)
            {
                if (item[1] > Math.PI) { ii++; };
                tw.WriteLine("{2:0.000};{1:0.000};{0}", map[i].Count, MathFunc.rad2deg((Math.PI / 2D) - item[0]), MathFunc.rad2deg(item[1] - Math.PI));
                i++;
            }
            tw.Flush();
            tw.Close();
        }
        //Записує усереднені градієнти для кожної клітинки сітки 
        public static void writeGreedToCsvFileWithMeasureS(List<double[]> greed,double[] data, string file)
        {
            System.IO.TextWriter tw = new System.IO.StreamWriter(file, false);
            int i = 0;
            foreach (var item in greed)
            {
                tw.WriteLine("{2:0.000};{1:0.000};{0}", data[i], MathFunc.rad2deg((Math.PI / 2D) - item[0]), MathFunc.rad2deg(item[1] - Math.PI));
                i++;
            }
            tw.Flush();
            tw.Close();
        }
        public static void writeMatrixToMatLabFile(MathNet.Numerics.LinearAlgebra.Generic.Matrix<double> matrix, string file,string name) {
            MathNet.Numerics.LinearAlgebra.IO.MatlabMatrixWriter wr = new MathNet.Numerics.LinearAlgebra.IO.MatlabMatrixWriter(file);
            wr.WriteMatrix(matrix, name);
            wr.Close();
            }
       //Запис висот геоїда і аномалій для сітки в текстовий файл  
       public static void writeGeoidHeightsAndAnomalysToTxt(List<double[]> greed, double[] heights, double[] anomalies,ReferenceSystem rs, string file) {
            System.IO.TextWriter w = new System.IO.StreamWriter(file, false);
            for (int i = 0; i < greed.Count; i++)
            {
            w.WriteLine(string.Format("{0} {1} {2,10:0.0000} {3,10:0.0000}",MathFunc.rad2deg(MathFunc.convertThethaToB(greed[i][0], rs)),MathFunc.rad2deg(greed[i][1]), heights[i], anomalies[i]));
            }
            w.Close();
        }
        // Запис моделі в текстовий файл у формат gfc
        public static void writeGravityModelToTxtFile(GravityModel gm, string file) {
            TextWriter w = new StreamWriter(file, false);
            w.WriteLine("product_type                gravity_field");
            w.WriteLine("modelname                   UGM_VA_13");
            w.WriteLine(string.Format("earth_gravity_constant                   {0}", gm.model_GM));
            w.WriteLine(string.Format("radius                   {0}", gm.model_a));
            w.WriteLine(string.Format("max_degree                   {0}", gm.maxDegree));
            for (int n = 0; n <= gm.maxDegree; n++)
            {
                for (int m = 0; m <= n; m++)
                {
                    w.WriteLine("gfc     {0}\t{1}\t{2:0.000000000000e+00}\t{3:0.000000000000e+00}", n, m, gm.c_coef[n][m], gm.s_coef[n][m]);
                }
            }
            w.Close();
        }
   }
}
