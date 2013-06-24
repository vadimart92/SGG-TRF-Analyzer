using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SphericalHarmonicAnalyze.Data;
using System.Threading.Tasks;

namespace SphericalHarmonicAnalyze.Data
{ 
    /// <summary>
    /// Клас GravityModel реалізує зручне зберігання моделі в пам'яті,
    /// а також операції над моделями та обчислення трансформант
    /// </summary>
   public class GravityModel
    {   
        public double[][] c_coef { get; private set; } //коефіцієнти c_nm
        public double[][] s_coef { get; private set; }//коефіцієнти s_nm
        public int maxDegree { get; private set; }//макс. ступінь і порядок
        public double model_GM { get; private set; }//Константа GM моделі
        public double model_a { get; private set; }//Константа a (екваторіальний радіус) моделі
        public GravityModel(int maxDegree) {
            this.maxDegree = maxDegree;
            this.c_coef = new double[maxDegree+1][];
            this.s_coef = new double[maxDegree+1][];
            this.model_a = double.NaN;
            this.model_GM = double.NaN;
        }
        public GravityModel(GravityModel other, double a, double GM)
        {
            this.c_coef = new double[other.c_coef.Length][];
            this.s_coef = new double[other.s_coef.Length][];
            for (int n = 0; n < other.c_coef.Length; n++)
            {
                this.c_coef[n] = new double[n + 1];
                this.s_coef[n] = new double[n + 1];
                for (int m = 0; m < other.c_coef[n].Length; m++)
                {
                    this.c_coef[n][m] = other.c_coef[n][m];
                    this.s_coef[n][m] = other.s_coef[n][m];
                }
            }
            this.model_a = a;
            this.model_GM = GM;
            this.maxDegree = other.maxDegree;
        }
        public GravityModel(GravityModel other)
        {
            this.c_coef = new double[other.c_coef.Length][];
            this.s_coef = new double[other.s_coef.Length][];
            for (int n = 0; n < other.c_coef.Length; n++)
            {
                this.c_coef[n]= new double[n+1];
                this.s_coef[n] = new double[n + 1];
                for (int m = 0; m < other.c_coef[n].Length; m++)
                {
                    this.c_coef[n][m] = other.c_coef[n][m];
                    this.s_coef[n][m] = other.s_coef[n][m];
                }}
            this.model_a = other.model_a;
            this.model_GM = other.model_GM;
            this.maxDegree = other.maxDegree;
        }
        //завантаження моделі з файлу "file"
        public void loadFromFile(string file, MainForm.setProgressDel d) {
            if (System.IO.File.Exists(file)){
                setLine();
                System.IO.TextReader tr = new System.IO.StreamReader(file);
            string line = null;
            string[] line_a = null;
            int l = 0;
            string s = " \t"; bool nfl = true;
                while( nfl && tr.Peek()!=(-1)){
                    l++;
                    line = tr.ReadLine();
                    line_a = line.Split(s.ToArray<char>(),StringSplitOptions.RemoveEmptyEntries);
                    if (line_a.Length > 4 && line_a[0].Equals("gfc", System.StringComparison.CurrentCultureIgnoreCase) && (line_a[1].Equals("0") || line_a[1].Equals("2")) && line_a[2] == "0")
                    {nfl = false;}
                    else if (line_a.Length == 2) {
                        if (line_a[0].Equals("earth_gravity_constant")) { this.model_GM = double.Parse(line_a[1].Replace('D', 'e'), System.Globalization.NumberStyles.Any); }
                        if (line_a[0].Equals("radius")) { this.model_a = double.Parse(line_a[1].Replace('D', 'e'), System.Globalization.NumberStyles.Any); }
                       //if (line_a[0].Equals("max_degree")) { this.maxDegree = int.Parse(line_a[1].Replace('D', 'e'), System.Globalization.NumberStyles.Any); if (this.maxDegree > this.c_coef.Length) { GravityModel gmn = new GravityModel(maxDegree); c_coef=gmn.c_coef; s_coef=gmn.s_coef; gmn = null; }; }
                    };}
                int n = 0, m = 0, max_n = 0,progress=5, cur_p=0;
                while ((line_a.Length > 4))
                {
                    if (line_a[0].Equals("gfc", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        n = int.Parse(line_a[1]); if (n > max_n) { max_n = n; };
                        m = int.Parse(line_a[2]);
                        if (n <= maxDegree && m <= n)
                        {
                            this.c_coef[n][m] = double.Parse(line_a[3].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any);
                            this.s_coef[n][m] = double.Parse(line_a[4].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any);
                        }
                        if (((System.IO.StreamReader)tr).BaseStream.Position < ((System.IO.StreamReader)tr).BaseStream.Length)
                        {
                            line_a = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            double p = (double)((System.IO.StreamReader)tr).BaseStream.Position / (double)((System.IO.StreamReader)tr).BaseStream.Length;
                            if (p > cur_p/100d) { cur_p += progress; d.Invoke((int)(p*100), 100, "Завантаження моделі"); }
                        }
                        else { this.maxDegree = max_n; break; }
                    }
                    else if (line_a[0].Equals("gfct", System.StringComparison.CurrentCultureIgnoreCase) && line_a.Length>=8) {
                        n = int.Parse(line_a[1]);
                        m = int.Parse(line_a[2]);
                        if (n <= maxDegree && m <= n)
                        {
                            double[] gfct = new double[]{ double.Parse(line_a[3].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any),double.Parse(line_a[4].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any)};
                            string da = line_a[7];
                            DateTime date = new DateTime(int.Parse(da.Substring(0, 4)), int.Parse(da.Substring(4, 2)), int.Parse(da.Substring(6, 2)));
                            double t_t0 = (DateTime.Today - date).Days / 365d;
                            line_a = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            double[] trnd = new double[] {double.Parse(line_a[3].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any),double.Parse(line_a[4].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any)};
                            line_a = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            double[] asin1 = new double[] { double.Parse(line_a[3].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any), double.Parse(line_a[4].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any) };
                            line_a = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            double[] acos1 = new double[] { double.Parse(line_a[3].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any), double.Parse(line_a[4].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any) };
                            line_a = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            double[] asin2 = new double[] { double.Parse(line_a[3].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any), double.Parse(line_a[4].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any) };
                            line_a = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            double[] acos2 = new double[] { double.Parse(line_a[3].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any), double.Parse(line_a[4].Replace('d', 'e').Replace('D', 'e'), System.Globalization.NumberStyles.Any) };
                            this.c_coef[n][m] = gfct[0] + trnd[0] * t_t0 + asin1[0] * Math.Sin(Math.PI * 2f * t_t0) + acos1[0] * Math.Cos(Math.PI * 2f * t_t0) + asin2[0] * Math.Sin(Math.PI * t_t0) + acos2[0] * Math.Cos(Math.PI * t_t0);
                            this.s_coef[n][m] = gfct[1] + trnd[1] * t_t0 + asin1[1] * Math.Sin(Math.PI * 2f * t_t0) + acos1[1] * Math.Cos(Math.PI * 2f * t_t0) + asin2[1] * Math.Sin(Math.PI * t_t0) + acos2[1] * Math.Cos(Math.PI * t_t0);
                        }
                        if (((System.IO.StreamReader)tr).BaseStream.Position < ((System.IO.StreamReader)tr).BaseStream.Length)
                        {
                            line_a = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        else { this.maxDegree = n; break; }
                    };
                } ;
            tr.Close();
            tr.Dispose();
            }
        }
       /// <summary>
        /// Маштабування моделі для використання з Нормальною Землею "rs"
       /// </summary>
        public GravityModel rescaleModel(ReferenceSystem rs)
        {
            GravityModel output = new GravityModel(this, rs.a, rs.GM);
            double rescaleFctor = 1;
            for (int n = 0; n < this.c_coef.Length; n++)
            {
                rescaleFctor = (this.model_GM / rs.GM) * Math.Pow(this.model_a / rs.a, n);
                for (int m = 0; m < this.c_coef[n].Length; m++)
                {
                    output.c_coef[n][m] = this.c_coef[n][m] * rescaleFctor;
                    output.s_coef[n][m] = this.s_coef[n][m] * rescaleFctor;
                }
            }
            return output;
        }
        //Обчислення коефіцієнтів збурюючого потенціалу відностно Нормальної Землі "rs"
        public static GravityModel getDisturbingModel(ReferenceSystem rs, GravityModel model) {
            GravityModel result = new GravityModel(model, model.model_a, model.model_GM);
            double[] normalModel = generateNormalModel(rs,8);
            for (int i = 0; i < normalModel.Length; i+=2)
            {result.c_coef[i][0] = result.c_coef[i][0] - normalModel[i];}
            return result;
        }
        //Метод генерує масив з коефіцієнтами моделі нормального потенціалу [1,0,c_20,0,c_40,0,c_80...c_maxDegree0]
        public static double[] generateNormalModel(ReferenceSystem rs, int maxDegree = 8)
        {
            double[] normalModel = new double[maxDegree + 1];
            normalModel[0] = 1D; double C_2k = 0, k = 0, tmp = 1d - 2d / 15d * (rs.m_gamma * Math.Sqrt(rs.secondExcentricity_2)) / rs.q0;
            for (int i = 2; i <= maxDegree; i += 2)
            {
                k = i / 2;
                C_2k = Math.Pow(-1d, k) * 3d * Math.Pow(rs.firstExcentricity_2, k) / ((2d * k + 1d) * (2d * k + 3d)) * (1d - k + 5d * k / 3d * tmp) / Math.Sqrt(2d * i + 1d);
                normalModel[i - 1] = 0D;
                normalModel[i] = C_2k;
            }
            return normalModel;
        }
       /// <summary>
        /// Генерує об'єкт GravityModel з коефіцієнтами Нормальної Землі "rs"
       /// </summary>
        public static GravityModel getNormalModel(ReferenceSystem rs, int maxDegree = 8) {
            double[] m = generateNormalModel(rs, maxDegree);
            GravityModel gm = new GravityModel(maxDegree);
            gm.model_a = rs.a;
            gm.model_GM = rs.GM;
            for (int i = 0; i < m.Length; i ++)
            {
                gm.c_coef[i]=new double[i+1];
                gm.s_coef[i] = new double[i + 1];
                gm.c_coef[i][0] = m[i];
                i++;
                if (i < gm.c_coef.Length)
                {
                    gm.c_coef[i] = new double[i + 1];
                    gm.s_coef[i] = new double[i + 1];
                }}
            return gm;
        }
        //Обчислення висот геоїда для сітки (в метрах) формула обчислення нормальної сили ваги "gamma_0_formula" (за замовчуванням ф-ла Сомільяни)
        public static double[] getGeoidHeight(ReferenceSystem rs, GravityModel model, List<double[]> grid, NormalGammaFormula gamma_0_formula = NormalGammaFormula.Somigliana, MainForm.setProgressDel d = null) {
            GravityModel gm;
            if (model.model_a != rs.a || model.model_GM != rs.GM) { gm = new GravityModel(model.rescaleModel(rs), rs.a, rs.GM); } else { gm = new GravityModel(model, model.model_a, model.model_GM); };
            gm = getDisturbingModel(rs, gm);
            double[] heights = new double[grid.Count];
            int[][] t_nm = MathFunc.get_nm(gm.maxDegree);
            double[] legendrePolys_old = null; double point_old = double.MinValue; object locker = new object(), locker2 = new object();
            int position = 0;
            int count =grid.Count,position_p=(int)Math.Round(0.01d*count);
            
           Parallel.For(0, grid.Count, (pointIndex) => {
                double gamma_0 = 0, B = 0, r = 0;
                double[] legendrePolys = null;
                double[] point = grid[pointIndex];
                lock (locker2)
                        {
                            if (point_old != double.MinValue)
                            {
                                if (point[0] == point_old)
                                {
                                legendrePolys = new double[legendrePolys_old.Length];
                                legendrePolys_old.CopyTo(legendrePolys, 0);
                                }
                                MathFunc.getLegendre(gm.maxDegree, point[0], out legendrePolys);
                                legendrePolys_old = new double[legendrePolys.Length];
                                legendrePolys.CopyTo(legendrePolys_old, 0);
                                point_old = point[0];
                            }
                            else {
                                MathFunc.getLegendre(gm.maxDegree, point[0], out legendrePolys);
                                legendrePolys_old = new double[legendrePolys.Length];
                                legendrePolys.CopyTo(legendrePolys_old,0);
                                point_old = point[0];
                            }
                        }
                B = MathFunc.convertThethaToB(point[0], rs);
                r=MathFunc.getGeocentrDistanceToPointOnElips(rs,B);
                if (gamma_0_formula == NormalGammaFormula.Somigliana)
                {gamma_0 = rs.gamma_a * (1d + rs.k * Math.Pow(Math.Sin(B), 2d)) / Math.Sqrt(1d - rs.firstExcentricity_2 * Math.Pow(Math.Sin(B), 2d));}
                else { gamma_0 = 9.78030d * (1d + 0.005302 * Math.Pow(Math.Sin(B), 2d) - 0.000007 * Math.Pow(Math.Sin(2d * B), 2d)); }
                double a1, a2=0, a3,cosMlambda,sinMlambda;
                a1 = rs.GM / (r * gamma_0);
                for (int n = 0; n < gm.maxDegree; n++) {
                    a3 = 0;
                    for (int m = 0; m <= n; m++){
                        cosMlambda = Math.Cos(m * point[1]);
                        sinMlambda = Math.Sin(m * point[1]);
                        lock (gm)
                        {a3 += (gm.c_coef[n][m] * cosMlambda + gm.s_coef[n][m] * sinMlambda) * legendrePolys[MathFunc.getArraySize(n - 1) + (n - m)];}
                    }
                    a2 += Math.Pow(rs.a / r, n)*a3;
                }
                lock (heights)
                {heights[pointIndex] = a1 * a2;}
                if (d!=null)
                {
                    position++;
                    if (true || position > position_p) { lock (locker) { position_p += position_p;}; d.Invoke(position,count,"Обчислено висоти для точок: ");};
                    if (position >= count) { d.Invoke(0, 1, ""); };
                }
	            });   
            return heights;
        }
        //Обчислення аномалій сили ваги для сітки (в мГал)
        public static double[] getAnomaly(ReferenceSystem rs, GravityModel model, List<double[]> grid, MainForm.setProgressDel d = null)
        {
            GravityModel gm;
            if (model.model_a != rs.a || model.model_GM != rs.GM) { gm = new GravityModel(model.rescaleModel(rs), rs.a, rs.GM); } else { gm = new GravityModel(model, model.model_a, model.model_GM); };
            gm =  getDisturbingModel(rs, gm);
            double[] heights = new double[grid.Count];
            int[][] t_nm = MathFunc.get_nm(gm.maxDegree);
            double[] legendrePolys_old = null; double point_old = double.MinValue; object locker = new object(), locker2 = new object();
            int position = 0;
            int count = grid.Count, position_p = (int)Math.Round(0.01d * count);
            Parallel.For(0, grid.Count, (pointIndex) =>
            {
                double  B = 0, r = 0;
                double[] legendrePolys = null;
                double[] point = grid[pointIndex];
                lock (locker2)
                {
                    if (point_old != double.MinValue)
                    {
                        if (point[0] == point_old)
                        {
                            legendrePolys = new double[legendrePolys_old.Length];
                            legendrePolys_old.CopyTo(legendrePolys, 0);
                        }
                        MathFunc.getLegendre(gm.maxDegree, point[0], out legendrePolys);
                        legendrePolys_old = new double[legendrePolys.Length];
                        legendrePolys.CopyTo(legendrePolys_old, 0);
                        point_old = point[0];
                    }
                    else
                    {
                        MathFunc.getLegendre(gm.maxDegree, point[0], out legendrePolys);
                        legendrePolys_old = new double[legendrePolys.Length];
                        legendrePolys.CopyTo(legendrePolys_old, 0);
                        point_old = point[0];
                    }
                }
                B = MathFunc.convertThethaToB(point[0], rs);
                r = MathFunc.getGeocentrDistanceToPointOnElips(rs, B);
                double a1, a2 = 0, a3, cosMlambda, sinMlambda;
                a1 = rs.GM / (r * r);
                for (int n = 0; n < gm.maxDegree; n++)
                {
                    a3 = 0;
                    for (int m = 0; m <= n; m++)
                    {
                        cosMlambda = Math.Cos(m * point[1]);
                        sinMlambda = Math.Sin(m * point[1]);
                        lock (gm)
                        { a3 += (gm.c_coef[n][m] * cosMlambda + gm.s_coef[n][m] * sinMlambda) * legendrePolys[MathFunc.getArraySize(n - 1) + (n - m)]; }
                    }
                    a2 += Math.Pow(rs.a / r, n) * (n - 1) * a3;
                }
                lock (heights)
                { heights[pointIndex] = a1 * a2; }
                if (d != null)
                {
                    position++;
                    if (position > position_p) { lock (locker) { position_p += position_p; }; d.Invoke(position, count, "Обчислено висоти для точок: "); };
                    if (position >= count) { d.Invoke(0, 1, ""); };
                }
            }); 
            return heights;
        }
        public static double[][] getGeoidHeightAndAnomalys(ReferenceSystem rs, GravityModel model, List<double[]> grid, System.Threading.CancellationToken ct,System.Threading.CancellationToken ct2, NormalGammaFormula gamma_0_formula = NormalGammaFormula.Somigliana, MainForm.setProgressDel d = null)
        {
            GravityModel gm;
            if (model.model_a != rs.a || model.model_GM != rs.GM) { gm = new GravityModel(model.rescaleModel(rs), rs.a, rs.GM); } else { gm = new GravityModel(model, model.model_a, model.model_GM); };
            gm = getDisturbingModel(rs, gm);
            double[] heights = new double[grid.Count];
            double[] anomaly = new double[grid.Count];
            int[][] t_nm = MathFunc.get_nm(gm.maxDegree);
            double[] legendrePolys_old = null; double point_old = double.MinValue; object locker = new object(), locker2 = new object();
            int position = 0;
            int count = grid.Count, position_p = (rs.maxDegree<150)?(int)Math.Round(0.01d * count):5;
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount;
            po.CancellationToken = ct;
            try 
	            {	        
            Parallel.For(0, grid.Count,po, (pointIndex) =>
            {
            Label1: if (ct2.IsCancellationRequested) { System.Threading.Thread.Sleep(1000); } else { goto label2; }
            goto Label1;
                label2:    double gamma_0 = 0, B = 0, r = 0;
                double[] legendrePolys = null;
                double[] point = grid[pointIndex];
                lock (locker2)
                {
                    if (point_old != double.MinValue)
                    {
                        if (point[0] == point_old)
                        {
                            legendrePolys = new double[legendrePolys_old.Length];
                            legendrePolys_old.CopyTo(legendrePolys, 0);
                        }
                        MathFunc.getLegendre(rs.maxDegree, point[0], out legendrePolys);
                        legendrePolys_old = new double[legendrePolys.Length];
                        legendrePolys.CopyTo(legendrePolys_old, 0);
                        point_old = point[0];
                    }
                    else
                    {
                        MathFunc.getLegendre(rs.maxDegree, point[0], out legendrePolys);
                        legendrePolys_old = new double[legendrePolys.Length];
                        legendrePolys.CopyTo(legendrePolys_old, 0);
                        point_old = point[0];
                    }
                }
                B = MathFunc.convertThethaToB(point[0], rs);
                r = MathFunc.getGeocentrDistanceToPointOnElips(rs, B);
                if (gamma_0_formula == NormalGammaFormula.Somigliana)
                { gamma_0 = rs.gamma_a * (1d + rs.k * Math.Pow(Math.Sin(B), 2d)) / Math.Sqrt(1d - rs.firstExcentricity_2 * Math.Pow(Math.Sin(B), 2d)); }
                else { gamma_0 = 9.78030d * (1d + 0.005302 * Math.Pow(Math.Sin(B), 2d) - 0.000007 * Math.Pow(Math.Sin(2d * B), 2d)); }
                double a1, a2_x = rs.a / r, a2_t, a3, cosMlambda, sinMlambda, a1_a, a2_a = 0, a2 = 0, a2_x_m = a2_x;
                a1 = rs.GM / (r * gamma_0);
                a1_a = rs.GM / (r * r);
                int az = 0;
                for (int n = 0; n < rs.maxDegree; n++)
                {
                    int x = (n == 0) ? 0 : -1;
                    a3 = 0;
                    az += (n - 1) + 1;
                    for (int m = 0; m <= n; m++)
                    {
                        cosMlambda = Math.Cos(m * point[1]);
                        sinMlambda = Math.Sin(m * point[1]);
                        a3 += (gm.c_coef[n][m] * cosMlambda + gm.s_coef[n][m] * sinMlambda) * legendrePolys[az + (n - m)];
                    }
                    if (n > 1) { a2_x *= a2_x_m; a2_t = a2_x; } else { a2_t = Math.Pow(a2_x, n); };
                    a2 += a2_t * a3;
                    a2_a += a2_t * (n - 1) * a3;
                }
           
                double tmp_h = a1 * a2, tmp_a = a1_a * a2_a*1e5;
                lock (heights)
                { heights[pointIndex] =tmp_h; }
                lock (anomaly)
                { anomaly[pointIndex] = tmp_a; }
                if (d != null)
                {
                    position++;
                    if  (position > position_p) { lock (locker) { position_p += position_p; }; d.Invoke(position, count, "Обчислено висоти для точок: "); };
                    if (position >= count) { d.Invoke(0, 1, ""); };
                }
            });
                }
            catch (OperationCanceledException)
            {
                return new double[2][];
            }
            return new double[][] { heights, anomaly };
        }
        //Додавання результатів уточнення (вектора невідомих) до моделі
        public void addDeltaCoef(double[] result) {
            int i=0;
            for (int n = 2; n < this.maxDegree; n++)
            {
                for (int m = 0; m <=n; m++)
                {
                    this.c_coef[n][m] = this.c_coef[n][m] - result[i];
                    i++;
                    if (m>0){
                        this.s_coef[n][m] = this.s_coef[n][m]-result[i];
                        i++;
                    }}}}
       /// <summary>
       /// Додавання до коефіцієнтів однієї моделі відповідних їм коефіцієнтів іншої
       /// </summary>
        public GravityModel addmodel(GravityModel add, double sign)
        {
            GravityModel gm = new GravityModel(this);
            int i = 0;
            int maxD = (gm.maxDegree > add.maxDegree) ? add.maxDegree : gm.maxDegree;
            for (int n = 2; n < maxD; n++)
            {
                for (int m = 0; m <= n; m++)
                {
                    gm.c_coef[n][m] = gm.c_coef[n][m] + sign * add.c_coef[n][m];
                    i++;
                    if (m > 0)
                    {
                        gm.s_coef[n][m] = gm.s_coef[n][m] + sign * add.s_coef[n][m];
                        i++;
                    }}}
            return gm;
        }
       /// <summary>
       /// Обчислення градієнту V_zz для заданих координат по моделі "in_model"
       /// </summary>
        public static double getGradient(ReferenceSystem rs,GravityModel in_model, double coLat, double longit, MathFunc.AngleType angType = MathFunc.AngleType.Radians)
        { 
            GravityModel gm;
            if (in_model.model_a != rs.a || in_model.model_GM != rs.GM) { gm = in_model.rescaleModel(rs); } else { gm = new GravityModel(in_model); };
            if (angType == MathFunc.AngleType.Degrees) { MathFunc.deg2rad(ref longit); }
            double grad = 0;
            double tmp = 0, sum_tmp = 0, sum_tmp_2 = 0;
            double[] legendrePolynoms = null;
            MathFunc.getLegendre(rs.maxDegree, coLat, out legendrePolynoms, angType);
            for (int i = 0; i <= rs.maxDegree; i++)
            {
                int n = MathFunc.getArraySize(i - 1) - 1;
                sum_tmp_2 = 0;
                sum_tmp = (i + 1) * (i + 2) * Math.Pow(rs.a / rs.satelliteSphere, i + 3);
                for (int m = 0; m < gm.c_coef[i].Length; m++)
                {
                    double a1 = legendrePolynoms[n + m + 1], a2 = gm.c_coef[i][m] * Math.Cos(m * longit), a3 = gm.s_coef[i][m] * Math.Sin(m * longit);
                    double x = a1 * (a2 + a3);
                    sum_tmp_2 += x;
                }
                tmp += sum_tmp * sum_tmp_2;
            }
            grad = rs.GM / Math.Pow(rs.a, 3D) * tmp;
            return grad;
        }
        /// <summary>
        /// Обчислення градієнту V_zz для сітки "grid"
        /// </summary>
        public double[] getGradientForGrid(ReferenceSystem rs, List<double[]> grid)
        {
            GravityModel gm;
            if (this.model_a != rs.a || this.model_GM != rs.GM) { gm = this.rescaleModel(rs); } else { gm = this; };
            double[] Gradients = new double[grid.Count];
            int progress = 0;
            Parallel.For(0, grid.Count, (i) =>
            {
			lock (Gradients){
                Gradients[i] = getGradient(rs,gm, grid[i][0], grid[i][1]);
				progress++;
				}
            });
            return Gradients;
        }
        #region addFunc
        private void setLine()
        {
            for (int i = 0; i <= this.maxDegree; i++)
            {
                this.s_coef[i] = new double[i + 1];
                this.c_coef[i] = new double[i + 1];
                for (int j = 0; j <= i; j++)
                {
                    this.c_coef[i][j] =0D;
                    this.s_coef[i][j] = 0D;
                }
            }
            
        }
               #endregion
        public enum NormalGammaFormula { Somigliana, Helmert};
    }
}
