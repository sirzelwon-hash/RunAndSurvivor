using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRun : MonoBehaviour

{
    //横移動の軸の限界
    const int MinLane = -2;
    const int MaxLane = 2;
    const float LaneWidth = 1.0f;

    //体力の最大値
    const int DefaultLife = 3;

    //ダメージを食らった時の硬直時間
    const float StunDuration = 0.5f;

    CharacterController controller;
    Animator animator;

    Vector3 moveDirection = Vector3.zero;　//移動すべき量
    int targetLane;　//向かうべきX座標
    int life = DefaultLife;　//現在の体力
    float recoverTime = 0.0f;　//復帰までの時間カウントダウン

    float currentMoveInputX; //InputSystemの入力値を格納予定
    Coroutine resetIntervalCol; //Inputを連続で認知しないためのインターバルのコルーチン

    public float gravity = 20.0f;　//重力加速地
    public float speedZ = 5.0f;　//前進スピード
    public float speedX = 3.0f;　//横移動スピード
    public float speedJump = 8.0f;　//ジャンプ力
    public float accelerationZ = 10.0f;　//前進加速力

    [Header("ソードのスクリプト")]
    public NormalSword normalSword;


    void OnMove(InputValue value)
    {
        if (normalSword.GetIsSword()) return; //NormalSwordスクリプトのisSword変数を見て攻撃中なら何もできない

        //すでに前に入力検知してインターバル宙であれば何もしない
        if (resetIntervalCol == null)
        {
            //検知した値(value)をVector2で表現して変数inputVectorに格納
            Vector2 inputVector = value.Get<Vector2>();
            //変数inputVectorのうち、x座標にまつわる値を変数currentMoveInputXに格納
            currentMoveInputX = inputVector.x;
        }
    }

    void OnJump(InputValue Value)
    {
        if (normalSword.GetIsSword()) return; //NormalSwordスクリプトのisSword変数を見て攻撃中なら何もできない

        //ジャンプに関するボタン検知をしたらジャンプメソッド
        Jump();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public int Life() //現在の体力を返す
    {
        return life;
    }

    public void LifeUP() //体力を1回復
    {
        life++;
        if (life > DefaultLife) life = DefaultLife;
        GameObject canvas = GameObject.FindGameObjectWithTag("UI"); //UIタグの検索
        canvas.GetComponent<UIController>().UpdateLife(Life()); //UIの更新
    }

    //ダメージによる体力の減少
    public void LifeDown()
    {
        life--;
        if (life > DefaultLife) life = DefaultLife;
        GameObject canvas = GameObject.FindGameObjectWithTag("UI"); //UIタグの検索
        canvas.GetComponent<UIController>().UpdateLife(Life()); //UIの更新
    }


    bool IsStun()　//Playerを硬直させるべきかチェックするメソッド
    {
        return recoverTime > 0 || life <= 0;
    }

    //Update is called once per frame
    void Update()
    {
        //Debug.Log(life);

        //ステージクリアかリザルト中はもう動けない
        if (GameManager.gameState == GameState.stageclear || GameManager.gameState == GameState.result) return;

        //    if (Input.GetKeyDown("left")) MoveToLeft;
        //    if (Input.GetKeyDown("right")) MoveToRight;
        //    if (Input.GetKeyDown("space")) Jump;

        //    float acceleratedZ = moveDirection Direction.z + (acceleratedZ * Time.deltaTime);
        //    moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

        //    float ratioX = (targetLane * LaneWidth - transform.position.x) / LaneWidth;
        //    moveDirection.x = rationX * speedX;

        //左を押されていたら
        if (currentMoveInputX < 0)
        {
            MoveToLeft();
        }
        //右を押されていたら
        if (currentMoveInputX > 0)
        {
            MoveToRight();
        }

        if (IsStun()) //硬直フラグをチェック
        {
            moveDirection.x = 0; //moveDirectionのxを0
            moveDirection.y = 0; //moveDirectionのyを0
            recoverTime -= Time.deltaTime; //硬直時間のカウントダウン。Time.deltaTime→1フレームにかかった時間
        }
        else
        {
            //前進のアルゴリズム
            //そのときのmoveDirevtion.zにaccelerationZの加速度を足していく
            float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
            //導き出した値に上限を設けてそれをmoveDirectionZとする
            moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

            //横移動のアルゴリズム
            //目的地への距離の差によって速さが変わる。初速が早くて目的地が近いほど弱まる（遅くなる）
            //目的地と自分の位置の差を取り、1レーンあたりの幅に対して割合を見る
            float ratioX = (targetLane * LaneWidth - transform.position.x) / LaneWidth;
            //割合に変数speedXを係数としてかけた値がmoveDirection.x
            moveDirection.x = ratioX * speedX;
        }

        //重力の加速度をmoveDirection.yに授ける
        moveDirection.y -= gravity * Time.deltaTime;
        //回転時、自分にとってのZ軸をグローバル座標の値に変換
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        //CharactorcontrollerコンポーネントのMoveメソッドに授けてPlayerを動かす
        controller.Move(moveDirection * Time.deltaTime);

        //地面についていたら重力をリセット
        if (controller.isGrounded) moveDirection.y = 0;

    }


    public void MoveToLeft()
    {
        //硬直フラグがtrueなら何もしない
        if (IsStun())
        {
            return;
        }
        //接地している、且つ ターゲットレーンが最小じゃないとき
        if (controller.isGrounded && targetLane > MinLane)
        {
            targetLane--;  //もっと低いレーンに行く
            currentMoveInputX = 0; //何も入力していない状況にリセット
            resetIntervalCol = StartCoroutine(ResetIntervalCol()); //次の入力検知を有効にするまでのインターバル
        }
    }

    public void MoveToRight()
    {
        //硬直フラグがtrueなら何もしない
        if (IsStun())
        {
            return;
        }
        //接地している、且つ ターゲットレーンが最大じゃないとき
        if (controller.isGrounded && targetLane < MaxLane)
        {
            targetLane++;  //もっと高いレーンに行く
            currentMoveInputX = 0; //何も入力していない状況にリセット
            resetIntervalCol = StartCoroutine(ResetIntervalCol()); //次の入力検知を有効にするまでのインターバル
        }
    }

    IEnumerator ResetIntervalCol()
    {
        yield return new WaitForSeconds(0.1f);　//とりあえず0.1秒待つ
        resetIntervalCol = null; //コルーチン情報を解除
    }

    public void Jump()
    {
        if (IsStun()) //硬直フラグがtrueなら何もしない
        {
            return;
        }
        if (controller.isGrounded) //地面にいたら
        {
            moveDirection.y = speedJump;
        }
    }

    //CharactorControllerコンポーネントが何かとぶつかった時
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (IsStun()) return;

        //相手がEnemyだったら
        if (hit.gameObject.tag == "Enemy")
        {
            LifeDown(); //体力が減る
            GetComponent<NormalShooter>().ShootPowerDown();
            recoverTime = StunDuration;　//定数の値にrecoverTimeにセッティング

            if (life <= 0) GameManager.gameState = GameState.gameover; //体力がなくなったらゲームオーバー

            //Destroy(hit.gameObject); //相手は消滅

            //相手のエフェクト発生と消滅を発動
            hit.gameObject.GetComponent<Wall>().CreateEffect();
        }
    }

    //ゴールに触れたらステータスをゲームクリアに変更
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Goal")
        {
            GameManager.gameState = GameState.stageclear;
        }
    }
}
