using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Role : MonoBehaviour
{
    public RoleType roleType;
    //Ѱ·���
    private NavMeshAgent agent;
    private GameObject FootEffect;
    private Animator ani;
   
    void Start()
    {
        // ��ȡNavMeshAgent���
         agent = GetComponent<NavMeshAgent>() ?? this.gameObject.AddComponent<NavMeshAgent>();
        //��ýŵ���Ч
        FootEffect = gameObject.transform.Find("FootEffect").gameObject;
        Is_HideFootEffect(true);
        //��ö������
        ani = GetComponentInChildren<Animator>() ?? this.gameObject.AddComponent<Animator>();
        ani.speed = agent.speed * 0.5f;
    }
    // Update is called once per frame
    void Update()
    {
        //��ɫ�ƶ�����
        ani.SetBool("Is_Move",agent.velocity != Vector3.zero);
        
    }
    /// <summary>
    /// �Ƿ�ѡ��
    /// </summary>
    /// <param name="ck"></param>
    public void Is_HideFootEffect(bool ck)
    {
        FootEffect.SetActive(!ck);
    }
    /// <summary>
    /// ������Ҫǰ����Ŀ���
    /// </summary>
    /// <param Ŀ���="pos"></param>
    public void SetDestnation(Vector3 pos)
    {
          agent.SetDestination(pos);
    }
   
}
