using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SphericalHarmonicAnalyze.Data
{
    /// <summary>
    /// Клас ReferenceSystem є об'єктом який відповідає прийнятій Нормальній Землі і 
    /// містить як її основні так і додаткові константи 
    /// </summary>
  public  class ReferenceSystem
    {
        public enum Default { TideFree, WGS84, ITRF, USC2000 };
        public Grid gridParameters { get; set; }
        public double satelliteSphere { get; set; }
        public int maxDegree { get; set; }
        public double GM { get; private set; }
        public double a { get; private set; }
        public double f { get; private set; }
        public double b { get; private set; }
        public double firstExcentricity_2 { get; private set; }
        public double secondExcentricity_2 { get; private set; }
        public double omega { get; private set; }
        public double J2 { get; private set; }
        public double m_gamma { get; private set; }
        public double C_J2 { get; private set; }
        public double q0 { get; private set; }
        public double q0_quote { get; private set; }
        public double gamma_a { get; private set; }
        public double gamma_b { get; private set; }
        public double k { get; private set; }
        //Конструктор для створення об'єкту за параметрами GM, a, f, omega, Grid, maxDegree, r (радіус сітки вимірів)  
        public ReferenceSystem(double GM, double a, double f, double omega, Grid gridParameters, int maxDegree=100, double satelliteSphere=6500000)
        {
            this.GM = GM;
            this.a = a;
            this.f = f;
            this.omega = omega;
            this.gridParameters = gridParameters;
            this.maxDegree = maxDegree;
            this.satelliteSphere = satelliteSphere;
            mainInit();
        }
        /// <summary>
        /// Конструктор для створення об'єкту ReferenceSystem з параметрами за замовчуванням
        /// </summary>
        public ReferenceSystem(ReferenceSystem.Default defaulParams, double satelliteSphere=6500000)
        {
            //Система "вільна від припливних чинників" 
            //http://earth-info.nga.mil/GandG/wgs84/gravitymod/egm2008/egm08_wgs84.html
            if (defaulParams == Default.TideFree)
            {
                this.GM = 0.3986004415E+15;
                this.a = 6378136.46;
                this.f = (double)1 / 298.257686;
                this.omega = 7292115E-11;
                this.gridParameters = new Grid(1.6,25);
                this.maxDegree = 90;
                this.satelliteSphere = satelliteSphere;
                mainInit();
            }
            if (defaulParams == Default.ITRF)
            {
                this.GM = 3.986004418E14;
                this.a = 6378136.6;
                this.f = (double)1 / 298.25642;
                this.omega = 7292115E-11;
                this.gridParameters = new Grid(1.6, 25);
                this.maxDegree = 90;
                this.satelliteSphere = satelliteSphere;
                mainInit();
            }
            if (defaulParams == Default.WGS84)
            {
                this.GM = 3986004.418E8;
                this.a = 6378137;
                this.f = (double)1 / 298.257223563;
                this.omega = 7292115E-11;
                this.gridParameters = new Grid(1.6, 25);
                this.maxDegree = 90;
                this.satelliteSphere = satelliteSphere;
                mainInit();
            }
            if (defaulParams == Default.USC2000)
            {
                this.GM = 3.986004418E14;
                this.a = 6378245;
                this.f = (double)1 / 298.3;
                this.omega = 7292115E-11;
                this.gridParameters = new Grid(1.6, 25);
                this.maxDegree = 90;
                this.satelliteSphere = satelliteSphere;
                mainInit();
            }
        }
      //Обчислення додаткових (геометричних і фізичних) констант
        private void mainInit() {
this.firstExcentricity_2 = 2*this.f-Math.Pow(this.f,2D);
this.secondExcentricity_2 = (this.firstExcentricity_2) / (1 - this.firstExcentricity_2);
this.b = (1D - this.f) * this.a;
this.m_gamma = (Math.Pow(this.omega, 2D) * Math.Pow(this.a, 2D) * this.b) / this.GM;
this.J2 = 2D / 3D * this.f - 1D / 3D * this.m_gamma - 1D / 3D * Math.Pow(this.f, 2D) + 2D / 21D * this.f * this.m_gamma;
this.C_J2 = this.GM * Math.Pow(this.a, 2D) * this.J2;
double e = Math.Sqrt(this.firstExcentricity_2);
this.q0 = 0.5 * ((1D + 3D / (this.secondExcentricity_2)) * Math.Atan(Math.Sqrt(this.secondExcentricity_2)) - 3D / Math.Sqrt(this.secondExcentricity_2));
this.q0_quote = 3D * (1D + 1D / this.secondExcentricity_2) * (1D - (Math.Atan(Math.Sqrt(this.secondExcentricity_2)) / (Math.Sqrt(this.secondExcentricity_2)))) - 1D;
this.gamma_a = this.GM/(this.a*this.b)*(1d-this.m_gamma-this.m_gamma/6d*Math.Sqrt(this.secondExcentricity_2)*this.q0_quote/this.q0);
this.gamma_b = this.GM / (this.a * this.a) * (1d + this.m_gamma / 3d * Math.Sqrt(this.secondExcentricity_2) * this.q0_quote / this.q0);
this.k = this.b * this.gamma_b / (this.a * this.gamma_a) - 1d;
        }
    }
        /// <summary>
        /// Реалізує сукупність параметрів сітки регуляризації
        /// </summary>
       public class Grid
    {
        public double cellSize = 0;//розмір клітинки
        public double coLatitudeBounds = 0;//мін. полярна відстань
        public double coLatitudeBounds2 = 180;//макс. полярна відстань
        public double longitudeBoundE = 360;//макс. довгота
        public double longitudeBoundW = 0;//мін. довгота
        public Grid() {
            this.cellSize = 1;
            this.coLatitudeBounds = 0;
            coLatitudeBounds2 = 180;
            this.longitudeBoundE = 360;
            this.longitudeBoundW = 0;
            }
        public Grid(double cellSize, double coLatitudeBounds, double longitudeBoundW=0, double longitudeBoundE=360)
        {
            this.cellSize = cellSize;
            this.coLatitudeBounds = coLatitudeBounds;
            this.longitudeBoundE = longitudeBoundE;
            this.longitudeBoundW = longitudeBoundW;
        }
    }
}
