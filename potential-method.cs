using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class constants
{
    public const int field_size = 40;
}

//y座標は0.3で統一（考えない）

public class sensor : MonoBehaviour
{   
    public int max_distance = 100;    //見える最大距離

    public GameObject goal;     //untiyのInspectorにgoalになるオブジェクトをスワイプして入れる

    public int[,,] seen = new int[constants.field_size+1,constants.field_size+1,constants.field_size+1];    //一度見た壁はもう見ない(計算量削減のために作ったがあまり機能していない)

    public int[,,] wall = new int[constants.field_size+1,constants.field_size+1,constants.field_size+1];    //壁・障害物がある座標を記録。添え字が座標、１で障害物、０で道


    private void init()     //配列の初期化(道空間なのでどこに壁や障害物があるかはわからないのでwallはすべて0にする)
    {   
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

    private double potential(float x,float z)   //x,z座標が与えられたとき、その座標のポテンシャルの計算を行う。細かいところは鈴木先生の資料を見る
    {
        Vector3 p = new Vector3(x,0.3f,z);
        double potential_sum = 0;
        Vector3 gp = goal.transform.position;   
        potential_sum += 3*Vector3.Distance(p,gp);  //3をかけているのは吸引力を増やしたかったから。エゴ。
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

    private void radar()
    {   
        Vector3 origin = transform.position;

        for(int i = 0;i<360;i++)    //360度回転し、周りの遮蔽物を記録
        {   
            transform.Rotate(0,1,0);    //1度ずつ回転
            Vector3 direction = transform.forward;
            Ray ray = new Ray(origin, direction);   //Ray型:光線を示す。origin:現在位置、direction:光線を出す方向
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))   //障害物が当たったらhitに情報が格納され、Raycast関数は1を返す
            {   
                string name = hit.collider.gameObject.name;
                // Debug.Log(name);
                Vector3 hit_vec = hit.point;
                int hit_int_x = (int)hit_vec.x;     //当たった座標をint型にキャスト
                int hit_int_y = (int)hit_vec.y;
                int hit_int_z = (int)hit_vec.z;
                if(seen[hit_int_x,hit_int_y,hit_int_z]==1) continue;
                // Debug.Log(hit_int_z);
                seen[hit_int_x,hit_int_y,hit_int_z]=1;
                wall[hit_int_x,0,hit_int_z]=1;      //wallを1にする
                // Debug.Log(hit_int_x+","+hit_int_z);
            }
        }

        Vector3 now = transform.position;

        float dx = (float)potential(now.x-0.5f,now.z) - (float)potential(now.x+0.5f,now.z);     //偏微分:-0.5から先にやっているのは大きい山から小さい山にベクトルを向けさせたいため
        float dz = (float)potential(now.x,now.z-0.5f) - (float)potential(now.x,now.z+0.5f);

        // Debug.Log(dx);
        // Debug.Log(dz);
        Vector3 field = new Vector3(dx,0f,dz);
            
        field.Normalize();  //正規化:ベクトルの大きさを1にする。やらないと挙動が無茶苦茶になる

        transform.position += field*3f*Time.deltaTime;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {   
        transform.Rotate(0,1,0);
        radar();
    }
}


