﻿using UnityEngine;
using StateMachine = IceMilkTea.Core.ImtStateMachine<Block, Block.StateEventId>;
using State = IceMilkTea.Core.ImtStateMachine<Block, Block.StateEventId>.State;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Block : MonoBehaviour
{
    // 状態イベントの定義
    internal enum StateEventId
    {
        Dead,
        Revive,
    }


    // 現在の状態が生存状態なら生存していることを返すプロパティ
    public bool IsAlive => stateMachine.IsCurrentState<AliveState>();


    private StateMachine stateMachine;



    private void Awake()
    {
        stateMachine = new StateMachine(this);
        stateMachine.AddTransition<AliveState, DeadState>(StateEventId.Dead);
        stateMachine.AddTransition<DeadState, AliveState>(StateEventId.Revive);


        stateMachine.SetStartState<AliveState>();
    }


    private void Start()
    {
        stateMachine.Update();
    }


    private void Update()
    {
        stateMachine.Update();
    }


    private void OnCollisionEnter(Collision collision)
    {
        // 衝突した相手がボールなら
        if (collision.gameObject.CompareTag("Ball"))
        {
            // 死亡イベントを送る
            stateMachine.SendEvent(StateEventId.Dead);
        }
    }


    public void Revive()
    {
        // ステートマシンに復活イベントを送る
        stateMachine.SendEvent(StateEventId.Revive);
    }



    private class AliveState : State
    {
        protected override void Enter()
        {
            Context.GetComponent<MeshRenderer>().enabled = true;
            Context.GetComponent<Collider>().enabled = true;
        }
    }


    private class DeadState : State
    {
        protected override void Enter()
        {
            Context.GetComponent<MeshRenderer>().enabled = false;
            Context.GetComponent<Collider>().enabled = false;
        }
    }
}