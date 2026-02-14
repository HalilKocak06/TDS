using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour
{
    public enum State { Entering, WaitingPlayer, Leaving }
    public State state { get; private set; }

    [Header("Arrive")]
    [SerializeField] float arriveEpsilon = 0.08f;
    [SerializeField] float speedDampTime = 0.12f;

    NavMeshAgent agent;
    Animator anim;
    Transform talkPoint;
    Transform exitPoint;

    [Header("Facing (No Tag)")]
    [SerializeField] Transform player;               // Spawner/Inspector set eder
    [SerializeField] bool facePlayerWhileWaiting = true;
    [SerializeField] float turnSpeed = 8f;

    [Header("Job")]
    [SerializeField] TireJobManager jobManager;
    TireOrder pendingOrder;
    bool jobStarted;


    public enum TalkStage
    {
        None,
        WaitingGreeting,   // Player tık bekliyoruz
        CustomerAsked,     // “Lastik istiyorum” dedi
        PlayerAccepted     // “Tamam abi” dedin
    }

    public TalkStage talkStage { get; private set; } = TalkStage.None;

    [SerializeField] float replyDelay = 0.2f;
    [SerializeField] float talkDistance = 2.5f; // çok uzaktaysa tık çalışmasın

    bool busyTalking;

    float animSpeed;
    float animSpeedVelRef;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (jobManager == null)
            jobManager = FindFirstObjectByType<TireJobManager>();

    }

    public void Init(Transform talk, Transform exit)
    {
        talkPoint = talk;
        exitPoint = exit;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void BeginEnterShop()
    {
        state = State.Entering;
        talkStage = TalkStage.None;
        busyTalking = false;

        if (agent)
        {
            agent.autoBraking = true;
            agent.isStopped = false;
            agent.updateRotation = true;

            if (talkPoint != null)
                agent.SetDestination(talkPoint.position);
        }
    }

    void Update()
    {
        UpdateAnimatorSpeed();

        if (state == State.Entering)
        {
            bool arrived =
                agent != null &&
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + arriveEpsilon &&
                agent.desiredVelocity.sqrMagnitude < 0.01f;

            if (arrived)
            {
                OnReachedTalkPoint();
            }
        }

        if (state == State.WaitingPlayer)
        {
            FacePlayer();
        }
    }

    void OnReachedTalkPoint()
    {
        state = State.WaitingPlayer;

        // ✅ artık rotasyonu biz yönetiyoruz (FacePlayer)
        if (agent) agent.updateRotation = false;

        // ✅ konuşma sırası tıkla başlasın
        talkStage = TalkStage.WaitingGreeting;
        busyTalking = false;
    }

    //Müşteri player'a döner .
    void FacePlayer()
    {
        if (!facePlayerWhileWaiting) return;
        if (!player) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    void UpdateAnimatorSpeed()
    {
        if (!anim || !agent) return;

        float target = agent.desiredVelocity.magnitude;

        bool shouldSnapToZero =
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + arriveEpsilon;

        if (shouldSnapToZero) target = 0f;

        animSpeed = Mathf.SmoothDamp(animSpeed, target, ref animSpeedVelRef, speedDampTime);
        anim.SetFloat("Speed", animSpeed);
    }

    public void LeaveShop()
    {
        state = State.Leaving;
        busyTalking = false;

        if (agent)
        {
            agent.isStopped = false;
            agent.updateRotation = true;

            if (exitPoint != null)
                agent.SetDestination(exitPoint.position);
        }
    }

    // Player sol tıkla çağırır
    public void OnPlayerGreetClicked()
    {
        // sadece talkPoint’te beklerken konuşsun
        if (state != State.WaitingPlayer) return;
        if (busyTalking) return;

        // uzaksa konuşmasın (gerçekçilik)
        if (player != null)
        {
            float d2 = (player.position - transform.position).sqrMagnitude;
            if (d2 > talkDistance * talkDistance) return;
        }

        // güvenlik: yanlışlıkla None kalırsa
        if (talkStage == TalkStage.None)
            talkStage = TalkStage.WaitingGreeting;

        if (talkStage == TalkStage.WaitingGreeting)
        {
            busyTalking = true;
            Debug.Log("Hoşgeldin abi");
            Invoke(nameof(SayNeedTire), replyDelay);
        }
        else if (talkStage == TalkStage.CustomerAsked)
        {
            busyTalking = true;
            Debug.Log("Tamam abi");
            talkStage = TalkStage.PlayerAccepted;
            Invoke(nameof(EndTalkBusy), 0.05f);

            if(!jobStarted)
            {
                jobStarted = true;
                
                if(jobManager == null)
                    jobManager = FindFirstObjectByType<TireJobManager>();

                if(jobManager != null && pendingOrder !=null)
                {
                    Debug.Log($"[Player] Tamam abi, başlıyorum -> {pendingOrder.Display}");
                    jobManager.StartJob(pendingOrder);
                }
                else
                {
                    Debug.LogWarning("[Customer] JobManager veya pendingOrder yok!");
                }    
            }
        }

        else if (talkStage == TalkStage.PlayerAccepted)
            {
                if (jobManager == null)
                    jobManager = FindFirstObjectByType<TireJobManager>();

                if (jobManager == null)
                {
                    Debug.LogWarning("[VALIDATE] JobManager yok!");
                    return;
                }

                bool done = jobManager.Validate();

                if (done)
                {
                    Debug.Log("[Customer] İş tamamlandı, çıkıyorum.");
                    LeaveShop();
                }
                else
                {
                    Debug.Log("[Customer] İş henüz tamamlanmadı.");
                }
            }
        // PlayerAccepted sonrası şimdilik ignore
    }

    void SayNeedTire()
    {
        pendingOrder = new TireOrder
        {
            size = new TireSize(195,55,16),
            season = TireSeason.Summer,
            condition = TireCondition.New,
            quantity = 4
        };


        Debug.Log($"Kolay gelsin ustam {pendingOrder.Display} istiyorum");
        talkStage = TalkStage.CustomerAsked;
        busyTalking = false;
    }

    void EndTalkBusy()
    {
        busyTalking = false;
    }
}
