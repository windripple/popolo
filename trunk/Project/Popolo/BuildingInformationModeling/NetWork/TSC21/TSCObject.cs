using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;

namespace Popolo.Utility.TSC21
{
    /// <summary>TSC21オブジェクト</summary>
    public class TSCObject : ImmutableTSCObject
    {

        #region クラス変数

        /// <summary>Nullオブジェクト</summary>
        public static readonly TSCObject NULL_OBJECT = new TSCObject("DEFAULT");

        /// <summary>正規表現オブジェクト</summary>
        private static readonly Regex RG1 = new Regex(@"^[a-z]*[A-Z]+(\[[a-zA-Z0-9]+\])?[a-z_]*$");
        private static readonly Regex RG2 = new Regex(@"^[a-z]+");
        private static readonly Regex RG3 = new Regex(@"^[A-Z]+");
        private static readonly Regex RG4 = new Regex(@"^(\[[a-zA-Z0-9]+\])");
        private static readonly Regex RG5 = new Regex(@"^[a-z_]+");

        /// <summary>接頭文字</summary>
        private string prefix = "";

        /// <summary>添字</summary>
        private string suffix = "";

        /// <summary>配列の文字</summary>
        private string arrayString = "";

        #endregion

        #region インスタンス変数

        /// <summary>機器・コア名称</summary>
        private string coreName = "NULL";

        #endregion

        #region プロパティ

        /// <summary>機器・コア名称を設定・取得する</summary>
        public string CoreName
        {
            get
            {
                return coreName;
            }
            set
            {
                if (value != null && coreName != "") coreName = value;
            }
        }

        /// <summary>接頭文字を取得する</summary>
        public string Prefix
        {
            get
            {
                return prefix;
            }
        }

        /// <summary>添字を取得する</summary>
        public string Suffix
        {
            get
            {
                return suffix;
            }
        }

        /// <summary>配列の文字を取得する</summary>
        public string ArrayString
        {
            get
            {
                return arrayString;
            }
        }

        /// <summary>子要素を設定・取得する</summary>
        public ImmutableTSCObject Child
        {
            get;
            set;
        }

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="coreName">コア名称</param>
        public TSCObject(string coreName)
        {
            this.coreName = coreName;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="coreName">コア名称</param>
        /// <param name="prefix">接頭文字</param>
        /// <param name="suffix">添字</param>
        public TSCObject(string coreName, string prefix, string suffix)
        {
            this.CoreName = coreName;
            this.prefix = prefix;
            this.suffix = suffix;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="coreName">コア名称</param>
        /// <param name="prefix">接頭文字</param>
        /// <param name="suffix">添字</param>
        /// <param name="arrayString">配列の文字</param>
        public TSCObject(string coreName, string prefix, string suffix, string arrayString)
        {
            this.CoreName = coreName;
            this.prefix = prefix;
            this.suffix = suffix;
            this.arrayString = arrayString;
        }

        #endregion

        #region 文字列変換処理

        /// <summary>TSC21コードに変換する</summary>
        /// <returns>TSC21コード</returns>
        public override string ToString()
        {
            //return GetGlobalName() + "/" + GetLocalName();
            return GetLocalName();
        }

        /// <summary>グローバル名称を取得する</summary>
        /// <returns>グローバル名称</returns>
        public string GetGlobalName()
        {
            throw new Exception("未実装");
        }

        /// <summary>ローカル名称を取得する</summary>
        /// <returns>ローカル名称</returns>
        public string GetLocalName()
        {
            StringBuilder sBuilder = new StringBuilder();

            if (Prefix != null && Prefix != "") sBuilder.Append(Prefix);
            sBuilder.Append(coreName);
            if (ArrayString != null && ArrayString != "") sBuilder.Append("[" + ArrayString + "]");
            if (Suffix != null && Suffix != "") sBuilder.Append(Suffix);

            if (Child != null && Child != NULL_OBJECT) sBuilder.Append("_" + Child.GetLocalName());

            return sBuilder.ToString();
        }

        #endregion

        #region TSC21コード設定処理

        /// <summary>接頭文字を設定する</summary>
        /// <param name="prefix">接頭文字</param>
        /// <returns>設定成功の真偽</returns>
        public bool SetPrefix(string prefix)
        {
            //1以上の小文字英字の並びの場合
            if (RG2.IsMatch(prefix))
            {
                this.prefix = prefix;
                return true;
            }
            else return false;
        }

        /// <summary>添字を設定する</summary>
        /// <param name="suffix">添字</param>
        /// <returns>設定成功の真偽</returns>
        public bool SetSuffix(string suffix)
        {
            //1以上の小文字英字の並びの場合
            if (RG5.IsMatch(suffix))
            {
                this.suffix = suffix;
                return true;
            }
            else return false;
        }

        /// <summary>配列の文字を設定する</summary>
        /// <param name="arrayString">配列の文字</param>
        /// <returns>設定成功の真偽</returns>
        public bool SetArrayString(string arrayString)
        {
            //1以上の小文字英字の並びの場合
            if (RG4.IsMatch(suffix))
            {
                this.arrayString = arrayString;
                return true;
            }
            else return false;
        }
        
        #endregion

        #region クラスメソッド

        /// <summary>TSCコードからTSCObjectを作成する</summary>
        /// <param name="tscCode">TSCコード</param>
        /// <param name="tscObject">TSCObject</param>
        /// <returns>作成成功の真偽</returns>
        public static bool TryMakeTSCObjectFromTSCCode(string tscCode, out TSCObject tscObject)
        {
            tscObject = NULL_OBJECT;

            Regex reg = new Regex("[A-Z]+");
            
            //階層を切断
            string[] objs = tscCode.Split('_');
            for (int i = 1; i < objs.Length; i++)
            {
                if (objs[i] == "") break;

                //大文字を含まない場合は添字
                while ((!reg.IsMatch(objs[i])) && (objs[i] != ""))
                {
                    objs[i - 1] += "_" + objs[i];
                    for (int j = i; j < objs.Length - 1; j++)
                    {
                        objs[j] = objs[j + 1];
                    }
                    objs[objs.Length - 1] = "";
                }
            }

            tscObject = makeTSCObject(objs[0]);
            if (tscObject == NULL_OBJECT) return false;

            TSCObject parent = tscObject;
            for (int i = 1; i < objs.Length; i++)
            {
                if (objs[i] == "") break;
                TSCObject child = makeTSCObject(objs[i]);
                if (tscObject == NULL_OBJECT) break;
                parent.Child = child;
                parent = child;
            }

            return true;
        }

        /// <summary>TSCコードからTSCObjectを作成する</summary>
        /// <param name="tscCode">TSCコード</param>
        /// <returns>TSCObject</returns>
        private static TSCObject makeTSCObject(string tscCode)
        {
            if (!RG1.IsMatch(tscCode)) return NULL_OBJECT;

            TSCObject tscObj = new TSCObject("DEFAULT");
            Match match = RG2.Match(tscCode);
            if (match.Success)
            {
                tscObj.SetPrefix(match.Value);
                tscCode = tscCode.Remove(0, match.Length);
            }
            match = RG3.Match(tscCode);
            if (match.Success)
            {
                tscObj.CoreName = match.Value;
                tscCode = tscCode.Remove(0, match.Length);
            }
            match = RG4.Match(tscCode);
            if (match.Success)
            {
                tscObj.SetArrayString(match.Value.Substring(1, match.Value.Length - 2));
                tscCode = tscCode.Remove(0, match.Length);
            }
            match = RG5.Match(tscCode);
            if (match.Success)
            {
                tscObj.SetSuffix(match.Value);
            }

            return tscObj;
        }

        #endregion

    }

    #region 読み取り専用TSC21オブジェクト

    /// <summary>読み取り専用TSC21オブジェクト</summary>
    public interface ImmutableTSCObject
    {
        /// <summary>機器・コア名称を取得する</summary>
        string CoreName
        {
            get;
        }

        /// <summary>接頭文字を取得する</summary>
        string Prefix
        {
            get;
        }

        /// <summary>添字を取得する</summary>
        string Suffix
        {
            get;
        }

        /// <summary>配列の文字を取得する</summary>
        string ArrayString
        {
            get;
        }

        /// <summary>子要素を取得する</summary>
        ImmutableTSCObject Child
        {
            get;
        }

        /// <summary>グローバル名称を取得する</summary>
        /// <returns>グローバル名称</returns>
        string GetGlobalName();

        /// <summary>ローカル名称を取得する</summary>
        /// <returns>ローカル名称</returns>
        string GetLocalName();
    }

    #endregion

}
