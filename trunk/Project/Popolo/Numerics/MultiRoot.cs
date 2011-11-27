/* MultiRoot.cs
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
    /// <summary>非線形連立代数方程式ソルバー</summary>
    public class MultiRoot
    {

        #region constant

        private const double DELTA_X = 1.0e-10;

        #endregion

        #region instance variables

        private double[] outputs1, outputs2;

        private Matrix jacobian;

        private Vector fnc;

        #endregion

        #region properties

        /// <summary>Get number of variables</summary>
        public uint VariableNumber
        {
            get;
            private set;
        }

        #endregion

        #region delegate
        
        /// <summary>Error function</summary>
        /// <param name="variables">variables</param>
        /// <param name="outputs">outputs</param>
        public delegate void errorFunction(double[] variables, ref double[] outputs);

        #endregion

        #region internal methods

        /// <summary>Constructor</summary>
        /// <param name="variableNumber">number of variables and functions</param>
        public MultiRoot(uint variableNumber)
        {
            this.VariableNumber = variableNumber;
            outputs1 = new double[variableNumber];
            outputs2 = new double[variableNumber];
            jacobian = new Matrix(variableNumber, variableNumber);
            fnc = new Vector(variableNumber);
        }

        /// <summary>非線形連立代数方程式ソルバ</summary>
        /// <param name="eFnc">誤差関数</param>
        /// <param name="x">入力変数</param>
        /// <param name="aError">絶対誤差許容値</param>
        /// <param name="rError">相対誤差許容値（変化量が小さくなった場合の停止判定）</param>
        /// <param name="num">反復回数上限値</param>
        /// <param name="iteration">反復回数</param>
        public void Solve(errorFunction eFnc, ref double[] x, double aError, double rError, int num, out uint iteration)
        {
            //指定回数繰り返す
            for (iteration = 0; iteration < num; iteration++)
            {
                //ヤコビアンを計算
                calculateJacobian(eFnc, x, ref outputs1, ref outputs2, ref jacobian);
                double errf = 0;
                for (uint j = 0; j < VariableNumber; j++)
                {
                    errf += Math.Abs(outputs1[j]);
                    fnc.SetValue(j, -outputs1[j]);
                }
                if (errf < aError) return;

                //LU分解で方程式を解く
                jacobian.LUDecomposition();
                jacobian.LUSolve(ref fnc);

                double errx = 0;
                for (uint j = 0; j < VariableNumber; j++)
                {
                    errx += Math.Abs(fnc.GetValue(j));
                    x[j] += fnc.GetValue(j);
                }
                if (errx < rError) return;
            }
        }

        #endregion

        #region private methods

        /// <summary>ヤコビアンを計算する</summary>
        /// <param name="eFnc"></param>
        /// <param name="x"></param>
        /// <param name="y1"></param>
        /// <param name="y2">計算用配列</param>
        /// <param name="jacobian"></param>
        private static void calculateJacobian(errorFunction eFnc, double[] x, ref double[] y1, ref double[] y2, ref Matrix jacobian)
        {
            uint varNum = jacobian.Columns;

            //現在の入力条件での出力を取得
            eFnc(x, ref y1);

            //Make jacobian matrix
            for (uint j = 0; j < varNum; j++)
            {
                //差分を戻す
                if (j != 0) x[j - 1] -= DELTA_X;
                x[j] += DELTA_X;

                //出力計算
                eFnc(x, ref y2);
                for (uint i = 0; i < varNum; i++)
                {
                    jacobian.SetValue(i, j, (y2[i] - y1[i]) / DELTA_X);
                }
            }
            //差分を戻す
            x[varNum - 1] -= DELTA_X;
        }

        #endregion

    }
}
