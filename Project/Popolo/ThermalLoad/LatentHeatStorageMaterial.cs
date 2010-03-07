using System;
using System.Collections.Generic;
using System.Text;

namespace Popolo.ThermalLoad
{
    /// <summary>潜熱蓄熱材料</summary>
    /// <remarks>温度区間に応じて熱伝導率と容積比熱が変化する材料</remarks>
    public class LatentHeatStorageMaterial
    {

        #region インスタンス変数

        /// <summary>蓄熱材料（特定温度区間）</summary>
        private List<lhMaterial> materials = new List<lhMaterial>();

        /// <summary>現在の蓄熱材料</summary>
        private lhMaterial currentLHMateral;

        #endregion

        #region プロパティ

        /// <summary>現在の温度を取得する</summary>
        public double CurrentTemperature
        {
            get;
            private set;
        }

        /// <summary>現在の材料物性を取得する</summary>
        public ImmutableWallMaterial CurrentMaterial
        {
            get
            {
                return currentLHMateral.Material;
            }
        }

        /// <summary>現在の材料物性番号を取得する</summary>
        public int CurrentMaterialIndex
        {
            get
            {
                return materials.IndexOf(currentLHMateral);
            }
            set
            {
                currentLHMateral = materials[value];
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="upperTemperature">温度上限[C]</param>
        /// <param name="material">壁材料</param>
        public LatentHeatStorageMaterial(double upperTemperature, WallMaterial material)
        {
            lhMaterial lhm = new lhMaterial(upperTemperature, material);
            materials.Add(lhm);
            currentLHMateral = lhm;
        }

        #endregion

        #region publicメソッド

        /// <summary>壁材料を追加する</summary>
        /// <param name="upperTemperature">温度上限[C]</param>
        /// <param name="material">壁材料</param>
        public void AddMaterial(double upperTemperature, ImmutableWallMaterial material)
        {
            materials.Add(new lhMaterial(upperTemperature, material));

            materials.Sort();
        }

        /// <summary>特定の温度区間の材料物性を取得する</summary>
        /// <param name="temperature">温度</param>
        /// <returns>特定の温度区間の材料物性</returns>
        public ImmutableWallMaterial GetMaterial(double temperature)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (temperature < materials[i].UpperTemperature) return materials[i].Material;
            }
            return materials[materials.Count - 1].Material;
        }

        /// <summary>初期化する</summary>
        /// <param name="temperature">温度</param>
        public void Initialize(double temperature)
        {
            currentLHMateral = materials[materials.Count - 1];
            for (int i = materials.Count - 2; 0 <= i; i--)
            {
                if (temperature < materials[i].UpperTemperature) currentLHMateral = materials[i];
            }
            CurrentTemperature = temperature;
        }

        /// <summary>温度1[C]から温度2[C]に変化した場合の蓄熱量[kJ/m3]を計算する</summary>
        /// <param name="temperature1">温度1[C]</param>
        /// <param name="temperature2">温度2[C]</param>
        /// <returns>温度1[C]から温度2[C]に変化した場合の蓄熱量[kJ/m3]</returns>
        public double GetHeatStorage(double temperature1, double temperature2)
        {
            if (temperature1 == temperature2) return 0;

            //初期の材料状態を取得
            int index = (int)getMaterialIndex(temperature1);

            //蓄熱量
            double heatStorage = 0;

            //高温側に変化した場合
            if (temperature1 < temperature2)
            {
                while (index != materials.Count - 1)
                {
                    if (materials[index].UpperTemperature < temperature2)
                    {
                        lhMaterial lwm = materials[index];
                        heatStorage += (lwm.UpperTemperature - temperature1) * lwm.Material.VolumetricSpecificHeat;
                        temperature1 = lwm.UpperTemperature;
                        index++;
                    }
                    else break;
                }
                heatStorage += (temperature2 - temperature1) * materials[index].Material.VolumetricSpecificHeat;
            }
            //低温側に変化した場合
            else
            {
                while (index != 0)
                {
                    if (temperature2 < materials[index - 1].UpperTemperature)
                    {
                        lhMaterial lwm = materials[index - 1];
                        heatStorage -= (temperature1 - lwm.UpperTemperature) * materials[index].Material.VolumetricSpecificHeat;
                        temperature1 = lwm.UpperTemperature;
                        index--;
                    }
                    else break;
                }
                heatStorage -= (temperature1 - temperature2) * materials[index].Material.VolumetricSpecificHeat;
            }

            return heatStorage;
        }

        #endregion

        #region internalメソッド

        /// <summary>温度に基づいて状態を更新する</summary>
        /// <param name="temperature">温度</param>
        /// <returns>状態変化が生じたか否か</returns>
        internal bool updateState(double temperature)
        {
            int index = materials.IndexOf(currentLHMateral);
            bool phaseChanged = false;

            //高温側に相変化する場合
            if (currentLHMateral.UpperTemperature < temperature)
            {
                bool hasPhaseChange = true;
                while (hasPhaseChange)
                {
                    hasPhaseChange = false;

                    //高温側に材料が設定されている場合
                    if (index < materials.Count - 1 &&
                        currentLHMateral.UpperTemperature < temperature)
                    {
                        //相変化後の材料物性
                        lhMaterial nlhMat = materials[index + 1];

                        //過剰な熱量[kJ/m3]を計算
                        double hh = (temperature - currentLHMateral.UpperTemperature) * currentLHMateral.Material.VolumetricSpecificHeat;
                        temperature = currentLHMateral.UpperTemperature + hh / nlhMat.Material.VolumetricSpecificHeat;

                        //現在の材料物性を更新
                        currentLHMateral = nlhMat;

                        //相変化有り
                        phaseChanged = hasPhaseChange = true;
                        index = materials.IndexOf(currentLHMateral);
                    }
                }
            }
            //低温側に相変化する場合
            else
            {
                bool hasPhaseChange = true;
                while (hasPhaseChange)
                {
                    hasPhaseChange = false;

                    //低温側に材料が設定されている場合
                    if (index != 0)
                    {
                        //相変化後の材料物性
                        lhMaterial nlhMat = materials[index - 1];
                        if (temperature < nlhMat.UpperTemperature)
                        {

                            //過剰な熱量[kJ/m3]を計算
                            double hh = (nlhMat.UpperTemperature - temperature) * currentLHMateral.Material.VolumetricSpecificHeat;
                            temperature = nlhMat.UpperTemperature - hh / nlhMat.Material.VolumetricSpecificHeat;

                            //現在の材料物性を更新
                            currentLHMateral = nlhMat;

                            //相変化有り
                            phaseChanged = hasPhaseChange = true;
                            index = materials.IndexOf(currentLHMateral);
                        }
                    }
                }
            }

            CurrentTemperature = temperature;
            return phaseChanged;
        }

        /// <summary>特定の温度区間の材料物性番号を取得する</summary>
        /// <param name="temperature">温度</param>
        /// <returns>特定の温度区間の材料物性番号</returns>
        internal uint getMaterialIndex(double temperature)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (temperature < materials[i].UpperTemperature) return (uint)i;
            }
            return (uint)materials.Count - 1;
        }

        #endregion

        #region インナークラス定義

        /// <summary>潜熱蓄熱材料（特定温度区間）</summary>
        private class lhMaterial : IComparable<lhMaterial>
        {

            /// <summary>上限温度[C]</summary>
            public double UpperTemperature
            {
                get;
                private set;
            }

            /// <summary>材料</summary>
            public ImmutableWallMaterial Material
            {
                get;
                private set;
            }

            public lhMaterial(double upperTemperature, ImmutableWallMaterial material)
            {
                this.UpperTemperature = upperTemperature;
                this.Material = material;
            }

            public int CompareTo(lhMaterial mat)
            {
                double tmp = mat.UpperTemperature;
                if (this.UpperTemperature < tmp) return -1;
                else if (tmp < this.UpperTemperature) return 1;
                else return 0;
            }

        }

        #endregion        

    }
}
