using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib
{
    /// <summary>
    /// 寄希望于客户端和服务器同时执行的帧同步保证
    /// </summary>
    class BackMove : SkillLogicComponent
    {
        //向后移动 距离和时间
        public float moveStep = -10;
        public float moveTime = 1;

        private SkillLayoutRunner runner;
        public override void Run()
        {
            runner = gameObject.parent.GetComponent<SkillLayoutRunner>();
            MoveBack();
        }

        //得到当前玩家释放技能所在位置
        private void MoveBack()
        {
            //从当前位置计算 合理的击退位置
            var cmd = runner.stateMachine.action;
            var pos = new MyVec3(cmd.X, cmd.Y, cmd.Z);
            var fpos = pos.ToFloat();
            var dir = cmd.Dir;
            var mapGrid = GetRoom().gridManager;

            var moveTotal = moveStep * moveTime;
            var rot = Quaternion.CreateFromYawPitchRoll(MathUtil.UnityYDegToMathRadian(dir), 0, 0);
            var rotDir = Vector3.Transform(MathUtil.forward, rot);

            var endPos = fpos + (rotDir * moveTotal);

            //得到击退的位置
            var newPos = mapGrid.RaycastNearestPoint(fpos, endPos);
            //逻辑上可以直接一段时间设置位置 表现上可能需要做平移

            //移动到位置
            var att = runner.stateMachine.attacker;
            //var phy = att.GetComponent<IPhysicCom>();
            att.SetPos(MyVec3.FromVec3(newPos));
            //phy.MoveToIgnorePhysic(newPos);
        }
    }
}
