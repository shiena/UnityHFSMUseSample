﻿using UnityEngine;
using StateBase = FSM.StateBase<Block.StateId>;
using StateMachine = FSM.StateMachine<Block.StateId, Block.EventId>;
using Transition = FSM.Transition<Block.StateId>;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Block : MonoBehaviour
{
    // 状態イベントの定義
    internal enum StateId
    {
        Dead,
        Revive,
    }

    internal enum EventId
    {
        Dead,
        Revive,
    }


    // 現在の状態が生存状態なら生存していることを返すプロパティ
    public bool IsAlive => stateMachine.ActiveStateName == StateId.Revive;


    private StateMachine stateMachine;



    private void Awake()
    {
        stateMachine = new StateMachine(this);
        stateMachine.AddState(StateId.Revive, new AliveState(this));
        stateMachine.AddState(StateId.Dead, new DeadState(this));

        stateMachine.AddTriggerTransition(EventId.Revive, StateId.Dead, StateId.Revive);
        stateMachine.AddTriggerTransition(EventId.Dead, StateId.Revive, StateId.Dead);

        stateMachine.SetStartState(StateId.Revive);

        stateMachine.Init();
    }


    private void Update()
    {
        stateMachine.OnLogic();
    }


    private void OnCollisionEnter(Collision collision)
    {
        // 衝突した相手がボールなら
        if (collision.gameObject.CompareTag("Ball"))
        {
            // 死亡イベントを送る
            stateMachine.Trigger(EventId.Dead);
        }
    }


    public void Revive()
    {
        // ステートマシンに復活イベントを送る
        stateMachine.Trigger(EventId.Revive);
    }



    private class AliveState : StateBase
    {
        private readonly MeshRenderer meshRenderer;
        private readonly Collider collider;

        public AliveState(Block block, bool needsExitTime = false) : base(needsExitTime)
        {
            meshRenderer = block.GetComponent<MeshRenderer>();
            collider = block.GetComponent<Collider>();
        }

        public override void OnEnter()
        {
            meshRenderer.enabled = true;
            collider.enabled = true;
        }
    }


    private class DeadState : StateBase
    {
        private readonly MeshRenderer meshRenderer;
        private readonly Collider collider;

        public DeadState(Block block, bool needsExitTime = false) : base(needsExitTime)
        {
            meshRenderer = block.GetComponent<MeshRenderer>();
            collider = block.GetComponent<Collider>();
        }

        public override void OnEnter()
        {
            meshRenderer.enabled = false;
            collider.enabled = false;
        }
    }
}