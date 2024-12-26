using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
public enum RoleType
{
    Hero,
    Grunt,
    Headhunter,
    Witchdoctor,
    Wyvernrider
}
public class Control : MonoBehaviour
{
    public float dis = 3;
    //�õ��������
    private LineRenderer lineRenderer;
    private bool isCheck = false;
    private Dictionary<int, List<Vector3>> Destnations = new Dictionary<int,List<Vector3>>();
    //�м����
    Vector3 Mouse_pos;
    Vector3 Start_pos;
    Vector3 End_pos;
    Vector3 Box_Forward;
    Vector3[] pos = new Vector3[4];
    RaycastHit hitInfo;
    Collider[] colliders;
    Role obj_role;
    List<Role> obj_roles = new List<Role>();
    List<Vector3> Destnation = new List<Vector3>();
    void Start()
    {
        Init_lineRenderer();
    }
    /// <summary>
    /// ��ʼ��linelineRenderer
    /// </summary>
    private void Init_lineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>() ?? this.gameObject.AddComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;

    }
    private void Update()
    {
        //ѡ�����

        Check_Obj();
        //����Ŀ���
        SetDestnaionObjs();
    }
    /// <summary>
    /// �õ�������ĵ�������
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMouseWorldPos()
    {
        //�������Ļ��λ�ã������������淢�����ߣ��õ�������ĵ�������
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000, 1 << LayerMask.NameToLayer("Ground")))
            return hitInfo.point;
        return Vector3.zero;
    }
    /// <summary>
    /// ѡ��ʿ��
    /// </summary>
    public void Check_Obj()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Cancel_SelectObj();
            Start_pos = GetMouseWorldPos();
            isCheck = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isCheck = false;
            lineRenderer.positionCount = 0;
            End_pos = GetMouseWorldPos();
            
     
        }
        if (isCheck)
        {
            Cancel_SelectObj();
            Select_obj(Start_pos, GetMouseWorldPos());
        }

    }
    /// <summary>
    /// ѡ�������ѡ�����
    /// </summary>
    /// <param name="Start"></param>
    /// <param name="End"></param>
    public void Select_obj(Vector3 Start, Vector3 End)
    {
        //�����ĸ��������
        pos[0] = Start;
        pos[1] = new Vector3(End.x, Start.y, Start.z);
        pos[2] = End;
        pos[3] = new Vector3(Start.x, Start.y, End.z);
        //������
        lineRenderer.positionCount = 4;
        lineRenderer.SetPositions(pos);
        colliders = Physics.OverlapBox(Start + (End - Start) / 2, new Vector3(Mathf.Abs(End.x - Start.x) / 2, 1, Mathf.Abs(End.z - Start.z) / 2), Quaternion.identity, 1 << LayerMask.NameToLayer("My_Role"));
        foreach (var item in colliders)
        {
            Debug.Log(item.gameObject.name);
            obj_role = item.gameObject.GetComponent<Role>();
            obj_role.Is_HideFootEffect(false);
            obj_roles.Add(obj_role);
        }
    }
    /// <summary>
    /// ���ѡ��Ķ���
    /// </summary>
    public void Cancel_SelectObj()
    {
        foreach (var item in obj_roles)
        {
            item.Is_HideFootEffect(true);
        }
        obj_roles.Clear();
    }
    /// <summary>
    /// Ϊѡ�еĶ�������Ŀ���
    /// </summary>
    public void SetDestnaionObjs()
    {
        if (obj_roles.Count == 0) return;
        if (Input.GetMouseButtonDown(1)) {
            Mouse_pos = GetMouseWorldPos();
            CreatDestnation();
            Sort_Objs(Mouse_pos);
            Box_Forward = Mouse_pos - (Start_pos + (End_pos - Start_pos) / 2.0f);
            for (int i = 0; i < obj_roles.Count; i++)
            {
                
                obj_roles[i].SetDestnation(Quaternion.Euler(0,-(Mathf.Atan2(Box_Forward.z, Box_Forward.x) * Mathf.Rad2Deg - 90),0) * Destnation[i] + Mouse_pos);
            }

        }
    }
    /// <summary>
    /// Ϊ������価�������Ŀ���
    /// </summary>
    public void Sort_Objs(Vector3 pos)
    {
        obj_roles.Sort((a,b) =>
        {
            if (a.roleType > b.roleType) return 1;
            else if (a.roleType == b.roleType)
            {
                if (Vector3.Distance(a.transform.position, pos) <= Vector3.Distance(b.transform.position, pos))
                    return -1;
                else
                    return 1;
            }
            else
                return -1;
            


        });

    }
    /// <summary>
    /// �õ�ѡ�еĽ�ɫ��Ϊ������
    /// </summary>
    /// <returns></returns>
    public int Get_row()
    {
        if ((int)Mathf.Sqrt(obj_roles.Count) * (int)Mathf.Sqrt(obj_roles.Count) == obj_roles.Count)
            return (int)Mathf.Sqrt(obj_roles.Count);
        return Mathf.CeilToInt(Mathf.Sqrt(obj_roles.Count));

    }
    /// <summary>
    /// ����Ŀ���
    /// </summary>
    /// <param ����Ŀ��������="pos"></param>
    public void CreatDestnation()
    {
       
        if (Destnations.ContainsKey(obj_roles.Count))
        {
            Destnation = Destnations[obj_roles.Count];
            return;
        }
        if(Destnations.ContainsValue(Destnation))
             Destnation = new List<Vector3>();
        if (obj_roles.Count <= 3)
        {
            CreatSingleDestnation(Vector3.zero, obj_roles.Count);
            Destnations.Add(obj_roles.Count,new List<Vector3>(Destnation));
            return;
        }
        int row = Get_row();
        int excess_col = obj_roles.Count % row;
        int col = obj_roles.Count / row + (excess_col == 0 ? 0 : 1);
        float x =  - (row-1) / 2 * dis - (row % 2 == 0 ? dis / 2 : 0);
        float y = 0;
        float z =   (col-1)/2 * dis + (col % 2 == 0 ? dis / 2 : 0);
        for (int i = 0; i < obj_roles.Count - excess_col; i++)
        {
           Destnation.Add(new Vector3(x + i % row * dis, y, z - i / row * dis));
        }
        if(excess_col > 0)
        {
            CreatSingleDestnation(new Vector3(x,y, - z),new Vector3(x + (row - 1) * dis,y, - z),excess_col);
        }
        Destnations.Add(obj_roles.Count, new List<Vector3>(Destnation));


    }
    /// <summary>
    /// ����һ�е�����
    /// </summary>
    /// <param һ�е����="start"></param>
    /// <param һ�е��յ�="end"></param>
    /// <param ��һ�е�����="cnt"></param>
    public void CreatSingleDestnation(Vector3 start, Vector3 end, int cnt = 0)
    {
        for (int i = 0; i < cnt / 2; i++)
        {
            Destnation.Add(new Vector3(start.x + Mathf.Abs(end.x - start.x) * 1.0f / cnt * i, start.y, start.z));
            Destnation.Add(new Vector3(end.x - Mathf.Abs(end.x - start.x) * 1.0f / cnt * i, start.y, start.z));
        }
        if (cnt % 2 == 1)
        {
            Destnation.Add(new Vector3(start.x + Mathf.Abs(end.x - start.x) * 1.0f / 2, start.y, start.z));
        }
    }
    /// <summary>
    /// ����һ�е�����
    /// </summary>
    /// <param һ�е��е�="pos"></param>
    /// <param һ�е�����="cnt"></param>
    public void CreatSingleDestnation(Vector3 pos,int cnt)
    {    
        
          Vector3 start = new Vector3(pos.x - (cnt-1) / 2 * dis - (cnt % 2 == 0 ? dis / 2 : 0), pos.y, pos.z);
          Vector3 end = new Vector3(pos.x + (cnt-1) / 2 * dis + (cnt % 2 == 0 ? dis / 2 : 0), pos.y, pos.z);
          print(pos.x + cnt / 2 * dis + (cnt % 2 == 0 ? dis / 2 : 0));
          for (int i = 0;i < cnt/2;i++)
          {
              Destnation.Add(new Vector3(start.x + Mathf.Abs(end.x - start.x) * 1.0f/ cnt*i,start.y,start.z));
              Destnation.Add(new Vector3(end.x - Mathf.Abs(end.x - start.x) * 1.0f / cnt * i, start.y, start.z));
          }
          if(cnt % 2 == 1)
          {
            Destnation.Add(new Vector3(start.x + Mathf.Abs(end.x - start.x) * 1.0f / 2, start.y, start.z));
          }
    }
    
   

    
}
