/* WallLayers.cs
 * 
 * Copyright (C) 2008 E.Togashi
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

using System.Runtime.Serialization;

namespace Popolo.ThermalLoad
{
    /// <summary>壁構成クラス</summary>
    [Serializable]
    public class WallLayers : ImmutableWallLayers
    {

        #region 定数宣言

        /// <summary>シリアライズ用バージョン情報</summary>
        private double S_VERSION = 1.0;

        #endregion

        #region インスタンス変数

        /// <summary>名称</summary>
        private string name = "新規壁構成";

        /// <summary>壁層リスト</summary>
        private List<Layer> layers = new List<Layer>();

        /// <summary>熱貫流率[W/(m2-K)]</summary>
        private double thermalTransmission;

        /// <summary>単位面積当たりの熱容量[J/(m^2-K)]</summary>
        private double heatCapacityPerUnitArea;

        #endregion

        #region プロパティ

        /// <summary>IDを設定・取得する</summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>名称を設定・取得する</summary>
        public string Name
        {
            set
            {
                if (value != null)
                {
                    name = value;
                }
            }
            get
            {
                return name;
            }
        }

        /// <summary>層の数を取得する</summary>
        public uint LayerNumber
        {
            get
            {
                return (uint)layers.Count;
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public WallLayers() { }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">壁構成名称</param>
        public WallLayers(string name)
        {
            this.name = name;
        }

        #endregion

        #region 壁層設定・取得処理

        /// <summary>壁層を取得する</summary>
        /// <param name="layerIndex">壁層番号</param>
        /// <returns>壁層オブジェクト</returns>
        public Layer GetLayer(uint layerIndex)
        {
            return layers[(int)layerIndex];
        }

        /// <summary>壁層を取得する</summary>
        /// <returns>壁層オブジェクト</returns>
        public Layer[] GetLayer()
        {
            return layers.ToArray();
        }

        /// <summary>壁層を設定する</summary>
        /// <param name="layerIndex">壁層番号</param>
        /// <param name="layer">壁層オブジェクト</param>
        public void ReplaceLayer(int layerIndex, Layer layer)
        {
            uint dvNum = layer.divisionNumber;
            if (dvNum != 1)
            {
                Layer nLayer = new Layer(layer.Material, layer.Thickness / dvNum);
                for (int i = 0; i < dvNum; i++)
                {
                    if (i == 0) layers[layerIndex] = nLayer;
                    else layers.Insert(layerIndex + i, nLayer);
                }
            }
            else layers[layerIndex] = (Layer)layer.Clone();

            updateThermalTransmission();
            updateHeatCapacityPerUnitArea();
        }

        /// <summary>壁層を追加する</summary>
        /// <param name="layer">壁層オブジェクト</param>
        public void AddLayer(Layer layer)
        {
            uint dvNum = layer.divisionNumber;
            if (dvNum != 1)
            {
                Layer nLayer = new Layer(layer.Material, layer.Thickness / dvNum);
                for (int i = 0; i < dvNum; i++)
                {
                    layers.Add(nLayer);
                } 
            }
            else layers.Add((Layer)layer.Clone());

            updateThermalTransmission();
            updateHeatCapacityPerUnitArea();
        }

        /// <summary>壁層を削除する</summary>
        /// <param name="layerIndex">壁層番号</param>
        public void RemoveLayer(int layerIndex)
        {
            layers.RemoveAt(layerIndex);

            updateThermalTransmission();
            updateHeatCapacityPerUnitArea();
        }

        #endregion

        #region publicメソッド

        /// <summary>壁素材を使っているか否かを返す</summary>
        /// <param name="material">壁素材</param>
        /// <returns>使っている場合は真</returns>
        public bool UsingMaterial(ImmutableWallMaterial material)
        {
            foreach (Layer layer in layers)
            {
                if (layer.Material.ID == material.ID) return true;
            }
            return false;
        }

        /// <summary>壁表面の総合熱伝達率[W/(m^2K)]を指定して熱貫流率[W/(m^2K)]を計算する</summary>
        /// <param name="filmCoefficient1">壁表面の総合熱伝達率1[W/(m^2K)]</param>
        /// <param name="filmCoefficient2">壁表面の総合熱伝達率2[W/(m^2K)]</param>
        /// <returns>熱貫流率[W/(m^2K)]</returns>
        public double GetThermalTransmission(double filmCoefficient1, double filmCoefficient2)
        {
            double rSum = 1 / filmCoefficient1 + 1 / filmCoefficient2 + 1 / thermalTransmission;
            return 1 / rSum;
        }

        /// <summary>熱貫流率[W/(m^2K)]を計算する</summary>
        /// <returns>熱貫流率[W/(m^2K)]</returns>
        /// <remarks>壁表面の総合熱伝達率は含まない</remarks>
        public double GetThermalTransmission()
        {
            return thermalTransmission;
        }

        /// <summary>単位面積当たりの熱容量[kJ/(m^2-K)]を計算する</summary>
        /// <returns></returns>
        public double GetHeatCapacityPerUnitArea()
        {
            return heatCapacityPerUnitArea;
        }

        #endregion

        #region privateメソッド

        /// <summary>熱貫流率[W/(m2-K)]を更新する</summary>
        private void updateThermalTransmission()
        {
            thermalTransmission = 0;
            foreach (Layer ly in layers)
            {
                //壁素材を特定
                ImmutableWallMaterial wm = ly.Material;
                //空気層の場合
                if (wm.VolumetricSpecificHeat == 0.0)
                {
                    thermalTransmission += 1.0 / wm.ThermalConductivity;
                }
                //一般の壁素材の場合
                else
                {
                    thermalTransmission += ly.Thickness / wm.ThermalConductivity;
                }
            }
            thermalTransmission = 1d / thermalTransmission;
        }

        /// <summary>単位面積当たりの熱容量[J/(m^2-K)]を更新する</summary>
        private void updateHeatCapacityPerUnitArea()
        {
            heatCapacityPerUnitArea = 0;
            foreach (Layer ly in layers) heatCapacityPerUnitArea += ly.HeatCapacityPerUnitArea;
        }

        #endregion

        #region ISerializable実装

        /// <summary>デシリアライズ用コンストラクタ</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected WallLayers(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            double version = sInfo.GetDouble("S_Version");

            //壁構成ID
            ID = sInfo.GetInt32("id");
            //壁構成名称
            name = sInfo.GetString("name");
            //壁材料リスト
            int wlNumber = sInfo.GetInt32("wlNumber");
            for (int i = 0; i < wlNumber; i++)
            {
                layers.Add((Layer)sInfo.GetValue("wallLayers" + i.ToString(), typeof(Layer)));
            }
            updateThermalTransmission();
            updateHeatCapacityPerUnitArea();
        }

        /// <summary>シリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //壁構成ID
            info.AddValue("id", ID);
            //壁構成名称
            info.AddValue("name", name);
            //壁材料リスト
            int wlNumber = layers.Count;
            info.AddValue("wlNumber", wlNumber);
            for (int i = 0; i < wlNumber; i++)
            {
                info.AddValue("wallLayers" + i.ToString(), layers[i]);
            }           
        }

        #endregion

        #region ICloneable実装

        /// <summary>WallCompositionオブジェクトの複製を返す</summary>
        /// <returns>WallCompositionオブジェクトの複製</returns>
        public object Clone()
        {
            WallLayers wc = (WallLayers)this.MemberwiseClone();
            wc.layers = new List<Layer>();
            foreach (Layer wLayer in this.layers)
            {
                wc.layers.Add((Layer)wLayer.Clone());
            }
            return wc;
        }

        #endregion

        #region インナークラス定義

        /// <summary>壁層クラス</summary>
        [Serializable]
        public class Layer : ISerializable
        {

            #region 定数宣言

            /// <summary>シリアライズ用バージョン情報</summary>
            private double S_VERSION = 1.0;

            #endregion

            #region インスタンス変数

            /// <summary>素材</summary>
            private ImmutableWallMaterial material;

            /// <summary>厚み[m]</summary>
            private double thickness = 0.0;

            /// <summary>熱抵抗[m2-K/W]</summary>
            private double resistance;

            /// <summary>単位面積当たりの熱容量[J/(m^2-K)]</summary>
            private double heatCapacityPerUnitArea;

            /// <summary>壁分割数</summary>
            private uint splitNum = 1;

            #endregion

            #region プロパティ

            /// <summary>素材を取得する</summary>
            public ImmutableWallMaterial Material
            {
                get
                {
                    return material;
                }
            }

            /// <summary>厚み[m]を取得する</summary>
            public double Thickness
            {
                get
                {
                    return thickness;
                }
                internal set
                {
                    thickness = value;
                }
            }

            /// <summary>壁分割数を取得する</summary>
            internal uint divisionNumber
            {
                get
                {
                    return splitNum;
                }
            }

            /// <summary>熱抵抗[m2 K/W]を取得する</summary>
            public double Resistance
            {
                get
                {
                    return resistance;
                }
            }

            /// <summary>単位面積当たりの熱容量[J/(m^2-K)]を取得する</summary>
            public double HeatCapacityPerUnitArea
            {
                get
                {
                    return heatCapacityPerUnitArea;
                }
            }

            #endregion

            #region コンストラクタ

            /// <summary>コンストラクタ</summary>
            /// <param name="material">素材</param>
            /// <param name="thickness">厚み[m]</param>
            public Layer(ImmutableWallMaterial material, double thickness)
            {
                this.material = material;
                this.thickness = thickness;

                //初期化処理
                initialize();
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="material">素材</param>
            /// <param name="thickness">厚み[m]</param>
            /// <param name="splitNumber">壁分割数</param>
            public Layer(ImmutableWallMaterial material, double thickness, uint splitNumber)
            {
                this.material = material;
                this.thickness = thickness;
                this.splitNum = splitNumber;

                //初期化処理
                initialize();
            }

            #endregion

            #region privateメソッド

            /// <summary>各種性能を初期化する</summary>
            private void initialize()
            {
                //熱抵抗[m2 K/W]を計算する
                if (material.VolumetricSpecificHeat == 0)
                {
                    resistance = 1.0 / material.ThermalConductivity;
                }
                else
                {
                    resistance = thickness / material.ThermalConductivity;
                }

                //層の熱容量[J/m2-K]を計算する
                heatCapacityPerUnitArea = thickness * material.VolumetricSpecificHeat* 1000d;
            }

            #endregion

            #region ICloneable実装

            /// <summary>Layerオブジェクトの複製を返す</summary>
            /// <returns>Layerオブジェクトの複製</returns>
            public object Clone()
            {
                return this.MemberwiseClone();
            }

            #endregion

            #region ISerializable実装

            /// <summary>デシリアライズ用コンストラクタ</summary>
            /// <param name="sInfo"></param>
            /// <param name="context"></param>
            protected Layer(SerializationInfo sInfo, StreamingContext context)
            {
                //バージョン情報
                double version = sInfo.GetDouble("S_Version");

                //素材
                material = (ImmutableWallMaterial)sInfo.GetValue("materal", typeof(ImmutableWallMaterial));
                //厚み[m]
                thickness = sInfo.GetDouble("thickness");
                //壁分割数
                splitNum = sInfo.GetUInt32("splitNum");

                //初期化処理
                initialize();
            }


            /// <summary>WallLayerシリアル化処理</summary>
            /// <param name="info"></param>
            /// <param name="context"></param>
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                //バージョン情報
                info.AddValue("S_Version", S_VERSION);

                //素材
                info.AddValue("materal", material);
                //厚み[m]
                info.AddValue("thickness", thickness);
                //壁分割数
                info.AddValue("splitNum", splitNum);
            }

            #endregion

        }

        #endregion

    }

    /// <summary>読み取り専用壁構成</summary>
    public interface ImmutableWallLayers : ISerializable, ICloneable
    {

        #region プロパティ

        /// <summary>IDを取得する</summary>
        int ID
        {
            get;
        }

        /// <summary>名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>層の数を取得する</summary>
        uint LayerNumber
        {
            get;
        }

        #endregion

        #region publicメソッド

        /// <summary>壁層情報を取得する</summary>
        /// <param name="layerIndex">壁層番号</param>
        /// <returns>壁層情報</returns>
        WallLayers.Layer GetLayer(uint layerIndex);

        /// <summary>壁層情報を取得する</summary>
        /// <returns>壁層情報</returns>
        WallLayers.Layer[] GetLayer();

        /// <summary>外表面総合熱伝達率[W/(m^2K)]を指定して熱貫流率[W/(m^2K)]を計算する</summary>
        /// <param name="surfaceAlpha1">外表面総合熱伝達率1[W/(m^2K)]</param>
        /// <param name="surfaceAlpha2">外表面総合熱伝達率2[W/(m^2K)]</param>
        /// <returns>熱貫流率[W/(m^2K)]</returns>
        double GetThermalTransmission(double surfaceAlpha1, double surfaceAlpha2);

        #endregion

    }

}
