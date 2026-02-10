using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour
{
    public enum State { Entering, WaitingPlayer, Leaving } //State değişkeni bu değişkenlerden birini alabilir.
    public State state { get; private set; } //dışarıdan okunabilen bir state alanı

    [Header("Arrive")]
    [SerializeField] float arriveEpsilon = 0.08f; // "tam durdu" toleransı
    [SerializeField] float speedDampTime = 0.12f; // anim smoothing
    [SerializeField] float lookTurnSpeed = 7f; // Oyuncuya baksın diye.
    Transform player; //player objesinin yeri.
    bool greetedOnce; //true/false selamlaşma yapıldı mı diye.

    NavMeshAgent agent; //navmesh objesi
    Animator anim; //animasyon objesi
    Transform talkPoint;  // talkPoint konum objesi
    Transform exitPoint; // exitPoint konum objesi

    float animSpeed;        // Animator'a verdiğimiz Speed
    float animSpeedVelRef;  // SmoothDamp ref

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); // direk awake'de agent ve anim objesini yakalar oyundan.
        anim = GetComponentInChildren<Animator>();
    }

    public void Init(Transform talk, Transform exit)
    {
        talkPoint = talk;
        exitPoint = exit;
    }

    public void BeginEnterShop() //CustomerSpawner'da çağrılıyor ve giriş bilgileride Init tarafından veriliyor zaten.
    {
        state = State.Entering;

        // Agent tuning (istersen inspector’dan da ayarla)
        agent.autoBraking = true;
        // agent.acceleration = 8f;
        // agent.stoppingDistance = 0.9f;

        agent.isStopped = false;
        agent.SetDestination(talkPoint.position);
    }

    void Update()
    {
        UpdateAnimatorSpeed();

        if (state == State.Entering)
        {
            // Asıl "vardım" koşulu: path hazır + remaining küçük + desiredVelocity çok küçük
            bool arrived =
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + arriveEpsilon &&
                agent.desiredVelocity.sqrMagnitude < 0.01f; // neredeyse durmak istiyor

            if (arrived)
            {
                // Burada agent’ı zorla 0’a çakmıyoruz; sadece anim’i 0’a oturtuyoruz.
                state = State.WaitingPlayer;
                // şimdilik debug dialogue
                if (!greetedOnce)
                {
                 greetedOnce = true;
                 Debug.Log("[NPC] Lastik istiyorum.");
                }
                // Invoke(nameof(LeaveShop), 4f);
            }
        }
        if(state == State.WaitingPlayer && player != null)
        {
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f; // sadece yatay dönüş aypıyoruz.
            if (toPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lookTurnSpeed *Time.deltaTime);
            }
        }

    }

    void UpdateAnimatorSpeed()
    {
        if (!anim || !agent) return;

        // 1) Hedef hız: normalde desiredVelocity (daha stabil)
        float target = agent.desiredVelocity.magnitude;

        // 2) Hedefe çok yaklaştıysan Speed'i 0'a snap et (jitter'i bitirir)
        bool shouldSnapToZero =
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + arriveEpsilon;

        if (shouldSnapToZero) target = 0f;

        // 3) Yumuşat (senin istediğin "yavaş yavaş" iniş)
        animSpeed = Mathf.SmoothDamp(animSpeed, target, ref animSpeedVelRef, speedDampTime);

        anim.SetFloat("Speed", animSpeed);
    }

    public void LeaveShop()
    {
        Debug.Log("[NPC] LeaveShop CALLED");

        state = State.Leaving;
        agent.isStopped = false;
        agent.ResetPath();

        agent.SetDestination(exitPoint.position);
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    public bool CanAccept()
    {
        return state == State.WaitingPlayer;
    }

    public void AcceptRequest()
    {
        if(!CanAccept()) return;

        Debug.Log("[NPC] Tamam usta , lastikleri sökelim.");

        var flow = FindObjectOfType<CustomerJobFlow>();
        if (flow != null) flow.StartJobFor(this);

    }


}
