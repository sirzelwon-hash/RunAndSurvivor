using NUnit.Framework;
using System.Collections.Concurrent;
using UnityEngine;
using System.Collections.Generic;

public class StageGenerator : MonoBehaviour
{

    const int StageChipSize = 30;
    int currentChipIndex;

    public Transform character; //プレイヤー
    public GameObject[] stageChips;　//生成されるステージのカタログ
    public int startChipIndex = 1; //最初のステージ番号
    public int preInstantiate = 5;　//どこまで先のステージを用意しておくか
    public List<GameObject> generatedStageList = new List<GameObject>(); //現在ヒエラルキーにあるステージ情報を生成順に取得


    void Start()
    {
        //初期の現在番号を定めている
        currentChipIndex = startChipIndex - 1;
        //初期の状態からまずはいくつかステージを生成
        UpdateStage(preInstantiate);
    }


    void Update()
    {
        //キャラクターの位置から現在のステージチップのインデックスを計算
        //キャラがどのステージのIndex番号にいるのかを常に把握
        int charaPositionIndex = (int)(character.position.z / StageChipSize);

        //次のステージチップに入ったらステージの更新処理を行う
        //キャラのいる位置＋5個先が作成済のステージ番号を上回ってしまったら
        if (charaPositionIndex + preInstantiate > currentChipIndex)
        {
            //不足分となってしるステージ番号を引数指定してステージ生成
            UpdateStage(charaPositionIndex + preInstantiate);
        }
    }

    //ステージ生成＆古いステージ廃棄
    void UpdateStage(int toChipIndex)
    {
        //作りたい番号（引数）より自分の番号の方が大きければ何もしない
        if (toChipIndex <= currentChipIndex) return;

        //指定のステージチップまでを生成
        for (int i = currentChipIndex + 1; i <= toChipIndex; i++)
        {
            //戻り値GameObjectがGenerateStageメソッドで帰ってくるので変数stageObjectに格納
            GameObject stageObject = GenerateStage(i);

            //確保したstageObject情報をリストに加える
            generatedStageList.Add(stageObject);
        }

        //ステージ保持上限内になるまで古いステージを削除
        //リストの数が8になったら廃棄開始
        while (generatedStageList.Count > preInstantiate + 2) DestroyOldestStage();　//廃棄
        currentChipIndex = toChipIndex;　//現在番号を更新
    }

    //指定のインデックス位置にstageオブジェクトをランダムに生成
    GameObject GenerateStage(int chipIndex)
    {
        int nextStageChip = Random.Range(0, stageChips.Length);

        GameObject stageObject = (GameObject)Instantiate(
            stageChips[nextStageChip],
            new Vector3(0, 0, chipIndex * StageChipSize),　//StageShipSizeは30。座標割り出す
            Quaternion.identity);

        return stageObject;
    }

    //1番古いステージを削除
    void DestroyOldestStage()
    {
        GameObject oldStage = generatedStageList[0];　//先頭0番の情報を取得
        generatedStageList.RemoveAt(0);　//先頭の情報を帳簿から抹消
        Destroy(oldStage); //ヒエラルキーからも対象ステージを抹消
    }
}
