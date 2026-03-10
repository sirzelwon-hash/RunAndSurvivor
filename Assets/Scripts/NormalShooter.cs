using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NormalShooter : MonoBehaviour
{
    [Header("Bullet管理スクリプトと連携")]
    public BulletManager bulletManager;

    [Header("生成オブジェクトと位置")]
    public GameObject bulletPrefabs;//生成対象プレハブ
    public GameObject gate; //生成位置

    [Header("弾速")]
    public float shootSpeed = 10.0f; //弾速

    GameObject bullets; //生成した弾をまとめるオブジェクト

    const int maxShootPower = 3; //最大威力
    int shootPower; //現在威力

    [Header("ソードのスクリプト")]
    public NormalSword normalSword; //ソード中の動きを封じるため


    //InputAction(Playerマップ)のAttackアクションがおされたら
    void OnAttack(InputValue value)
    {
        if (normalSword.GetIsSword()) return; //ソード中なら何もできない

        //リトライ状態の時ならアクションボタンで先のシーンへ
        if (GameManager.gameState == GameState.retry)
        {
            GameManager.RetryScene();　//staticメソッドなので簡単に呼び出し
        }
        else if (GameManager.gameState == GameState.result)　//リザルト状態ならアクションボタンでネクストシーンへ
        {
            //行き先が自由に記述できるようpublic変数を使っているのでNextSceneはstaticメソッドにできず地道に呼び出し
            GameManager gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
            gm.NextScene(gm.nextScene);
        }
        else //ゲームステータスがプレイ中なら
        {
            Shoot();
        }

    }

    void Shoot()
    {
        if (bulletManager.GetBulletRemaining() > 0)
        {
            //プレハブの生成と生成情報の取得
            GameObject obj = Instantiate(
                bulletPrefabs,
                gate.transform.position,
                Quaternion.Euler(90, 0, 0)
                );

            //生成したBulletをBulletsオブジェクトの子供にしてまとめる
            obj.transform.parent = bullets.transform;

            //bulletを消費
            bulletManager.ConsumeBullet();

            Rigidbody bulletRbody = obj.GetComponent<Rigidbody>();
            bulletRbody.AddForce(new Vector3(0, 0, shootSpeed), ForceMode.Impulse);
        }
        else
        {
            //残数がなければマガジンを消費して補充開始
            bulletManager.RecoverBullet();
        }
    }

    void Start()
    {
        //指定したタグを持っているオブジェクトを取得
        bullets = GameObject.FindGameObjectWithTag("Bullets");
    }

    public void ShootPowerUp()
    {
        shootPower++; //威力を上げる
        if (shootPower > maxShootPower) shootPower = maxShootPower; //最大威力までに抑える
        GameObject canvas = GameObject.FindGameObjectWithTag("UI");
        canvas.GetComponent<UIController>().UpdateGun(); //UIを更新
    }

    public void ShootPowerDown()
    {
        shootPower--; //威力を下げる
        if (shootPower <= 0) shootPower = 1; //最小威力1に抑える
        GameObject canvas = GameObject.FindGameObjectWithTag("UI");
        canvas.GetComponent<UIController>().UpdateGun(); //UIを更新
    }

    //現在の威力の取得
    public int GetShootPower()
    {
        return shootPower;
    }
}
