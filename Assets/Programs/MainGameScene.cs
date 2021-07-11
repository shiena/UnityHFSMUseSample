using UnityEngine;
using StateMachine = IceMilkTea.Core.ImtStateMachine<MainGameScene, MainGameScene.StateEventId>;
using State = IceMilkTea.Core.ImtStateMachine<MainGameScene, MainGameScene.StateEventId>.State;

[DefaultExecutionOrder(100)]
public class MainGameScene : MonoBehaviour
{
    // ステートマシンのイベントID列挙型
    internal enum StateEventId
    {
        Play,
        Miss,
        Retry,
        Exit,
        AllBlockBroken,
        Finish,
    }



    [SerializeField]
    private int availablePlayCount = 3;
    [SerializeField]
    private Transform ballStartTransform = null;
    [SerializeField]
    private GameObject ball = null;
    [SerializeField]
    private float ballSpeed = 3.0f;
    [SerializeField]
    private Transform playerStartTransform = null;
    [SerializeField]
    private Player player = null;
    [SerializeField]
    private Block[] blocks = null;

    // ステートマシン変数の定義、もちろんコンテキストは MainGameScene クラス
    private StateMachine stateMachine;
    private int missCount;



    // コンポーネントの初期化
    private void Awake()
    {
        // ステートマシンの遷移テーブルを構築（コンテキストのインスタンスはもちろん自分自身）
        stateMachine = new StateMachine(this);
        stateMachine.AddTransition<ResetState, StandbyState>(StateEventId.Finish);
        stateMachine.AddTransition<StandbyState, PlayingState>(StateEventId.Play);
        stateMachine.AddTransition<PlayingState, MissState>(StateEventId.Miss);
        stateMachine.AddTransition<PlayingState, GameClearState>(StateEventId.AllBlockBroken);
        stateMachine.AddTransition<MissState, StandbyState>(StateEventId.Retry);
        stateMachine.AddTransition<MissState, GameOverState>(StateEventId.Exit);
        stateMachine.AddTransition<GameClearState, ResetState>(StateEventId.Finish);
        stateMachine.AddTransition<GameOverState, ResetState>(StateEventId.Finish);


        // 起動状態はReset
        stateMachine.SetStartState<ResetState>();
    }


    private void Start()
    {
        // ステートマシンを起動
        stateMachine.Update();
    }


    private void Update()
    {
        // ステートマシンの更新
        stateMachine.Update();
    }


    public void MissSignal()
    {
        // ステートマシンにミスイベントを送る
        stateMachine.SendEvent(StateEventId.Miss);
    }


    private class ResetState : State
    {
        protected override void Enter()
        {
            foreach (var block in Context.blocks)
            {
                block.Revive();
            }


            Context.player.ResetPosition(Context.playerStartTransform.position);
            Context.player.DisableMove();
            Context.ball.transform.position = Context.ballStartTransform.position;
            Context.ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Context.missCount = 0;


            StateMachine.SendEvent(StateEventId.Finish);
        }
    }


    private class StandbyState : State
    {
        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StateMachine.SendEvent((int)StateEventId.Play);
            }
        }
    }


    private class PlayingState : State
    {
        protected override void Enter()
        {
            var xDirection = Random.Range(-1.0f, 1.0f);
            var zDirection = Random.Range(0.5f, 1.0f);
            Context.ball.GetComponent<Rigidbody>().velocity = new Vector3(xDirection, 0.0f, zDirection).normalized * Context.ballSpeed;


            Context.player.EnableMove();
        }


        protected override void Update()
        {
            var blockAllDead = true;
            foreach (var block in Context.blocks)
            {
                if (block.IsAlive)
                {
                    blockAllDead = false;
                    break;
                }
            }


            if (blockAllDead)
            {
                StateMachine.SendEvent(StateEventId.AllBlockBroken);
            }
        }
    }


    private class MissState : State
    {
        protected override void Enter()
        {
            Context.player.DisableMove();
            Context.ball.transform.position = Context.ballStartTransform.position;
            Context.ball.GetComponent<Rigidbody>().velocity = Vector3.zero;


            Context.missCount += 1;
            if (Context.missCount == Context.availablePlayCount)
            {
                StateMachine.SendEvent(StateEventId.Exit);
                return;
            }


            StateMachine.SendEvent(StateEventId.Retry);
        }
    }


    private class GameClearState : State
    {
        protected override void Enter()
        {
            Debug.Log("GameClear!!!");
            StateMachine.SendEvent(StateEventId.Finish);
        }
    }


    private class GameOverState : State
    {
        protected override void Enter()
        {
            Debug.Log("GameOver...");
            StateMachine.SendEvent(StateEventId.Finish);
        }
    }
}