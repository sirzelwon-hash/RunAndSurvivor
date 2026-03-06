using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NejikoController : MonoBehaviour
{

    CharacterController controller;
    //Animator animator;

    Vector3 moveDirection = Vector3.zero; //移動するべき量 new Vector3(0,0,0)の意

    public float gravity; //重力加速度
    public float speedZ; //前進する力
    public float speedJump; //ジャンプ力

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //必要なコンポーネントを自動取得
        controller = GetComponent<CharacterController>();
        //Animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Update動作中");
        //Debug.Log(Input.GetAxis("Vertical"));
        //Debug.Log(controller.isGrounded);

        if (controller.isGrounded) //characterCotrollerコンポーネントが持っている接地のチェック（bool）
        {
            //垂直方向のボタン入力をチェック（Vertical）
            if (Input.GetAxis("Vertical") > 0.0f)
            {
                //このフレームにおける前進交代の移動量が決まる
                moveDirection.z = Input.GetAxis("Vertical") * speedZ;
            }
            else
            {
                moveDirection.z = 0;
            }

            //左右キーを押したときの回転
            transform.Rotate(0, Input.GetAxis("Horizontal") * 3, 0);

            if (Input.GetButton("Jump"))　//スペースキー押下時
            {
                moveDirection.y = speedJump;
                //Animator.SetTrigger("jump");
            }

            //ここまででそのフレームの移動するべき量が決まる(moveDirectionのxとy)

            //重力分の力を毎フレーム追加
            moveDirection.y -= gravity * Time.deltaTime;

            //引数に与えたVector3値を、そのオブジェクトの向きに合わせてグローバルな値としては何が正しいかに変換
            Vector3 globalDirection = transform.TransformDirection(moveDirection);

            //Moveメソッドに与えたVector3値分だけ実際にPlayerが動く
            controller.Move(globalDirection * Time.deltaTime);

            //移動後接地していたらY方向の速度はリセットする
            if (controller.isGrounded) moveDirection.y = 0;

            //速度が0以上なら走っているフラグをtrueにする
            //Animator.SetBool("run", moveDirection.z > 0.0f);

        }

    }
}
