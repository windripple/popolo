/* OrdinaryDifferenctialEquations.cs
 * 
 * Copyright (C) 2011 E.Togashi
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or (at
 * your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Popolo.Numerics
{
    /// <summary>常微分方程式ソルバ</summary>
    public class OrdinaryDifferentialEquations
    {

        #region instance variables

        /// <summary>一次記憶領域</summary>
        double[] dym, dyt, yt, dysav, ysav, ytemp;

        #endregion

        #region enumerators

        /// <summary>ソルバの種類</summary>
        public enum SolverType
        {
            /// <summary></summary>
            RungeKutta4 = 0
        }

        #endregion

        #region delegates

        /// <summary>error function</summary>
        /// <param name="time">evaluating time</param>
        /// <param name="outputs">outputs of function at time</param>
        /// <param name="derivative">derivative of function at time</param>
        public delegate void errorFunction(double time, double[] outputs, ref double[] derivative);

        #endregion

        #region constructor

        /// <summary>Constructor</summary>
        /// <param name="solverType">solver type</param>
        /// <param name="variableNumber">number of variables</param>
        public OrdinaryDifferentialEquations(SolverType solverType, uint variableNumber)
        {
            dym = new double[variableNumber];
            dyt = new double[variableNumber];
            yt = new double[variableNumber];
            dysav = new double[variableNumber];
            ysav = new double[variableNumber];
            ytemp = new double[variableNumber];
        }

        #endregion        

        #region internal methods

        /// <summary></summary>
        /// <param name="eFnc"></param>
        /// <param name="ystart"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="eps"></param>
        /// <param name="h1"></param>
        /// <param name="hmin"></param>
        public void Solve(errorFunction eFnc, ref double[] ystart, double x1, double x2, double eps, double h1, double hmin)
        {
            const int MAXSTP = 10000;
            const double TINY = 1.0e-30;

            double hdid = 0;
            double hnext = 0;
            double[] yscal = new double[ystart.Length];
            double[] y = new double[ystart.Length];
            double[] dydx = new double[ystart.Length];

            double x = x1;
            double h = h1 * Math.Sign(x2 - x1);

            for (int i = 0; i < yscal.Length; i++) y[i] = ystart[i];

            for (int nstp = 0; nstp < MAXSTP; nstp++)
            {
                eFnc(x, y, ref dydx);
                for (int i = 0; i < yscal.Length; i++) yscal[i] = Math.Abs(y[i]) + Math.Abs(dydx[i] * h) + TINY;

                if ((x + h - x2) * (x + h - x1) > 0.0) h = x2 - x;  //終端部の場合

                rungeKutta4QualityControl(eFnc, y, dydx, ref x, h, eps, yscal, ref hdid, ref hnext);
                if ((x - x2) * (x2 - x1) >= 0.0)
                {
                    for (int i = 0; i < yscal.Length; i++) ystart[i] = y[i];
                    return;
                }
                if (Math.Abs(hnext) <= hmin) throw new Exception("Step size too small");
                h = hnext;
            }
            throw new Exception("Too many steps");
        }

        #endregion

        #region Runge Kutta method

        /// <summary></summary>
        /// <param name="eFnc"></param>
        /// <param name="y"></param>
        /// <param name="dydx"></param>
        /// <param name="h"></param>
        /// <param name="x"></param>
        /// <param name="yout"></param>
        private void rungeKutta4(errorFunction eFnc, double[] y, double[] dydx, double h, double x, ref double[] yout)
        {
            double hh = h * 0.5f;
            double h6 = h / 6.0f;
            double xh = x + hh;

            for (int i = 0; i < dydx.Length; i++) yt[i] = y[i] + hh * dydx[i];  //k1
            eFnc(xh, yt, ref dyt);  //k2計算用の微分値
            for (int i = 0; i < dydx.Length; i++) yt[i] = y[i] + hh * dyt[i];  //k2
            eFnc(xh, yt, ref dym);  //k3計算用の微分値
            for (int i = 0; i < dydx.Length; i++)
            {
                yt[i] = y[i] + h * dym[i];
                dym[i] += dyt[i];
            }
            eFnc(x + h, yt, ref dyt);  //k4計算用の微分値
            for (int i = 0; i < dydx.Length; i++)
            {
                yout[i] = y[i] + h6 * (dydx[i] + dyt[i] + 2.0f * dym[i]);
            }
        }

        /// <summary></summary>
        /// <param name="eFnc">誤差評価関数</param>
        /// <param name="y">yの初期値</param>
        /// <param name="dydx">dy/dxの初期値</param>
        /// <param name="x">xの初期値</param>
        /// <param name="htry">刻幅hの初期候補値</param>
        /// <param name="eps">許容誤差</param>
        /// <param name="yscal">許容誤差を調整するベクトル</param>
        /// <param name="hdid">実際の刻幅</param>
        /// <param name="hNext">次のステップの刻幅候補値</param>
        private void rungeKutta4QualityControl(errorFunction eFnc, double[] y, double[] dydx,
            ref double x, double htry, double eps, double[] yscal, ref double hdid, ref double hNext)
        {
            //初期値を保存しておく
            double xsav = x;
            for (int i = 0; i < y.Length; i++)
            {
                ysav[i] = y[i];
                dysav[i] = dydx[i];
            }

            double h = htry;

            while (true)
            {
                double hh = 0.5 * h;
                rungeKutta4(eFnc, ysav, dysav, hh, xsav, ref ytemp);
                x = xsav + hh;
                eFnc(x, ytemp, ref dydx);
                rungeKutta4(eFnc, ytemp, dydx, hh, x, ref y);
                x = xsav + h;
                if (x == xsav) throw new Exception("Step size too small");
                rungeKutta4(eFnc, ysav, dysav, h, xsav, ref ytemp);
                double errmax = 0.0;
                for (int i = 0; i < y.Length; i++)
                {
                    ytemp[i] = y[i] - ytemp[i];
                    double temp = Math.Abs(ytemp[i] / yscal[i]);
                    if (errmax < temp) errmax = temp;
                }
                errmax /= eps;
                if (errmax <= 1.0)
                {
                    hdid = h;
                    hNext = (errmax > 6.0e-4 ? 0.9 * h * Math.Exp(-0.2 * Math.Log(errmax)) : 4.0 * h);
                    break;
                }
                h = 0.9 * h * Math.Exp(-0.2 * Math.Log(errmax));
            }
            for (int i = 0; i < y.Length; i++) y[i] += ytemp[i] * 0.06666666;
        }

        #endregion

    }
}
