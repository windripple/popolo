/* Water.cs
 * 
 * Copyright (C) 2007 E.Togashi
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at
 * your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;

namespace Popolo.ThermophysicalProperty
{
    /// <summary>水の状態を取り扱う静的メソッドを提供するクラス</summary>
    public static class Water
    {

        #region HVACSIM+(J)から移植

        /// <summary>圧力[kPa]から蒸気の温度[C]を求める</summary>
        /// <param name="pkpa">圧力[kPa]</param>
        /// <returns>蒸気温度[C]</returns>
        public static double Tsats(double pkpa) {
            const double a1 = 42.6776d;
            const double b1 = -3892.70d;
            const double c1 = -9.48654d;
            const double tconv = -273.15d;
            const double a2 = -387.592d;
            const double b2 = -12587.5d;
            const double c2 = -15.2578d;
            const double pconv = 0.001d;

            double p = pkpa * pconv;
            if (p < 12.33d) return tconv + a1 + b1 / (Math.Log(p) + c1);
            else return tconv + a2 + b2 / (Math.Log(p) + c2);
        }

        /// <summary>温度[C]から飽和水蒸気圧[kPa]を求める</summary>
        /// <param name="tc">温度[C]</param>
        /// <returns>飽和水蒸気圧[kPa]</returns>
        public static double Psats(double tc) {
            const double a0 = 10.4592d;
            const double a1 = -0.40489e-2d;
            const double a2 = -0.417520e-4d;
            const double a3 = 0.368510e-6d;
            const double a4 = -0.101520e-8d;
            const double a5 = 0.865310e-12d;
            const double a6 = 0.903668e-15d;
            const double a7 = -0.199690e-17d;
            const double a8 = 0.779287e-21d;
            const double a9 = 0.191482e-24d;
            const double a10 = -3968.06d;
            const double a11 = 39.5735d;
            const double tconv = 273.15d;
            const double pconv = 1000.0d;

            double t = tc + tconv;
            double plog = a0 + t * (a1 + t * (a2 + t * (a3 + t * (a4 + t * (a5 + t * (a6 + t * (a7 + t * (a8 + t * a9)))))))) + a10 / (t - a11);
            return pconv * Math.Exp(plog);
        }

        /// <summary>蒸気比体積[m3/kg]を蒸気温度[C]および蒸気圧力[kPa]から計算する</summary>
        /// <param name="pkpa">蒸気圧力[kPa]</param>
        /// <param name="tc">蒸気温度[C]</param>
        /// <returns>蒸気比体積[m3/kg]</returns>
        public static double Vsats(double pkpa, double tc) {
            const double a = 1.0d;
            const double b = 1.6351057d;
            const double c = 52.584599d;
            const double d = -44.694653;
            const double e1 = -8.9751114d;
            const double e2 = -0.43845530d;
            const double e3 = -19.179576d;
            const double e4 = 36.765319d;
            const double e5 = -19.462437d;
            const double tcr = 647.3d;
            const double pcr = 22.089d;
            const double vcr = 3.155e-3d;
            const double tcnv = 23.15d;
            const double pcnv = 0.001d;

            double tr = (tcr - tc - tcnv) / tcr;
            double y = a + b * Math.Pow(tr, 1.0d / 3.0d) + c * Math.Pow(tr, 5.0d / 6.0d) + d * Math.Pow(tr, 0.875d);
            y += tr * (e1 + tr * (e2 + tr * (e3 + tr * (e4 + tr * e5))));
            return y * pcr * vcr / (pkpa * pcnv);
        }

        /// <summary>水比体積[m3/kg]を水温度[C]から計算する</summary>
        /// <param name="tc">水温度[C]</param>
        /// <returns>水比体積[m3/kg]</returns>
        public static double Vsatw(double tc) {
            const double a = 1.0d;
            const double b = -1.9153882d;
            const double c = 12.015186d;
            const double d = -7.8464025d;
            const double e1 = -3.8886414d;
            const double e2 = 2.0582238d;
            const double e3 = -2.0829991d;
            const double e4 = 0.82180004d;
            const double e5 = 0.47549742d;
            const double tcr = 647.3d;
            const double vcr = 3.155e-3d;
            const double tcnv = 273.15d;

            double tr = (tcr - tc - tcnv) / tcr;
            double y = a + b * Math.Pow(tr, 1.0d / 3.0d) + c * Math.Pow(tr, 5.0d / 6.0d) + d * Math.Pow(tr, 0.875d);
            y += tr * (e1 + tr * (e2 + tr * (e3 + tr * (e4 + tr * e5))));
            return y * vcr;
        }

        /// <summary>水エンタルピー[kJ/kg]を水温度[C]から求める</summary>
        /// <param name="tc">水温度[C]</param>
        /// <returns>水エンタルピー[kJ/kg]</returns>
        public static double Hsatw(double tc) {
            const double e11 = 624.698837d;
            const double e21 = -2343.85369d;
            const double e31 = -9508.12101d;
            const double hfcr = 2099.3d;
            const double e41 = 71628.7928d;
            const double e51 = -163535.221d;
            const double e61 = 166531.093d;
            const double tcnv = 273.15d;
            const double e71 = -64785.4585d;
            const double a2 = 0.8839230108d;
            const double e12 = -2.67172935d;
            const double e22 = 6.22640035d;
            const double e32 = -13.1789573d;
            const double e42 = -1.91322436d;
            const double e52 = 68.793763d;
            const double e62 = -124.819906d;
            const double e72 = 72.1435404d;
            const double a3 = 1.0d;
            const double b3 = -0.441057805d;
            const double c3 = -5.52255517d;
            const double d3 = 6.43994847d;
            const double e13 = -1.64578795d;
            const double e23 = -1.30574143d;
            const double tcr = 647.3d;

            double tk = tc + tcnv;
            double tr = (tcr - tk) / tcr;
            double y;
            if (tk < 300.0d) y = tr * (e11 + tr * (e21 + tr * (e31 + tr * (e41 + tr * (e51 + tr * (e61 + tr * e71))))));
            else if (tk < 600.0d) y = tr * (e12 + tr * (e22 + tr * (e32 + tr * (e42 + tr * (e52 + tr * (e62 + tr * e72)))))) + a2;
            else y = a3 + b3 * Math.Pow(tr, 1.0d / 3.0d) + c3 * Math.Pow(tr, 5.0d / 6.0d) + d3 * Math.Pow(tr, 0.875) + tr * (e13 + tr * e23);
            return y * hfcr;
        }

        /// <summary>水の気化熱[kJ/kg]を水温度[C]から求める</summary>
        /// <param name="tc">水温度[C]</param>
        /// <returns>水の気化熱[kJ/kg]</returns>
        public static double Hfg(double tc) {
            const double e1 = -3.87446d;
            const double e2 = 2.94553d;
            const double e3 = -8.06395d;
            const double e4 = 11.5633d;
            const double e5 = -6.02884d;
            const double b = 0.779221d;
            const double c = 4.62668d;
            const double d = -1.07931d;
            const double hfgtp = 2500.9d;
            const double tcr = 647.3d;
            const double tcnv = 273.15d;

            if (tc < 0.0d) tc = 0.0d;
            double tr = (tcr - tc - tcnv) / tcr;
            if (tr < 0.0d) return 0.0d;
            double y = b * Math.Pow(tr, 1.0d / 3.0d) + c * Math.Pow(tr, 5.0d / 6.0d) + d * Math.Pow(tr, 0.875d);
            y += tr * (e1 + tr * (e2 + tr * (e3 + tr * (e4 + tr * e5))));
            return y * hfgtp;
        }

        /// <summary>飽和水蒸気のエンタルピー[kJ/kg]を温度[C]から求める</summary>
        /// <param name="tc">度[C]</param>
        /// <returns>飽和水蒸気のエンタルピー[kJ/kg]</returns>
        public static double Hsats(double tc) {
            const double e1 = -4.81351884d;
            const double e2 = 2.69411792d;
            const double e3 = -7.39064542d;
            const double e4 = 10.4961689d;
            const double e5 = -5.46840036d;
            const double b = 0.457874342d;
            const double c = 5.08441288d;
            const double d = -1.48513244d;
            const double a = 1.0d;
            const double tcr = 647.3d;
            const double hcr = 2099.3d;
            const double tcnv = 273.15d;

            double tr = (tcr - tc - tcnv) / tcr;
            double y = a + b * Math.Pow(tr, 1.0d / 3.0d) + c * Math.Pow(tr, 5.0d / 6.0d) + d * Math.Pow(tr, 0.875d);
            y += tr * (e1 + tr * (e2 + tr * (e3 + tr * (e4 + tr * e5))));
            return y * hcr;
        }

        /// <summary>飽和蒸気の水エントロピー[kJ/kgK]を温度[C]から求める</summary>
        /// <param name="tc">飽和蒸気の温度[C]</param>
        /// <returns>飽和蒸気の水エントロピー[kJ/kgK]</returns>
        public static double Ssatw(double tc) {
            const double e11 = -1836.92956d;
            const double e21 = 14706.6352d;
            const double e31 = -43146.6046d;
            const double scr = 4.4289d;
            const double e41 = 48606.6733d;
            const double e51 = 7997.5096d;
            const double e61 = -58333.9887d;
            const double tcnv = 273.15d;
            const double e71 = 33140.0718d;
            const double a2 = 0.912762917d;
            const double e12 = -1.75702956d;
            const double tcr = 647.3d;
            const double e22 = 1.68754095d;
            const double e32 = 5.82215341d;
            const double e42 = -63.3354786d;
            const double e52 = 188.076546d;
            const double e62 = -252.344531d;
            const double e72 = 128.058531d;
            const double a3 = 1.0d;
            const double b3 = -0.324817650d;
            const double c3 = -2.990556709d;
            const double d3 = 3.2341900d;
            const double e13 = -0.678067859d;
            const double e23 = -1.91910364d;

            double tk = tc + tcnv;
            double tr = (tcr - tk) / tcr;
            double y;
            if (tk < 300.0d) y = tr * (e11 + tr * (e21 + tr * (e31 + tr * (e41 + tr * (e51 + tr * (e61 + tr * e71))))));
            else if (tk < 600.0d) y = tr * (e12 + tr * (e22 + tr * (e32 + tr * (e42 + tr * (e52 + tr * (e62 + tr * e72)))))) + a2;
            else y = a3 + b3 * Math.Pow(tr, 1.0d / 3.0d) + c3 * Math.Pow(tr, 5.0d / 6.0d) + d3 * Math.Pow(tr, 0.875d) + tr * (e13 + tr * e23);

            return y * scr;
        }

        /// <summary>飽和水蒸気のエントロピー[kJ/kgK]を温度[C]から求める</summary>
        /// <param name="tc">飽和水蒸気の温度[C]</param>
        /// <returns>飽和水蒸気のエントロピー[kJ/kgK]</returns>
        public static double Ssats(double tc) {
            const double e1 = -4.34839d;
            const double e2 = 1.34672d;
            const double e3 = 1.75261d;
            const double e4 = -6.22295d;
            const double e5 = 9.99004d;
            const double a = 1.0d;
            const double b = 0.377391d;
            const double c = -2.78368d;
            const double d = 6.93135d;
            const double tcr = 647.3d;
            const double scr = 4.4289d;
            const double tcnv = 273.15d;
            double tr = (tcr - tc - tcnv) / tcr;
            double y = a + b * Math.Pow(tr, 1.0d / 3.0d) + c * Math.Pow(tr, 5.0d / 6.0d) + d * Math.Pow(tr, 0.875d);
            y += tr * (e1 + tr * (e2 + tr * (e3 + tr * (e4 + tr * e5))));
            return y * scr;
        }

        /// <summary>加熱蒸気の比体積[m3/kg]を圧力[kPa]および温度[C]から求める</summary>
        /// <param name="pkpa">圧力[kPa]</param>
        /// <param name="tc">温度[C]</param>
        /// <returns>加熱蒸気の比体積[m3/kg]</returns>
        public static double Vs(double pkpa, double tc) {
            const double r = 4.61631e-4d;
            const double b1 = 5.27993e-2d;
            const double b2 = 3.75928e-3d;
            const double b3 = 0.022d;
            const double em = 40.0d;
            const double a0 = -3.741378d;
            const double a1 = -4.7838281e-3d;
            const double a2 = 1.5923434e-5d;
            const double tcnv = 273.15d;
            const double a3 = 10.0d;
            const double c1 = 42.6776d;
            const double c2 = -3892.70d;
            const double c3 = -9.48654d;
            const double pcnv = 0.001d;
            const double c4 = -387.592d;
            const double c5 = -12587.5d;
            const double c6 = -15.2578d;

            double p = pkpa * pcnv;
            double t = tc + tcnv;
            double ts = c1 + c2 / (Math.Log(p) + c3);
            if (p >= 12.33d) ts = c4 + c5 / (Math.Log(p) + c6);
            return r * t / p - b1 * Math.Exp(-b2 * t) + (b3 - Math.Exp(a0 + ts * (a1 + ts * a2))) / (a3 * p) * Math.Exp((ts - t) / em);
        }

        /// <summary>加熱蒸気のエンタルピー[kJ/kg]を圧力[kPa]と温度[C]から求める</summary>
        /// <param name="pkpa">圧力[kPa]</param>
        /// <param name="tc">温度[C]</param>
        /// <returns>加熱蒸気のエンタルピー[kJ/kg]</returns>
        public static double Hs(double pkpa, double tc) {
            const double b11 = 2041.21d;
            const double b12 = -40.4002d;
            const double b13 = -0.48095d;
            const double b21 = 1.610693d;
            const double b22 = 5.472051e-2d;
            const double b23 = 7.517537e-4d;
            const double b31 = 3.383117e-4d;
            const double b32 = -1.975736e-5d;
            const double b33 = -2.87409e-7d;
            const double b41 = 1707.82d;
            const double b42 = -16.99419d;
            const double b43 = 6.2746295e-2d;
            const double b44 = -1.0284259e-4d;
            const double b45 = 6.4561298e-8d;
            const double em = 45.0d;
            const double c1 = 42.6776d;
            const double c2 = -3892.70d;
            const double c3 = -9.48654d;
            const double pcnv = 0.001d;
            const double c4 = -387.592d;
            const double c5 = -12587.5d;
            const double c6 = -15.2578d;
            const double tcnv = 273.15d;

            double p = pkpa * pcnv;
            double t = tc + tcnv;
            double ts = c1 + c2 / (Math.Log(p) + c3);
            if (p >= 12.33d) ts = c4 + c5 / (Math.Log(p) + c6);
            double a0 = b11 + p * (b12 + p * b13);
            double a1 = b21 + p * (b22 + p * b23);
            double a2 = b31 + p * (b32 + p * b33);
            double a3 = b41 + ts * (b42 + ts * (b43 + ts * (b44 + ts * b45)));
            return a0 + t * (a1 + t * a2) - a3 * Math.Exp((ts - t) / em);
        }

        /// <summary>加熱蒸気のエントロピー[kJ/kgK]を圧力[kPa]と温度[C]から求める</summary>
        /// <param name="pkpa">圧力[kPa]</param>
        /// <param name="tc">温度[C]</param>
        /// <returns>加熱蒸気のエントロピー[kJ/kgK]</returns>
        public static double Ss(double pkpa, double tc) {
            const double a0 = 4.6162961d;
            const double a1 = 1.039008e-2d;
            const double a2 = -9.873085e-6d;
            const double a3 = 5.4311e-9d;
            const double a4 = -1.170465e-12d;
            const double b1 = -0.4650306d;
            const double b2 = 0.001d;
            const double b3 = 10.0d;
            const double c0 = 1.777804d;
            const double c1 = -1.802468e-2d;
            const double c2 = 6.854459e-5d;
            const double c3 = -1.184434e-7d;
            const double em = 85.0d;
            const double c4 = 8.142201e-11d;
            const double e1 = 42.6776d;
            const double e2 = -3892.70d;
            const double e3 = -9.48654d;
            const double e4 = -387.592d;
            const double e5 = -12587.5d;
            const double e6 = -15.2578d;
            const double tcnv = 273.15d;

            double p = pkpa * b2;
            double t = tc + tcnv;
            double ts = e1 + e2 / (Math.Log(p) + e3);
            if (p >= 12.33d) ts = e4 + e5 / (Math.Log(p) + e6);
            return a0 + t * (a1 + t * (a2 + t * (a3 + t * a4))) + b1 * Math.Log(b2 + p * b3) - Math.Exp((ts - t) / em) * (c0 + ts * (c1 + ts * (c2 + ts * (c3 + ts * c4))));
        }

        /// <summary>加熱蒸気の温度[C]を圧力[kPa]とエントロピー[kJ/kgK]から求める</summary>
        /// <param name="p">圧力[kPa]</param>
        /// <param name="s">エントロピー[kJ/kgK]</param>
        /// <returns>加熱蒸気の温度[C]</returns>
        public static double Tpss(double p, double s) {
            const double e1 = 42.6776d;
            const double e2 = -3892.70d;
            const double e3 = -9.48654d;
            const double pcnv = 0.001d;
            const double e4 = -387.592d;
            const double e5 = -12587.5d;
            const double e6 = -15.2578d;
            const double tabs = 273.15d;

            //compare input entropy with saturation value
            double t0 = e1 - tabs + e2/(Math.Log(p * pcnv) + e3);
            if (p >= 12330.0d) t0 = e4 - tabs + e5 / (Math.Log(p * pcnv) + e6);
            double s0 = Ssats(t0);
            if (s0 >= s) return t0;

            //Initial guess TA is based on assumption of constant specific heat.
            //Subsequent approximations made by interpolation.
            double ta = (t0 + tabs) * (1.0d + (s - s0) / Cps(t0)) - tabs;
            double sa = Ss(p, ta);
            double t = 0.0d;
            for (int i = 0; i < 10; i++) {
                t = ta + (t0 - ta) * (s - sa) / (s0 - sa);
                if (Math.Abs(t - ta) < 0.05d) break;
                t0 = ta;
                s0 = sa;
                ta = t;
                sa = Ss(p, ta);
                if (i == 9) throw new Exception("FUNCTION WaterProperty.Tpss FAILS TO CONVERGE");
            }
            return t;
        }

        /// <summary>蒸気の比熱[kJ/kg/K]を温度[C]から求める</summary>
        /// <param name="t">温度[C]</param>
        /// <returns>蒸気の比熱[kJ/kg/K]</returns>
        /// <remarks>
        /// Specific heat equation from "Fundamentals of Classical Thermodynamics-SI Version" by Van Wylen and Sonntag Table A.9, pg. 683.
        /// Valid for T between 300-3500 K   max error = .43%
        /// </remarks>
        public static double Cps(double t) {
            const double c1 = 143.05d;
            const double c2 = -183.54d;
            const double c3 = 82.751d;
            const double c4 = -3.6989d;
            const double e1 = 0.25d;
            const double e2 = 0.5d;

            double tk = t + 273.15d;
            if (tk < 300.0d || tk > 3500.0d) throw new Exception("FUNCTION WaterProperty.Cps: T OUT OF RANGE");
            double t1 = tk / 100.0d;
           return (c1 + c2 * Math.Pow(t1, e1) + c3 * Math.Pow(t1, e2) + c4 * t1) / 18.015d;
        }

        /// <summary>比熱[kJ/kg/K]を比体積[m3/kg]と温度[C]から求める</summary>
        /// <param name="v">比体積[m3/kg]</param>
        /// <param name="t">温度[C]</param>
        /// <returns>比熱[kJ/kg/K]</returns>
        public static double Cvs(double v, double t) {
            const double tc = 1165.11d;
            const double tfr = 459.67d;
            const double b1 = 0.0063101d;
            const double a0 = 0.99204818d;
            const double a1 = -33.137211d;
            const double a2 = 416.29663d;
            const double a3 = 0.185053d;
            const double a4 = 5.475d;
            const double a5 = -2590.5815d;
            const double a6 = 113.95968d;

            double tr = 9.0d / 5.0d * t + 32.0d + tfr;
            double ve = (v - b1) / 0.062428d;
            return (a0 + a1 / Math.Sqrt(tr) + a2 / tr - a3 * Math.Pow(a4, 2) * tr / Math.Pow(tc, 2) * Math.Exp(-a4 * tr / tc) * (a5 / ve + a6 / Math.Pow(ve, 2))) * 4.1868;
        }

        /// <summary>飽和水蒸気の動粘性係数[kg/m-s]を圧力[kPa]から求める</summary>
        /// <param name="p">圧力[kPa]</param>
        /// <returns>飽和水蒸気の動粘性係数[kg/m-s]</returns>
        /// <remarks>'Heat Transfer' by Alan J. Chapman, 1974.</remarks>
        public static double Vissv(double p) {
            const double c1 = 0.0314d;
            const double c2 = 2.9675e-5d;
            const double c3 = -1.60583e-8d;
            const double c4 = 3.768986e-12d;

            //Convert pressure from kPa to psi
            double psi = p / 6.894757d;
            double vissv = c1 + c2 * psi + c3 * Math.Pow(psi, 2) + c4 * Math.Pow(psi, 3);
            //Convert viscosity from lbm/ft-hr to kg/m-s
            return vissv * 4.1338e-4d;
        }

        /// <summary>加熱蒸気の動粘性係数[kg/m-s]を温度[C]から求める</summary>
        /// <param name="t">温度[C]</param>
        /// <returns>加熱蒸気の動粘性係数[kg/m-s]</returns>
        /// <remarks>'Heat Transfer' by Alan J. Chapman, 1974. (Note: there is little  variation in viscosity at higher pressures.)</remarks>
        public static double Vissph(double t) {
            const double c1 = 0.0183161d;
            const double c2 = 5.7067e-5d;
            const double c3 = -1.42253e-8d;
            const double c4 = 7.241555e-12d;

            //Convert temperature from C to F
            double tf = t * 1.8 + 32.0d;
            double vissph = c1 + c2 * tf + c3 * Math.Pow(tf, 2) + c4 * Math.Pow(tf, 3);
            //Convert viscosity from lbm/ft-hr to kg/m-s
            return vissph * 4.1338e-4d;
        }

        /// <summary>加熱蒸気の熱伝導率[kW/m-C]を温度[C]から求める</summary>
        /// <param name="t">温度[C]</param>
        /// <returns>加熱蒸気の熱伝導率[kW/m-C]</returns>
        /// <remarks>'Heat Transfer' by Alan J. Chapman, 1974.</remarks>
        public static double Steamk(double t)
        {
            const double c1 = 0.824272d;
            const double c2 = 0.00254627d;
            const double c3 = 9.848539e-8d;

            //Convert temperature from C to F
            double tf = t * 1.8d + 32.0d;
            double steamk = (c1 + c2 * tf + c3 * Math.Pow(tf, 2)) * 0.01d;
            //Convert K from Btu/hr-ft-F to kW/m-C
            return steamk * 0.0017308d;
        }

        /// <summary>1[atm]での水密度[kg/m3]を温度[C]から求める</summary>
        /// <param name="tw">温度[C]</param>
        /// <returns>1[atm]での水密度[kg/m3]</returns>
        /// <remarks>CRC Handbook of Chem. and Phys., 61st Edition (1980-1981), p. F-6.</remarks>
        public static double Wrho(double tw) {
            const double ar0 = 999.83952d;
            const double ar1 = 16.945176d;
            const double ar2 = -0.0079870401d;
            const double ar3 = -46.170461e-6d;
            const double ar4 = 105.56302e-9d;
            const double ar5 = -280.54253e-12d;
            const double ar6 = 0.01687985d;

            return (ar0 + tw * (ar1 + tw * (ar2 + tw * (ar3 + tw * (ar4 + tw * ar5))))) / (1.0d + ar6 * tw);
        }

        /// <summary>1[atm]での水の粘性[kg/m-s]を温度[C]から求める</summary>
        /// <param name="tw">温度[C]</param>
        /// <returns>1[atm]での水の粘性[kg/m-s]</returns>
        /// <remarks>Fit to data from Karlekar and Desmond</remarks>
        public static double Wmu(double tw) {
            const double am0 = -3.30233d;
            const double am1 = 1301.0d;
            const double am2 = 998.333d;
            const double am3 = 8.1855d;
            const double am4 = 0.00585d;
            const double am5 = 1.002d;
            const double am6 = -1.3272d;
            const double am7 = -0.001053d;
            const double am8 = 105.0d;
            const double am10 = 0.68714d;
            const double am11 = -0.0059231d;
            const double am12 = 2.1249e-5d;
            const double am13 = -2.69575e-8d;

            double wmu;
            if (tw < 20.0d) wmu = Math.Pow(10.0d, am0 + am1 / (am2 + (tw - 20.0d) * (am3 + am4 * (tw - 20.0d)))) * 100.0d;
            else if (tw > 100.0d) wmu = am10 + tw * (am11 + tw * (am12 + tw * am13));
            else wmu = am5 * Math.Pow(10.0d, (tw - 20.0d) * (am6 + (tw - 20.0d) * am7) / (tw + am8));
            return 0.001 * wmu;
        }

        /// <summary>1[atm]での蒸気の熱伝導率[kW/m-K]を温度[C]から求める</summary>
        /// <param name="tw">温度[C]</param>
        /// <returns>1[atm]での蒸気の熱伝導率[kW/m-K]</returns>
        public static double Wk(double tw) {
            const double ak0 = 0.560101d;
            const double ak1 = 0.00211703d;
            const double ak2 = -1.05172e-5d;
            const double ak3 = 1.497323e-8d;
            const double ak4 = -1.48553e-11d;

            return 0.001d * (ak0 + tw * (ak1 + tw * (ak2 + tw * (ak3 + tw * ak4))));
        }

        /// <summary>1[atm]での空気の比熱[kJ/kg-C]を温度[C]から求める</summary>
        /// <param name="tw">温度[C]</param>
        /// <returns>1[atm]での空気の比熱[kJ/kg-C]</returns>
        public static double Wcp(double tw) {
            const double acp0 = 4.21534d;
            const double acp1 = -0.00287819d;
            const double acp2 = 7.4729e-5d;
            const double acp3 = -7.79624e-7d;
            const double acp4 = 3.220424e-9d;
            const double acp5 = 2.9735d;
            const double acp6 = 0.023049d;
            const double acp7 = -0.00013953d;
            const double acp8 = 3.092474e-7d;

            if (tw > 100.0d) return acp5 + tw * (acp6 + tw * (acp7 + tw * acp8));
            else return acp0 + tw * (acp1 + tw * (acp2 + tw * (acp3 + tw * acp4)));
        }

        #endregion

    }
}
