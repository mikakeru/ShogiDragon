using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using ForSCIP;

namespace RyuKiki
{
    /*
     *   aeb
     *   f g
     *   chd
     * 
     *  abcdを先に決めたので、efghをあとで追加
     */

    class RyuKiki
    {
        enum Koma
        {
            Uma,
            Ryu,
        };

        /// <summary>
        /// 変数を変えることによって問題をおおまかに変更
        /// 細かく変えるときは、Mondai2Footerを修正
        /// </summary>
        static Koma s_Koma = Koma.Ryu;
        static int s_kiki = 4;
        static int s_num = 35;
        static int s_tate = 9;
        static int s_yoko = 9;

        /// <summary>
        /// 解答をメモリ内に保持すための形式
        /// </summary>
        class Kotae
        {
            public int[,] array;
            public readonly int m_tate;
            public readonly int m_yoko;

            public Kotae(int tate, int yoko)
            {
                m_tate = tate;
                m_yoko = yoko;

                array = new int[tate, yoko];
                for (int t = 0; t < tate; t++)
                {
                    for (int y = 0; y < y++; y++)
                    {
                        array[t, y] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// solファイルを読んでKotaeにする
        /// </summary>
        /// <param name="filesol"></param>
        /// <param name="tate"></param>
        /// <param name="yoko"></param>
        /// <returns></returns>
        static Kotae Sol2Kotae(string filesol, int tate, int yoko)
        {
            Kotae kotae = new Kotae(tate, yoko);

            using (StreamReader objReader = new StreamReader(filesol))
            {
                while (true)
                {
                    string sLine = objReader.ReadLine();
                    if (sLine == null)
                    {
                        break;
                    }
                    if (sLine[0] == 'b' && sLine[3] == ' ')
                    {
                        int t = sLine[1] - '1';
                        int y = sLine[2] - '1';
                        kotae.array[t, y] = 1;
                    }
                }
            }
            return kotae;
        }

        /// <summary>
        /// 盤面内であれば、-btyを書き出す
        /// </summary>
        /// <param name="tate"></param>
        /// <param name="yoko"></param>
        /// <param name="t"></param>
        /// <param name="y"></param>
        /// <param name="sw"></param>
        static void WroteTheNameIfInTheArea(int tate, int yoko, int t, int y, StringWriter sw)
        {
            if (1 <= t && t <= tate && 1 <= y && y <= yoko)
            {
                sw.Write($"- b{t}{y} ");
            }
        }

        /// <summary>
        /// 盤面内であれば、指定のstrを書き出す
        /// </summary>
        /// <param name="tate"></param>
        /// <param name="yoko"></param>
        /// <param name="t"></param>
        /// <param name="y"></param>
        /// <param name="str"></param>
        /// <param name="sw"></param>
        static void WriteTheStringIfInTheArea(int tate, int yoko, int t, int y, string str, StringWriter sw)
        {
            if (1 <= t && t <= tate && 1 <= y && y <= yoko)
            {
                sw.Write(str);
            }
        }

        /// <summary>
        /// ひとつの位置に関して、８つの方向の「利きの式」を記載
        /// </summary>
        /// <param name="tate"></param>
        /// <param name="yoko"></param>
        /// <param name="t"></param>
        /// <param name="y"></param>
        /// <param name="sw"></param>
        static void WriteForEachSquare(int tate, int yoko, int t, int y, StringWriter sw)
        {
            sw.WriteLine("\\ kiki");
            sw.WriteLine($" b{t}{y} = 1 -> n{t}{y} = {s_kiki}");

            // 利き数を数える      wa -member0 -member1 - member2 - member3 = 0 という形
            sw.WriteLine("\\ wa");
            sw.Write($" n{t}{y} ");

            // 全部で８方向
            if (s_Koma == Koma.Uma)
            {
                WriteTheStringIfInTheArea(tate, yoko, t - 1, y - 1, $" - b{t}{y}a ", sw);
                WroteTheNameIfInTheArea(tate, yoko, t - 1, y + 0, sw);
                WriteTheStringIfInTheArea(tate, yoko, t - 1, y + 1, $" - b{t}{y}b ", sw);

                WroteTheNameIfInTheArea(tate, yoko, t + 0, y - 1, sw);
                WroteTheNameIfInTheArea(tate, yoko, t + 0, y + 1, sw);

                WriteTheStringIfInTheArea(tate, yoko, t + 1, y - 1, $" - b{t}{y}c ", sw);
                WroteTheNameIfInTheArea(tate, yoko, t + 1, y + 0, sw);
                WriteTheStringIfInTheArea(tate, yoko, t + 1, y + 1, $" - b{t}{y}d ", sw);
            }
            else if (s_Koma == Koma.Ryu)
            {
                // 龍だとefghはブチヌキの利き  
                //    e
                //  f   g
                //    h  とする
                WroteTheNameIfInTheArea(tate, yoko, t - 1, y - 1, sw);
                WriteTheStringIfInTheArea(tate, yoko, t - 1, y + 0, $" - b{t}{y}e ", sw);
                WroteTheNameIfInTheArea(tate, yoko, t - 1, y + 1, sw);

                WriteTheStringIfInTheArea(tate, yoko, t + 0, y - 1, $" - b{t}{y}f ", sw);
                WriteTheStringIfInTheArea(tate, yoko, t + 0, y + 1, $" - b{t}{y}g ", sw);

                WroteTheNameIfInTheArea(tate, yoko, t + 1, y - 1, sw);
                WriteTheStringIfInTheArea(tate, yoko, t + 1, y + 0, $" - b{t}{y}h ", sw);
                WroteTheNameIfInTheArea(tate, yoko, t + 1, y + 1, sw);
            }

            sw.WriteLine(" = 0");
        }

        /// <summary>
        /// lp用のヘッダー生成
        /// </summary>
        static string Mondai2Header()
        {
            using (StringWriter sw = new StringWriter())
            {
                //  日付などかいて
                sw.WriteLine(@"\ " + DateTime.Now.ToString());

                //  これはミニマイズはダミー
                sw.WriteLine("Maximize");
                sw.WriteLine(" value: dummy");

                return sw.ToString();
            }
        }

        /// <summary>
        /// 問題lpファイルの下部分を作成する
        /// </summary>
        /// <param name="tate"></param>
        /// <param name="yoko"></param>
        /// <returns></returns>
        static string Mondai2Footer(int tate, int yoko)
        {
            using (StringWriter sw = new StringWriter())
            {
                sw.WriteLine("Subject To");

                // インデックスをt=1 からt=9までとする

                // 総数を数える
                sw.Write(" sum");
                for (int t = 1; t <= tate; t++)
                {
                    for (int y = 1; y <= yoko; y++)
                    {
                        sw.Write($" - b{y}{t}");
                    }
                }
                sw.WriteLine(" = 0");

                // 条件
                for (int t = 1; t <= tate; t++)
                {
                    for (int y = 1; y <= yoko; y++)
                    {
                        WriteForEachSquare(tate, yoko, t, y, sw);
                    }
                }

                List<string> list = new List<string>();

                // のびる方向対応
                for (int d8 = 0; d8 < 8; d8++)
                {
                    if (s_Koma == Koma.Ryu && (d8 == 1 || d8 == 3 || d8 == 4 || d8 == 6))
                    {
                        for (int t = 1; t <= tate; t++)
                        {
                            for (int y = 1; y <= yoko; y++)
                            {
                                int length = 0;
                                int dt = 0;
                                int dy = 0;
                                string symbol = "?";

                                if (d8 == 1)
                                {
                                    length = t - 1;
                                    dt = -1;
                                    symbol = "e";
                                }
                                else if (d8 == 3)
                                {
                                    length = y - 1;
                                    dy = -1;
                                    symbol = "f";
                                }
                                else if (d8 == 4)
                                {
                                    length = yoko - y;
                                    dy = 1;
                                    symbol = "g";
                                }
                                else if (d8 == 6)
                                {
                                    length = tate - t;
                                    dt = 1;
                                    symbol = "h";
                                }

                                if (length >= 1)
                                {
                                    sw.WriteLine("\\ cond");

                                    // 利きあるなし
                                    string bKiki = $"b{t}{y}{symbol}";

                                    list.Add(bKiki);

                                    // 利き数
                                    string nKiki = $"n{t}{y}{symbol}";

                                    List<string> memberList = new List<string>();
                                    for (int i = 1; i < length + 1; i++)
                                    {
                                        int it = t + i * dt;
                                        int iy = y + i * dy;

                                        memberList.Add($"b{it}{iy}");
                                    }

                                    // 和
                                    sw.Write($" {nKiki}");
                                    foreach (string member in memberList)
                                    {
                                        sw.Write($" - {member}");
                                    }
                                    sw.WriteLine(" = 0");

                                    sw.WriteLine($" 10 {bKiki} - {nKiki} > 0");
                                    sw.WriteLine($" 10 {bKiki} - {nKiki} < 9");

                                }
                            }
                        }
                    }
                }

                // 設定
                sw.WriteLine($" sum > {s_num}");
                sw.WriteLine($" sum > 1");

                // 
                sw.WriteLine("Bounds");
                sw.WriteLine(" dummy = 0");

                //sw.WriteLine(" b88 = 0");
                //sw.WriteLine(" b23 = 0");
                //sw.WriteLine(" b55 = 1");

                sw.WriteLine("Binary");

                for (int t = 1; t <= tate; t++)
                {
                    for (int y = 1; y <= yoko; y++)
                    {
                        sw.WriteLine($" b{y}{t}");
                    }
                }

                // ナナメ変数
                foreach (string s in list)
                {
                    sw.WriteLine($" {s}");
                }

                sw.WriteLine("End");

                return sw.ToString();
            }
        }

        /// <summary>
        /// Kotaeから図をテキストで掃き出す
        /// </summary>
        /// <param name="kotae"></param>
        /// <param name="filename"></param>
        static void Kotae2Txt(Kotae kotae, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
            {
                for (int t = 0; t < kotae.m_tate; t++)
                {
                    for (int y = 0; y < kotae.m_yoko; y++)
                    {
                        if (kotae.array[t, y] == 0)
                        {
                            sw.Write("＋");
                        }
                        else
                        {
                            sw.Write("●");
                        }
                    }
                    sw.WriteLine("");
                }
            }
        }

        /// <summary>
        /// main関数
        /// forループを使って別解探索などにあてる
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string filetmp = "tmp.txt";
            int tate = s_tate;
            int yoko = s_yoko;

            // 一つのファイルをもとにファイル名の拡張番号付きシステムを作る
            FilenameNumExt fileNameExt = new FilenameNumExt(filetmp);

            // サブジェクトの下地を作る
            Oazukari oazukari = new Oazukari();
            oazukari.Set(Oazukari.HSF.Header, Mondai2Header());
            oazukari.Set(Oazukari.HSF.Footer, Mondai2Footer(tate, yoko));

            // for文候補
            {
                // 現行のlpを記録
                string filelp = fileNameExt.GetFilename(".lp");
                oazukari.Save(filelp);

                //  LPを解いてSOLにする
                string filesol = fileNameExt.GetFilename(".sol");
                Util.Lp2Sol(filelp, filesol);

                //  SOL読む
                Kotae kotae = Sol2Kotae(filesol, tate, yoko);

                // 試しに書きだす
                Kotae2Txt(kotae, fileNameExt.GetFilename(".kotae.txt"));
            }

            // ファイルの削除
            string[] files = Directory.GetFiles(@".\", @"tmp.txt*");
            for (int i = 0; i < files.Length; i++)
            {
                //                File.Delete(files[i]);
            }

        }


    }
}

/// <summary>
/// SCIP利用ようのアイテム群
// </summary>
namespace ForSCIP
{
    /// <summary>
    /// lpfileの内容をTXTとしてあずかる
    /// 別解探索のための追加機構あり
    /// </summary>
    class Oazukari
    {
        public enum HSF
        {
            Header,
            Subject,
            Footer
        };

        string m_header;
        string m_subject;
        string m_footer;


        public Oazukari()
        {
            m_header = "";
            m_subject = "";
            m_footer = "";
        }

        public void Set(HSF hsf, string str)
        {
            switch (hsf)
            {
                case HSF.Header: m_header = str + m_header; break;
                case HSF.Subject: m_subject = str + m_subject; break;
                case HSF.Footer: m_footer = str + m_footer; break;
            }
        }

        public void Save(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(m_header);
                sw.Write(m_subject);
                sw.Write(m_footer);
            }
        }
    }


    /// <summary>
    /// あるファイル名をもとに、カウントアップ数字を含んだファイル名を次々と作り出す。
    /// 履歴を追うのに便利
    /// </summary>
    class FilenameNumExt
    {
        readonly string m_stem;    //  ピリオドなし
        int num;

        public FilenameNumExt(string stem)
        {
            m_stem = stem;
            num = 0;
        }

        /// <summary>
        /// 呼ぶたびにあらたなファイル名を起こしてくれる
        /// 数字を挟むけどあとはただ足す
        /// </summary>
        /// <param name="ext">ピリオドはじまり拡張子</param>
        /// <returns></returns>
        public string GetFilename(string ext)
        {
            string ret = m_stem + num.ToString("D2") + ext; //  ただ足す
            num++;
            return (ret);
        }

    }

    /// <summary>
    /// lpファイルを解いてsolファイルに書き出す
    /// </summary>
    static class Util
    {
        /// <summary>
        /// 問題の種類に限らず共通
        /// </summary>
        /// <param name="lpfile">lpfile名</param>
        /// <param name="solfile">solfile名</param>
        public static void Lp2Sol(string lpfile, string solfile)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = @"C:\Program Files\SCIPOptSuite 7.0.2\bin\scip.exe";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;

            myProcess.Start();

            StreamWriter myStreamWriter = myProcess.StandardInput;
            StreamReader myStreamReader = myProcess.StandardOutput;

            myStreamWriter.WriteLine("read " + lpfile);
            myStreamWriter.WriteLine("optimize");
            myStreamWriter.WriteLine("write solution " + solfile);
            myStreamWriter.WriteLine("quit");
            myStreamWriter.Close();

            for (; ; )
            {
                string str = myStreamReader.ReadLine();
                if (str == null)
                {
                    break;
                }
                else
                {
                    Debug.WriteLine(str);
                }
            }

            myProcess.WaitForExit();
            myProcess.Close();
        }


    }


}

/*
0 -9
＋＋＋＋＋●＋＋＋
＋＋＋●＋＋＋＋＋
＋＋＋＋＋＋●＋＋
●＋＋＋＋＋＋＋＋
＋＋●＋＋＋＋＋＋
＋＋＋＋＋＋＋＋●
＋＋＋＋●＋＋＋＋
＋●＋＋＋＋＋＋＋
＋＋＋＋＋＋＋●＋

    1-12                  14ないかも
＋＋●＋●＋＋＋＋
＋＋＋＋＋＋●＋●
＋＋＋●＋＋＋＋＋
＋＋＋＋＋＋＋●＋
●＋＋＋＋＋＋＋＋
●＋＋＋＋＋＋＋＋
＋＋＋＋＋＋＋●＋
＋●＋＋＋●＋＋＋
＋＋＋●＋＋＋＋＋

    2-18                   19はないのかも
＋＋＋●＋＋●＋＋
＋＋＋●＋＋＋＋＋
＋＋＋＋＋●＋●＋
＋●＋＋＋＋＋●＋
＋●＋＋＋＋＋＋＋
＋●＋＋＋＋＋＋●
＋＋＋●●＋●＋＋
●＋＋＋＋＋＋＋●
●＋●＋＋●＋＋＋


3-28                       29はなさそう
＋●●●●＋●●＋
●＋＋＋＋＋＋＋●
＋＋●＋＋＋＋＋●
＋＋●＋●●●＋●
●＋●＋＋＋＋＋●
●＋＋＋＋●＋＋＋
＋＋＋＋＋●＋●＋
●＋＋＋＋●＋●＋
＋●＋●＋＋＋＋●


4-35                     36ない

＋＋●＋＋＋●＋＋
＋●●●＋●＋●＋
●＋＋＋＋●＋●●
＋●●●＋●＋●＋
＋＋＋＋＋●＋●＋
＋●●●●●＋●●
●＋＋＋＋＋＋●＋
＋●●●●●●＋＋
＋＋●＋＋●＋＋＋


*/
