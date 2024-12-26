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
    //得到射线组件
    private LineRenderer lineRenderer;
    private bool isCheck = false;
    private Dictionary<int, List<Vector3>> Destnations = new Dictionary<int,List<Vector3>>();
    //中间变量
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
    /// 初始化linelineRenderer
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
        //选择对象

        Check_Obj();
        //设置目标点
        SetDestnaionObjs();
    }
    /// <summary>
    /// 得到鼠标点击的地面坐标
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMouseWorldPos()
    {
        //鼠标在屏幕的位置，由摄像机向地面发射射线，得到鼠标点击的地面坐标
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000, 1 << LayerMask.NameToLayer("Ground")))
            return hitInfo.point;
        return Vector3.zero;
    }
    /// <summary>
    /// 选择士兵
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
    /// 选择矩阵内选择对象
    /// </summary>
    /// <param name="Start"></param>
    /// <param name="End"></param>
    public void Select_obj(Vector3 Start, Vector3 End)
    {
        //矩阵四个点的坐标
        pos[0] = Start;
        pos[1] = new Vector3(End.x, Start.y, Start.z);
        pos[2] = End;
        pos[3] = new Vector3(Start.x, Start.y, End.z);
        //画矩阵
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
    /// 清除选择的对象
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
    /// 为选中的对象设置目标点
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
    /// 为对象分配尽量合理的目标点
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
    /// 得到选中的角色分为的行数
    /// </summary>
    /// <returns></returns>
    public int Get_row()
    {
        if ((int)Mathf.Sqrt(obj_roles.Count) * (int)Mathf.Sqrt(obj_roles.Count) == obj_roles.Count)
            return (int)Mathf.Sqrt(obj_roles.Count);
        return Mathf.CeilToInt(Mathf.Sqrt(obj_roles.Count));

    }
    /// <summary>
    /// 生成目标点
    /// </summary>
    /// <param 所有目标点的中心="pos"></param>
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
    /// 生成一行的排列
    /// </summary>
    /// <param 一行的起点="start"></param>
    /// <param 一行的终点="end"></param>
    /// <param 这一行的数量="cnt"></param>
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
    /// 生成一行的排列
    /// </summary>
    /// <param 一行的中点="pos"></param>
    /// <param 一行的数量="cnt"></param>
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
