using AngleSharp.Html.Parser;

namespace HypoListGetter
{
    /// <summary>
    /// JeqDB_Converterから移植したものがほとんどです。
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var addText = File.Exists("lastGetPlus1.dat") ? "最終取得の翌日 " + File.ReadAllText("lastGetPlus1.dat") + " がおすすめです。" : "2023/04/01から有効です。";
            var startDate = (DateTime)ConAsk("開始日時を入力してください。" + addText, typeof(DateTime));
            var EndDate = (DateTime)ConAsk("終了日時を入力してください。二日前の " + DateTime.Now.AddDays(-2).ToString("yyyy/MM/dd") + " がおすすめです。", typeof(DateTime));
            DateTime getDate;
            ConWrite($"高速大量アクセスを防ぐため取得毎に1秒待機します。予想処理時間は{(EndDate - startDate + TimeSpan.FromDays(1)).TotalDays}秒({(EndDate - startDate + TimeSpan.FromDays(1)).TotalDays / 60d:F1}分)+取得・処理時間です。");
            for (getDate = startDate; getDate <= EndDate; getDate += TimeSpan.FromDays(1))
            {
                GetHypo(getDate);
            }
            ConWrite("終了しました。");
        }

        public static HttpClient client = new();
        public static readonly HtmlParser parser = new();

        /// <summary>
        /// 震源リスト(無感含む)を震度データベース互換に変換
        /// </summary>
        /// <remarks>震度は---になります</remarks>
        public static void GetHypo(DateTime getDate)
        {
            try
            {
                var url = $"https://www.data.jma.go.jp/eqev/data/daily_map/{getDate:yyyyMMdd}.html";
                //ConWrite("取得中...");
                var response = client.GetStringAsync(url).Result;
                //ConWrite("解析中...");
                var document = parser.ParseDocument(response);
                var pre = document.QuerySelector("pre")!.TextContent;
                var lines_converted = pre.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(2).Select(HypoText2EqdbData);
                //var datas = lines.Select(HypoText2Data);
                ConWrite($"取得: {getDate:yyyy/MM/dd}, データ個数: {lines_converted.Count()}", ConsoleColor.Green);

                //var csv = new StringBuilder("地震の発生日,地震の発生時刻,震央地名,緯度,経度,深さ,Ｍ,最大震度\n");
                var csv = "地震の発生日,地震の発生時刻,震央地名,緯度,経度,深さ,Ｍ,最大震度\n" + (string.Join('\n', lines_converted))
                    .Replace(",- km,", ",不明,").Replace(",-,", ",不明,").Replace("'", "′")
                    .Replace("/1/", "/01/").Replace("/2/", "/02/").Replace("/3/", "/03/").Replace("/4/", "/04/").Replace("/5/", "/05/")//月調整
                    .Replace("/6/", "/06/").Replace("/7/", "/07/").Replace("/8/", "/08/").Replace("/9/", "/09/")
                    .Replace("/1,", "/01,").Replace("/2,", "/02,").Replace("/3,", "/03,").Replace("/4,", "/04,").Replace("/5,", "/05,")//日調整
                    .Replace("/6,", "/06,").Replace("/7,", "/07,").Replace("/8,", "/08,").Replace("/9,", "/09,")
                    .Replace(":1.", ":01.").Replace(":2.", ":02.").Replace(":3.", ":03.").Replace(":4.", ":04.").Replace(":5.", ":05.")//秒調整
                    .Replace(":6.", ":06.").Replace(":7.", ":07.").Replace(":8.", ":08.").Replace(":9.", ":09.").Replace(":0.", ":00.")
                    .Replace("°1.", "°01.").Replace("°2.", "°02.").Replace("°3.", "°03.").Replace("°4.", "°04.").Replace("°5.", "°05.")//緯度経度分調整
                    .Replace("°6.", "°06.").Replace("°7.", "°07.").Replace("°8.", "°08.").Replace("°9.", "°09.").Replace("°0.", "°00.")
                    + "\n";
                var savePath = $"output\\{getDate:yyyy\\\\MM\\\\dd}.csv";
                /*
                foreach (var data in datas)
                {
                    //2024/01/03,20:07:02.4,能登半島沖,37°12.6′N,136°40.9′E,8 km,2.9,震度１
                    csv.Append(data.Time.ToString("yyyy/MM/dd"));
                    csv.Append(',');
                    csv.Append(data.Time.ToString("HH:mm:ss.f"));
                    csv.Append(',');
                    csv.Append(data.Hypo);
                    csv.Append(',');
                    csv.Append(LatLonDouble2String(data.Lat, true));
                    csv.Append(',');
                    csv.Append(LatLonDouble2String(data.Lon, false));
                    csv.Append(',');
                    csv.Append(data.Depth);
                    csv.Append(" km,");
                    csv.Append(data.Mag);
                    csv.Append(",---");
                    csv.AppendLine();
                }*/

                Directory.CreateDirectory($"output\\{getDate:yyyy\\\\MM}");
                File.WriteAllText(savePath, csv.ToString());
                //ConWrite("保存しました。");
                File.WriteAllText("lastGetPlus1.dat", getDate.AddDays(1).ToString("yyyy/MM/dd"));
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                ConWrite(ex);
                throw;
            }
        }

        /// <summary>
        /// 震源リスト1行のデータをData形式にします。
        /// </summary>
        /// <param name="text">csv1行</param>
        /// <returns>Data形式のデータ</returns>
        public static Data HypoText2Data(string text)
        {
            //0    1  2  3     4     5         6              7      8    9
            //2024 11 22 00:28 56.7  41° 1.5'N 141° 3.6'E   13     0.2  陸奥湾                   //33°18.2'N 135°27.9'E
            var datas = text.Replace("° ", "°").Split(' ', StringSplitOptions.RemoveEmptyEntries);//無駄に空白が入るため
            var dt = new Data
            {
                Time = DateTime.Parse($"{datas[0]}/{datas[1]}/{datas[2]} {datas[3]}:{datas[4]}"),
                Hypo = datas[9],
                Lat = LatLonString2Double(datas[5]),
                Lon = LatLonString2Double(datas[6]),
                Depth = int.Parse(datas[7]),
                Mag = double.Parse(datas[8].Replace("-", "-1")),
                MaxInt = -1
            };
            dt.Simple = dt.Time + "," + dt.Hypo + "," + dt.Lat + "," + dt.Lon + "," + dt.Depth + "," + dt.Mag + ",-";
            return dt;
        }

        /// <summary>
        /// 震源リスト1行のデータを震度データベース形式に変換します。
        /// </summary>
        /// <param name="text">csv1行</param>
        /// <returns>震度データベース形式のデータ</returns>
        public static string HypoText2EqdbData(string text)
        {
            var datas = text.Replace("° ", "°").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return (datas[0] + "/" + datas[1] + "/" + datas[2] + "," + datas[3] + ":" + datas[4] + "," + datas[9] + "," + datas[5] + "," + datas[6] + "," +
                datas[7] + " km," + datas[8] + ",---");
        }

        /// <summary>
        /// 緯度や経度を60進数表記からdoubleに変換します。
        /// </summary>
        /// <param name="ll">緯度や経度</param>
        /// <returns>doubleの経度</returns>
        public static double LatLonString2Double(string ll)
        {
            string[] lls = ll.Replace("N", "").Replace("E", "").Replace("'", "").Split(['°', '′']);
            return double.Parse(lls[0]) + double.Parse(lls[1]) / 60d;
        }

        /// <summary>
        /// 緯度や経度を60進数表記からdoubleに変換します。
        /// </summary>
        /// <param name="ll">緯度や経度</param>
        /// <param name="isLat">緯度か</param>
        /// <returns>doubleの経度</returns>
        public static string LatLonDouble2String(double ll, bool isLat)
        {//37°12.6′N
            var d = (int)ll;
            var m = (ll - d) * 60;
            return $"{d}°{Math.Round(m, 2, MidpointRounding.AwayFromZero):F1}′" + (isLat ? "N" : "E");
        }

        /// <summary>
        /// データ保存用クラス
        /// </summary>
        public class Data
        {
            /// <summary>
            /// 地震の発生日時
            /// </summary>
            public DateTime Time { get; set; }

            /// <summary>
            /// 震央地名
            /// </summary>
            public string Hypo { get; set; } = "";//CS8618回避

            /// <summary>
            /// 緯度
            /// </summary>
            public double Lat { get; set; }

            /// <summary>
            /// 経度
            /// </summary>
            public double Lon { get; set; }

            /// <summary>
            /// 深さ
            /// </summary>
            public int Depth { get; set; }

            /// <summary>
            /// マグニチュード
            /// </summary>
            public double Mag { get; set; }

            /// <summary>
            /// 最大震度
            /// </summary>
            public int MaxInt { get; set; }

            /// <summary>
            /// 独自簡素版
            /// </summary>
            public string Simple { get; set; } = "";
        }

        /// <summary>
        /// ユーザーに値の入力を求めます。
        /// </summary>
        /// <remarks>入力値が変換可能なとき返ります。</remarks>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="resType">変換するタイプ</param>
        /// <param name="nullText">何も入力されなかった場合に選択</param>
        /// <returns><paramref name="resType"/>で指定したタイプに変換された入力された値</returns>
        public static object ConAsk(string message, Type resType, string? nullText = null)
        {
            while (true)
                try
                {
                    ConWrite(message);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    var input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        if (string.IsNullOrEmpty(nullText))
                            throw new Exception("値を入力してください。");
                        input = nullText;
                        ConWrite(nullText + "(自動入力)", ConsoleColor.Cyan);
                    }
                    return resType.Name switch
                    {
                        "String" => input,
                        "Int32" => int.Parse(input),
                        "Double" => double.Parse(input),
                        "DateTime" => DateTime.Parse(input),
                        "TimeSpan" => TimeSpan.Parse(input),
                        _ => throw new Exception($"変換タイプが未定義です({resType})。"),
                    };
                }
                catch (Exception ex)
                {
                    ConWrite("入力の処理に失敗しました。" + ex.Message, ConsoleColor.Red);
                }
        }

        /// <summary>
        /// コンソールのデフォルトの色
        /// </summary>
        public static readonly ConsoleColor defaultColor = Console.ForegroundColor;

        /// <summary>
        /// コンソールにデフォルトの色で出力します。
        /// </summary>
        /// <param name="text">出力するテキスト</param>
        /// <param name="withLine">改行するか</param>
        public static void ConWrite(string? text, bool withLine = true)
        {
            ConWrite(text, defaultColor, withLine);
        }

        /// <summary>
        /// 例外のテキストを赤色で出力します。
        /// </summary>
        /// <param name="ex">出力する例外</param>
        public static void ConWrite(Exception ex)
        {
            ConWrite(ex.ToString(), ConsoleColor.Red);
        }

        /// <summary>
        /// コンソールに色付きで出力します。色は変わったままとなります。
        /// </summary>
        /// <param name="text">出力するテキスト</param>
        /// <param name="color">表示する色</param>
        /// <param name="withLine">改行するか</param>
        public static void ConWrite(string? text, ConsoleColor color, bool withLine = true)
        {
            Console.ForegroundColor = color;
            if (withLine)
                Console.WriteLine(text);
            else
                Console.Write(text);
        }
    }
}
