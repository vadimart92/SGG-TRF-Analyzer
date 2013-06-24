using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SphericalHarmonicAnalyze
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
        }
        public void show1(ref CancellationToken ct) { this.Show(); while (true) { Thread.Sleep(64); if (ct.IsCancellationRequested) { this.Dispose(); break; };  } }


    }
}
