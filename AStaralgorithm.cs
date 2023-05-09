using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;


//using System.Drawing;
//using System.Diagnostics;

public class AStaralgorithm : MonoBehaviour
{
    public const int field_size = 40;
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
                
                Vector3 hit_vec = hit.point;

                int hit_int_x = (int)hit_vec.x;     //�����������W��int�^�ɃL���X�g
                int hit_int_y = (int)hit_vec.y;
                int hit_int_z = (int)hit_vec.z;
//----------------------�ύX�ӏ�------------------------------
                if (hit_vec == gp)
                {
                    Debug.Log("Find Goal");
                    return;
                }
                if (wall[hit_int_x, 0, hit_int_z] == 0)
                {
                    wall[hit_int_x, 0, hit_int_z] = 1;      
                }

                Debug.DrawRay(origin, direction, Color.red, Vector3.Distance(origin, hit_vec)); //���[�U�[������
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

    /*public List<Vector3> GetShortestWay(Vector3 StartVec, Vector3 GoalVec)
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
    List<Vector3> _shortestWay;
    List<Vector3> res = new List<Vector3>();
    List<Vector3> passedCells = new List<Vector3>();
    List<AStarInfo> recentTargets = new List<AStarInfo>();
    AStarInfo goalInfo = null;
    void Start()
    {
        init();

        passedCells.Add(sp);
        recentTargets.Add(GetAStarInfo(sp, gp, null)); //GetShortestway�̍ŏ��̕���

    }
    /*private IEnumerator DelayCoroutine()
    {

        foreach (var Item in _shortestWay)
        {
            yield return new WaitForSeconds(1);
            Debug.Log(Item);
            obj.transform.position = Item;
        }

    }*/
    void Update()
    {
        //getshortestwa��where�̕���
        if (goalInfo == null)
        {

            // recentTargets�̂���weight���ł��Ⴂ���̂��v�Z�ΏۂƂ���
            AStarInfo currentTarget = recentTargets
                .OrderBy(info => info.Weight)
                .FirstOrDefault();

            radar(currentTarget.cell);  //�c�����[�_�[����Ȃ��ĒT���ς̂܂����F�łʂꂽ�炢����
            //Debug.Log(currentTarget.cell);
            //Debug.Log(currentTarget.step);
            //obj.transform.position = currentTarget.cell;

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

            // recentTargets��passedCells���X�V
            recentTargets.Remove(currentTarget);
            recentTargets.AddRange(adjacentInfos);
            passedCells.Add(currentTarget.cell);
            Debug.Log(string.Join(", ", GetAdjacences(currentTarget.cell))); //���X�g�����s�Ȃ��ŏo�͂�����@
            // �S�[�����܂܂�Ă����炻���ŏI��
            goalInfo = adjacentInfos.FirstOrDefault(info => info.cell == gp);
            if (goalInfo != null)
            {
                Debug.Log("Find Goal");

                Debug.Log("print passedCells");
                Debug.Log(string.Join(",", passedCells));        
            }
            // recentTargets���[����������s���~�܂�Ȃ̂ŏI��
            if (recentTargets.Count == 0)
            {
                Debug.Log("Not Find Goal");

            }
        }
    }
    //----------------------�ύX�ӏ�------------------------------
}