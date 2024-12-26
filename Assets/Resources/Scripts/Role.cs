using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Role : MonoBehaviour
{
    public RoleType roleType;
    //寻路组件
    private NavMeshAgent agent;
    private GameObject FootEffect;
    private Animator ani;
   
    void Start()
    {
        // 获取NavMeshAgent组件
         agent = GetComponent<NavMeshAgent>() ?? this.gameObject.AddComponent<NavMeshAgent>();
        //获得脚底特效
        FootEffect = gameObject.transform.Find("FootEffect").gameObject;
        Is_HideFootEffect(true);
        //获得动画组件
        ani = GetComponentInChildren<Animator>() ?? this.gameObject.AddComponent<Animator>();
        ani.speed = agent.speed * 0.5f;
    }
    // Update is called once per frame
    void Update()
    {
        //角色移动动画
        ani.SetBool("Is_Move",agent.velocity != Vector3.zero);
        
    }
    /// <summary>
    /// 是否被选中
    /// </summary>
    /// <param name="ck"></param>
    public void Is_HideFootEffect(bool ck)
    {
        FootEffect.SetActive(!ck);
    }
    /// <summary>
    /// 设置需要前往的目标点
    /// </summary>
    /// <param 目标点="pos"></param>
    public void SetDestnation(Vector3 pos)
    {
          agent.SetDestination(pos);
    }
   
}
