using UnityEngine;
using StateBase = FSM.StateBase<MainGameScene.StateId>;
using StateMachine = FSM.StateMachine<MainGameScene.StateId, MainGameScene.EventId>;
using Transition = FSM.Transition<MainGameScene.StateId>;

public class MainGameScene : MonoBehaviour
{
    // ステートマシンのイベントID列挙型
    internal enum StateId
    {
        Reset,
        Standby,
        Playing,
        Miss,
        GameClear,
        GameOver,
    }

    internal enum EventId
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

        stateMachine.AddState(StateId.Miss, new MissState(this));
        stateMachine.AddState(StateId.Playing, new PlayingState(this));
        stateMachine.AddState(StateId.Reset, new ResetState(this));
        stateMachine.AddState(StateId.Standby, new StandbyState());
        stateMachine.AddState(StateId.GameClear, new GameClearState());
        stateMachine.AddState(StateId.GameOver, new GameOverState());

        stateMachine.AddTriggerTransition(EventId.Finish, StateId.Reset, StateId.Standby);
        stateMachine.AddTriggerTransition(EventId.Play, StateId.Standby, StateId.Playing);
        stateMachine.AddTriggerTransition(EventId.Miss, StateId.Playing, StateId.Miss);
        stateMachine.AddTriggerTransition(EventId.AllBlockBroken, StateId.Playing, StateId.GameClear);
        stateMachine.AddTriggerTransition(EventId.Retry, StateId.Miss, StateId.Standby);
        stateMachine.AddTriggerTransition(EventId.Exit, StateId.Miss, StateId.GameOver);
        stateMachine.AddTriggerTransition(EventId.Finish, StateId.GameClear, StateId.Reset);
        stateMachine.AddTriggerTransition(EventId.Finish, StateId.GameOver, StateId.Reset);

        // 起動状態はReset
        stateMachine.SetStartState(StateId.Reset);
    }


    private void Start()
    {
        // ステートマシンを起動
        stateMachine.Init();
    }


    private void Update()
    {
        // ステートマシンの更新
        stateMachine.OnLogic();
    }


    public void MissSignal()
    {
        // ステートマシンにミスイベントを送る
        stateMachine.Trigger(EventId.Miss);
    }


    private class ResetState : StateBase
    {
        private readonly MainGameScene mainGameScene;

        public ResetState(MainGameScene mainGameScene, bool needsExitTime = false) : base(needsExitTime)
        {
            this.mainGameScene = mainGameScene;
        }

        public override void OnEnter()
        {
            foreach (var block in mainGameScene.blocks)
            {
                block.Revive();
            }


            mainGameScene.player.ResetPosition(mainGameScene.playerStartTransform.position);
            mainGameScene.player.DisableMove();
            mainGameScene.ball.transform.position = mainGameScene.ballStartTransform.position;
            mainGameScene.ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            mainGameScene.missCount = 0;


            (fsm as StateMachine)?.Trigger(EventId.Finish);
        }
    }


    private class StandbyState : StateBase
    {
        public StandbyState(bool needsExitTime = false) : base(needsExitTime)
        {
        }

        public override void OnLogic()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                (fsm as StateMachine)?.Trigger(EventId.Play);
            }
        }
    }


    private class PlayingState : StateBase
    {
        private readonly MainGameScene mainGameScene;

        public PlayingState(MainGameScene mainGameScene, bool needsExitTime = false) : base(needsExitTime)
        {
            this.mainGameScene = mainGameScene;
        }

        public override void OnEnter()
        {
            var xDirection = Random.Range(-1.0f, 1.0f);
            var zDirection = Random.Range(0.5f, 1.0f);
            mainGameScene.ball.GetComponent<Rigidbody>().velocity = new Vector3(xDirection, 0.0f, zDirection).normalized * mainGameScene.ballSpeed;


            mainGameScene.player.EnableMove();
        }


        public override void OnLogic()
        {
            var blockAllDead = true;
            foreach (var block in mainGameScene.blocks)
            {
                if (block.IsAlive)
                {
                    blockAllDead = false;
                    break;
                }
            }


            if (blockAllDead)
            {
                (fsm as StateMachine)?.Trigger(EventId.AllBlockBroken);
            }
        }
    }


    private class MissState : StateBase
    {
        private readonly MainGameScene mainGameScene;

        public MissState(MainGameScene mainGameScene, bool needsExitTime = false) : base(needsExitTime)
        {
            this.mainGameScene = mainGameScene;
        }

        public override void OnEnter()
        {
            mainGameScene.player.DisableMove();
            mainGameScene.ball.transform.position = mainGameScene.ballStartTransform.position;
            mainGameScene.ball.GetComponent<Rigidbody>().velocity = Vector3.zero;


            mainGameScene.missCount += 1;
            if (mainGameScene.missCount == mainGameScene.availablePlayCount)
            {
                (fsm as StateMachine)?.Trigger(EventId.Exit);
                return;
            }


            (fsm as StateMachine)?.Trigger(EventId.Retry);
        }
    }


    private class GameClearState : StateBase
    {
        public GameClearState(bool needsExitTime = false) : base(needsExitTime)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("GameClear!!!");
            (fsm as StateMachine)?.Trigger(EventId.Finish);
        }
    }


    private class GameOverState : StateBase
    {
        public GameOverState(bool needsExitTime = false) : base(needsExitTime)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("GameOver...");
            (fsm as StateMachine)?.Trigger(EventId.Finish);
        }
    }
}