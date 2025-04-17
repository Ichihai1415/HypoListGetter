# HypoListGetter

[気象庁震源リスト](https://www.data.jma.go.jp/eqev/data/daily_map/index.html)のデータを震度データベース形式(震度は---)、独自形式(ほぼ変わらない)に変換し保存します。[JeqDB-Converter](https://github.com/Ichihai1415/JeqDB-Converter)にあるコードを調整したものです。

([Data/HypoList](https://github.com/Ichihai1415/Data/tree/release/HypoList)) に出力データがあります。`eqdb`が震度データベース形式、`original`が独自形式です。

# 更新履歴

## v1.0.1
2025/04/18

- 緯度経度の分の小数が四捨五入されていたので修正
- .NET9化 x64構成追加
- データを削除(別レポジトリを参照)

## v1.0.0
2024/11/25

- 初回公開
- 2024/11/23までのデータ追加
