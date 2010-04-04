using System;
using System.Collections.Generic;

using GSLNET;

namespace Popolo.CircuitNetwork
{
    /// <summary>回路網ソルバ</summary>
    public class CircuitSolver : IDisposable
    {

        #region インスタンス変数

        /// <summary>計算時間間隔[sec]</summary>
        private double timeStep = 1;

        /// <summary>計算対象の回路網</summary>
        private Circuit circuit;

        /// <summary>多次元関数求根クラス</summary>
        private MultiRoot mRoot;

        /// <summary>常微分方程式ソルバ</summary>
        private OrdinaryDifferentialEquations odes;

        /// <summary>静的エネルギーリスト</summary>
        private double[] staticPotentials;

        /// <summary>静的状態変数対応リスト</summary>
        private int[] staticIndices;

        /// <summary>動的エネルギーリスト</summary>
        private double[] dynamicPotentials;

        /// <summary>動的状態変数対応リスト</summary>
        private int[] dynamicIndices;

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="circuit">計算対象の回路網</param>
        public CircuitSolver(Circuit circuit)
        {
            this.circuit = circuit;

            //初期化
            initialize();
        }

        /// <summary>初期化処理</summary>
        private void initialize()
        {
            //境界条件節点以外の節点エネルギーが状態変数となる
            List<double> pot = new List<double>();
            List<int> ind = new List<int>();
            List<double> dpot = new List<double>();
            List<int> dind = new List<int>();

            for (int i = 0; i < circuit.NodesNumber; i++)
            {
                ImmutableNode nd = circuit.GetNode(i);
                if (!nd.IsBoundaryNode)
                {
                    //容量を持つ場合
                    if (0 < nd.Capacity)
                    {
                        dpot.Add(nd.Potential);
                        dind.Add(i);
                    }
                    //容量を持たない場合
                    else
                    {
                        pot.Add(nd.Potential);
                        ind.Add(i);
                    }
                }
            }
            staticPotentials = pot.ToArray();
            staticIndices = ind.ToArray();
            dynamicPotentials = dpot.ToArray();
            dynamicIndices = dind.ToArray();

            //ソルバ初期化
            if (0 < dynamicIndices.Length)
            {
                odes = new OrdinaryDifferentialEquations(OrdinaryDifferentialEquations.SolverType.ImplicitGear2, dynamicIndices.Length);
            }
            if (0 < staticIndices.Length)
            {
                mRoot = new MultiRoot(MultiRoot.SolverType.ScalingHybrid, (uint)staticIndices.Length);
            }
        }

        #endregion

        #region プロパティ

        /// <summary>計算時間間隔[sec]を設定・取得する</summary>
        public double TimeStep
        {
            get
            {
                return timeStep;
            }
            set
            {
                if (0 < value)
                {
                    timeStep = value;
                }
            }
        }

        /// <summary>計算対象の回路網を取得する</summary>
        public ImmutableCircuit TargetCircuit
        {
            get
            {
                return circuit;
            }
        }

        #endregion

        #region 回路網計算処理

        /// <summary>回路網を解いて状態を更新する</summary>
        public void Solve()
        {
            int iterNum;

            if (0 < dynamicIndices.Length)
            {
                double initTStep = timeStep;
                double initTime = 0;
                odes.Solve(resFunction1, ref initTime, timeStep, ref initTStep, ref dynamicPotentials, out iterNum);
            }
            else if(0 < staticIndices.Length)
            {
                mRoot.Solve(resFunction2, ref staticPotentials, 1e-5, 1e-5, 100, out iterNum);
            }
        }

        /// <summary>微分方程式用 誤差評価関数</summary>
        /// <param name="time">時刻</param>
        /// <param name="variables">状態変数（節点のエネルギーベクトル）</param>
        /// <param name="variablesDT">状態変数変化量</param>
        private ErrorNumber resFunction1(double time, double[] variables, ref double[] variablesDT)
        {
            //節点にエネルギーを設定
            for (int i = 0; i < dynamicIndices.Length; i++)
            {
                circuit.SetPotential(variables[i], dynamicIndices[i]);
            }

            if (0 < staticIndices.Length)
            {
                int iterNum;
                mRoot.Solve(resFunction2, ref staticPotentials, 1e-5, 1e-5, 100, out iterNum);
            }

            //各節点の変化量を計算
            for (int i = 0; i < dynamicIndices.Length; i++)
            {
                ImmutableNode nd = circuit.GetNode(dynamicIndices[i]);
                variablesDT[i] = nd.GetTotalFlow() / nd.Capacity;
            }
            return ErrorNumber.GSL_SUCCESS;
        }

        /// <summary>非線形連立代数方程式用 誤差評価関数</summary>
        /// <param name="variables">状態変数（節点のエネルギーベクトル）</param>
        /// <param name="errors">誤差</param>
        private ErrorNumber resFunction2(double[] variables, ref double[] errors)
        {
            //節点にエネルギーを設定
            for (int i = 0; i < staticIndices.Length; i++)
            {
                circuit.SetPotential(variables[i], staticIndices[i]);
            }

            //各節点の流量残差を計算
            for (int i = 0; i < staticIndices.Length; i++)
            {
                errors[i] = circuit.GetNode(staticIndices[i]).GetTotalFlow();
            }
            return ErrorNumber.GSL_SUCCESS;
        }

        #endregion

        #region IDisposable実装

        /// <summary>終了処理</summary>
        public void Dispose()
        {
            mRoot.Dispose();
            odes.Dispose();
        }

        #endregion

    }
}
