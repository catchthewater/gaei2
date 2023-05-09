using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

static class constants
{
    public const int field_size = 40;
}

public class move : MonoBehaviour
{   
    public int max_distance = 100;    //見える最大距離

    public float distance;

    public GameObject goal;
    
    private  Vector3 sp = new Vector3(0, 0, 0);//start.transform.position;

    private  Vector3 gp = new Vector3(10, 0, 10);//goal.transform.position;

    public int[,,] seen = new int[constants.field_size+1,constants.field_size+1,constants.field_size+1];

    public int[,,] wall = new int[constants.field_size+1,constants.field_size+1,constants.field_size+1];


    private void init()
    {   
        gp = goal.transform.position;
        for(int i = 0;i<=constants.field_size;i++)
        {
            for(int j = 0;j<=constants.field_size;j++)
            {
                for(int k = 0;k<=constants.field_size;k++)
                {
                    seen[i,j,k] = 0;
                    wall[i,j,k] = 0;
                }
            }
        }
    }

    private double potential(float x,float z)
    {
        Vector3 p = new Vector3(x,0.3f,z);
        double potential_sum = 0;
        Vector3 gp = goal.transform.position;
        potential_sum += 3*Vector3.Distance(p,gp);
        for(int i = 0;i<=constants.field_size;i++)
        {
            for(int j = 0;j<=constants.field_size;j++)
            {
                if(wall[i,0,j]==1){
                    Vector3 w = new Vector3(i,0.3f,j);
                    potential_sum += (3/Vector3.Distance(w,p));
                    // Debug.Log(Vector3.Distance(w,p));
                }
            }
        }
        return potential_sum;
    }

    private void radar(Vector3 origin)
    {   

        for(int i = 0;i<360;i++)
        {   
            transform.Rotate(0,1,0);
            Vector3 direction = transform.forward;
            Ray ray = new Ray(origin, direction);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {   
                string name = hit.collider.gameObject.name;
                // Debug.Log(name);
                Vector3 hit_vec = hit.point;
                int hit_int_x = (int)hit_vec.x;
                int hit_int_y = (int)hit_vec.y;
                int hit_int_z = (int)hit_vec.z;
                if(seen[hit_int_x,hit_int_y,hit_int_z]==1) continue;
                // Debug.Log(hit_int_z);
                seen[hit_int_x,hit_int_y,hit_int_z]=1;
                wall[hit_int_x,0,hit_int_z]=1;
                wall[hit_int_x+1,0,hit_int_z+1] =1;
                // Debug.Log(hit_int_x+","+hit_int_z);
            }
        }

        Vector3 now = transform.position;

        float dx = (float)potential(now.x-0.5f,now.z) - (float)potential(now.x+0.5f,now.z);
        float dz = (float)potential(now.x,now.z-0.5f) - (float)potential(now.x,now.z+0.5f);

        int now_x = (int)dx;
        int now_z = (int)dz;

        if(now_x==0&&now_z==0)
        {
            // _shortestWay = GetShortestWay(now,gp);
            // foreach (var Item in _shortestWay)
            // {
            //     transform.position = Item;
            // }
        }

        // Debug.Log(dx);
        // Debug.Log(dz);
        Vector3 field = new Vector3(dx,0f,dz);
            
        field.Normalize();

        transform.position += field*3f*Time.deltaTime;
        
    }

    public List<Vector3> GetAdjacences(Vector3 v)
    {
        var adjacences = new List<Vector3>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;
                //if (dx*dz == -1 || dx*dz == 1 || (dx == 0 && dz == 0)) continue;
                Vector3 cell = new Vector3(v.x + dx, v.y, v.z + dz);
                if (cell.x < 0 || cell.x > constants.field_size || cell.z < 0 || cell.z > constants.field_size) continue;
                adjacences.Add(cell);
            }
        }
        return adjacences;
    }

    private class AStarInfo
    {
        public Vector3 cell;
        public AStarInfo previous;
        public float step;
        public float distance;
        public float Weight { get { return step + distance; } }
    }

    public List<Vector3> GetShortestWay(Vector3 StartVec, Vector3 GoalVec)
    {
        List<Vector3> res = new List<Vector3>();
        List<Vector3> passedCells = new List<Vector3>();
        List<AStarInfo> recentTargets = new List<AStarInfo>();
        passedCells.Add(StartVec);
        recentTargets.Add(GetAStarInfo(StartVec, GoalVec, null));
        AStarInfo goalInfo = null;

        while (true)
        {
            // recentTargetsのうちweightが最も低いものを計算対象とする
            AStarInfo currentTarget = recentTargets
                .OrderBy(info => info.Weight)
                .FirstOrDefault();

            radar(currentTarget.cell);
            //transform.position = currentTarget.cell;

            // ターゲットの隣接セルの情報を取得する
            List<AStarInfo> adjacentInfos = GetAdjacences(currentTarget.cell)
                .Where(cell => {
                    // タイプが道でもなくゴールのセルでもない場合は対象外
                    if (wall[(int)cell.x, (int)cell.y, (int)cell.z] == 1 && cell != GoalVec)
                    {
                        return false;
                    }
                    // 計算済みのセルは対象外
                    if (passedCells.Contains(cell))
                    {
                        return false;
                    }
                    return true;
                })
                .Select(cell => GetAStarInfo(cell, GoalVec, currentTarget))
                .ToList();

            // recentTargetsとpassedCellsを更新
            recentTargets.Remove(currentTarget);
            recentTargets.AddRange(adjacentInfos);
            passedCells.Add(currentTarget.cell);

            // ゴールが含まれていたらそこで終了
            goalInfo = adjacentInfos.FirstOrDefault(info => info.cell == GoalVec);
            if (goalInfo != null)
            {
                break;
            }
            // recentTargetsがゼロだったら行き止まりなので終了
            if (recentTargets.Count == 0)
            {
                
                break;
            }
        }

        // ゴールが結果に含まれていない場合は最短経路が見つからなかった
        if (goalInfo == null)
        {
            Debug.Log("Not Find Goal");
            return res;
        }

        Debug.Log("Find Goal");

        // Previousを辿ってセルのリストを作成する
        res.Add(goalInfo.cell);
        AStarInfo current = goalInfo;
        while (true)
        {
            if (current.previous != null)
            {
                res.Add(current.previous.cell);
                current = current.previous;
            }
            else
            {
                break;
            }
        }
        return res;
    }


    private AStarInfo GetAStarInfo(Vector3 targetVec, Vector3 goalVec, AStarInfo previousInfo)
    {
        var result = new AStarInfo();
        result.cell = targetVec;
        result.previous = previousInfo;
        result.step = previousInfo == null ? 0 : previousInfo.step + 1;
        result.distance = Vector3.Distance(targetVec, goalVec);
        return result;
    }

    List<Vector3> _shortestWay;

    // Start is called before the first frame update
    void Start()
    {
        init();
        _shortestWay = GetShortestWay(transform.position,gp);
            foreach (var Item in _shortestWay)
            {   
                Vector3 now = transform.position;
                Vector3 vec = Item-now;
                vec.Normalize();
                transform.position += vec*0.3f*Time.deltaTime;
            }
    }

    // Update is called once per frame
    void Update()
    {   
        transform.Rotate(0,1,0);
        // radar(transform.position);
    }
}


