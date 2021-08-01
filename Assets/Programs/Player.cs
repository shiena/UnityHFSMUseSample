using UnityEngine;
using StateBase = FSM.StateBase<Player.StateId>;
using StateMachine = FSM.StateMachine<Player.StateId, Player.EventId>;
using Transition = FSM.Transition<Player.StateId>;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Transform))]
public class Player : MonoBehaviour
{
    // ステートマシンのイベントID列挙型
    internal enum StateId
    {
        Enable,
        Disable,
    }

    internal enum EventId
    {
        Enable,
        Disable,
    }



    [SerializeField]
    private float moveSpeed = 1.0f;

    // ステートマシン変数の定義、もちろんコンテキストは Player クラス
    private StateMachine stateMachine;
    private Rigidbody myRigidbody;
    private Vector3 leftRayOrigin;
    private Vector3 rightRayOrigin;



    // コンポーネントの初期化
    private void Awake()
    {
        // リキッドボディのコンポーネントを取得
        myRigidbody = GetComponent<Rigidbody>();


        // ステートマシンの遷移テーブルを構築（コンテキストのインスタンスはもちろん自分自身）
        stateMachine = new StateMachine(this);
        stateMachine.AddState(StateId.Enable, new EnabledState(this));
        stateMachine.AddState(StateId.Disable, new DisabledState());
        stateMachine.AddTriggerTransition(EventId.Enable, StateId.Disable, StateId.Enable);
        stateMachine.AddTriggerTransition(EventId.Disable, StateId.Enable, StateId.Disable);


        // 起動状態はDisabled
        stateMachine.SetStartState(StateId.Disable);
    }


    private void Start()
    {
        // 壁判定用レイの原点を求める
        var myTransform = transform;
        var boundingBox = GetComponent<MeshFilter>().sharedMesh.bounds;
        leftRayOrigin = boundingBox.center + new Vector3(boundingBox.min.x * myTransform.localScale.x, 0.0f, 0.0f);
        rightRayOrigin = boundingBox.center + new Vector3(boundingBox.max.x * myTransform.localScale.x, 0.0f, 0.0f);


        // ステートマシンを起動
        stateMachine.Init();
    }


    // Playerクラスと言っておきながら移動コンポーネントなのでFixedUpdateでステートマシンを回す
    private void FixedUpdate()
    {
        // ステートマシンの更新
        stateMachine.OnLogic();
    }


    // プレイヤーの操作を有効にします
    public void EnableMove()
    {
        // ステートマシンに有効イベントを叩きつける
        stateMachine.Trigger(EventId.Enable);
    }


    // プレイヤーの操作を無効にします
    public void DisableMove()
    {
        // ステートマシンに無効イベントを叩きつける
        stateMachine.Trigger(EventId.Disable);
    }


    public void ResetPosition(Vector3 position)
    {
        myRigidbody.position = position;
    }



    // プレイヤーの移動も何も出来ない哀れな状態クラス
    private class DisabledState : StateBase
    {
        public DisabledState(bool needsExitTime = false) : base(needsExitTime)
        {
        }
    }



    // プレイヤーの移動が許された状態クラス
    private class EnabledState : StateBase
    {
        private readonly Player player;

        public EnabledState(Player player, bool needsExitTime = false) : base(needsExitTime)
        {
            this.player = player;
        }

        // 状態の更新を行います
        public override void OnLogic()
        {
            // 移動ベクトルを作る（ステートマシンが所属しているコンテキストの値は Context プロパティからアクセスが可能です）
            var inputValue = Input.GetKey(KeyCode.LeftArrow) ? -1.0f : Input.GetKey(KeyCode.RightArrow) ? 1.0f : 0.0f;
            var moveVector = new Vector3(inputValue, 0.0f, 0.0f).normalized * player.moveSpeed * Time.fixedDeltaTime;


            // 移動方向によって先の壁判定の為のレイを飛ばす
            var raycastResult = default(RaycastHit);
            var raycastHit = false;
            if (inputValue < 0.0f)
            {
                raycastHit = Physics.Raycast(player.leftRayOrigin + player.myRigidbody.position, Vector3.left, out raycastResult, moveVector.magnitude);
            }
            else if (inputValue > 0.0f)
            {
                raycastHit = Physics.Raycast(player.rightRayOrigin + player.myRigidbody.position, Vector3.right, out raycastResult, moveVector.magnitude);
            }


            // レイがヒットしたのなら
            if (raycastHit)
            {
                // ヒットした距離を調べて0.05以下なら
                if (raycastResult.distance <= 0.05f)
                {
                    // 移動量は殺す
                    moveVector = Vector3.zero;
                }
                else
                {
                    // 移動に十分な量を設定するが0.05の距離は確保する
                    moveVector = moveVector * (raycastResult.distance - 0.05f);
                }
            }


            // 現在の座標を取得して移動ベクトルを加算して移動関数を叩く
            var nextPosition = player.myRigidbody.position + moveVector;
            player.myRigidbody.MovePosition(nextPosition);
        }
    }
}