/* Vector.cs
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
    /// <summary>Vector class</summary>
    internal class Vector
    {

        #region instance variables

        /// <summary>Is vector view?</summary>
        private bool isVectorView = false;

        /// <summary>start number at original vector</summary>
        private uint viewStartNumber;

        /// <summary>size of vector view</summary>
        private uint viewSize;

        /// <summary>originalMatrix</summary>
        private Vector originalVector;

        /// <summary>vector</summary>
        private double[] vect;

        #endregion

        #region properties

        ///<summary>Get size of vector</summary>
        internal uint Size
        {
            get
            {
                if (isVectorView) return viewSize;
                else return (uint)vect.Length;
            }
        }

        #endregion

        #region constructor

        /// <summary>constructor</summary>
        /// <param name="size">size of vector</param>
        internal Vector(uint size)
        {
            vect = new double[size];
        }

        /// <summary>Make vector object from other vector object</summary>
        /// <param name="size">size of vector</param>
        /// <param name="startNumber"></param>
        /// <param name="originalVector"></param>
        internal Vector(uint size, uint startNumber, Vector originalVector)
        {
            this.isVectorView = true;
            this.viewSize = size;
            this.viewStartNumber = startNumber;
            this.originalVector = originalVector;
        }

        #endregion

        #region internal methods

        /// <summary>要素の値を取得する</summary>
        /// <param name="index">要素番号</param>
        /// <returns>要素の値</returns>
        internal double GetValue(uint index)
        {
            if (isVectorView) return originalVector.GetValue(index + viewStartNumber);
            else return vect[index];
        }

        /// <summary>>要素に値を設定する</summary>
        /// <param name="index">要素番号</param>
        /// <param name="value">設定する実数値</param>
        internal void SetValue(uint index, double value)
        {
            if (isVectorView) originalVector.SetValue(index + viewStartNumber, value);
            else vect[index] = value;            
        }

        ///<summary>要素に値を設定する</summary>
        ///<param name="value">設定する実数値</param>
        internal void SetValue(double value)
        {
            if (isVectorView)
            {
                for (uint i = 0; i < viewSize; i++) originalVector.SetValue(i + viewStartNumber, value);
            }
            else
            {
                for (int i = 0; i < vect.Length; i++) vect[i] = value;
            }
        }

        /// <summary>要素に値を加算する</summary>
        /// <param name="index">要素番号</param>
        /// <param name="value">加算する実数値</param>
        internal void AddValue(uint index, double value)
        {
            if (isVectorView) originalVector.AddValue(index + viewStartNumber, value);
            else vect[index] += value;             
        }

        /// <summary>ベクトルを配列にコピーする</summary>
        /// <param name="array">コピー先の配列</param>
        internal void CopyTo(ref double[] array)
        {
            if (isVectorView)
            {
                for (uint i = 0; i < viewSize; i++) array[i] = originalVector.GetValue(i + viewStartNumber);
            }
            else
            {
                for (int i = 0; i < vect.Length; i++) array[i] = vect[i];
            }            
        }

        /// <summary>ベクトルをベクトルにコピーする</summary>
        /// <param name="vector">コピー先のベクトル</param>
        internal void CopyTo(ref Vector vector)
        {
            if (isVectorView)
            {
                for (uint i = 0; i < viewSize; i++) vector.vect[i] = originalVector.GetValue(i + viewStartNumber);
            }
            else
            {
                for (int i = 0; i < this.vect.Length; i++) vector.vect[i] = this.vect[i];
            }              
        }

        /// <summary>配列の内容をベクトルにコピーする</summary>
        /// <param name="array">コピーもとの配列</param>
        internal void CopyFrom(double[] array)
        {
            if (isVectorView)
            {
                for (uint i = 0; i < viewSize; i++) originalVector.SetValue(i + viewStartNumber, array[i]);
            }
            else
            {
                for (int i = 0; i < vect.Length; i++) vect[i] = array[i];
            }             
        }

        /// <summary>ベクトルの内容をピーする</summary>
        /// <param name="vector">コピーもとのベクトル</param>
        internal void CopyFrom(Vector vector)
        {
            if (isVectorView)
            {
                for (uint i = 0; i < viewSize; i++) originalVector.SetValue(i + viewStartNumber, vector.vect[i]);
            }
            else
            {
                for (int i = 0; i < this.vect.Length; i++) this.vect[i] = vector.vect[i];
            }           
        }        

        #endregion


    }
}
