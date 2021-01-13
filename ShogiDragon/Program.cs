using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using ForSkicp;


/*
 *   aeb
 *   f g
 *   chd
 * 
 *  abcdを先に決めたので、efghをあとで追加
 */

namespace RyuKiki
{

    class CheckCell
    {
        public readonly int m_t;                //  X座標  
        public readonly int m_y;                //  Y座標
        public readonly int m_group;            //  m_group<0 グループ立ち上げますかチェック　m_group>=0 グループ所属お誘いチェック　
        //  int m_groupcnt      //  はまた別カウント

        public CheckCell(int t, int y, int group)
        {
            m_t = t;
            m_y = y;
            m_group = group;
        }
    };


    class RyuKiki
    {
        enum Kind
        {
            Tatesen,
            Yokosen,
            Masu,
            Juji
        };

        enum Koma
        {
            Uma,
            Ryu,
        };

        static Koma s_Koma = Koma.Ryu;
        static int s_kiki = 4;
        static int s_num = 35;
        static int s_tate = 9;
        static int s_yoko = 9;
        // 10 数秒 15 数秒  20 数秒 26 1,2分

        /// <summary>
        /// 全部の種類
        /// 
        /// 文字コードのままやりきる手はあるがとりあえずTAGに直す方法で
        /// 
        /// </summary>
        enum AllTag
        {
            //  ベースとしてデバッグ用には何もなしがあった方が
            NL,     //  NULL
            //  問題文に現れるもの
            WH,     //  白地
            BC,     //  ●指定
        }

        enum EvenOdd /* 偶数奇数の組み合わせで */
        {
            Cross,
            Vline,
            Hline,  /* 横が奇数　なのは　横線　*/
            Square,
        }

        class Cell
        {
            readonly public int m_tate;
            readonly public int m_yoko;

            // CeLL 作り出すときに、奇数偶数チェックや外だとかは入れてしまう。
            /// <summary>
            /// 
            /// </summary>
            /// <param name="tate">サイズ</param>
            /// <param name="yoko">サイズ</param>
            /// <param name="t">ポジション</param>
            /// <param name="y">ポジション</param>
            /// <param name="soto"></param>
            public Cell(int etate, int eyoko, int t, int y)
            {
                m_tate = t;
                m_yoko = y;
                //                m_symbol = makeSymbol("x", t, y);
            }
        };

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




        #region /* マスの種類 */


        /// <summary>
        /// 文字から数字に直す
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        static int Char2Number(int ch)
        {
            // '0'-'9'
            return ch - '0';
        }

        static char AllTag2Char(AllTag tag)
        {
            switch (tag)
            {
                case AllTag.NL: return (' ');
                case AllTag.WH: return ('-');
                case AllTag.BC: return ('*');
            }
            return ('?');
        }

        static int AllTag2Num(AllTag tag)
        {
            return (-1);
        }

        /// <summary>
        /// とりあえずPDF基準で
        /// </summary>
        enum Tag
        {
            WH,
            NW,
            NE,
            SW,
            SE,
            /// <summary>
            /// 代表として黒
            /// </summary>
            BL,
        }

        //  外側がある
        //  数字ののった黒
        //  最初からただの黒


        #endregion

        #region /* シンボルはシンボル作りコーナーを通してから */

        /// <summary>
        /// 2次元の値から　xシンボルにする
        /// 
        /// 方向はでよいか
        /// 0  1
        /// 3  2
        /// 
        /// wh
        /// nw
        /// ne
        /// 
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static string makeSymbol(string str, int t, int y)
        {
            return str + t.ToString("D2") + y.ToString("D2");
        }

        /// <summary>
        /// 2次元の値から　sumシンボルにする
        /// </summary>
        /// <param name="t"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static string ty2sum(int t, int y)
        {
            return "sum" + t.ToString("D2") + y.ToString("D2");
        }

        #endregion


        //
        //  線を引くけどひと枠多いところから
        //  お外の線はなくてもよいが帰って迷うので大外の線から
        //　・－・－
        //　｜外｜
        //　・－・－
        //　｜　｜端
        //　・－・

        /// <summary>
        /// 原稿の位置からテーブルの位置に直す
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        static int genkou2tablepos(int x)
        {
            //  0だったら３
            return (x * 2 + 3);
        }

        /// <summary>
        /// 原稿データの数からtableの数に直す
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        static int genkou2tablesize(int x)
        {
            return (x * 2 + 5);
        }

        static Kotae Sol2Table(string filesol, int tate, int yoko)
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


        static void Kotae2Final(Cell[,] mondai, Cell[,] kotae, Cell[,] kotae1, string filefinal)
        {
#if false
            int tate = kotae.GetLength(0);
            int yoko = kotae.GetLength(1);

            using (StreamWriter sw = new StreamWriter(filefinal))
            {
                sw.WriteLine(@"\ " + DateTime.Now.ToString());
                for (int t = 0; t < tate; t++)
                {
                    for (int y = 0; y < yoko; y++)
                    {
                        if (kotae[t, y].m_tag == AllTag.ON)
                        {
                            switch (kotae[t, y].m_tateyokoevenodd)
                            {
                                case EO.Hline: sw.Write("―"); break;
                                case EO.Vline: sw.Write("｜"); break;
                                case EO.Cross: sw.Write("・"); break;
                                case EO.Square:
                                    switch (mondai[t, y].m_tag)
                                    {
                                        case AllTag.N0: sw.Write("０"); break;
                                        case AllTag.N1: sw.Write("１"); break;
                                        case AllTag.N2: sw.Write("２"); break;
                                        case AllTag.N3: sw.Write("３"); break;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            sw.Write("　");
                        }
                    }
                    sw.WriteLine("");
                }

                if (kotae1 != null)
                {
                    sw.WriteLine(@"\ " + DateTime.Now.ToString());
                    for (int t = 0; t < tate; t++)
                    {
                        for (int y = 0; y < yoko; y++)
                        {
                            if (kotae[t, y].m_tag == AllTag.ON && kotae1[t,y].m_tag!=AllTag.ON)
                            {
                                switch (kotae[t, y].m_tateyokoevenodd)
                                {
                                    case EO.Hline: sw.Write("―"); break;
                                    case EO.Vline: sw.Write("｜"); break;
                                    case EO.Cross: sw.Write("・"); break;
                                    case EO.Square:
                                        switch (mondai[t, y].m_tag)
                                        {
                                            case AllTag.N0: sw.Write("０"); break;
                                            case AllTag.N1: sw.Write("１"); break;
                                            case AllTag.N2: sw.Write("２"); break;
                                            case AllTag.N3: sw.Write("３"); break;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                sw.Write("　");
                            }
                        }
                        sw.WriteLine("");
                    }
                }
            }
#endif
        }


#if false
        static void Display(int[,] kotae)
        {
            int tate = kotae.GetLength(0);
            int yoko = kotae.GetLength(1);

            for (int t = 0; t < tate; t++)
            {
                for (int y = 0; y < yoko; y++)
                {
                    if (kotae[t, y] == 0)
                    {
                        Debug.Write("□");
                    }
                    else
                    {
                        Debug.Write("■");
                    }
                }
                Debug.WriteLine("");
            }
        }
#endif

#if false
        /// <summary>
        /// TAGからCに直す
        /// </summary>
        /// <param name="mondai"></param>
        static void DebugWrite(Cell[,] mondai)
        {
            int tate = mondai.GetLength(0);
            int yoko = mondai.GetLength(1);

            for (int t = 0; t < tate; t++)
            {
                for (int y = 0; y < yoko; y++)
                {
                    char c=AllTag2Char(mondai[t,y].m_num);
                    Debug.Write(c);
                }
                Debug.WriteLine("");
            }
        }
#endif



        //  方向Tag(intの方が気楽だが・・・）から　dt dy 縦横変化分を返すよ
        //  左上基準
        static void Tag2ty(Tag tag, out int dt, out int dy)
        {
            switch (tag)
            {
                case Tag.NW: dt = 0; dy = 0; break;
                case Tag.NE: dt = 0; dy = 1; break;
                case Tag.SW: dt = 1; dy = 0; break;
                case Tag.SE: dt = 1; dy = 1; break;
                default: dt = 0; dy = 0; break;
            }
        }

        static Tag gyaku(Tag tag)
        {
            switch (tag)
            {
                case Tag.NW: return (Tag.SE);
                case Tag.NE: return (Tag.SW);
                case Tag.SW: return (Tag.NE);
                case Tag.SE: return (Tag.NW);
            }
            return (Tag.BL);    // ここへはこないのだが
        }

        static void Append(string src, string log)
        {
            string[] str = File.ReadAllLines(src, Encoding.GetEncoding("shift_jis"));
            File.AppendAllLines(log, str, Encoding.GetEncoding("shift_jis"));
        }

        // ヘッダーは実質中身なし
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
        /// 一次元の長さから
        /// </summary>
        /// <param name="length"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        static int Position2NumChoices(int length, int position)
        {
            int a = position;
            int b = (length - 1 - position);
            int c = Math.Min(a, b);
            return c;
        }

        static void condadd(List<string> list, int t, int y)
        {
            if (t >= 0 && t < 4 && y >= 0 && y < 4)
            {
                list.Add("c" + t.ToString() + y.ToString());
            }
        }


        static List<string> make_ato_cell(int t, int y)
        {
            List<string> list = new List<string>();

            condadd(list, t - 1, y);
            condadd(list, t + 1, y);
            condadd(list, t, y - 1);
            condadd(list, t, y + 1);

            return list;
        }

        static void WroteTheNameIfInTheArea(int tate, int yoko, int t, int y, StringWriter sw)
        {
            if (1 <= t && t <= tate && 1 <= y && y <= yoko)
            {
                sw.Write($"- b{t}{y} ");
            }
        }

        static void WriteTheStringIfInTheArea(int tate, int yoko, int t, int y, string str, StringWriter sw)
        {
            if (1 <= t && t <= tate && 1 <= y && y <= yoko)
            {
                sw.Write(str);
            }
        }

        static void Mondai2FooterSub0(int tate, int yoko, int t, int y, StringWriter sw)
        {
            sw.WriteLine("\\ kiki");
            sw.WriteLine($" b{t}{y} = 1 -> n{t}{y} = {s_kiki}");

            // 聞き数を数える      wa -member0 -member1 - member2 - member3 = 0 という形
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
                // efghはブチヌキの利き  龍だと
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

        static void abcd2dtdy(int abcd, out int dt, out int dy)
        {
            dt = 0;
            dy = 0;
            switch (abcd)
            {
                case 0: dt = -1; dy = -1; break;
                case 1: dt = -1; dy = +1; break;
                case 2: dt = +1; dy = -1; break;
                case 3: dt = +1; dy = +1; break;
            }
            return;
        }


        static void efgh2etey(int abcd, int tate, int yoko, out int et, out int ey)
        {
            et = 0;
            ey = 0;

            // 上も下もどちらもぎり内部の値
            switch (abcd)
            {
                case 0: et = 1; ey = 1; break;
                case 1: et = 1; ey = yoko; break;
                case 2: et = tate; ey = 1; break;
                case 3: et = tate; ey = yoko; break;
            }
            return;
        }


        static string abcd2symbol(int abcd)
        {
            string symbol = "";
            switch (abcd)
            {
                case 0: symbol = "a"; break;
                case 1: symbol = "b"; break;
                case 2: symbol = "c"; break;
                case 3: symbol = "d"; break;
            }
            return symbol;
        }


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
                        Mondai2FooterSub0(tate, yoko, t, y, sw);
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

        //        static private void checkSub(CheckCell cl, Cell[,] kotae,ref int[,] checkmap,ref int m_group,ref Stack<CheckCell> checkstack)
        /// <summary>
        /// ループをわけて登録したい
        /// </summary>
        /// <param name="kotae"></param>
        /// <returns></returns>
        static string Kotae2Subject(Kotae kotae)
        {
            using (StringWriter sw = new StringWriter())
            {
                //// 最初のスペース
                //sw.Write(" ");
                //for (int a = 0; a < 8; a++)
                //{
                //    sw.Write($"a{a}_{kotae.aarray[a]} + ");
                //}
                //for (int a = 0; a < 8; a++)
                //{
                //    sw.Write($"a{a}_{kotae.aarray[a]} + ");
                //}
                //sw.WriteLine("0 < 15");
                return sw.ToString();
            }
        }

        static string Bc2Str(bool bBc)
        {
            if (bBc)
            {
                return ("●");
            }
            else
            {
                return ("　");
            }
        }

        static void Mondai2Csv(Cell[,] mondai, string csvfile)
        {
        }

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
        /// 
        /// 細かいループ封じネタがいるので記憶をもった
        /// 
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string fileprob = "tmp.txt";
            int tate = s_tate;
            int yoko = s_yoko;

            // 一つのファイルをもとにファイル名の拡張番号付きシステムを作る
            FilenameNumExt fileNameExt = new FilenameNumExt(fileprob);

            // サブジェクトの下地を作る
            // スクリプトの
            Oazukari oazukari = new Oazukari();
            oazukari.Set(Oazukari.HSF.Header, Mondai2Header());
            oazukari.Set(Oazukari.HSF.Footer, Mondai2Footer(tate, yoko));

            int kai = 0;
            Kotae prevKotae = null;
            for (int i = 0; ; i++)
            {
                // 現行のlpを記録
                string filelp = fileNameExt.GetFilename(".lp");
                oazukari.Save(filelp);

                //  LPを解いてSOLにする
                string filesol = fileNameExt.GetFilename(".sol");
                Util.Lp2Sol(filelp, filesol);

                //  SOL読む
                Kotae kotae = Sol2Table(filesol, tate, yoko);

                // 試しに書きだす
                Kotae2Txt(kotae, fileNameExt.GetFilename(".kotae.txt"));

                break;
            }

            string[] files = Directory.GetFiles(@".\", @"tmp.txt*");

            for (int i = 0; i < files.Length; i++)
            {
                //                File.Delete(files[i]);
            }

        }


    }
}
namespace ForSkicp
{
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


    static class Util
    {
        /// <summary>
        /// 変数を作り出す
        /// シンボル＋二桁＋二桁
        /// </summary>
        /// <param name="str"></param>
        /// <param name="t"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static string SymbolD2D2(string str, int t, int y)
        {
            return str + t.ToString("D2") + y.ToString("D2");
        }

        public static string SymbolD2(string str, int t)
        {
            return str + t.ToString("D2");
        }

        /// <summary>
        /// 問題の種類に限らず共通
        /// </summary>
        /// <param name="lpfile"></param>
        /// <param name="result"></param>
        public static void Lp2Sol(string lpfile, string result)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = "../../exe/scip-3.1.0.win.x86_64.msvc.opt.spx.mt.exe";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;

            myProcess.Start();

            StreamWriter myStreamWriter = myProcess.StandardInput;
            StreamReader myStreamReader = myProcess.StandardOutput;

            myStreamWriter.WriteLine("read " + lpfile);
            myStreamWriter.WriteLine("optimize");
            myStreamWriter.WriteLine("write solution " + result);
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
