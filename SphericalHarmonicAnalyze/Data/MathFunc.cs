using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics;
using System.Threading.Tasks;

namespace SphericalHarmonicAnalyze.Data
{
    /// <summary>
    /// Клас являє собою сукупність математичних алгоритмів які використовуються в процесі гармонійного аналізу
    /// </summary>
  public static class MathFunc
    {
        delegate double func_l(int l);
        delegate double func_lm(int l, int m);
        /// <summary>
        /// Генерує повністю нормовані приєднані поліномами Лежандра для ступеня n і порядку від 0 до n, і полярної відстані  coLat
        /// </summary>
        /// <param name="n">Максимальний порядок</param>
        /// <param name="coLat">Полярна відстань theta=90deg-phi</param>
        /// <param name="resType">Одиниці, в яких задана полярна відстань</param>
        /// <returns>масив double[] з поліномами від нуля до n ({P_00,P_10,P_11,P_20,P_21,P_22,P_30,...,P_nn})</returns>
        public static double[] getLegendrePolynomialsMy(int maxDegree, double coLat, MathFunc.AngleType resType = AngleType.Radians)
        {
            func_l f1 = (l) => { return Math.Sqrt((2D*l+1)/(2D*l)); };
            func_l f2 = (l) => { return Math.Sqrt(2D * (double)l + 1D); };
            func_lm W3 = (l, m) => { return Math.Sqrt((4D * Math.Pow((double)l,2D) - 1D) / (double)(l*l-m*m)); };
            #region перевірка
            if (resType == MathFunc.AngleType.Degrees)
            {
                deg2rad(ref coLat);
            }
            #endregion
            List<double> res = new List<double> (getArraySize(maxDegree));
            double[][]tmp = new double[maxDegree+1][];
            int[][] indexes = get_nm(maxDegree);
            double sinColat = Math.Sin(coLat), cosColat = Math.Cos(coLat);
            tmp[0] = new double[] {1};
            tmp[1] = new double[2];
            tmp[1][1] = Math.Sqrt(3D) * sinColat;
            for (int i = 2; i < tmp.Length; i++)
            {
                tmp[i] = new double[i+1];
                tmp[i][i] = f1(i) * sinColat * tmp[i-1][i-1];
            }
            int mm = 0;
            for (int i = 1; i < tmp.Length; i++)
            {
                tmp[i][i-1] = f2(i) * cosColat * tmp[i-1][i-1];
                if (i > 1) {
                    for (int j = 2; j <=i; j++)
                    {   
                        mm=i-j;
                        tmp[i][mm] = W3(i,mm)*(cosColat*tmp[i-1][mm]-1D/W3(i-1,mm)*tmp[i-2][mm]);
                    }
                }
            }
            for (int i = 0; i < tmp.Length; i++)
            {
                foreach (var item in tmp[i])
                {
                    res.Add(item);
                }
            }
            return res.ToArray();
        }
        /// <summary>
        /// Генерує повністю нормовані приєднані поліномами Лежандра для ступеня n і порядку від 0 до n, і полярної відстані  coLat
        /// </summary>
        /// <param name="i">Максимальний порядок</param>
        /// <param name="d">Полярна відстань d=90deg-phi</param>
        /// <param name="ad">Масив в який буде записано результат, він буде перезаписаний</param>
        /// <param name="angType">Одиниці, в яких задана полярна відстань</param>
        /// <returns>масив double[] з поліномами від нуля до n ({P_00,P_10,P_11,P_20,P_21,P_22,P_30,...,P_nn})</returns>
        public static void getLegendre(int i, double d, out double[] res, MathFunc.AngleType angType = AngleType.Radians)
        {
            #region proverka
            if (angType == MathFunc.AngleType.Degrees)
            {
                deg2rad(ref d);
            }
            #endregion
            int k_ = ((i + 1) * (i + 2)) / 2;
            double[] ad = new double[k_];
            ad[0] = 1.0D;                       //00
            double d1 = Math.Sin(d);
            double d2 = Math.Cos(d);
            ad[1] = Math.Sqrt(3D) * d1;         //11
            ad[2] = Math.Sqrt(3D) * d2;         //10
            if (i <= 1)
            {
                res = ad;
                return;
            }
            int j = 3;
            int k = 1;
            int l = 2;
            for (int i1 = 2; i1 <= i; i1++)
            {
                int j1 = 2 * i1 + 1;
                int k1 = j1 - 2;
                int l1 = k1 - 2;
                j++;
                ad[j - 1] = (Math.Sqrt((double)j1) / (double)i1) * (Math.Sqrt((double)k1) * d1 * ad[l - 1] - ((double)(i1 - 1) * ad[k - 1]) / Math.Sqrt((double)l1));
                double d3 = Math.Sqrt(k1) * d2;
                double d4 = Math.Sqrt(2D) * d3 * ad[l - 1];
                double d6 = Math.Sqrt(k1) * d1;
                j++;
                if (i1 >= 3)
                    d4 += (Math.Sqrt((i1 - 1) * (i1 - 2)) * ad[k]) / Math.Sqrt(l1);
                ad[j - 1] = (Math.Sqrt(j1) * d4) / Math.Sqrt(i1 * (i1 + 1));
                for (int i2 = 2; i2 <= i1; i2++)
                {
                    int j2 = i1 + i2;
                    int k2 = j2 - 1;
                    j++;
                    double d5 = d3 * ad[(l + i2) - 2];
                    double d7 = d6 * ad[(l + i2) - 1];
                    if (i2 + 2 > i1)
                    {
                        ad[j - 1] = (Math.Sqrt(j1) * d5) / Math.Sqrt(j2 * k2);
                    }
                    else
                    {
                        d7 -= (Math.Sqrt(k2 * (i1 - i2 - 1)) * ad[(k + i2) - 1]) / Math.Sqrt(l1);
                        ad[j - 1] = (Math.Sqrt(j1) * d7) / Math.Sqrt(j2 * (i1 - i2));
                    }
                }
                k += i1 - 1;
                l += i1;
            }
            res = ad;
        }
        private delegate double W_mm(int m);
        private delegate double W_lm(int l, int m);
        public static double[][] getLegendreP_lm_cos_phi(int n, double phi){
            W_mm W_MM = (m) => { return Math.Sqrt((2d * (double)m + 1d) / (2d * (double)m)); };
            W_lm W_LM = (l, m) => { return Math.Sqrt((4d*Math.Pow(l,2d)-1)/(Math.Pow(l,2d)-Math.Pow(m,2d))); };
            double[][] P_lm = new double[n+1][];
            double cosPhi = Math.Cos(phi);
            double sinPhi = Math.Sin(phi);
            P_lm[0] = new double[]{1};
            P_lm[1] = new double[] {0,Math.Sqrt(3)*cosPhi};
            for (int l = 2; l <= n; l++)
            {
                P_lm[l] = new double[l+1];
                P_lm[l][l] = W_MM(l)*cosPhi*P_lm[l-1][l-1];
            }
            for (int l = 1; l <= n; l++)
            {
                for (int m = l-1; m >=0; m--)
                {
                    double P_l2_m = (l - 2 >= 0) ? ((P_lm[l - 2].Length>m)?P_lm[l - 2][m]:0) : 0;
                    double tmp = (Math.Abs(P_l2_m)>0)?P_l2_m/W_LM(l-1,m):0;
                    P_lm[l][m] = W_LM(l, m) * (sinPhi * P_lm[l - 1][m] - tmp);
                }
            }
            return P_lm;
        }
        /// <summary>
        /// Обчислює масив з коефіцієнтами рядка в матриці
        /// </summary>
        /// <param name="rs">Еліпсоїд</param>
        /// <param name="n">Порядок розвинення</param>
        /// <param name="r">Радіус сфери, до якої віднесені виміри</param>
        /// <param name="coLatitude">Полярний кут tetha = 90deg-phi</param>
        /// <param name="longitude">Довгота</param>
        /// <param name="angType">Одиниці, в яких задано попередні кути</param>
        /// <returns></returns>
        public static double[] getCoefMatrixLine(ReferenceSystem rs, int n, int[][] t_nm, double r, double coLatitude, double longitude, MathFunc.AngleType angType = AngleType.Radians)
        {
            if (angType == AngleType.Degrees) { deg2rad(ref longitude); }
            double[] line = new double[(getArraySize(n) - 3) * 2-(rs.maxDegree-1)];
            double u_1 = rs.GM / (Math.Pow(rs.a, 3d));
            double[] legPol = null;
            getLegendre(n, coLatitude, out legPol, angType);
            double tmp_n = 0;
            double tmp_m = 0;
            int lineIndex = 0;
            for (int i = 0; i <= legPol.Length; i++)
            {
                if (i < 4) { continue; }
                tmp_n = t_nm[i-1][0];
                tmp_m = t_nm[i-1][1];
                double b = legPol[getArraySize(t_nm[i - 1][0] - 1) + t_nm[i - 1][0] - t_nm[i - 1][1]];
                double a = (tmp_n + 1D) * (tmp_n + 2D) * Math.Pow(rs.a / rs.satelliteSphere, (double)tmp_n + 3d)*u_1;
                double a1 = a * Math.Cos(tmp_m * longitude);
                line[lineIndex] = a1 * b;
                if (tmp_m > 0)
                {
                    double a2 = a * Math.Sin(tmp_m * longitude);
                    line[lineIndex + 1] = a2 * b;
                    lineIndex += 2;
                }
                else {
                    lineIndex++;
                }
                
            }
            return line;
        }
        //Формування рядка матриці коефіцієнтів рівнянь поправок
        public static double[] getCoefMatrixLineKoop(ReferenceSystem rs, int n, int[][] t_nm, double r, double coLatitude, double longitude, MathFunc.AngleType angType = AngleType.Radians)
        {
            if (angType == AngleType.Degrees) { deg2rad(ref longitude); }
            List<double> line = new List<double>((getArraySize(n) - 3) * 2 - (rs.maxDegree - 1));
            double[][] legPol = getLegendreP_lm_cos_phi(n, Math.PI / 2d - coLatitude);
            double u_1 = rs.GM / Math.Pow(rs.a, 3d), u_2 = rs.a / rs.satelliteSphere;
            for (int l = 2; l <= rs.maxDegree; l++)
            {
                double u_3 = u_1 * Math.Pow(u_2, l + 3)*(l+1)*(l+2);
                for (int m = 0; m <= l; m++)
                {
                        line.Add(u_3 * legPol[l][m] * Math.Cos((double)m * longitude));
                    if (m != 0) {
                        line.Add(u_3 * legPol[l][m] * Math.Sin((double)m * longitude));
                    }
                }
            }
            return line.ToArray();
        }
        /// <summary>
        /// Визначає, до якої клітинки сітки відноситься кожен результат. Кожен елемент List() відповідає елементу Greed, і є масивом номерів точок з pointsData
        /// </summary>
        /// <param name="rs">Референсна система</param>
        /// <param name="pointsData">Масив точок типу: {radius,coLatitude,Longitude,Gradient}</param>
        /// <param name="greed">Масив з клітинками сітки типу: {{colat,long},{colat,long},{colat,long}...}</param>
        /// <param name="rowCount">Кількість рядків сітки</param>
        /// <param name="colsCount">Кількість стовпчиків сітки</param>
        /// <returns>Повертає масив з номерами точок з масиву pointsData</returns>
        public static List<int>[] getMappingOfPoints(ReferenceSystem rs, double[][] pointsData, double[][] greed, int rowCount, int colsCount, double avgRadius)
        {
            List<int>[] map = new List<int>[greed.Length];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = new List<int>();
            }
            double cellSize = MathFunc.deg2rad(rs.gridParameters.cellSize);
            double zero = greed[0][0] - cellSize / 2D;
            double l_zero = greed[0][1] - cellSize / 2d;
            Parallel.For(0, pointsData.Length, (i) =>
            {
                double fi,lambda,r;
                lock (pointsData)
                {
                    fi = pointsData[i][1]; lambda = pointsData[i][2]; r = pointsData[i][0];
                }
                if (fi >= greed[0][0] && fi <= greed[greed.Length-1][0] && lambda >= greed[0][1] && lambda<=greed[greed.Length-1][1] && Math.Abs(r-avgRadius)<10000d)
                {
                    fi = fi - (zero);
                    lambda = lambda - l_zero;
                        int n = (int)Math.Floor(fi / cellSize);
                        int m = (int)Math.Floor(lambda / cellSize);
                        int index = (colsCount * n) + m;
                        lock (map)
                        {
                            var x1 =cellSize/2D-Math.Abs(greed[index][0]-zero - fi);
                            var x2 = cellSize / 2D - Math.Abs(greed[index][1]-l_zero - lambda);
                            if (x1 < 0 || x2 < 0) { System.Windows.Forms.MessageBox.Show("getMappingOfPoints: неправильна визначена клітинка точки "+ i.ToString()); }
                            else
                            {
                                map[index].Add(i);
                            }
                       }
                };
            });
            return map;
        }
        public static double getAvgRadius(double[][] pd) {
            double avg=0;    
            foreach (double[] point in pd)
	            {
		                avg+=point[0];
	            }
            return avg/pd.Length;
        }
        /// <summary>
        /// Рахує градієнт як середньозважене з усіх точок, які потрапили в клітинку сітки для кожної клітинки greed
        /// </summary>
        /// <param name="pointsData">Масив точок типу: {radius,coLatitude,Longitude,Gradient}</param>
        /// <param name="greed">Масив з клітинками сітки типу: {{colat,long},{colat,long},{colat,long}...}</param>
        /// <param name="map">Масив номерів точок, які віднесені до кожної клітинки</param>
        /// <returns></returns>
        public static double[] regularization(double[][] pointsData, double[][] greed, List<int>[] map, out double[] sphereRadius)
        {
            double[] sphere = new double[greed.Length];
            double radius = 0; int count = 0;
            object locker0 = new object(), locker1 = new object(), locker2 = new object();
            double minRadius = pointsData[0][0], maxRadius = 0;
            Parallel.For(0, map.Length, (el) =>
            {
                double w_sum = 0, mult_sum = 0, weight = 0, res, c, b;
                foreach (int index in map[el])
                {
                    c = Math.Abs(pointsData[index][1] - greed[el][0]);
                    b = Math.Abs(pointsData[index][2] - greed[el][1]);
                    weight = 1D / Math.Acos((Math.Cos(c) * Math.Cos(b)));
                    mult_sum += weight * pointsData[index][3];
                    w_sum += weight;
                    lock (locker0)
                    {
                        radius += pointsData[index][0];
                        count++;
                    }
                    if (pointsData[index][0] > maxRadius)
                    {
                        lock (locker1)
                        {
                            maxRadius = pointsData[index][0];
                        }
                    }
                    if (pointsData[index][0] < minRadius)
                    {
                        lock (locker2)
                        {
                            minRadius = pointsData[index][0];
                        }
                    }
                }
                res = mult_sum / w_sum;
                lock (sphere)
                {
                    sphere[el] = res;
                }
            });
            sphereRadius = new double[] { radius / count, minRadius, maxRadius };
            return sphere;
        }
       //Метод перевіряє чи не залишились пусті комірки сітки, якщо залишились - їх буде виключено
       // так як в такому випадку відсутній вільний член в рівнянні поправки
        public static void checkGreed(ref List<double[]> greed, List<int>[] map, out List<int>[] newMap)
        {
            List<int> not = new List<int>();
            Parallel.For(0, map.Length, (i) => {
                if (map[i].Count == 0) {
                    lock (not)
                    {
                        not.Add(i);
                    }
                }
            });
            List<List<int>> map_new = new List<List<int>>(greed.Count - not.Count);
            List<double[]> newGreed = new List<double[]>(greed.Count-not.Count);
            for (int i = 0; i < greed.Count; i++)
            {
                if (!not.Remove(i)) { newGreed.Add(greed[i]); map_new.Add(map[i]); };
            }
            greed = newGreed;
            newMap = map_new.ToArray();
        }

        #region Grid Generetor
        /// <summary>
        /// Генерує сітку 
        /// </summary>
        public static List<double[]> generateGrid(
            double cellSize,
            out int outColumns,
            out int outRows,
            double coLatitudeBound = 0,
            double coLatitudeBound2 = 180,
            double LongitudeBoundW = 0,
            double LongitudeBoundE = 360,
            AngleType resType = AngleType.Radians
            )
        {
            if (coLatitudeBound > 90 || coLatitudeBound < 0) { throw new Exception("coLatitudeBound повинна бути: 0 <= coLatitude <= 90"); }
            if (LongitudeBoundW >= 360 || LongitudeBoundW < 0 || LongitudeBoundE > 360 || LongitudeBoundE <= 0) { throw new Exception                           ("LongitudeBounds must be 0<=LongitudeBound<360"); }
            if (LongitudeBoundW > LongitudeBoundE) { throw new Exception("West longituge bound повинна бути менша за East longituge"); }
            double gridRows = (180 - coLatitudeBound- (180 - coLatitudeBound2)) / cellSize;
            double gridColumns = (360 - LongitudeBoundW - (360 - LongitudeBoundE)) / cellSize;
            int addRow = 0, addCol = 0;
            if ((Math.Round(gridRows) - gridRows) < 0) { addRow = 1; }
            if ((Math.Round(gridColumns) - gridColumns) < 0) { addCol = 1; }
            gridColumns = (int)gridColumns + addCol;
            outColumns = (int)gridColumns;
            gridRows = (int)gridRows + addRow;
            outRows = (int)gridRows;
            int gridCells = (int)(gridColumns * gridRows);
            List<double[]> greed = new List<double[]>(gridCells);
            double coLatitudeStart = coLatitudeBound + (cellSize / 2D);
            double LongStart = deg2rad(LongitudeBoundW);
            #region ifRadians
            if (resType == AngleType.Radians)
            {
                MathFunc.deg2rad(ref cellSize);
                MathFunc.deg2rad(ref coLatitudeStart);
            }
            #endregion
            for (int r = 0; r < gridRows; r++)
            {
                for (int c = 0; c < gridColumns; c++)
                {
                    greed.Add(new double[] { coLatitudeStart + r * cellSize, LongStart+cellSize * (0.5 + c)});
                }
            }
            return greed;
        }
        #endregion
        //Допоміжні функції
        public static int getArraySize(int n)
        {
            int res = 0;
            if (n < 0) { return 0; }
            for (int i = 0; i <= n; i++)
            {
                res += i + 1;
            }
            return res;
        }
        public static void rad2deg(ref double radians)
        {
            radians = radians / Constants.Degree;
        }
        public static double rad2deg(double radians)
        {
            return radians / Constants.Degree;
        }
        public static void deg2rad(ref double radians)
        {
            radians = (radians * Constants.Degree);
        }
        public static double deg2rad(double radians)
        {
            return (radians * Constants.Degree);
        }
        public static int[][] get_nm(int degree)
        {
            int size = getArraySize(degree);
            int[][] res = new int[degree + 1][];
            for (int i = 0; i <= degree; i++)
            {
                res[i] = new int[i + 1];
                for (int j = 0; j <= i; j++)
                {
                    res[i][j] = j;
                }
            }
            int[][] tmp = new int[getArraySize(degree)][];
            int ind = 0;
            for (int i = 0; i < res.Length; i++)
            {
                foreach (var item in res[i])
                {
                    tmp[ind] = new int[2];
                    tmp[ind][0] = i;
                    tmp[ind][1] = item;
                    ind++;
                }
            }
            return tmp;
        }
        public static double convertThethaToB(double thetha, ReferenceSystem rs, MathFunc.AngleType angType = MathFunc.AngleType.Radians)
        {
            double phi = 0;
            if (angType == MathFunc.AngleType.Degrees) { phi = deg2rad(90d - thetha); } else { phi = Math.PI / 2d - thetha; };
            return (Math.Atan(Math.Tan(phi) / (1d - rs.firstExcentricity_2)));
        }
        public static double convertBToThetha(double thetha, ReferenceSystem rs, MathFunc.AngleType angType = MathFunc.AngleType.Radians)
        {
            double phi = 0;
            if (angType == MathFunc.AngleType.Degrees) { phi = deg2rad(90d - thetha); } else { phi = Math.PI / 2d - thetha; };
            return (Math.Atan(Math.Tan(phi) * (1d - rs.firstExcentricity_2)));
        }
        public static double getGeocentrDistanceToPointOnElips(ReferenceSystem rs, double B)
        {
            return (rs.a * Math.Sqrt((1d - rs.firstExcentricity_2) / (1d - Math.Pow(Math.Sqrt(rs.firstExcentricity_2) * Math.Sin(B), 2d))));
        }
        public enum AngleType { Degrees, Radians };
        #region DELEGATES and Events
        public delegate void updateProgressBar(int progress, int all, System.Windows.Forms.ProgressBar pb);
        private static void setProgressVal(int prog, int all, System.Windows.Forms.ProgressBar pb)
        {
            if (prog > 0 && all != 0)
            {
                pb.Value = prog / all * 100;
            }
        }
        public static updateProgressBar event_1 = new updateProgressBar(setProgressVal);

        #endregion
    }
}
