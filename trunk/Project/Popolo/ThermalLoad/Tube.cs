/* Tube.cs
 * 
 * Copyright (C) 2009 E.Togashi
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

namespace Popolo.ThermalLoad
{
    /// <summary>壁体内に埋め込むチューブ</summary>
    public class Tube : ImmutableTube
    {

        #region イベント

        /// <summary>delegate宣言</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void FlowRateChangeEventHandler(object sender, EventArgs e);

        /// <summary>イベント定義</summary>
        public event FlowRateChangeEventHandler FlowRateChangeEvent;

        #endregion

        #region Properties

        /// <summary>熱通過有効度[-]を取得する</summary>
        public double Epsilon
        {
            private set;
            get;
        }

        /// <summary>流体の比熱[J/kg-K]を取得する</summary>
        public double FluidSpecificHeat
        {
            private set;
            get;
        }

        /// <summary>流体の流量[kg/s]を取得する</summary>
        public double FluidFlowRate
        {
            private set;
            get;
        }

        /// <summary>フィン効率[-]を取得する</summary>
        /// <remarks>満遍なくチューブが敷き詰められている場合には1.0とする</remarks>
        public double FinEfficiency
        {
            private set;
            get;
        }

        /// <summary>流体の温度[C]を設定・取得する</summary>
        public double FluidTemperature
        {
            set;
            get;
        }

        /// <summary>チューブへの熱移動量[W]を設定・取得する</summary>
        public double HeatTransferToFluid
        {
            get;
            internal set;
        }

        #endregion

        #region Constructor

        /// <summary>Constructor</summary>
        /// <param name="epsilon">熱通過有効度[-]</param>
        /// <param name="finEfficiency">フィン効率[-]</param>
        /// <param name="fluidSpecificHeat">流体の比熱[J/kg-K]</param>
        public Tube(double epsilon, double finEfficiency, double fluidSpecificHeat)
        {
            Epsilon = Math.Max(0, Math.Min(1, epsilon));
            FinEfficiency = Math.Max(0, Math.Min(1, finEfficiency));
            if (fluidSpecificHeat <= 0) FluidSpecificHeat = 4.186d;
            else FluidSpecificHeat = fluidSpecificHeat;
        }

        #endregion

        #region public methods

        /// <summary>流体の流量[kg/s]を設定する</summary>
        /// <param name="flowRate">流体の流量[kg/s]</param>
        public void SetFlowRate(double flowRate)
        {
            if (FluidFlowRate != flowRate)
            {
                //流体への移動熱量を更新
                if (FluidFlowRate == 0) HeatTransferToFluid = 0;
                else HeatTransferToFluid *= flowRate / FluidFlowRate;

                FluidFlowRate = flowRate;
                if(FlowRateChangeEvent != null) FlowRateChangeEvent(this, new EventArgs());
            }
        }

        /// <summary>流体への熱移動[W]を指定して流体の出口温度[C]を計算する</summary>
        /// <param name="heatTransferToFluid">流体への熱移動[W]</param>
        /// <returns>流体の出口温度[C]</returns>
        public double GetOutletFluidTemperature(double heatTransferToFluid)
        {
            if (FluidFlowRate == 0) return FluidTemperature;
            return FluidTemperature + heatTransferToFluid / FluidSpecificHeat / FluidFlowRate;
        }

        /// <summary>流体の出口温度[C]を計算する</summary>
        /// <returns>流体の出口温度[C]</returns>
        public double GetOutletFluidTemperature()
        {
            if (FluidFlowRate == 0) return FluidTemperature;
            return FluidTemperature + HeatTransferToFluid / FluidSpecificHeat / FluidFlowRate;
        }

        #endregion

    }

    /// <summary>壁体内に埋め込むチューブ（読み取り専用）</summary>
    public interface ImmutableTube
    {
        #region イベント

        /// <summary>イベント定義</summary>
        event Tube.FlowRateChangeEventHandler FlowRateChangeEvent;

        #endregion        

        #region Properties

        /// <summary>熱通過有効度[-]を取得する</summary>
        double Epsilon
        {
            get;
        }

        /// <summary>流体の比熱[J/kg-K]を取得する</summary>
        double FluidSpecificHeat
        {
            get;
        }

        /// <summary>流体の流量[kg/s]を取得する</summary>
        double FluidFlowRate
        {
            get;
        }

        /// <summary>フィン効率[-]を取得する</summary>
        /// <remarks>満遍なくチューブが敷き詰められている場合には1.0とする</remarks>
        double FinEfficiency
        {
            get;
        }

        /// <summary>流体の温度[C]を取得する</summary>
        double FluidTemperature
        {
            get;
        }

        #endregion
    }

}
