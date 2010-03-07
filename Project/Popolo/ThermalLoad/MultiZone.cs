using System;
using System.Collections.Generic;
using System.Text;

namespace Popolo.ThermalLoad
{
    /// <summary></summary>
    public class MultiZone
    {

        #region インスタンス変数

        /// <summary>放射熱伝達比率[-]</summary>
        private Dictionary<ISurface, Dictionary<ISurface, double>> phi = 
            new Dictionary<ISurface, Dictionary<ISurface, double>>();

        #endregion

        #region プロパティ

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="zones">ゾーンリスト</param>
        public MultiZone(Zone[] zones)
        {
            int surfaceNumber = 0;
            double surfaceArea = 0;
            List<ISurface> surfaces = new List<ISurface>();

            for (int i = 0; i < zones.Length; i++)
            {
                surfaces.AddRange(zones[i].getSurfaces());
            }
            for (int i = 0; i < surfaces.Count; i++)
            {
                surfaceNumber++;
                surfaceArea += surfaces[i].Area;
            }

            //放射熱伝達比率を面積比で初期化
            for (int i = 0; i < surfaces.Count; i++)
            {
                Dictionary<ISurface, double> lis = new Dictionary<ISurface, double>();
                phi.Add(surfaces[i], lis);                
                double aSum = surfaceArea - surfaces[i].Area;

                for (int j = 0; j < surfaces.Count; j++)
                {
                    if (surfaces[i] == surfaces[j]) lis[surfaces[j]] = 0.0;
                    else lis[surfaces[j]] = surfaces[j].Area / aSum;
                }
            }
        }

        #endregion

        #region publicメソッド

        /// <summary>表面1から表面2への放射熱伝達比率[-]を取得する</summary>
        /// <param name="surface1">表面1</param>
        /// <param name="surface2">表面2</param>
        /// <returns>表面1から表面2への放射熱伝達比率[-]</returns>
        public double GetRadiativeHeatTransferRate(ISurface surface1, ISurface surface2)
        {
            return phi[surface1][surface2];
        }

        /// <summary>表面1から表面2への放射熱伝達比率[-]を取得する</summary>
        /// <param name="surface1">表面1</param>
        /// <param name="surface2">表面2</param>
        /// <param name="rate">表面1から表面2への放射熱伝達比率[-]</param>
        public void SetRadiativeHeatTransferRate(ISurface surface1, ISurface surface2, double rate)
        {
            phi[surface1][surface2] = rate;
        }

        #endregion

    }
}
