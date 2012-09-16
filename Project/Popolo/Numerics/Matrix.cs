/* Matrix.cs
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

namespace Popolo.Numerics
{
    /// <summary>Matrix class</summary>
    internal class Matrix
    {

        #region instance variables

        /// <summary>Is matrix view?</summary>
        private bool isMatrixView = false;

        /// <summary>column number of original matrix</summary>
        private uint columnStartNumber;

        /// <summary>row number of original matrix</summary>
        private uint rowStartNumber;

        /// <summary>column size of matrix view</summary>
        private uint viewColumnSize;

        /// <summary>row size of matrix view</summary>
        private uint viewRowSize;

        /// <summary>originalMatrix</summary>
        private Matrix originalMatrix;

        /// <summary>matrix array</summary>
        private double[,] mat;

        /// <summary>置換ベクトル</summary>
        private uint[] perm;

        #endregion

        #region properties

        /// <summary>Get number of columns</summary>
        internal uint Columns
        {
            get
            {
                if (isMatrixView) return viewColumnSize;
                else return (uint)mat.GetLength(0);
            }
        }

        /// <summary>Get number of rows</summary>
        internal uint Rows
        {
            get
            {
                if (isMatrixView) return viewRowSize;
                else return (uint)mat.GetLength(1);
            }
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        /// <param name="rowSize">size of rows</param>
        /// <param name="columnSize">size of columns</param>
        internal Matrix(uint rowSize, uint columnSize)
        {
            mat = new double[rowSize, columnSize];
            perm = new uint[rowSize];
        }

        /// <summary>Make matrix object from other matrix object</summary>
        /// <param name="columnSize">size of columns</param>
        /// <param name="rowSize">size of rows</param>
        /// <param name="columnStartNumber">column start number</param>
        /// <param name="rowStartNumber">row start number</param>
        /// <param name="originalMatrix">originalMatrix</param>
        internal Matrix(uint rowSize, uint columnSize, uint rowStartNumber, uint columnStartNumber, Matrix originalMatrix)
        {
            isMatrixView = true;
            this.columnStartNumber = columnStartNumber;
            this.rowStartNumber = rowStartNumber;
            this.viewColumnSize = columnSize;
            this.viewRowSize = rowSize;
            this.originalMatrix = originalMatrix;
            perm = new uint[rowSize];
        }

        #endregion

        #region internal methods

        /// <summary>Initialize matrix</summary>
        /// <param name="value">value of elements</param>
        internal void Initialize(double value)
        {
            if (isMatrixView)
            {
                for (uint i = 0; i < viewRowSize; i++)
                {
                    for (uint j = 0; j < viewColumnSize; j++)
                    {
                        originalMatrix.SetValue(i + viewRowSize, j + viewColumnSize, value);
                    }
                }
            }
            else
            {
                for (int i = 0; i < mat.GetLength(0); i++)
                {
                    for (int j = 0; j < mat.GetLength(1); j++)
                    {
                        mat[i, j] = value;
                    }
                }
            }
        }

        /// <summary>単位行列を作成する</summary>
        internal void MakeUnitMatrix()
        {
            if (isMatrixView)
            {
                for (uint i = 0; i < viewRowSize; i++)
                {
                    for (uint j = 0; j < viewColumnSize; j++)
                    {
                        if (i == j) originalMatrix.SetValue(i + viewRowSize, j + viewColumnSize, 1);
                        else originalMatrix.SetValue(i + viewRowSize, j + viewColumnSize, 0);
                    }
                }
            }
            else
            {
                for (int i = 0; i < mat.GetLength(0); i++)
                {
                    for (int j = 0; j < mat.GetLength(1); j++)
                    {
                        if (i == j) mat[i, j] = 1;
                        else mat[i, j] = 0;
                    }
                }
            }
        }

        /// <summary>行列の値を取得する</summary>
        /// <param name="rowNumber">行番号</param>
        /// <param name="columnNumber">列番号</param>        
        /// <returns>行列の値</returns>
        internal double GetValue(uint rowNumber, uint columnNumber)
        {
            if (isMatrixView)
            {
                return originalMatrix.GetValue(rowNumber + rowStartNumber, columnNumber + columnStartNumber);
            }
            else
            {
                return mat[rowNumber, columnNumber];
            }
        }

        /// <summary>行列要素に値を設定する</summary>
        /// <param name="rowNumber">行番号</param>
        /// <param name="columnNumber">列番号</param>
        /// <param name="value">設定する値</param>
        internal void SetValue(uint rowNumber, uint columnNumber, double value)
        {            
            if (isMatrixView)
            {
                originalMatrix.SetValue(rowNumber + rowStartNumber, columnNumber + columnStartNumber, value);
            }
            else
            {
                mat[rowNumber, columnNumber] = value;
            }
        }

        /// <summary>行列要素に値を加算する</summary>
        /// <param name="rowNumber">行番号</param>
        /// <param name="columnNumber">列番号</param>
        /// <param name="value">加算する値</param>
        internal void AddValue(uint rowNumber, uint columnNumber, double value)
        {
            if (isMatrixView)
            {
                originalMatrix.AddValue(rowNumber + rowStartNumber, columnNumber + columnStartNumber, value);
            }
            else
            {
                mat[rowNumber, columnNumber] += value;
            }
        }

        /// <summary>行列とベクトルの積と和を計算する（y = α op(A) x + βy）</summary>
        /// <param name="vectorX">ベクトルX</param>
        /// <param name="vectorY">ベクトルY（解が上書きされる）</param>
        /// <param name="alpha">第一項の係数</param>
        /// <param name="beta">第二項の係数</param>
        internal void VectorProduct(Vector vectorX, ref Vector vectorY, double alpha, double beta)
        {
            for (uint i = 0; i < this.Columns; i++)
            {
                vectorY.SetValue(i, vectorY.GetValue(i) * beta);
                for (uint j = 0; j < this.Rows; j++)
                {
                    vectorY.AddValue(i, alpha * this.GetValue(i, j) * vectorX.GetValue(j));
                }
            }
        }

        /// <summary>行列と行列の積と和を計算する（y = α op(A) x + βy）</summary>
        /// <param name="matrixX">行列X</param>
        /// <param name="matrixY">行列Y（解が上書きされる）</param>
        /// <param name="alpha">第一項の係数</param>
        /// <param name="beta">第二項の係数</param>
        internal void MatrixProduct(Matrix matrixX, ref Matrix matrixY, double alpha, double beta)
        {
            for (uint i = 0; i < this.Columns; i++)
            {
                for (uint j = 0; j < matrixX.Rows; j++)
                {
                    double sum = 0;
                    for (uint k = 0; k < this.Rows; k++)
                    {
                        sum += this.GetValue(i, k) * matrixX.GetValue(k, j);
                    }
                    matrixY.SetValue(i, j, alpha * sum + beta * matrixY.GetValue(i, j));
                }
            }
        }

        /// <summary></summary>
        /// <remarks>Transrated from "numerical recipies in C"</remarks>
        internal void LUDecomposition()
        {
            const double TINY = 1.0e-20;
            for (uint i = 0; i < perm.Length; i++) perm[i] = i;

            if (isMatrixView)
            {
                //スケーリングを記録
                double[] vv = new double[Rows];
                for (uint i = 0; i < Rows; i++)
                {
                    double big = 0.0d;
                    for (uint j = 0; j < Columns; j++)
                    {
                        double tmp = Math.Abs(GetValue(i, j));
                        if (tmp > big) big = tmp;
                    }
                    if (big == 0.0) throw new Exception("Singular matrix in routine LUDecomposition");
                    vv[i] = 1.0 / big;
                }

                for (uint j = 0; j < Columns; j++)
                {
                    double sum = 0;
                    double big = 0.0d;
                    uint imax = 0;

                    //Crout法
                    for (uint i = 0; i < j; i++)
                    {
                        sum = GetValue(i, j);
                        for (uint k = 0; k < i; k++) sum -= GetValue(i, k) * GetValue(k, j);
                        SetValue(i, j,sum);
                    }

                    for (uint i = j; i < Rows; i++)
                    {
                        sum = GetValue(i, j);
                        for (uint k = 0; k < j; k++) sum -= GetValue(i, k) * GetValue(k, j);
                        SetValue(i, j, sum);
                        double dum = vv[i] * Math.Abs(sum);
                        if (dum >= big)
                        {
                            big = dum;
                            imax = i;
                        }
                    }

                    //行交換の必要判定
                    if (j != imax)
                    {
                        for (uint k = 0; k < Rows; k++)
                        {
                            double dum = GetValue(imax, k);
                            SetValue(imax, k, GetValue(j, k));
                            SetValue(j, k, dum);
                        }
                        vv[imax] = vv[j];
                    }
                    perm[j] = imax;
                    if (GetValue(j, j) == 0.0) SetValue(j, j, TINY);

                    if (j != Columns)
                    {
                        for (uint i = j + 1; i < Columns; i++) SetValue(i, j, GetValue(i, j) / GetValue(j, j));
                    }
                }
            }
            else
            {
                //スケーリングを記録
                double[] vv = new double[Columns];
                for (uint i = 0; i < Columns; i++)
                {
                    double big = 0.0d;
                    for (uint j = 0; j < Rows; j++)
                    {
                        double tmp = Math.Abs(mat[i, j]);
                        if (tmp > big) big = tmp;
                    }
                    if (big == 0.0) throw new Exception("Singular matrix in routine LUDecomposition");
                    vv[i] = 1.0 / big;
                }

                for (uint j = 0; j < Rows; j++)
                {
                    double sum = 0;
                    double big = 0.0d;
                    uint imax = 0;


                    //Crout法
                    for (uint i = 0; i < j; i++)
                    {
                        sum = mat[i, j];
                        for (uint k = 0; k < i; k++) sum -= mat[i, k] * mat[k, j];
                        mat[i, j] = sum;
                    }

                    for (uint i = j; i < Columns; i++)
                    {
                        sum = mat[i, j];
                        for (uint k = 0; k < j; k++) sum -= mat[i, k] * mat[k, j];
                        mat[i, j] = sum;
                        double dum = vv[i] * Math.Abs(sum);
                        if (dum >= big)
                        {
                            big = dum;
                            imax = i;
                        }
                    }

                    //行交換の必要判定
                    if (j != imax)
                    {
                        for (uint k = 0; k < Columns; k++)
                        {
                            double dum = mat[imax, k];
                            mat[imax, k] = mat[j, k];
                            mat[j, k] = dum;
                        }
                        vv[imax] = vv[j];
                    }
                    perm[j] = imax;
                    if (mat[j, j] == 0.0) mat[j, j] = TINY;

                    if (j != Columns)
                    {
                        for (uint i = j + 1; i < Columns; i++) mat[i, j] = mat[i, j] / mat[j, j];
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name="vector"></param>
        internal void LUSolve(ref Vector vector)
        {
            if (isMatrixView)
            {
                uint ii = 0;
                for (uint i = 0; i < Rows; i++)
                {
                    uint ip = perm[i];
                    double sum = vector.GetValue(ip);
                    vector.SetValue(ip, vector.GetValue(i));
                    if (ii != 0)
                    {
                        for (uint j = ii - 1; j < i; j++) sum -= GetValue(i, j) * vector.GetValue(j);
                    }
                    else if (sum != 0) ii = i + 1;
                    vector.SetValue(i, sum);
                }
                for (int i = (int)Rows - 1; i >= 0; i--)
                {
                    uint iii = (uint)i;
                    double sum = vector.GetValue(iii);
                    for (uint j = iii + 1; j < Columns; j++) sum -= GetValue(iii, j) * vector.GetValue(j);
                    vector.SetValue(iii, sum / GetValue(iii, iii));
                }
            }
            else
            {
                uint ii = 0;
                for (uint i = 0; i < Rows; i++)
                {
                    uint ip = perm[i];
                    double sum = vector.GetValue(ip);
                    vector.SetValue(ip, vector.GetValue(i));
                    if (ii != 0)
                    {
                        for (uint j = ii - 1; j < i; j++) sum -= mat[i, j] * vector.GetValue(j);
                    }
                    else if (sum != 0) ii = i + 1;
                    vector.SetValue(i, sum);
                }
                for (int i = (int)Rows - 1; i >= 0; i--)
                {
                    uint iii = (uint)i;
                    double sum = vector.GetValue(iii);
                    for (uint j = iii + 1; j < Columns; j++) sum -= mat[iii, j] * vector.GetValue(j);
                    vector.SetValue(iii, sum / mat[iii, iii]);
                }
            }
        }
        
        /// <summary>matrix1の逆行列を計算してmatrix2に設定する</summary>
        /// <param name="matrix"></param>
        internal void GetInverse(ref Matrix matrix)
        {
            LUDecomposition();
            Vector col = new Vector(Columns);
            for (uint j = 0; j < Columns; j++)
            {
                col.SetValue(0);
                col.SetValue(j, 1);
                LUSolve(ref col);
                for (uint i = 0; i < Columns; i++) matrix.SetValue(i, j, col.GetValue(i));
            }
        }

        #endregion

    }
}
