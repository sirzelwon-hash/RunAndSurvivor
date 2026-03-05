using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerFollow : MonoBehaviour
{
    Vector3 diff;

    public GameObject target;
    public float followSpeed = 5.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        diff = target.transform.position - transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Lerpによる補間関数　第一引数→第二引数へ、第三引数は進捗率
        transform.position = Vector3.Lerp(
            transform.position,
            target.transform.position - diff,
            Time.deltaTime * followSpeed);
    }
}
