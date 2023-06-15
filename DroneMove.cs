using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

//�}�b�v�̃T�C�Y��^���āA�傫�ȋ��ɐ؂��Ă���T������ƃf�[�^�ʂ����Ȃ����炩�ȓ����ɂȂ�̂ł́H
//���_�Ƃ��Ă͕ǂ��܂ރ}�X�́A�󔒂��傫���Ȃ邪�A�ǂɋ߂Â������Ȃ��悤�ɂȂ�A�s�����悭�Ȃ�
//�܂荡�̂Ƃ���1x1�}�X�����A���{�����ăC�`�}�X��傫������΂����̂ł͂Ȃ����H�Ƃ�������

//using System.Drawing;
//using System.Diagnostics;

public class DroneMove : MonoBehaviour
{
    public const int field_size = 220;
    public GameObject obj;

    public Vector3 sp; //�X�^�[�g�ʒu�̐������W

    public Vector3 gp;//�S�[���ʒu

    public int[,,] wall = new int[field_size + 1, field_size + 1, field_size + 1];

    private void init()     //�z��̏�����(����ԂȂ̂łǂ��ɕǂ��Q�������邩�͂킩��Ȃ��̂�wall�͂��ׂ�0�ɂ���)
    {
        for (int i = 0; i <= field_size; i++)
        {
            for (int j = 0; j <= field_size; j++)
            {
                for (int k = 0; k <= field_size; k++)
                {
                    wall[i, j, k] = 0;
                }
            }
        }
    }

    private void radar(Vector3 origin) //�����Ɍ��݈ʒu
    {

        for (int i = 0; i < 360; i++)    //360�x��]���A����̎Օ������L�^
        {
            transform.Rotate(0, 1, 0);    //1�x����]
            Vector3 direction = transform.forward;
            Ray ray = new Ray(origin, direction);   //Ray�^:�����������Borigin:���݈ʒu�Adirection:�������o������
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))   //��Q��������������hit�ɏ�񂪊i�[����ARaycast�֐���1��Ԃ�
            {

                // Debug.Log(hit.point);

                Vector3 hit_vec = hit.point;

                double hit_int_x = Math.Round(hit_vec.x);     //�����������W��int�^�ɃL���X�g
                double hit_int_y = Math.Round(hit_vec.y);
                double hit_int_z = Math.Round(hit_vec.z);

                double dist = Vector3.Distance(origin, hit_vec);

                if (WalltoObjDist > dist) WalltoObjDist = dist;
                //----------------------�ύX�ӏ�------------------------------
                if (hit_vec == gp)
                {
                    Debug.Log("Find Goal");
                    return;
                }
                Debug.Log(hit_int_z);
                if (wall[(int)hit_int_x, (int)hit_int_y, (int)hit_int_z] == 0)
                {
                    wall[(int)hit_int_x, (int)hit_int_y, (int)hit_int_z] = 1;
                }

                // Debug.DrawRay(origin, direction, Color.red, Vector3.Distance(origin, hit_vec)); //���[�U�[������
                //----------------------�ύX�ӏ�------------------------------
            }
        }
    }
    public List<Vector3> GetAdjacences(Vector3 v) //���[���h���̗אڂ��锪�}�X���i�[
    {
        List<Vector3> adjacences = new List<Vector3>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;     //�אڔ��}�X
                //if (dx*dz == -1 || dx*dz == 1 || (dx == 0 && dz == 0)) continue;�@//�אڎl�}�X
                Vector3 cell = new Vector3(v.x + dx, v.y, v.z + dz);
                if (cell.x < 0 || cell.x > field_size || cell.z < 0 || cell.z > field_size)
                {
                    continue;
                }
                //----------------------�ύX�ӏ�------------------------------
                if (passedCells.Contains(cell))
                {
                    continue;
                }
                if (wall[(int)cell.x, (int)cell.y, (int)cell.z] == 1)
                {
                    continue;
                }
                //----------------------�ύX�ӏ�------------------------------
                adjacences.Add(cell);
            }
        }
        return adjacences;
    }


    public class AStarInfo
    {
        public Vector3 cell;
        public AStarInfo previous;
        public float step;
        public float distance;
        public float Weight { get { return (step + distance); } } //�ꉞ�������ł�������
    }

    //----------------------�ύX�ӏ�------------------------------
    //���̊֐��͎g��Ȃ��ŁAwhile����Update�̂Ȃ��ŏ��������Ă���B
    //����ɂ��A���[�_�[�ƈړ����̂̒����T���������ł���悤�ɂȂ��Ă���
    //�T���I�����܂�����Scene�������B
    /*
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
            // recentTargets�̂���weight���ł��Ⴂ���̂��v�Z�ΏۂƂ���
            AStarInfo currentTarget = recentTargets
                .OrderBy(info => info.Weight)
                .FirstOrDefault();

            radar(currentTarget.cell);

            //Debug.Log(currentTarget.cell);
            //obj.transform.position = currentTarget.cell;

            // �^�[�Q�b�g�̗אڃZ���̏����擾����
            List<AStarInfo> adjacentInfos = new List<AStarInfo>();
            foreach (Vector3 c in GetAdjacences(currentTarget.cell))
            {
                if (wall[(int)c.x, (int)c.y, (int)c.z] == 1 && c != GoalVec)
                {
                    continue;
                }
                // �v�Z�ς݂̃Z���͑ΏۊO
                if (passedCells.Contains(c))
                {
                    continue;
                }
                adjacentInfos.Add(GetAStarInfo(c, GoalVec, currentTarget));
            }

            // recentTargets��passedCells���X�V
            recentTargets.Remove(currentTarget);
            recentTargets.AddRange(adjacentInfos);
            passedCells.Add(currentTarget.cell);

            // �S�[�����܂܂�Ă����炻���ŏI��
            goalInfo = adjacentInfos.FirstOrDefault(info => info.cell == GoalVec);
            if (goalInfo != null)
            {
                break;
            }
            // recentTargets���[����������s���~�܂�Ȃ̂ŏI��
            if (recentTargets.Count == 0)
            {

                break;
            }
        }

        // �S�[�������ʂɊ܂܂�Ă��Ȃ��ꍇ�͍ŒZ�o�H��������Ȃ�����
        if (goalInfo == null)
        {
            Debug.Log("Not Find Goal");
            return res;
        }

        Debug.Log("Find Goal");

        // Previous��H���ăZ���̃��X�g���쐬����
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
    */

    public List<Vector3> RouteFind()
    {
        int flag = 0;
        passedCells.Clear();
        recentTargets.Clear();
        goalInfo = null;
        recentTargets.Add(GetAStarInfo(transform.position, gp, null));
        // Debug.Log(transform.position);
        //getshortestwa��where�̕���
        if (goalInfo == null)
        {

            // recentTargets�̂���weight���ł��Ⴂ���̂��v�Z�ΏۂƂ���

            //-------------�ύX----------
            // radar(currentTarget.cell);  //�c�����[�_�[����Ȃ��ĒT���ς̂܂����F�łʂꂽ�炢����
            //radar�̓h���[�����甭��������̂����炱�̏�������out
            //---------------------------


            // Debug.Log(currentTarget.cell);
            //Debug.Log(currentTarget.step);
            //obj.transform.position = currentTarget.cell;
            while (true)
            {
                AStarInfo currentTarget = recentTargets
                    .OrderBy(info => info.Weight)
                    .FirstOrDefault();

                // �^�[�Q�b�g�̗אڃZ���̏����擾����
                List<AStarInfo> adjacentInfos = new List<AStarInfo>();
                foreach (Vector3 c in GetAdjacences(currentTarget.cell))
                {
                    if (wall[(int)c.x, (int)c.y, (int)c.z] == 1 && c != gp)
                    {
                        continue;
                    }
                    // �v�Z�ς݂̃Z���͑ΏۊO
                    if (passedCells.Contains(c))
                    {
                        continue;
                    }
                    var tmp = GetAStarInfo(c, gp, currentTarget);
                    bool can = false;
                    foreach (var Item in recentTargets) //where����Ȃ�����
                    {
                        if (Item.cell == tmp.cell)      //269�s����278�̑���ŉ����B
                        {
                            can = true;
                            break;
                        }
                    }
                    if (can)
                    {
                        continue;
                    }
                    adjacentInfos.Add(tmp);
                }

                // Debug.Log(currentTarget.cell);

                // recentTargets��passedCells���X�V
                recentTargets.Remove(currentTarget);
                recentTargets.AddRange(adjacentInfos);
                passedCells.Add(currentTarget.cell);
                // Debug.Log(string.Join(", ", GetAdjacences(currentTarget.cell))); //���X�g�����s�Ȃ��ŏo�͂�����@
                // �S�[�����܂܂�Ă����炻���ŏI��
                // goalInfo = adjacentInfos.FirstOrDefault(info => info.cell == gp);
                for (int k = 0; k < adjacentInfos.Count(); k++)
                {
                    Vector3 adj_tmp = new Vector3(0, 0, 0);
                    adj_tmp = adjacentInfos[k].cell;
                    double adj_x = Math.Round(adj_tmp.x);
                    float adj_y = 0.5f;
                    double adj_z = Math.Round(adj_tmp.z);
                    if ((float)adj_x == gp.x && (float)adj_y == gp.y && (float)adj_z == gp.z)
                    {
                        goalInfo = adjacentInfos[k];
                        break;
                    }
                }
                if (goalInfo != null)
                {
                    Debug.Log("Find Goal");

                    Debug.Log("print passedCells");
                    // Debug.Log(string.Join(",", passedCells));

                    //�o�H����
                    res.Clear();
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
                    res.Reverse();
                    Debug.Log(string.Join(",", res));
                    flag = 1;
                    break;
                }
                // recentTargets���[����������s���~�܂�Ȃ̂ŏI��
                if (recentTargets.Count == 0)
                {
                    Debug.Log("Not Find Goal");
                    break;
                }
            }
        }
        if (flag == 1) return res;
        else return null;
    }


    private double potential(float x, float z)   //x,z���W���^����ꂽ�Ƃ��A���̍��W�̃|�e���V�����̌v�Z���s���B�ׂ����Ƃ���͗�ؐ搶�̎���������
    {
        Vector3 p = new Vector3(x, 0.3f, z);
        double potential_sum = 0;
        potential_sum += Vector3.Distance(p, gp);
        for (int i = 0; i <= field_size; i++)
        {
            for (int j = 0; j <= field_size; j++)
            {
                if (wall[i, 0, j] == 1)
                {
                    Vector3 w = new Vector3(i, 0.3f, j);
                    potential_sum += (3 / Vector3.Distance(w, p));
                    // Debug.Log(Vector3.Distance(w,p));
                }
            }
        }
        return potential_sum;
    }



    private AStarInfo GetAStarInfo(Vector3 targetVec, Vector3 goalVec, AStarInfo previousInfo)
    {
        AStarInfo result = new AStarInfo();
        result.cell = targetVec;
        result.previous = previousInfo;
        result.step = (previousInfo == null) ? 0 : (previousInfo.step + 1); //�ꉞ�������ł�����
        result.distance = Vector3.Distance(targetVec, goalVec);
        return result;
    }

    //----------------------�ύX�ӏ�------------------------------
    List<Vector3> _shortestWay = new List<Vector3>();
    List<Vector3> res = new List<Vector3>();
    List<Vector3> passedCells = new List<Vector3>();
    List<AStarInfo> recentTargets = new List<AStarInfo>();
    AStarInfo goalInfo = null;
    int can = 0;
    void Start()
    {
        init();

        passedCells.Add(sp);
        recentTargets.Add(GetAStarInfo(sp, gp, null)); //GetShortestway�̍ŏ��̕���

        radar(transform.position);
        _shortestWay = RouteFind();

    }
    int count = 0;
    double WalltoObjDist = field_size;
    void Update()
    {
        if (goalInfo != null)
        {
            WalltoObjDist = field_size;
            radar(transform.position);
            // Debug.Log(WalltoObjDist);
            if (WalltoObjDist <= 10.4)
            {
                Debug.Log("Warning");
                Vector3 now = transform.position;

                float dx = (float)potential(now.x - 0.5f, now.z) - (float)potential(now.x + 0.5f, now.z);     //�Δ���:-0.5�����ɂ���Ă���̂͑傫���R���珬�����R�Ƀx�N�g��������������������
                float dz = (float)potential(now.x, now.z - 0.5f) - (float)potential(now.x, now.z + 0.5f);
                Vector3 field = new Vector3(dx, 0, dz);
                field.Normalize();  //���K��:�x�N�g���̑傫����1�ɂ���B���Ȃ��Ƌ����������ꒃ�ɂȂ�
                Debug.Log(field);
                transform.position += field * 4.0f * Time.deltaTime;
                WalltoObjDist = field_size;
                radar(transform.position);
                if (WalltoObjDist >= 10.4)
                {
                    _shortestWay = RouteFind();
                    count = 0;
                }
            }
            else
            {
                // transform.position = Vector3.Lerp(transform.position,res[count],Time.deltaTime * 3.3f);
                Vector3 vec = res[count] - transform.position;
                vec.Normalize();
                transform.position += 4.0f * vec * Time.deltaTime;
            }
            if (Vector3.Distance(transform.position, res[count]) < 0.1f)
            {
                count++;
            }
            // Debug.Log(count);
        }
    }
    /*private IEnumerator DelayCoroutine()
    {

        for (int i = 0; i < res.Count()-1; i++)
        {
            yield return new WaitForSeconds(1);
            Debug.Log(res[i]);
            obj.transform.position = Vector3.MoveTowards(res[i], res[i+1], 0.5f);
        }

    }*/
    //----------------------�ύX�ӏ�------------------------------
}