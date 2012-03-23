/* SETStarCalculator.cs
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

namespace Popolo.ThermalComfort
{
    /// <summary>SET*計算クラス</summary>
    public static class SETStarCalculator
    {

        #region Constants

        private const double sbc = 5.67 / 100000000;          //ステファンボルツマン係数
        private const double kclo = 0.25;                     //Standard着衣面積率用
        private const double csw = 170;                       //発汗
        private const double cdil = 200;                      //血管拡張
        private const double dcv = 0.5;                       //血管収縮
        private const double tskn = 33.7;                     //皮膚セットポイント温度
        private const double tcrn = 36.8;                     //コアセットポイント温度
        private const double tbn = 36.49;                     //体温セットポイント
        private const double skbfn = 6.3;                     //初期皮膚血流量

        #endregion

        #region public methods

        ///<summary>SET値[-]等を計算する</summary>
        ///<param name="dryBulbTemperature">乾球温度[CDB]</param>
        ///<param name="meanRadiantTemperature">平均放射温度[C]</param>
        ///<param name="relativeAirVelocity">気流速度[m/s]</param>
        ///<param name="relativeHumidity">相対湿度[%]</param>
        ///<param name="clothing">着衣量[clo]</param>
        ///<param name="rm">代謝量[W/m2]</param>
        ///<param name="externalWork">外部仕事量[W/m2]</param>
        ///<param name="pb">大気圧[kPa]</param>
        ///<param name="weight">体重[kg]</param>
        ///<param name="bodySurface">体表面積[m2]</param>
        ///<param name="et">ET*値[-]</param>
        ///<param name="setStar">SET*値[-]</param>
        ///<returns>計算成功の真偽</returns>
        public static bool TryCalculateSET(double dryBulbTemperature, double meanRadiantTemperature, double relativeHumidity,
           double relativeAirVelocity, double clothing, double rm, double externalWork, double pb, double weight, double bodySurface,
            out double et, out double setStar)
        {
            et = setStar = -1;

            double ps = svpcal(dryBulbTemperature);     //飽和水蒸気分圧[mmHg]
            double pa = relativeHumidity * ps / 100d;   //水蒸気圧[mmHg]

            //初期値を与える
            double tsk = tskn;
            double tcr = tcrn;
            double skbf = skbfn;
            double mshiv = 0;
            double alfa = 0.1;
            double esk = 0.1 * rm;

            //単位変換
            double atm = pb / 101.325;                  //kPa→atm
            double rcl = 0.155 * clothing;               //皮膚表面から着衣外表面までの顕熱抵抗（clo→㎡℃/W）
            double facl = 1 + 0.15 * clothing;           //着衣面積率
            double lr = 2.2 / atm;                  //SeaLevelで、ルイス係数2.2

            double m = rm;                          //産熱量m=代謝量rm+ふるえ産熱量mshiv

            //ぬれ率上限
            double wcrit, icl;
            if (clothing <= 0)
            {
                wcrit = 0.38 * Math.Pow(relativeAirVelocity, -0.29);
                icl = 1;
            }
            else
            {
                wcrit = 0.59 * Math.Pow(relativeAirVelocity, -0.08);
                icl = 0.45;
            }

            //対流熱伝達率（代謝量か、気流できまる）
            double chc = 3 * Math.Pow(atm, 0.53);

            double chca;
            if ((rm / 58.2) < 0.85) chca = 0;
            else chca = 5.66 * Math.Pow(((rm / 58.2) - 0.85) * atm, 0.39);

            double chcv = 8.600001 * Math.Pow(relativeAirVelocity * atm, 0.53);
            if (chc <= chca) chc = chca;
            if (chc < chcv) chc = chcv;

            //初期着衣温度
            double chr = 4.7;
            double ctc = chr + chc;
            double ra = 1 / (facl * ctc);                           //着衣外表面から環境までの顕熱抵抗
            double top = (chr * meanRadiantTemperature + chc * dryBulbTemperature) / ctc;               //作用温度
            double tcl = top + (tsk - top) / (ctc * (ra + rcl));    //着衣温度

            //tclとchrは繰り返し計算により求まる　 H(Tsk-To)=CTC(Tcl-To)
            //ここで、H=1/(Ra+Rcl)and Ra=1/(Facl*CTC)

            double tcld;
            do
            {
                tcld = tcl;
                chr = 4 * sbc * Math.Pow((tcl + meanRadiantTemperature) / 2 + 273.15, 3) * 0.72;
                ctc = chr + chc;
                ra = 1 / (facl * ctc);                       //空気層顕熱抵抗
                top = (chr * meanRadiantTemperature + chc * dryBulbTemperature) / ctc;           //作用温度
                tcl = (ra * tsk + rcl * top) / (ra + rcl);   //着衣温度
            }
            while (Math.Abs(tcl - tcld) > 0.01);

            //繰り返し計算開始+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            int dtim = 1;
            double cres, eres, dry, emax, rea, recl, pwet;
            cres = eres = dry = emax = rea = recl = pwet = 0;
            for (int tim = 1; tim < 60; tim += dtim)
            {
                //着衣温度を再計算
                do
                {
                    tcld = tcl;
                    chr = 4 * sbc * Math.Pow((tcl + meanRadiantTemperature) / 2 + 273.15, 3) * 0.72;
                    ctc = chr + chc;
                    ra = 1 / (facl * ctc);
                    top = (chr * meanRadiantTemperature + chc * dryBulbTemperature) / ctc;
                    tcl = (ra * tsk + rcl * top) / (ra + rcl);
                }
                while (Math.Abs(tcl - tcld) > 0.01);

                dry = (tsk - top) / (ra + rcl);                  //顕熱損失量
                double hfcs = (tcr - tsk) * (5.28 + 1.163 * skbf);      //コアから皮膚への熱流量
                eres = 0.0023 * m * (44 - pa);                   //呼吸による潜熱損失量
                cres = 0.0014 * m * (34 - dryBulbTemperature);                   //呼吸による顕熱損失量

                double scr = m - hfcs - eres - cres - externalWork;               //コア熱平衡式
                double ssk = hfcs - dry - esk;                          //皮膚熱平衡式

                //double tcsk = 0.97 * alfa * externalWork;                         //皮膚熱容量
                double tcsk = 58.2 * alfa * 70;
                //double tccr = 0.97 * (1 - alfa) * externalWork;                   //コア熱容量
                double tccr = 58.2 * (1 - alfa) * 70;

                double dtsk = ((ssk * bodySurface) / tcsk) / 60;                 //皮膚の1分間の温度変化幅
                double dtcr = ((scr * bodySurface) / tccr) / 60;                 //コアの1分間の温度変化幅

                tsk = tsk + dtsk * dtim;                        //新しい皮膚温
                tcr = tcr + dtcr * dtim;                        //新しいコア温
                double tb = alfa * tsk + (1 - alfa) * tcr;              //alfaを重み付けして体温を求める

                //シグナルの計算
                double warms, colds, warmc, coldc, warmb, coldb;
                warms = colds = warmc = coldc = warmb = coldb = 0;
                double sksig = tsk - tskn;
                if (sksig < 0) colds = -sksig * (-1);
                else warms = sksig * (-1);
                double crsig = tcr - tcrn;
                if (crsig < 0) coldc = -crsig * (-1);
                else warmc = crsig * (-1);
                double bdsig = tb - tbn;
                if (bdsig < 0) coldb = -bdsig * (-1);
                else warmb = bdsig * (-1);

                //制御系の計算

                //皮膚血流量
                skbf = (skbfn + cdil * warmc) / (1 + dcv * colds);   //コアのWarmシグナルと皮膚のColdシグナル
                if (skbf > 90) skbf = 90;                   //上限
                if (skbf < 0.5) skbf = 0.5;                 //下限

                //発汗量（調節発汗+不感蒸泄）
                //調節発汗
                double regsw = csw * warmb * Math.Exp(warms / 10.7);             //体温のWarmシグナルと皮膚のWarmシグナル
                if (regsw > 500) regsw = 500;              //上限
                double ersw = 0.68 * regsw;                                //潜熱に変換

                //不感蒸泄
                rea = 1 / (lr * facl * chc);                 //着衣外表面から環境までの潜熱抵抗
                recl = rcl / (lr * icl);                     //皮膚表面から着衣外表面までの潜熱抵抗（icl=0.45)

                emax = (svpcal(tsk) - pa) / (rea + recl);    //最大蒸発熱損失量
                double prsw = ersw / emax;                          //発汗分ぬれ率
                pwet = 0.06 + 0.94 * prsw;                   //不感蒸泄を加えたぬれ率
                double edif = pwet * emax - ersw;                   //不感蒸泄による熱損失量

                esk = ersw + edif;                           //皮膚からの蒸発熱損失量

                //ぬれ率を上限を上回る場合
                if (pwet > wcrit)
                {
                    pwet = wcrit;                            //ぬれ率上限
                    prsw = (wcrit - 0.06) / 0.94;            //発汗分ぬれ率
                    ersw = prsw * emax;                      //発汗による熱損失量
                    edif = 0.06 * (1 - prsw) * emax;         //不感蒸泄による熱損失量
                    esk = ersw + edif;                       //皮膚からの蒸発熱損失量
                }

                //最大蒸発熱損失量が負値となる場合
                if (emax < 0)
                {
                    edif = 0;
                    ersw = 0;
                    pwet = wcrit;
                    prsw = wcrit;
                    esk = emax;
                }

                //    esk = ersw + edif      'バークレー版にあるが、最大蒸発熱損失量が負値となる場合には必要ない

                //ふるえ熱産生量
                mshiv = 19.4 * colds * coldc;                        //皮膚のColdシグナルとコアのColdシグナル
                m = rm + mshiv;                                      //産熱量=代謝量+ふるえ熱産生量

                //皮膚とコアの割合alfa
                alfa = 0.0417737 + 0.7451833 / (skbf + 0.585417);

                if (tim < 60) tcl = (ra * tsk + rcl * top) / (ra + rcl);

            }


            //SET*&ET*++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            double store = m - externalWork - cres - eres - dry - esk;        //蓄熱量
            double hsk = dry + esk;                                 //皮膚からの熱損失量
            double rn = m - externalWork;                                     //正味の熱産生量（m=代謝量+ふるえ熱産生量）
            double ecomf = 0.42 * (rn - 58.15);                     //快適時の蒸発熱損失量（FromFanger）
            if (ecomf < 0) ecomf = 0;
            double ereq = rn - eres - cres - dry;                   //熱平衡に必要な蒸発熱損失量
            emax = emax * wcrit;                             //ぬれ率上限を考慮した最大蒸発熱損失量

            double hd = 1 / (ra + rcl);                             //皮膚から環境まで顕熱伝達率
            double he = 1 / (rea + recl);                          //皮膚から環境まで潜熱伝達率
            double w = pwet;                                        //ぬれ率
            double pssk = svpcal(tsk);                              //皮膚表面の飽和水蒸気圧

            //definition of ASHRAE standard environment...denoted "s"+++++++++++++++++++++
            //放射熱伝達率
            double chrs = chr;

            //対流熱伝達率
            double chcs;
            if ((rm / 58.2) < 0.85) chcs = 3;
            else
            {
                chcs = 5.66 * Math.Pow((rm / 58.2) - 0.85, 0.39);
                if (chcs < 3) chcs = 3;
            }

            //顕熱伝達率
            double ctcs = chcs + chrs;

            //代謝量による着衣量の修正　バークレー版から'86年版に変更--------
            //    rclos = 1.52 / ((rm - wk) / 58.15 + 0.6944) - 0.1835
            double rclos = 1.3264 / ((rm - externalWork) / 58.15 + 0.7383) - 0.0953;
            //------------------------------------------------------------

            double rcls = 0.155 * rclos;                   //皮膚表面から着衣外表面までの顕熱抵抗（clo→㎡℃/W）

            double facls = 1 + kclo * rclos;                                               //着衣面積率
            double fcls = 1 / (1 + 0.155 * facls * ctcs * rclos);                          //衣服の伝熱効率Fcl

            double ims = 0.45;                                                             //標準im係数
            double icls = ims * chcs / ctcs * (1 - fcls) / (chcs / ctcs - fcls * ims);     //imをiclに変換

            double ras = 1 / (facls * ctcs);                //着衣外表面から環境までの顕熱抵抗
            double reas = 1 / (lr * facls * chcs);          //着衣外表面から環境までの潜熱抵抗
            double recls = rcls / (lr * icls);              //皮膚表面から着衣外表面までの潜熱抵抗

            double hds = 1 / (ras + rcls);                  //皮膚表面から環境まで顕熱伝達率
            double hes = 1 / (reas + recls);                //'皮膚表面から環境まで潜熱伝達率

            //ET*・・・・・・・・・・・・・・・
            double delta = 0.0001;
            double xold = tsk - hsk / hd;               //まずET*下限を設定
            double err1 = hsk - hd * (tsk - xold) - w * he * (pssk - 0.5 * svpcal(xold));
            double yold = xold + delta;
            double err2 = hsk - hd * (tsk - yold) - w * he * (pssk - 0.5 * svpcal(yold));
            double x = xold - delta * err1 / (err2 - err1);

            int iter = 0;
            do
            {
                xold = x;
                err1 = hsk - hd * (tsk - xold) - w * he * (pssk - 0.5 * svpcal(xold));
                yold = xold + delta;
                err2 = hsk - hd * (tsk - yold) - w * he * (pssk - 0.5 * svpcal(yold));
                x = xold - delta * err1 / (err2 - err1);

                iter++;
                if (50 < iter) return false;
            }
            while (Math.Abs(x - xold) > 0.01);

            et = x;

            //SET*・・・・・・・・・・・・・・・
            xold = tsk - hsk / hds;              //まずSET*下限を設定
            err1 = hsk - hds * (tsk - xold) - w * hes * (pssk - 0.5 * svpcal(xold));
            yold = xold + delta;
            err2 = hsk - hds * (tsk - yold) - w * hes * (pssk - 0.5 * svpcal(yold));
            x = xold - delta * err1 / (err2 - err1);

            iter = 0;
            do
            {
                xold = x;
                err1 = hsk - hds * (tsk - xold) - w * hes * (pssk - 0.5 * svpcal(xold));
                yold = xold + delta;
                err2 = hsk - hds * (tsk - yold) - w * hes * (pssk - 0.5 * svpcal(yold));
                x = xold - delta * err1 / (err2 - err1);
                iter++;
                if (50 < iter) return false;
            }
            while (Math.Abs(x - xold) > 0.01);

            setStar = x;

            return true;
        }

        #endregion

        #region private methods

        /// <summary></summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static double svpcal(double t)
        {
            return Math.Exp((186686 - 40301830 / (t + 235)) / 10000);
        }

        #endregion

    }
}
