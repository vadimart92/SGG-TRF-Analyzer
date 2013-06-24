using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SphericalHarmonicAnalyze.Data;
using SphericalHarmonicAnalyze.View;
using System.Threading.Tasks;
using MathNet.Numerics;
using System.Threading;

namespace SphericalHarmonicAnalyze
{
    public partial class MainForm : Form
    {
        CancellationTokenSource ts_p = new CancellationTokenSource(); 
        CancellationToken ct_p ;
        CancellationTokenSource ts,ts2;
        CancellationToken ct,ct2;
        Task tsk;
        public MainForm()
        {
            ts = new CancellationTokenSource();
            ct = ts.Token;
            Task splash = Task.Factory.StartNew(() => { new SplashScreen().show1(ref ct); },ct);
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            loadVals();
            timer1.Interval = 1000;
            timer1.Start();
            timer1_Tick(sender,e);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.PerformanceCounter pc = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");
            toolStripStatusLabel2.Text = "Залишилось пам'яті: " + pc.NextValue().ToString();
        }
        private void loadVals() {
            textBox1.Text = SphericalHarmonicAnalyze.Properties.Settings.Default.modelMaxOrder.ToString();
            textBox2.Text = SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel;
            textBox3.Text = SphericalHarmonicAnalyze.Properties.Settings.Default.GridCellSize.ToString();
            textBox4.Text = SphericalHarmonicAnalyze.Properties.Settings.Default.SGG_measures;
            textBox5.Text = SphericalHarmonicAnalyze.Properties.Settings.Default.outGravityModel;
            textBox7.Text = (90-SphericalHarmonicAnalyze.Properties.Settings.Default.minCoLatitude).ToString();
            textBox6.Text = (90-SphericalHarmonicAnalyze.Properties.Settings.Default.maxCoLatitude).ToString();
            textBox8.Text = SphericalHarmonicAnalyze.Properties.Settings.Default.longW.ToString();
            textBox9.Text = SphericalHarmonicAnalyze.Properties.Settings.Default.longE.ToString();
            listBox1.Items.Add(ReferenceSystem.Default.ITRF);
            listBox1.Items.Add(ReferenceSystem.Default.TideFree);
            listBox1.Items.Add(ReferenceSystem.Default.USC2000);
            listBox1.Items.Add(ReferenceSystem.Default.WGS84);
            listBox1.SelectedIndex = 0;
            label12.Text = string.Format("{0} шт.",SphericalHarmonicAnalyze.Properties.Settings.Default.pointsCount);
            button5.Enabled = false;
        }
#region Підтримка взаємодії
        public void writeOut(string message) {
            richTextBox1.AppendText(message);
            richTextBox1.AppendText("\r\n");
        }
        public delegate void setProgressDel(long val, long all, string state);
        public delegate void addTextToRichbox(string str, params object[] p);
        public void addVal(long val, long all, string state)
        {
            if (!toolStripProgressBar1.ProgressBar.InvokeRequired)
            {
                int v = (int)((double)val / (double)all * 100D);
                if (v <= 100)
                { toolStripProgressBar1.Value = v; }
                else { toolStripProgressBar1.Value = 100; }
                if (val > 0) { toolStripStatusLabel1.Text = string.Format("{0}: {1}/{2}", state, val, all); } else { toolStripStatusLabel1.Text = ""; };
            }
            else {
                toolStripProgressBar1.ProgressBar.Invoke(new setProgressDel(addVal),val,all,state);
            }
            
        }
        public void addText(string str, params object[] p)
        {
            if (!richTextBox1.InvokeRequired)
            {
                richTextBox1.AppendText(string.Format(str, p));
                richTextBox1.Refresh();
            }
            else {
                richTextBox1.Invoke(new addTextToRichbox(addText),new object[]{str,p});
            }
           
        }
#endregion
        
        private void button4_Click(object sender, EventArgs e)
        {
            Action fileProc = () =>
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                MathNet.Numerics.Control.LinearAlgebraProvider = new MathNet.Numerics.Algorithms.LinearAlgebra.Mkl.MklLinearAlgebraProvider();
                MathNet.Numerics.Control.NumberOfParallelWorkerThreads = Environment.ProcessorCount;
                addText("Обробка файлу вимірювань...\r\n");
                double[][] SGG_data = null;
                if (System.IO.File.Exists("sgg_data.bin"))
                {
                    SGG_data = IOFunc.binLoad_SGG_data("sgg_data.bin");
                }
                else
                {
                    SGG_data = Data.IOFunc.read_SGG_data(SphericalHarmonicAnalyze.Properties.Settings.Default.SGG_measures, new setProgressDel(addVal));
                    IOFunc.binwrite_SGG_data("sgg_data.bin", SGG_data);
                }
                addText("Дані вимірювань оброблено: {0} шт.\r\n", SGG_data.Length); Thread.Sleep(500);
                ReferenceSystem elipsoid = new ReferenceSystem(ReferenceSystem.Default.TideFree);
                elipsoid.gridParameters.cellSize = SphericalHarmonicAnalyze.Properties.Settings.Default.GridCellSize;
                elipsoid.gridParameters.coLatitudeBounds = SphericalHarmonicAnalyze.Properties.Settings.Default.minCoLatitude;
                elipsoid.maxDegree = SphericalHarmonicAnalyze.Properties.Settings.Default.modelMaxOrder;
                int greedColumnsCount, greedRowsCount;
                List<double[]> greed = MathFunc.generateGrid(elipsoid.gridParameters.cellSize, out greedColumnsCount, out greedRowsCount, elipsoid.gridParameters.coLatitudeBounds,180 - elipsoid.gridParameters.coLatitudeBounds);
                addText("Сітку згенеровано: {0} комірок \r\n", greed.Count);
                double avgR = MathFunc.getAvgRadius(SGG_data);
                List<int>[] map = MathFunc.getMappingOfPoints(elipsoid, SGG_data, greed.ToArray(), greedRowsCount, greedColumnsCount, avgR); sw.Stop(); addText("Точки віднесено до комірок сітки за: {0}.\r\n", sw.Elapsed.ToString());
                addText("Кількість клітинок сітки всього: {0}\r\n", greed.Count);
                int res1 = 0; foreach (var item in map) { res1 += item.Count; } addText("Використано вимірів: {0}\r\nСер радіус: {1}\r\n", res1, avgR);
                test.checkMap(SGG_data, map, greed, elipsoid);
                List<int>[] newMap = null;
                MathFunc.checkGreed(ref greed, map, out newMap);
                addText("Кількість клітинок сітки, в яких присутні дані вимірювань: {0}\r\n", greed.Count);
                map = newMap; newMap = null;
                IOFunc.writeGreedToCsvFileWithMeasureCount(greed, map, "greed_new_map.txt");
                double[] avgRadius; sw.Restart();
                double[] regularisedValues = MathFunc.regularization(SGG_data, greed.ToArray(), map, out avgRadius); sw.Stop(); addText("Регуляризація (на основі сферичної відстані) виконана за: {0}.\r\n", sw.Elapsed.ToString());
                IOFunc.writeGreedToCsvFileWithMeasureS(greed,regularisedValues, "greed_regular_grad.txt");
                avgRadius[0] = Math.Round(avgRadius[0]);
                elipsoid.satelliteSphere = avgRadius[0];
                addText("Середній радіус: {0,10:0.000}.\r\nМінімальний радіус: {1,10:0.0000}\r\nМаксимальний радіус:{2,10:0.0000}\r\n", avgRadius[0], avgRadius[1], avgRadius[2]);
                SGG_data = null; map = null;
                int[][] t_nm = MathFunc.get_nm(elipsoid.maxDegree);
                sw.Restart();
                MathNet.Numerics.LinearAlgebra.Double.DenseMatrix dm = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(greed.Count, (MathFunc.getArraySize(elipsoid.maxDegree) - 3) * 2 - (elipsoid.maxDegree-1));
                sw.Stop(); addText("Пам'ять для матриці коефіцієнтів виділено за: {0}.\r\n", sw.Elapsed.ToString());
                sw.Restart();
                int progress = 0;
                //Обчислення елементів матриці
                var p= Parallel.For(0, dm.RowCount, (i) =>
                {
                    double[] line = MathFunc.getCoefMatrixLineKoop(elipsoid, elipsoid.maxDegree, t_nm, elipsoid.satelliteSphere, greed[i][0], greed[i][1]);
                    lock (dm)
                    {
                         dm.SetRow(i,line);
                       
                    }
                    progress++;
                    if (progress / 100D == Math.Round(progress / 100D)) {addVal(progress, dm.RowCount, "Визначено");} 
                });

                if (!p.IsCompleted) { throw new Exception("Parallel.For"); };
                IOFunc.writeMatrixToMatLabFile(dm, @"matlab\A.mat","A");
                sw.Stop();
                richTextBox1.Invoke(new setProgressDel(addVal), new object[] { 0, dm.RowCount, "" });
                addText("Матриця {0} на {1} ({2}MB) згенерована за: {3,10}\r\n", dm.RowCount, dm.ColumnCount, dm.ColumnCount * dm.RowCount * 8 / 1000000,sw.Elapsed.ToString()/* + "\r\nЗапис у файл...\r\n"*/);
                if(true){
                GravityModel gm08 = new GravityModel(elipsoid.maxDegree);
                gm08.loadFromFile("GO_CONS_EGM_GCF_2.gfc", new setProgressDel(addVal));
                MathNet.Numerics.LinearAlgebra.Double.DenseVector dmL = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(gm08.getGradientForGrid(elipsoid,greed));//regularisedValues);
                MathNet.Numerics.LinearAlgebra.Double.DenseVector dmL2;
                GravityModel gm = new GravityModel(elipsoid.maxDegree);
                    if (radioButton1.Checked) {
                    sw.Restart();
                    gm.loadFromFile(SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel, new setProgressDel(addVal));
                    sw.Stop(); addText("Вихідна модель завантажена за: {0}.\r\n", sw.Elapsed.ToString());
                    sw.Restart();
                    dmL2 = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(gm.getGradientForGrid(elipsoid,greed));
                    sw.Stop(); addText("Градієнти за вихідною моделлю обчислені для сітки за: {0}.\r\n", sw.Elapsed.ToString());
                }
                else
                {
                    sw.Restart();
                    gm = GravityModel.getNormalModel(elipsoid, elipsoid.maxDegree);
                    dmL2 = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(gm.getGradientForGrid(elipsoid, greed));
                    sw.Stop(); addText("Нормальні градієнти обчислені для сітки за: {0}.\r\n", sw.Elapsed.ToString());
                }
                dmL = dmL - dmL2;
                dmL2 = null;
                IOFunc.writeMatrixToMatLabFile(dmL.ToColumnMatrix(), @"matlab\L.mat", "L");
                    sw.Restart();
                MathNet.Numerics.LinearAlgebra.Double.DenseVector dmLNormal = null;
                dmLNormal = (MathNet.Numerics.LinearAlgebra.Double.DenseVector)dm.TransposeThisAndMultiply(dmL);
                dmL = null;
                IOFunc.writeMatrixToMatLabFile(dmLNormal.ToColumnMatrix(), @"matlab\LNorm.mat", "LNorm"); 
                sw.Stop(); addText("Стовпчик вільних членів обчислений за: {0}.\r\n", sw.Elapsed.ToString());
                MathNet.Numerics.LinearAlgebra.Double.DenseMatrix dmANorm = null;
                sw.Restart();
                dmANorm = (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix)dm.TransposeThisAndMultiply(dm); dm = null;
                sw.Stop(); addText("Нормальна матриця коефіціэнтів обчислена за: {0}.\r\n", sw.Elapsed.ToString());
                IOFunc.writeMatrixToMatLabFile(dmANorm, @"matlab\ANorm.mat", "ANorm");
                //dmLNormal = (MathNet.Numerics.LinearAlgebra.Double.DenseVector)dmLNormal.Multiply(5e-8);
                var x = dmANorm.Inverse();
                var res = (MathNet.Numerics.LinearAlgebra.Double.DenseVector)x.Multiply(dmLNormal);
                IOFunc.writeModeVectorlToTxtFile(res, elipsoid, @"matlab\_out.AL");
                addText(@"Результат за методом A\L знайдено...");
                x = null;
                GravityModel gm_R = new GravityModel(gm);
                gm_R.addDeltaCoef(res.ToArray()); res = null;
                double[] h = GravityModel.getGeoidHeight(elipsoid, gm_R, greed);
                double[] dg = GravityModel.getAnomaly(elipsoid, gm_R, greed);
                IOFunc.writeGeoidHeightsAndAnomalysToTxt(greed, h, dg, elipsoid, @"output\result_AL.txt");
                IOFunc.writeGravityModelToTxtFile(gm_R, @"output\model_AL.gcf");
                sw.Restart();
                addText(dmANorm.Rank().ToString() + "\r\n");
                dmANorm = null;
                dmLNormal = null;
                sw.Stop(); addText("Невідомі знайдено за: {0}.\r\n", sw.Elapsed.ToString());
                
            }
            };

            if (System.IO.File.Exists(SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel)) { 
            tabControl1.SelectedTab = tabControl1.TabPages[1];
            this.UseWaitCursor = true;
            ts = new CancellationTokenSource();
            ct = ts.Token;
            tsk = Task.Factory.StartNew(fileProc,ct);
            var setCur = Task.Factory.StartNew(() => { tsk.Wait(); this.UseWaitCursor = false; addText("Обчислення завершені!"); });
            richTextBox1.SaveFile(@"output\zvit.rtf");
            }
            

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            ts.Cancel();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ts.Cancel();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button5.Enabled = true;
            int t = 0;
            if (int.TryParse(textBox1.Text, out t))
            {
                label11.Text = string.Format("=> Невідомих: {0}", MathFunc.getArraySize(t)*2 - 6);
            }
            else { label11.Text = ""; };
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int t = 0; double d = 0,c=0,c2=0,le=0,lw=0;
            if (int.TryParse(textBox1.Text, out t) && double.TryParse(textBox3.Text, out d)
                && double.TryParse(textBox6.Text, out c2) && double.TryParse(textBox7.Text, out c) 
                && double.TryParse(textBox8.Text, out lw) && double.TryParse(textBox9.Text, out le)
                && c2<c && le>lw)
            {
                c = 90-c; c2 = 90-c2;
                SphericalHarmonicAnalyze.Properties.Settings.Default.modelMaxOrder = t;
                SphericalHarmonicAnalyze.Properties.Settings.Default.GridCellSize = d;
                SphericalHarmonicAnalyze.Properties.Settings.Default.minCoLatitude = c;
                SphericalHarmonicAnalyze.Properties.Settings.Default.maxCoLatitude = c2;
                SphericalHarmonicAnalyze.Properties.Settings.Default.longW = lw;
                SphericalHarmonicAnalyze.Properties.Settings.Default.longE = le;
                button4.Enabled = true;
                button5.Enabled = false;
                int col = 0, row = 0;
                var gr = MathFunc.generateGrid(d, out col, out row, c, c2, lw, le);
                SphericalHarmonicAnalyze.Properties.Settings.Default.pointsCount = (col * row);
                label12.Text = string.Format("{0} шт.", SphericalHarmonicAnalyze.Properties.Settings.Default.pointsCount);
                SphericalHarmonicAnalyze.Properties.Settings.Default.Save();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            button5.Enabled = true;
            button4.Enabled = false;

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            button5.Enabled = true;
            button4.Enabled = false;
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            ReferenceSystem elipsoid = new ReferenceSystem(ReferenceSystem.Default.WGS84);
            elipsoid.gridParameters.cellSize = 30d;
            elipsoid.gridParameters.coLatitudeBounds = 15D;
            elipsoid.maxDegree = 100;
            double[] gmN = GravityModel.generateNormalModel(elipsoid, 10);
            GravityModel gm = new GravityModel(100);
            gm.loadFromFile(SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel, new setProgressDel(addVal));
            int greedColumnsCount, greedRowsCount;
            GravityModel gm2 = new GravityModel(gm);
            List<double[]> greed = MathFunc.generateGrid(elipsoid.gridParameters.cellSize, out greedColumnsCount, out greedRowsCount, elipsoid.gridParameters.coLatitudeBounds); 
            double[] h = GravityModel.getGeoidHeight(elipsoid,gm2,greed);
            double[] dg = GravityModel.getAnomaly(elipsoid, gm, greed);
            IOFunc.writeGeoidHeightsAndAnomalysToTxt(greed, h, dg, elipsoid, "result.txt");
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Моделі *.gfc|*.gfc|Всі файли *.*|*.*";
            openFileDialog1.FileName = textBox2.Text;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                System.IO.FileInfo f = new System.IO.FileInfo(openFileDialog1.FileName);
                if (f.DirectoryName.Equals(Application.StartupPath, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    textBox2.Text = f.Name;
                }
                else
                {
                    textBox2.Text = openFileDialog1.FileName;
                    textBox2.Select(textBox2.TextLength, 0);
                    textBox2.ScrollToCaret();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Текстові дані *.SGG|*.SGG|Всі файли *.*|*.*";
            openFileDialog1.FileName = textBox4.Text;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(openFileDialog1.FileName);
                if (f.DirectoryName.Equals(Application.StartupPath, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    textBox4.Text = f.Name;
                }
                else
                {
                    textBox4.Text = openFileDialog1.FileName;
                    textBox4.Select(textBox2.TextLength, 0);
                    textBox4.ScrollToCaret();
                }
                
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            SphericalHarmonicAnalyze.Properties.Settings.Default.SGG_measures = textBox4.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel = textBox2.Text;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            SphericalHarmonicAnalyze.Properties.Settings.Default.outGravityModel = textBox5.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Уточнена модель *.gfc|*.gfc|Всі файли *.*|*.*";
            saveFileDialog1.FileName = textBox5.Text;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(saveFileDialog1.FileName);
                if (f.DirectoryName.Equals(Application.StartupPath, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    textBox5.Text = f.Name;
                }
                else
                {
                    
                    textBox5.Text = saveFileDialog1.FileName;
                    textBox5.Select(textBox2.TextLength, 0);
                    textBox5.ScrollToCaret();
                }
            }
        }

        private void обчисленняЗаВихідноюМоделлюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabControl1.TabPages[1];
            this.Refresh();
            var task = Task.Factory.StartNew(() => {
                addText("Обчислення розпочато...\r\n");
                string file = SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel;
            GravityModel gm = new GravityModel(SphericalHarmonicAnalyze.Properties.Settings.Default.modelMaxOrder);
            gm.loadFromFile(SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel, new setProgressDel(addVal));
            ReferenceSystem elipsoid = new ReferenceSystem(ReferenceSystem.Default.WGS84);
            elipsoid.gridParameters.cellSize = SphericalHarmonicAnalyze.Properties.Settings.Default.GridCellSize;
            elipsoid.gridParameters.coLatitudeBounds = SphericalHarmonicAnalyze.Properties.Settings.Default.minCoLatitude;
            elipsoid.maxDegree = SphericalHarmonicAnalyze.Properties.Settings.Default.modelMaxOrder;
            int greedColumnsCount, greedRowsCount;
            List<double[]> greed = MathFunc.generateGrid(elipsoid.gridParameters.cellSize, out greedColumnsCount, out greedRowsCount, elipsoid.gridParameters.coLatitudeBounds);
            double[] h = GravityModel.getGeoidHeight(elipsoid, gm, greed);
            double[] dg = GravityModel.getAnomaly(elipsoid, gm, greed);
            IOFunc.writeGeoidHeightsAndAnomalysToTxt(greed, h, dg, elipsoid, file + "B_L_N_dg.txt");
            addText("Готово...\r\nРезультати записано в файл: " + file + "B_L_N_dg.txt");
            });
            }

        private void button6_Click(object sender, EventArgs e)
        {
            обчисленняЗаВихідноюМоделлюToolStripMenuItem_Click(sender, e);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SphericalHarmonicAnalyze.Properties.Settings.Default.Save();
        }

        private void проПрограмуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SphericalHarmonicAnalyze.AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            
        }

        private void вихідToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ts2 = new CancellationTokenSource(); 
            ct2 = ts2.Token;
            ct_p = ts_p.Token;
            tabControl1.SelectedTab = tabControl1.TabPages[1];
            this.Refresh();
            ReferenceSystem.Default def = (ReferenceSystem.Default)listBox1.SelectedItem;
            відмінитиПоточнуОпераціюToolStripMenuItem.Enabled = true;
            var task = Task.Factory.StartNew(() =>
            {
                addText("Обчислення розпочато...\r\n");
                string file = SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel;
                GravityModel gm = new GravityModel(SphericalHarmonicAnalyze.Properties.Settings.Default.modelMaxOrder);
                gm.loadFromFile(SphericalHarmonicAnalyze.Properties.Settings.Default.inGravityModel, new setProgressDel(addVal));
                ReferenceSystem elipsoid = new ReferenceSystem(def);
                elipsoid.gridParameters.cellSize = SphericalHarmonicAnalyze.Properties.Settings.Default.GridCellSize;
                elipsoid.gridParameters.coLatitudeBounds = SphericalHarmonicAnalyze.Properties.Settings.Default.minCoLatitude;
                elipsoid.maxDegree = SphericalHarmonicAnalyze.Properties.Settings.Default.modelMaxOrder;
                int greedColumnsCount, greedRowsCount;
                List<double[]> greed = MathFunc.generateGrid(elipsoid.gridParameters.cellSize, out greedColumnsCount, out greedRowsCount, SphericalHarmonicAnalyze.Properties.Settings.Default.minCoLatitude, SphericalHarmonicAnalyze.Properties.Settings.Default.maxCoLatitude, SphericalHarmonicAnalyze.Properties.Settings.Default.longW, SphericalHarmonicAnalyze.Properties.Settings.Default.longE);
                addText("Колонок: {0}\r\n",greed.Count);
                double[][] h_dg = GravityModel.getGeoidHeightAndAnomalys(elipsoid, gm, greed, d: new setProgressDel(addVal),ct: ct2,ct2:ct_p);
                if (ct2.IsCancellationRequested) { addText("Перервано...\r\n"); addVal(0,1,""); Thread.CurrentThread.Abort(); };
                addText("dg обчислено\r\n");
                IOFunc.writeGeoidHeightsAndAnomalysToTxt(greed, h_dg[0], h_dg[1], elipsoid, file + "B_L_N_dg.txt");
                addText("Готово...\r\nРезультати записано в файл: " + file + "NEW____________B_L_N_dg.txt\r\n");
            },ct2);
            var t3 = Task.Factory.StartNew(() => {
                label1:  if (task.IsCompleted)
                {
                     if (checkBox1.Checked) { System.Diagnostics.Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0"); };
                }
                else { task.Wait(); goto label1; }
            });
           
        }

        private void відмінитиПоточнуОпераціюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ts2.Cancel();
            відмінитиПоточнуОпераціюToolStripMenuItem.Enabled = false;
        }

        private void паузаToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (ts_p.IsCancellationRequested) { ts_p = new CancellationTokenSource();} else { ts_p.Cancel(); };
        }         
    }
}
