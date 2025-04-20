from flask import Flask, request, jsonify
import requests
from bs4 import BeautifulSoup
import os

app = Flask(__name__)

# 停用詞檔案名稱
STOPWORDS_FILE = "stopwords_from_nltk.txt"

def get_web_content(url):
    response = requests.get(url)
    response.encoding = 'utf-8'
    return response.text

def word_filter(html_content):
    # 1. 讀取並處理停用詞
    with open(STOPWORDS_FILE, "r", encoding="utf-8") as f:
        raws = f.read()
    stopwords = [
        raw.strip()[1:-1].lower() 
        for raw in raws.split(",") 
        if len(raw.strip()) >= 2
    ]

    # 2. 擷取 HTML 文字
    soup = BeautifulSoup(html_content, 'html.parser')
    text = soup.get_text().lower()

    # 3. 只保留字母、數字、空格和撇號
    clean_text = ''.join(
        ch if (ch.isalnum() or ch.isspace() or ch == "'") else " " 
        for ch in text
    )

    # 4. 分詞並過濾
    words = clean_text.split()
    filtered = [w for w in words if w not in stopwords]

    # 5. 重組並回傳
    return " ".join(filtered)

def top_10_words(content):
    filtered = word_filter(content)

    word_count = {}
    for word in filtered.split():
        cleaned_word = word.strip().lower()
        if cleaned_word:
            word_count[cleaned_word] = word_count.get(cleaned_word, 0) + 1

    # 取前 10 名
    top10 = sorted(word_count.items(), key=lambda x: x[1], reverse=True)[:10]

    # 回傳格式：字串陣列 [word1, count1, word2, count2, ...]
    result = []
    for word, count in top10:
        result.append(f"{word}:{str(count)}")
    return result

@app.route('/Web2String', methods=['GET'])
def web2string():
    url = request.args.get('url')
    try:
        content = get_web_content(url)
        return content
    except Exception as e:
        return str(e), 500

@app.route('/WordFilter', methods=['POST'])
def filter_words():
    try:
        html_content = request.get_data(as_text=True)
        filtered = word_filter(html_content)
        return filtered
    except Exception as e:
        return str(e), 500

@app.route('/Top10Words', methods=['POST'])
def top10words():
    try:
        html_content = request.get_data(as_text=True)
        result = top_10_words(html_content)
        return jsonify(result)
    except Exception as e:
        return str(e), 500

if __name__ == '__main__':
    app.run(port=5000)
