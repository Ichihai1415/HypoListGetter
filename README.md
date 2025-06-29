# HypoListGetter

[気象庁震源リスト](https://www.data.jma.go.jp/eqev/data/daily_map/index.html)のデータを震度データベース形式(震度は---)に変換し保存します。数字で0[JeqDB-Converter](https://github.com/Ichihai1415/JeqDB-Converter)にあるコードを調整したものです。

([Data/HypoList](https://github.com/Ichihai1415/Data/tree/release/HypoList)) に出力データがあります。

# 更新履歴

### v1.0.3

2025/06/29

- 時刻の0合わせの置換ミスを修正
- README、LICENSEのビルド時コピー追加

## v1.0.2

2025/06/29

- マグニチュード不明、マイナスの場合の値を修正
- 独自形式廃止
- 最終取得から最終取得+1日に変更
- 処理方法変更

## v1.0.1

2025/04/18

- 緯度経度の分の小数が四捨五入されていたので修正
- .NET9化 x64構成追加
- データを削除(別レポジトリを参照)

## v1.0.0
2024/11/25

- 初回公開
- 2024/11/23までのデータ追加
