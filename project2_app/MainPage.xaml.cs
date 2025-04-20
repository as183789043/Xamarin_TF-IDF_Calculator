using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Xamarin.Forms;
namespace project2_app
{
    public partial class MainPage : ContentPage
    {
        public static Dictionary<string, string> GlobalResultDictionary { get; set; } = new Dictionary<string, string>(); // 保存Fetch3Button_Clicked結果
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Web2StringButton_Clicked(object sender, EventArgs e)
        {
            string result;
            var encodedUrl = System.Net.WebUtility.UrlEncode(Encrypturl.Text);
            var Web2Stringurl = "http://10.0.2.2:5000/Web2String?url=" + encodedUrl;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.ConnectionClose = true;
                if (DownloadResultFrame.IsVisible == false)
                {
                    try
                    {
                        var response = await client.GetAsync(Web2Stringurl);
                        response.EnsureSuccessStatusCode();
                        result = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        result = "Error: " + ex.Message;
                    }

                    DownloadResultLabel.Text = result;
                    Web2StringButton.Text = "Hide Content";
                    DownloadResultFrame.IsVisible = true;
                }

                else
                {
                    DownloadResultFrame.IsVisible = false;
                    Web2StringButton.Text = "Download";
                }

            }
        }
        private async void FilterButton_Clicked(object sender, EventArgs e)
        {
            string result;
            string content = DownloadResultLabel.Text;
            var url = "http://10.0.2.2:5000//WordFilter";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.ConnectionClose = true;
                if (FilterResultFrame.IsVisible == false)
                {
                    try
                    {
                        var response = await client.PostAsync(url, new StringContent(content));
                        response.EnsureSuccessStatusCode();
                        result = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        result = "Error: " + ex.Message;
                    }

                    FilterResultLabel.Text = result;
                    FilterButton.Text = "Hide Content";
                    FilterResultFrame.IsVisible = true;
                }

                else
                {
                    FilterResultFrame.IsVisible = false;
                    FilterButton.Text = "Filter Content";
                }

            }
        }

        private async void TOP10Button_Clicked(object sender, EventArgs e)
        {
            string result;
            string content;
            var encodedUrl = System.Net.WebUtility.UrlEncode(Encrypturl.Text);
            var Web2Stringurl = "http://10.0.2.2:5000/Web2String?url=" + encodedUrl;
            var top10URL = "http://10.0.2.2:5000/Top10Words";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.ConnectionClose = true;
                if (TOP10ResultFrame.IsVisible == false)
                {
                    try
                    {
                        var response = await client.GetAsync(Web2Stringurl);
                        response.EnsureSuccessStatusCode();
                        content = await response.Content.ReadAsStringAsync();

                        var response2 = await client.PostAsync(top10URL, new StringContent(content));
                        response2.EnsureSuccessStatusCode();
                        result = await response2.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        result = "Error: " + ex.Message;
                    }

                    TOP10ResultLabel.Text = result;
                    TOP10Button.Text = "Hide Content";
                    TOP10ResultFrame.IsVisible = true;
                }

                else
                {
                    TOP10ResultFrame.IsVisible = false;
                    TOP10Button.Text = "TOP 10";
                }

            }
        }
        private async void Fetch3Button_Clicked(object sender, EventArgs e)
        {
            //定義變數
            var initialUrl = Encrypturl.Text;
            var Web2StringURL = "http://10.0.2.2:5000/Web2String?url=";
            var FilterURL = "http://10.0.2.2:5000//WordFilter";
            var urlsList = new List<string>();
            urlsList.Add(initialUrl);




            using (var client = new HttpClient())
            {
                //初始網頁內容
                var webContent = await client.GetAsync(Web2StringURL + initialUrl);
                webContent.EnsureSuccessStatusCode();
                string content = await webContent.Content.ReadAsStringAsync();

                //網頁內容取出另外2個href.text
                var doc = new HtmlDocument();
                doc.LoadHtml(content);
                var links = doc.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    int i = 0;
                    foreach (var link in links)
                    {
                        if (i < 2)
                        {
                            var hrefValue = link.GetAttributeValue("href", string.Empty);
                            //將網址加入List
                            urlsList.Add(hrefValue);
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //迴圈取出網址過濾內容
                    foreach (var url in urlsList)
                    {
                        //取出網址內容
                        var webContent2 = await client.GetAsync(Web2StringURL + url);
                        webContent2.EnsureSuccessStatusCode();
                        string content2 = await webContent2.Content.ReadAsStringAsync();
                        //過濾網址內容
                        var response = await client.PostAsync(FilterURL, new StringContent(content2));
                        response.EnsureSuccessStatusCode();
                        string result = await response.Content.ReadAsStringAsync();

                        if (!GlobalResultDictionary.ContainsKey(url))
                        {
                            GlobalResultDictionary.Add(url, result);
                        }
                    }
                    Fetch3URLButton.IsVisible = false;
                }




            }
        }

        private async void TFIDFButton_Clicked(object sender, EventArgs e)
        {
            if (TFIDFResultFrame.IsVisible == false)
            {
                //設定變數
                string top10URL = "http://10.0.2.2:5000/Top10Words";
                string fullContent = "";
                string result;

                //從字典取出內文合併成一篇
                foreach (string key in GlobalResultDictionary.Keys)
                {
                    fullContent += GlobalResultDictionary[key];
                }

                //計算top10
                using (var client = new HttpClient())
                {
                    var response2 = await client.PostAsync(top10URL, new StringContent(fullContent));
                    response2.EnsureSuccessStatusCode();
                    result = await response2.Content.ReadAsStringAsync();
                }

                //解開json格式
                List<string> topWordsWithCounts = JsonSerializer.Deserialize<List<string>>(result);
                //將top10的字詞和數量取出取出
                List<string> topWords = new List<string>();
                List<int> topWordsNumber = new List<int>();
                foreach (var wordWithCount in topWordsWithCounts)
                {
                    var parts = wordWithCount.Split(':');
                    if (parts.Length == 2)
                    {
                        topWords.Add(parts[0]);
                        topWordsNumber.Add(int.Parse(parts[1]));
                    }
                }


                //迴圈計算top10的字詞在三個文檔出現的字頻  tf
                var topWordsCountInDoc = new Dictionary<string, int[]>();
                for (int i = 0; i < topWords.Count; i++)
                {
                    List<int> wordIndocList = new List<int>();
                    foreach (var key in GlobalResultDictionary.Keys)
                    {
                        int number = GlobalResultDictionary[key]
                            .ToLower() // 確保不區分大小寫
                            .Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Count(w => w.Equals(topWords[i].ToLower())); // 確保比較不區分大小寫
                        wordIndocList.Add(number);
                    }
                    topWordsCountInDoc.Add(topWords[i], wordIndocList.ToArray());
                }

                //比較top10在三個文檔出現的次數 一個文檔出現一次加一 三個文檔最多三次 
                var topWordsCount = new Dictionary<string, int>();
                foreach (var word in topWords)
                {
                    int count = 0;
                    foreach (var key in GlobalResultDictionary.Keys)
                    {
                        if (GlobalResultDictionary[key].Contains(word))
                        {
                            count++;
                        }
                    }
                    topWordsCount[word] = count;
                }
                //計算tf-idf
                string results = "Word : d1, d2, d3 \n";
                int docCount = GlobalResultDictionary.Keys.Count; // 文档总数

                for (int i = 0; i < topWords.Count; i++)
                {
                    List<double> tfidfValues = new List<double>(); // 为每个词创建新的列表

                    // 计算IDF (log(总文档数/出现该词的文档数))
                    double idf = Math.Log((double)docCount / topWordsCount[topWords[i]], 2);

                    for (int j = 0; j < docCount; j++)
                    {
                        // TF是词在文档中的出现次数
                        double tf = topWordsCountInDoc[topWords[i]][j];
                        double tfIdf = tf * idf;
                        tfidfValues.Add(Math.Round(tfIdf, 4)); // 四舍五入到4位小数
                    }

                    results += topWords[i] + " : " + string.Join(", ", tfidfValues) + "\n";
                }
                TFIDFResultLabel.Text = results;
                TFIDFButton.Text = "Hide Content";
                TFIDFResultFrame.IsVisible = true;
            }
            else
            {
                TFIDFResultFrame.IsVisible = false;
                TFIDFButton.Text = "Calculate the TF-IDF of top 10 words in the three documents";











            }
        }
    }
}
