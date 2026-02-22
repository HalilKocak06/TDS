using System;
using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour
{
    public enum State { Entering, WaitingPlayer, Leaving }
    public State state { get; private set; }

    [Header("Arrive")]
    [SerializeField] float arriveEpsilon = 0.5f;
    [SerializeField] float speedDampTime = 0.12f;

    NavMeshAgent agent;
    Animator anim;
    Transform talkPoint;
    Transform exitPoint;

    Transform outsidePoint;

    Transform waitingPoint;

    [Header("Facing")]
    [SerializeField] Transform player;
    [SerializeField] bool facePlayerWhileWaiting = true;
    [SerializeField] float turnSpeed = 8f;

    [Header("Coordinator")]
    [SerializeField] ShopCoordinator coordinator;
    [SerializeField] bool waitingForBay;
    ServiceBay assignedBay;
    bool departureFinalized;

    [Header("Job")]
    [SerializeField] TireJobManager jobManager; // coordinator set edecek
    TireOrder pendingOrder;
    bool jobStarted;

    public enum TalkStage { None, WaitingGreeting, CustomerAsked, PlayerAccepted }
    public TalkStage talkStage { get; private set; } = TalkStage.None;

    [SerializeField] float replyDelay = 0.2f;
    [SerializeField] float talkDistance = 2.5f;

    bool busyTalking;
    float animSpeed;
    float animSpeedVelRef;

    public event Action<CustomerController> OnFreedTalkPoint;
    bool isInWaitingArea;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    public void Init(Transform talk, Transform outside, Transform waiting, Transform exit, ShopCoordinator coord)
    {
        talkPoint = talk;
        outsidePoint = outside;
        waitingPoint = waiting;
        exitPoint = exit;
        coordinator = coord;
    }

    public void SetPlayer(Transform playerTransform) => player = playerTransform;

    // Coordinator buradan set edecek
    public void SetJobManager(TireJobManager jm) => jobManager = jm;
    public void SetAssignedBay(ServiceBay bay) => assignedBay = bay;

    public void SetWaitingForBay(bool v) => waitingForBay = v;

    public TireOrder GetPendingOrder() => pendingOrder;

    public void NotifyJobStarted() => jobStarted = true;

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

        //NPC dükkana giriyor.
        if (state == State.Entering)
        {
            bool arrived =
                agent != null &&
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + arriveEpsilon &&
                agent.desiredVelocity.sqrMagnitude < 0.01f;

            if (arrived)
            {
                //Waiting area'ya geldiyse idle gibi dursun
                if(isInWaitingArea)
                {
                    state = State.WaitingPlayer; // idle anim + facePlayer
                    if(agent) agent.updateRotation = false;
                }
                else
                {
                    //Talk'a geldiğinde asıl konuşma statei
                    OnReachedTalkPoint();
                }
            }
        }


        //NPC - DÜkkanda bekliyor
        if (state == State.WaitingPlayer)
            FacePlayer();
    }

    void OnTriggerEnter(Collider other)
    {
        if(departureFinalized) return;
        if(state != State.Leaving) return;

        //Exit zone'a girdiyse finalize
        var zone = other.GetComponent<ExitZone>();
        if(zone == null) return;

        // Eğer zone bay referansı veriyorsa, assignedBay'i override edebilirsin
        if(zone.Bay != null)
            assignedBay = zone.Bay;

        FinalizeDeparture();    
    }

    void FinalizeDeparture()
    {
        Debug.Log("FİNALİZEDEPARTURE GİRİLDİ ");
        departureFinalized = true;

        coordinator?.ReleaseWaitingSpot(this); //waitingSpot'u temizliyoruz.

        //1-Bay'i boşaltıyoruz + arabayı siliyoruz
        if(assignedBay != null)
        {
            //bay'in arabasını siliyoruz
            if(assignedBay.carJob != null)
                assignedBay.carJob.DespawnCar();

            //Bay'i serbest bırakıyoruz
            assignedBay.Release();    
        }

        //2-NPC'yi yok et
        Destroy(gameObject);
    }

    void OnReachedTalkPoint()
    {
        state = State.WaitingPlayer;
        if (agent) agent.updateRotation = false;

        talkStage = TalkStage.WaitingGreeting;
        busyTalking = false;
    }

    void FacePlayer()
    {
        if (!facePlayerWhileWaiting || !player) return;

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

    public void OnPlayerGreetClicked()
    {
        if (state != State.WaitingPlayer) return;
        if (busyTalking) return;

        if (player != null)
        {
            float d2 = (player.position - transform.position).sqrMagnitude;
            if (d2 > talkDistance * talkDistance) return;
        }

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

            // ✅ iş burada coordinator’a devredildi
            if (!jobStarted)
            {
                if (coordinator == null)
                {
                    Debug.LogWarning("[Customer] Coordinator yok!");
                    return;
                }

                if (pendingOrder == null)
                {
                    Debug.LogWarning("[Customer] pendingOrder yok!");
                    return;
                }

                Debug.Log($"[Player] Tamam abi -> bay istiyorum: {pendingOrder.Display}");
                var result = coordinator.DealAccepted(this, pendingOrder);

                if(coordinator.TryReserveWaitingSpot(this))
                {
                    GoToWaitingPoint();
                }
                else
                {
                    //talkta kalır ve talk slotu boşlatılmaz.
                    Debug.Log("[Customer] waitingPoint dolu -> talkPointte kaliyorum");
                }
                
            }

        }
        else if (talkStage == TalkStage.PlayerAccepted)
        {
            if (waitingForBay)
            {
                Debug.Log("[Customer] Lift dolu, sırada bekliyorum...");
                return;
            }

            if (jobManager == null)
            {
                Debug.LogWarning("[VALIDATE] JobManager yok! (Bay atanmamış olabilir)");
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
    }

    void SayNeedTire()
    {
        pendingOrder = new TireOrder
        {
            size = new TireSize(195, 55, 16),
            season = TireSeason.Summer,
            condition = TireCondition.New,
            quantity = 4
        };

        Debug.Log($"Kolay gelsin ustam {pendingOrder.Display} istiyorum");
        talkStage = TalkStage.CustomerAsked;
        busyTalking = false;
    }

    void EndTalkBusy() => busyTalking = false;

    public void GoToOutsidePoint()
    {
        state = State.Entering; //"yürüyor" gibi kullanıyoruz
        talkStage = TalkStage.None;
        busyTalking = false;
        isInWaitingArea = false;

        if(agent && outsidePoint != null)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(outsidePoint.position);
        }
    }

    public void GoToTalkPoint()
    {
        state = State.Entering;
        talkStage = TalkStage.None;
        busyTalking = false;
        isInWaitingArea = false;

        if(agent && talkPoint != null)
        {
            agent.isStopped= false;
            agent.updateRotation = true;
            agent.SetDestination(talkPoint.position);
        }
    }

    void GoToWaitingPoint()
    {
        //deal accepted sonrası konuşma bitti -> talk boşalsın
        isInWaitingArea = true;

        //talkPoint boşaldı eventi
        OnFreedTalkPoint?.Invoke(this);

        //artık talkpointte beklemeyecek
        state = State.Entering; //yine yürüme state'i gibi kullanıyoruz
        busyTalking = false;

        if(agent && waitingPoint != null)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(waitingPoint.position);
        }
    }

    public void OnBayAssigned()
    {
        //zaten waiting'deyse gerek yok
        if(isInWaitingArea) return;

        //waiting spot boşsa reserve edip waiting'e yürü
        if(coordinator != null && coordinator.TryReserveWaitingSpot(this))
        {
            GoToWaitingPoint(); //talk slot boşalır.
        }
    }

    public void GoToSpecificPoint(Transform t)
    {
        state = State.Entering;
        talkStage = TalkStage.None;
        busyTalking = false;
        isInWaitingArea = false;

        if(agent && t != null)
        {
            agent.isStopped=false;
            agent.updateRotation = true;
            agent.SetDestination(t.position);
        }
    }

    
}
