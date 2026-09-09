# ノード

YMM4（ゆっくりMovieMaker4）向けの、ノードベース画像処理映像エフェクトプラグインです。
<img width="1399" height="434" alt="image" src="https://github.com/user-attachments/assets/1c164f22-472f-4c42-b66a-bb4bb2d7600a" />

## 概要

映像エフェクト「ノード」を追加するプラグインです。ノードを接続することで、画像をその接続順に連鎖的に加工することができます。

ノードには「端子」と呼ばれる小さな丸が端に付いています。左側が「入力端子」、右側が「出力端子」です。ある端子から接続線を延ばして別のノードの端子につなぐと、出力から入力へデータが送られます。入力端子に何も接続せず、その端子の値を直接設定することができる端子もあります。多数のノードを組み合わせることで、YMM4単体では実現できない加工を行えます。

## インストール

最新リリースの`.ymme`ファイルを取得し、実行してください。

## 使い方

- ノードの追加：編集画面上で右クリックすると、検索ボックス付きのノード追加メニューが開きます。ノード名を検索するか、カテゴリから選んで追加します。
- ノードの削除：ノードを選択し、Deleteキーを押します。
- コピー・貼り付け：選択したノードをCtrl+Cでコピーし、Ctrl+Vで貼り付けます。
- 接続：出力端子から入力端子へ接続線をドラッグして繋ぎます。

## 主なノードのカテゴリ

- 合成（Composition）：アルファ合成、ブレンド、マスクなど
- 変形（Transform）
- YMM4の映像エフェクト（別途追加されたプラグインにも対応しています。）（一部正常に動作しないものがあります。）
- 生成（Generator）
  - ブラシ
  - 画像ソース
  - アウトライン（生成・変形・結合・ラスタライズ）
- 関数（Func）：サブグラフの引数・戻り値
- 数値計算（MathCalc）：基本演算、イージング、乱数、関数
- 値（Value）：色、文字列、リテラル

## ローカライズ

- 日本語（ja-jp）
- アラビア語（ar-sa）
- 英語（en-us）
- スペイン語（es-es）
- インドネシア語（id-id）
- 韓国語（ko-kr）
- 簡体字中国語（zh-cn）
- 繁体字中国語（zh-tw）

## バグ報告

バグを発見した場合、発生条件（直前の操作内容）を明記した上でIssueを立ててください。

---

## 開発者向け情報

### リポジトリ構成

- `Node/` : プラグイン本体（C#、WPF、.NET 10）
- `Node.Shader/` : マスク処理用HLSLシェーダー（Direct2Dカスタムエフェクトとしてコンパイルし、`.cso`として`Release/`に同梱）
- `YukkuriMovieMaker.Generator/` : ローカライズ用ソースジェネレータ。Gitサブモジュール（`manju-summoner/YukkuriMovieMaker.Generator`）

### `Node/`以下の主なモジュール

- `Graph/` : ノードグラフのコアモデル。`Executor`、`NodeGraph`、`Port`関連クラス、`Snapshot`（保存・復元用シリアライズ）
- `Nodes/` : 各ノード実装（`Effect/Composition`、`Effect/Transform`、`Effect/DynamicLoaded`、`Generator/Brush`、`Generator/Image`、`Generator/Outline`、`Func`、`MathCalc`、`Value`）
- `Editor/` : ノードエディタUI（WPF、MVVM）。ポートコントロール（Bool、Color、Enum、Number、Text、Bezier）、ノード・接続線の描画、追加メニュー（`AddNodePopup`）、選択・ドラッグ処理
- `ValueTypes/` : ノード間でやり取りする値のラッパー（Image、Brush、Mask、Outline）
- `Utility/` : エフェクト・画像のロード処理
- `Localize/` : 多言語リソース

### ビルド手順

1. サブモジュールを含めてクローンする（`git clone --recurse-submodules`、または既存クローンに対し`git submodule update --init`）。
2. `Directory.Build.props`の`YMM4DirPath`を、実際のYMM4インストール先に合わせて設定する。
3. `Node.sln`をビルドする。`YMM4Proj`プロパティが未設定の場合、ビルド後に`PostBuild`ターゲットが実行され、成果物が`%YMM4DirPath%\user\plugin\node\`へコピーされる。
4. `Node.Shader`はC++（`vcxproj`）プロジェクトであり、HLSLをコンパイルして`.cso`を生成する。

### ライセンス

`LICENSE`ファイルにUnlicense（パブリックドメイン相当）の全文が記載されている。
