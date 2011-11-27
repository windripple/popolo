/* WallMaterial.cs
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

using System.Runtime.Serialization;

namespace Popolo.ThermalLoad
{
    /// <summary>壁体を構成する材料</summary>
    [Serializable]
    public class WallMaterial : ISerializable, ICloneable, ImmutableWallMaterial
    {

        #region Constants

        /// <summary>シリアライズ用バージョン情報</summary>
        private uint S_VERSION = 1;

        #endregion

        #region Instance variables

        /// <summary>素材ID</summary>
        private int id = 0;

        /// <summary>素材名称</summary>
        private string name = "Mortar";

        /// <summary>熱伝導率[W/mK]</summary>
        private double thermalConductivity = 1.512d;

        /// <summary>容積比熱[kJ/m3K]</summary>
        private double volumetricSpecificHeat = 1591.0d;

        /// <summary>素材タイプ</summary>
        private PredefinedMaterials pMaterial = PredefinedMaterials.Mortar;

        #endregion

        #region enumerators

        /// <summary>定義済の素材</summary>
        public enum PredefinedMaterials
        {
            /// <summary>セメント・モルタル</summary>
            Mortar,
            /// <summary>鉄筋コンクリート</summary>
            ReinforcedConcrete,
            /// <summary>軽量骨材コンクリート1種</summary>
            LightweightAggregateConcrete1,
            /// <summary>軽量骨材コンクリート2種</summary>
            LightweightAggregateConcrete2,
            /// <summary>軽量気泡コンクリート（ALC）</summary>
            AutomaticLevelControl,
            /// <summary>普通れんが</summary>
            Brick,
            /// <summary>耐火れんが</summary>
            FireBrick,
            /// <summary>銅</summary>
            Copper,
            /// <summary>アルミニウム合金</summary>
            Aluminum,
            /// <summary>鋼材</summary>
            Steel,
            /// <summary>鉛</summary>
            Lead,
            /// <summary>ステンレス鋼</summary>
            StainlessSteel,
            /// <summary>フロートガラス(窓ガラスではない）</summary>
            FloatGlass,
            /// <summary>PVC(塩化ビニル)</summary>
            PolyvinylChloride,
            /// <summary>天然木材1類（桧、杉、えぞ松等）</summary>
            Wood1,
            /// <summary>天然木材2類（松、ラワン等）</summary>
            Wood2,
            /// <summary>天然木材3類（ナラ、サクラ、ブナ等）</summary>
            Wood3,
            /// <summary>合板</summary>
            Plywood,
            /// <summary>断熱木毛セメント</summary>
            WoodWoolCement,
            /// <summary>木片セメント</summary>
            WoodChipCement,
            /// <summary>ハードボード</summary>
            HardBoard,
            /// <summary>パーティクルボード</summary>
            ParticleBoard,
            /// <summary>せっこうボード</summary>
            PlasterBoard,
            /// <summary>せっこうプラスター</summary>
            GypsumPlaster,
            /// <summary>漆喰</summary>
            WhiteWash,
            /// <summary>土壁</summary>
            SoilWall,
            /// <summary>繊維質上塗材</summary>
            FiberCoating,
            /// <summary>畳床</summary>
            Tatami,
            /// <summary>タイル</summary>
            Tile,
            /// <summary>プラスチック（P）タイル</summary>
            PlasticTile,
            /// <summary>住宅用グラスウール断熱材 10K相当</summary>
            GlassWoolInsulation_10K,
            /// <summary>住宅用グラスウール断熱材 16K相当</summary>
            GlassWoolInsulation_16K,
            /// <summary>住宅用グラスウール断熱材 24K相当</summary>
            GlassWoolInsulation_24K,
            /// <summary>住宅用グラスウール断熱材 32K相当</summary>
            GlassWoolInsulation_34K,
            /// <summary>高性能グラスウール断熱材 16K相当</summary>
            HighGradeGlassWoolInsulation_16K,
            /// <summary>高性能グラスウール断熱材 24K相当</summary>
            HighGradeGlassWoolInsulation_24K,
            /// <summary>吹込用グラスウール断熱材1種 13K相当</summary>
            BlowingGlassWoolInsulation_13K,
            /// <summary>吹込用グラスウール断熱材2種 18K相当</summary>
            BlowingGlassWoolInsulation_18K,
            /// <summary>吹込用グラスウール断熱材2種 30K相当</summary>
            BlowingGlassWoolInsulation_30K,
            /// <summary>吹込用グラスウール断熱材2種 35K相当</summary>
            BlowingGlassWoolInsulation_35K,
            /// <summary>住宅用ロックウール断熱材 マット</summary>
            RockWoolInsulationMat,
            /// <summary>住宅用ロックウール断熱材 フェルト</summary>
            RockWoolInsulationFelt = 42,
            /// <summary>住宅用ロックウール断熱材 ボード</summary>
            RockWoolInsulationBoard = 43,
            /// <summary>吹込用ロックウール断熱材 25K</summary>
            BlowingRockWoolInsulation_25K,
            /// <summary>吹込用ロックウール断熱材 35K</summary>
            BlowingRockWoolInsulation_35K,
            /// <summary>ロックウール化粧吸音板</summary>
            RockWoolAcousticBoard,
            /// <summary>吹付けロックウール</summary>
            SprayedRockWool,
            /// <summary>ビーズ法ポリスチレンフォーム特号</summary>
            BeadMethodPolystyreneFoam_S,
            /// <summary>ビーズ法ポリスチレンフォーム1号</summary>
            BeadMethodPolystyreneFoam_1,
            /// <summary>ビーズ法ポリスチレンフォーム2号</summary>
            BeadMethodPolystyreneFoam_2,
            /// <summary>ビーズ法ポリスチレンフォーム3号</summary>
            BeadMethodPolystyreneFoam_3,
            /// <summary>ビーズ法ポリスチレンフォーム4号</summary>
            BeadMethodPolystyreneFoam_4,
            /// <summary>押出法ポリスチレンフォーム 1種</summary>
            ExtrudedPolystyreneFoam_1,
            /// <summary>押出法ポリスチレンフォーム 2種</summary>
            ExtrudedPolystyreneFoam_2,
            /// <summary>押出法ポリスチレンフォーム 3種</summary>
            ExtrudedPolystyreneFoam_3,
            /// <summary>硬質ウレタンフォーム保温版1種1号</summary>
            RigidUrethaneFoam_1_1,
            /// <summary>硬質ウレタンフォーム保温版1種2号</summary>
            RigidUrethaneFoam_1_2,
            /// <summary>硬質ウレタンフォーム保温版1種3号</summary>
            RigidUrethaneFoam_1_3,
            /// <summary>硬質ウレタンフォーム保温版2種1号</summary>
            RigidUrethaneFoam_2_1,
            /// <summary>硬質ウレタンフォーム保温版2種2号</summary>
            RigidUrethaneFoam_2_2,
            /// <summary>硬質ウレタンフォーム保温版2種3号</summary>
            RigidUrethaneFoam_2_3,
            /// <summary>硬質ウレタンフォーム（現場発泡品）</summary>
            RigidUrethaneFoam_OnSite,
            /// <summary>ポリエチレンフォーム A</summary>
            PolyethyleneFoam_A,
            /// <summary>ポリエチレンフォーム B</summary>
            PolyethyleneFoam_B,
            /// <summary>フェノールフォーム保温版 1種1号</summary>
            PhenolicFoam_1_1,
            /// <summary>フェノールフォーム保温版 1種2号</summary>
            PhenolicFoam_1_2,
            /// <summary>フェノールフォーム保温版 2種1号</summary>
            PhenolicFoam_2_1,
            /// <summary>フェノールフォーム保温版 2種2号</summary>
            PhenolicFoam_2_2,
            /// <summary>A級インシュレーションボード</summary>
            InsulationBoard_A,
            /// <summary>タタミボード</summary>
            TatamiBoard,
            /// <summary>シージングボード</summary>
            SheathingInsulationBoard,
            /// <summary>吹込用セルローズファイバー断熱材1</summary>
            CelluloseFiberInsulation_1,
            /// <summary>吹込用セルローズファイバー断熱材2</summary>
            CelluloseFiberInsulation_2,
            /// <summary>土壌（ローム質）</summary>
            Soil,
            /// <summary>EPS</summary>
            ExpandedPolystyrene,
            /// <summary>外装材</summary>
            CoveringMaterial,
            /// <summary>合成樹脂・リノリウム</summary>
            Linoleum,
            /// <summary>カーペット</summary>
            Carpet,
            /// <summary>石綿スレート</summary>
            AsbestosPlate,
            /// <summary>密閉空気層</summary>
            SealedAirGap,
            /// <summary>非密閉空気層</summary>
            AirGap,
            /// <summary>ポリスチレンフォーム</summary>
            PolystyreneFoam,
            /// <summary>スチレン発泡板</summary>
            StyreneFoam,
            /// <summary>ゴムタイル</summary>
            RubberTile,
            /// <summary>瓦</summary>
            Kawara,
            /// <summary>軽量コンクリート</summary>
            LightweightConcrete,
            /// <summary>防水層（アスファルトルーフィング）</summary>
            Asphalt,
            /// <summary>フレキシブルボード</summary>
            FrexibleBoard,
            /// <summary>珪酸カルシウム板</summary>
            CalciumSilicateBoard,
            /// <summary>高性能フェノールボード</summary>
            PhenolicFoam,
            /// <summary>花崗岩</summary>
            Granite,
            /// <summary>アクリル樹脂</summary>
            AcrylicResin,
            /// <summary>その他</summary>
            Other
        }

        #endregion

        #region Properties

        /// <summary>素材IDを設定・取得する</summary>
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>素材の名称を設定・取得する</summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value != null)
                {
                    name = value;
                    pMaterial = PredefinedMaterials.Other;
                }
            }
        }

        /// <summary>壁材料を取得する</summary>
        public PredefinedMaterials Material
        {
            get
            {
                return pMaterial;
            }
        }

        /// <summary>熱伝導率[W/(mK)]を設定・取得する</summary>
        public double ThermalConductivity
        {
            get
            {
                return thermalConductivity;
            }
            set
            {
                if (0 < value)
                {
                    thermalConductivity = value;
                    pMaterial = PredefinedMaterials.Other;
                }
            }
        }

        /// <summary>容積比熱[kJ/(m^3K)]を設定・取得する</summary>
        public double VolumetricSpecificHeat
        {
            get
            {
                return volumetricSpecificHeat;
            }
            set
            {
                if (0 <= value)
                {
                    volumetricSpecificHeat = value;
                    pMaterial = PredefinedMaterials.Other;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>デフォルトConstructor</summary>
        public WallMaterial() { }

        /// <summary>デフォルトConstructor</summary>
        /// <param name="mType">素材タイプ</param>
        public WallMaterial(PredefinedMaterials mType)
        {
            Initialize(mType);
        }

        /// <summary>Constructor</summary>
        /// <param name="name">素材名称</param>
        /// <param name="thermalConductivity">熱伝導率[W/(mK)]</param>
        /// <param name="volumetricSpecificHeat">容積比熱[kJ/(m^3K)]</param>
        public WallMaterial(string name, double thermalConductivity, double volumetricSpecificHeat)
        {
            Initialize(name, thermalConductivity, volumetricSpecificHeat);
        }

        /// <summary>コピーConstructor</summary>
        /// <param name="wallMaterial">コピーする壁素材</param>
        public WallMaterial(ImmutableWallMaterial wallMaterial)
        {
            this.id = wallMaterial.ID;
            this.name = wallMaterial.Name;
            this.thermalConductivity = wallMaterial.ThermalConductivity;
            this.volumetricSpecificHeat = wallMaterial.VolumetricSpecificHeat;
            this.pMaterial = wallMaterial.Material;
        }

        #endregion

        #region 初期化処理

        /// <summary>初期化処理</summary>
        /// <param name="name">素材名称</param>
        /// <param name="thermalConductivity">熱伝導率[W/mK]</param>
        /// <param name="volumetricSpecificHeat">容積比熱[kJ/m3K]</param>
        public void Initialize(string name, double thermalConductivity, double volumetricSpecificHeat)
        {
            initialize(name, thermalConductivity, volumetricSpecificHeat, PredefinedMaterials.Other);
        }

        /// <summary>初期化処理</summary>
        /// <param name="name">素材名称</param>
        /// <param name="thermalConductivity">熱伝導率[W/mK]</param>
        /// <param name="volumetricSpecificHeat">容積比熱[kJ/m3K]</param>
        /// <param name="pMaterial">素材タイプ</param>
        private void initialize(string name, double thermalConductivity, double volumetricSpecificHeat, PredefinedMaterials pMaterial)
        {
            this.name = name;
            this.thermalConductivity = thermalConductivity;
            this.volumetricSpecificHeat = volumetricSpecificHeat;
            this.pMaterial = pMaterial;
        }

        /// <summary>初期化処理</summary>
        /// <param name="mType">素材タイプ</param>
        public void Initialize(PredefinedMaterials mType)
        {
            switch (mType)
            {
                case PredefinedMaterials.Mortar:
                    initialize("Mortar", 1.512, 1591.0, mType);
                    break;
                case PredefinedMaterials.ReinforcedConcrete:
                    initialize("Reinforced Concrete", 1.600, 1896.0, mType);
                    break;
                case PredefinedMaterials.LightweightAggregateConcrete1:
                    initialize("Lightweight Aggregate Concrete 1", 0.810, 1900.0, mType);
                    break;
                case PredefinedMaterials.LightweightAggregateConcrete2:
                    initialize("Lightweight Aggregate Concrete 2", 0.580, 1599.0, mType);
                    break;
                case PredefinedMaterials.AutomaticLevelControl:
                    initialize("Automatic Level Control", 0.170, 661.4, mType);
                    break;
                case PredefinedMaterials.Brick:
                    initialize("Brick", 0.620, 1386.0, mType);
                    break;
                case PredefinedMaterials.FireBrick:
                    initialize("FireBrick", 0.990, 1553.0, mType);
                    break;
                case PredefinedMaterials.Copper:
                    initialize("Copper", 370.100, 3144.0, mType);
                    break;
                case PredefinedMaterials.Aluminum:
                    initialize("Aluminum", 200.000, 2428.0, mType);
                    break;
                case PredefinedMaterials.Steel:
                    initialize("Steel", 53.010, 3759.0, mType);
                    break;
                case PredefinedMaterials.Lead:
                    initialize("Lead", 35.010, 1469.0, mType);
                    break;
                case PredefinedMaterials.StainlessSteel:
                    initialize("Stainless Steel", 15.000, 3479.0, mType);
                    break;
                case PredefinedMaterials.FloatGlass:
                    initialize("Float Glass", 1.000, 1914.0, mType);
                    break;
                case PredefinedMaterials.PolyvinylChloride:
                    initialize("Polyvinyl Chloride", 0.170, 1023.0, mType);
                    break;
                case PredefinedMaterials.Wood1:
                    initialize("Wood (Cedar)", 0.120, 519.1, mType);
                    break;
                case PredefinedMaterials.Wood2:
                    initialize("Wood (Pine, Lauan)", 0.150, 648.8, mType);
                    break;
                case PredefinedMaterials.Wood3:
                    initialize("Wood (Cherry, Fagaceae)", 0.190, 845.6, mType);
                    break;
                case PredefinedMaterials.Plywood:
                    initialize("Plywood", 0.190, 716.0, mType);
                    break;
                case PredefinedMaterials.WoodWoolCement:
                    initialize("Wood Wool Cement", 0.100, 841.4, mType);
                    break;
                case PredefinedMaterials.WoodChipCement:
                    initialize("Wood Chip Cement", 0.170, 1679.0, mType);
                    break;
                case PredefinedMaterials.HardBoard:
                    initialize("Hard Board", 0.170, 1233.0, mType);
                    break;
                case PredefinedMaterials.ParticleBoard:
                    initialize("Particle Board", 0.150, 715.8, mType);
                    break;
                case PredefinedMaterials.PlasterBoard:
                    initialize("Plaster Board", 0.170, 1030.0, mType);
                    break;
                case PredefinedMaterials.GypsumPlaster:
                    initialize("Gypsum Plaster", 0.600, 1637.0, mType);
                    break;
                case PredefinedMaterials.WhiteWash:
                    initialize("White Wash", 0.700, 1093.0, mType);
                    break;
                case PredefinedMaterials.SoilWall:
                    initialize("Soil Wall", 0.690, 1126.0, mType);
                    break;
                case PredefinedMaterials.FiberCoating:
                    initialize("Fiber Coating", 0.120, 4.2, mType);
                    break;
                case PredefinedMaterials.Tatami:
                    initialize("Tatami", 0.110, 527.4, mType);
                    break;
                case PredefinedMaterials.Tile:
                    initialize("Tile", 1.300, 2018.0, mType);
                    break;
                case PredefinedMaterials.PlasticTile:
                    initialize("Plastic Tile", 0.190, 4.2, mType);
                    break;
                case PredefinedMaterials.GlassWoolInsulation_10K:
                    initialize("Glass Wool Insulation 10kg/m3", 0.050, 8.4, mType);
                    break;
                case PredefinedMaterials.GlassWoolInsulation_16K:
                    initialize("Glass Wool Insulation 16kg/m3", 0.045, 13.4, mType);
                    break;
                case PredefinedMaterials.GlassWoolInsulation_24K:
                    initialize("Glass Wool Insulation 24kg/m3", 0.038, 20.1, mType);
                    break;
                case PredefinedMaterials.GlassWoolInsulation_34K:
                    initialize("Glass Wool Insulation 32kg/m3", 0.036, 26.8, mType);
                    break;
                case PredefinedMaterials.HighGradeGlassWoolInsulation_16K:
                    initialize("High Grade Glass Wool Insulation 16kg/m3", 0.038, 13.4, mType);
                    break;
                case PredefinedMaterials.HighGradeGlassWoolInsulation_24K:
                    initialize("High Grade Glass Wool Insulation 24kg/m3", 0.036, 20.1, mType);
                    break;
                case PredefinedMaterials.BlowingGlassWoolInsulation_13K:
                    initialize("Blowing Glass Wool Insulation 13kg/m3", 0.052, 10.9, mType);
                    break;
                case PredefinedMaterials.BlowingGlassWoolInsulation_18K:
                    initialize("Blowing Glass Wool Insulation 18kg/m3", 0.052, 16.7, mType);
                    break;
                case PredefinedMaterials.BlowingGlassWoolInsulation_30K:
                    initialize("Blowing Glass Wool Insulation 30kg/m3", 0.040, 29.3, mType);
                    break;
                case PredefinedMaterials.BlowingGlassWoolInsulation_35K:
                    initialize("Blowing Glass Wool Insulation 35kg/m3", 0.040, 37.7, mType);
                    break;
                case PredefinedMaterials.RockWoolInsulationMat:
                    initialize("Rock Wool Insulation Mat", 0.038, 33.5, mType);
                    break;
                case PredefinedMaterials.RockWoolInsulationFelt:
                    initialize("Rock Wool Insulation Felt", 0.038, 41.9, mType);
                    break;
                case PredefinedMaterials.RockWoolInsulationBoard:
                    initialize("Rock Wool Insulation Board", 0.036, 58.6, mType);
                    break;
                case PredefinedMaterials.BlowingRockWoolInsulation_25K:
                    initialize("Blowing Rock Wool Insulation 25kg/m3", 0.047, 20.9, mType);
                    break;
                case PredefinedMaterials.BlowingRockWoolInsulation_35K:
                    initialize("Blowing Rock Wool Insulation 35kg/m3", 0.051, 29.3, mType);
                    break;
                case PredefinedMaterials.RockWoolAcousticBoard:
                    initialize("Rock Wool Acoustic Board", 0.058, 293.9, mType);
                    break;
                case PredefinedMaterials.SprayedRockWool:
                    initialize("Sprayed Rock Wool", 0.047, 167.9, mType);
                    break;
                case PredefinedMaterials.BeadMethodPolystyreneFoam_S:
                    initialize("Bead Method Polystyrene Foam S", 0.034, 33.9, mType);
                    break;
                case PredefinedMaterials.BeadMethodPolystyreneFoam_1:
                    initialize("Bead Method Polystyrene Foam 1", 0.036, 37.7, mType);
                    break;
                case PredefinedMaterials.BeadMethodPolystyreneFoam_2:
                    initialize("Bead Method Polystyrene Foam 2", 0.037, 31.4, mType);
                    break;
                case PredefinedMaterials.BeadMethodPolystyreneFoam_3:
                    initialize("Bead Method Polystyrene Foam 3", 0.040, 25.1, mType);
                    break;
                case PredefinedMaterials.BeadMethodPolystyreneFoam_4:
                    initialize("Bead Method Polystyrene Foam 4", 0.043, 18.8, mType);
                    break;
                case PredefinedMaterials.ExtrudedPolystyreneFoam_1:
                    initialize("Extruded Polystyrene Foam 1", 0.040, 25.1, mType);
                    break;
                case PredefinedMaterials.ExtrudedPolystyreneFoam_2:
                    initialize("Extruded Polystyrene Foam 2", 0.034, 25.1, mType);
                    break;
                case PredefinedMaterials.ExtrudedPolystyreneFoam_3:
                    initialize("Extruded Polystyrene Foam 3", 0.028, 25.1, mType);
                    break;
                case PredefinedMaterials.RigidUrethaneFoam_1_1:
                    initialize("Rigid Urethane Foam 1_1", 0.024, 56.1, mType);
                    break;
                case PredefinedMaterials.RigidUrethaneFoam_1_2:
                    initialize("Rigid Urethane Foam 1_2", 0.024, 44.0, mType);
                    break;
                case PredefinedMaterials.RigidUrethaneFoam_1_3:
                    initialize("Rigid Urethane Foam 1_3", 0.026, 31.4, mType);
                    break;
                case PredefinedMaterials.RigidUrethaneFoam_2_1:
                    initialize("Rigid Urethane Foam 2_1", 0.023, 56.1, mType);
                    break;
                case PredefinedMaterials.RigidUrethaneFoam_2_2:
                    initialize("Rigid Urethane Foam 2_2", 0.023, 44.0, mType);
                    break;
                case PredefinedMaterials.RigidUrethaneFoam_2_3:
                    initialize("Rigid Urethane Foam 2_3", 0.024, 31.4, mType);
                    break;
                case PredefinedMaterials.RigidUrethaneFoam_OnSite:
                    initialize("Rigid Urethane Foam (OnSite)", 0.026, 49.8, mType);
                    break;
                case PredefinedMaterials.PolyethyleneFoam_A:
                    initialize("Polyethylene Foam A", 0.038, 62.8, mType);
                    break;
                case PredefinedMaterials.PolyethyleneFoam_B:
                    initialize("Polyethylene Foam B", 0.042, 62.8, mType);
                    break;
                case PredefinedMaterials.PhenolicFoam_1_1:
                    initialize("Phenolic Foam 1_1", 0.033, 37.7, mType);
                    break;
                case PredefinedMaterials.PhenolicFoam_1_2:
                    initialize("Phenolic Foam 1_2", 0.030, 37.7, mType);
                    break;
                case PredefinedMaterials.PhenolicFoam_2_1:
                    initialize("Phenolic Foam 2_1", 0.036, 56.5, mType);
                    break;
                case PredefinedMaterials.PhenolicFoam_2_2:
                    initialize("Phenolic Foam 2_2", 0.034, 56.5, mType);
                    break;
                case PredefinedMaterials.InsulationBoard_A:
                    initialize("Insulation Board A", 0.049, 324.8, mType);
                    break;
                case PredefinedMaterials.TatamiBoard:
                    initialize("Tatami Board", 0.045, 15.1, mType);
                    break;
                case PredefinedMaterials.SheathingInsulationBoard:
                    initialize("Sheathing Insulation Board", 0.052, 390.1, mType);
                    break;
                case PredefinedMaterials.CelluloseFiberInsulation_1:
                    initialize("Cellulose Fiber Insulation 1", 0.040, 37.7, mType);
                    break;
                case PredefinedMaterials.CelluloseFiberInsulation_2:
                    initialize("Cellulose Fiber Insulation 2", 0.040, 62.8, mType);
                    break;
                case PredefinedMaterials.Soil:
                    initialize("Soil", 1.047, 3340.0, mType);
                    break;
                case PredefinedMaterials.ExpandedPolystyrene:
                    initialize("Expanded Polystyrene", 0.035, 300.0, mType);
                    break;
                case PredefinedMaterials.CoveringMaterial:
                    initialize("Covering Material", 0.140, 1680.0, mType);
                    break;
                case PredefinedMaterials.Linoleum:
                    initialize("Linoleum", 0.190, 1470.0, mType);
                    break;
                case PredefinedMaterials.Carpet:
                    initialize("Carpet", 0.080, 318.0, mType);
                    break;
                case PredefinedMaterials.AsbestosPlate:
                    initialize("Asbestos Plate", 1.200, 1820.0, mType);
                    break;
                case PredefinedMaterials.SealedAirGap:
                    initialize("Sealed AirGap", 5.800, 0.0, mType);
                    break;
                case PredefinedMaterials.AirGap:
                    initialize("Air Gap", 11.600, 0.0, mType);
                    break;
                case PredefinedMaterials.PolystyreneFoam:
                    initialize("Polystyrene Foam", 0.035, 80.0, mType);
                    break;
                case PredefinedMaterials.StyreneFoam:
                    initialize("Styrene Foam", 0.035, 10.0, mType);
                    break;
                case PredefinedMaterials.RubberTile:
                    initialize("Rubber Tile", 0.400, 784.0, mType);
                    break;
                case PredefinedMaterials.Kawara:
                    initialize("Kawara", 1.000, 1506.0, mType);
                    break;
                case PredefinedMaterials.LightweightConcrete:
                    initialize("Lightweight Concrete", 0.780, 1607.0, mType);
                    break;
                case PredefinedMaterials.Asphalt:
                    initialize("Asphalt", 0.110, 920.0, mType);
                    break;
                case PredefinedMaterials.FrexibleBoard:
                    initialize("Frexible Board", 0.350, 1600.0, mType);
                    break;
                case PredefinedMaterials.CalciumSilicateBoard:
                    initialize("Calcium Silicate Board", 0.130, 680.0, mType);
                    break;
                case PredefinedMaterials.PhenolicFoam:
                    initialize("Phenolic Foam", 0.020, 37.7, mType);
                    break;
                case PredefinedMaterials.Granite:
                    initialize("Granite", 4.300, 2.9, mType);
                    break;
                case PredefinedMaterials.AcrylicResin:
                    initialize("Acrylic Resin", 0.210, 1666.0, mType);
                    break;
                case PredefinedMaterials.Other:
                    initialize("Other materials", 0.0, 0.0, mType);
                    break;
                default:
                    throw new Exception("wall material is not defined");
            }
        }

        #endregion

        #region ISerializableメソッド実装

        /// <summary>デシリアライズ用Constructor</summary>
        /// <param name="sInfo"></param>
        /// <param name="context"></param>
        protected WallMaterial(SerializationInfo sInfo, StreamingContext context)
        {
            //バージョン情報
            uint version = sInfo.GetUInt32("S_Version");

            //素材ID
            id = sInfo.GetInt32("id");
            //素材名称
            name = sInfo.GetString("name");
            //熱伝導率[W/mK]
            thermalConductivity = sInfo.GetDouble("thermalConductivity");
            //容積比熱[kJ/m3K]
            volumetricSpecificHeat = sInfo.GetDouble("volumetricSpecificHeat");
            //素材タイプ
            pMaterial = (PredefinedMaterials)sInfo.GetValue("mType", typeof(PredefinedMaterials));
        }

        /// <summary>WallMaterialシリアル化処理</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //バージョン情報
            info.AddValue("S_Version", S_VERSION);

            //素材ID
            info.AddValue("id", id);
            //素材名称
            info.AddValue("name", name);
            //熱伝導率[W/mK]
            info.AddValue("thermalConductivity", thermalConductivity);
            //容積比熱[kJ/m3K]
            info.AddValue("volumetricSpecificHeat", volumetricSpecificHeat);
            //素材タイプ
            info.AddValue("mType", pMaterial);
        }

        #endregion

        #region ICloneableメソッド実装

        /// <summary>WallMaterialオブジェクトの複製を返す</summary>
        /// <returns>WallMaterialオブジェクトの複製</returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion

    }

    #region 読み取り専用WallMaterialインターフェース

    /// <summary>読み取り専用WallMaterialインターフェース</summary>
    public interface ImmutableWallMaterial
    {

        #region Properties

        /// <summary>素材IDを取得する</summary>
        int ID
        {
            get;
        }

        /// <summary>素材の名称を取得する</summary>
        string Name
        {
            get;
        }

        /// <summary>壁材料を取得する</summary>
        WallMaterial.PredefinedMaterials Material
        {
            get;
        }

        /// <summary>熱伝導率[W/(mK)]を取得する</summary>
        double ThermalConductivity
        {
            get;
        }

        /// <summary>容積比熱[kJ/(m^3K)]を取得する</summary>
        double VolumetricSpecificHeat
        {
            get;
        }

        #endregion

    }

    #endregion

}
