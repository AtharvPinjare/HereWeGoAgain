using System.Collections;
using UnityEngine;

public class AttackingDollsAnomaly : AnomalyBase
{
    [System.Serializable]
    public class DollAgent
    {
        public GameObject dollGameObject;
        public Transform[] waypoints;
        public Collider contactCollider;
    }

    [SerializeField] private DollAgent[] dolls = new DollAgent[5];
    [SerializeField] private float baseMovementSpeed = 1.2f;
    [SerializeField] private float speedIncreasePerDoll = 0.4f;
    [SerializeField] private float activationInterval = 8.0f;
    [SerializeField] private float contactRadius = 0.5f;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private float hitCooldown = 1.0f;

    private Coroutine activationCoroutine = null;
    private Coroutine[] dollMovementCoroutines;
    private float[] lastHitTimes;
    private bool isFailStateTriggered = false;
    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

    protected override void Awake()
    {
        base.Awake();

        dollMovementCoroutines = new Coroutine[5];
        lastHitTimes = new float[5];
        originalPositions = new Vector3[5];
        originalRotations = new Quaternion[5];

        for (int i = 0; i < lastHitTimes.Length; i++)
        {
            lastHitTimes[i] = -999f;
        }

        if (dolls == null)
        {
            return;
        }

        int captureCount = Mathf.Min(dolls.Length, originalPositions.Length);
        for (int i = 0; i < captureCount; i++)
        {
            if (dolls[i] == null || dolls[i].dollGameObject == null)
            {
                continue;
            }

            Transform dollTransform = dolls[i].dollGameObject.transform;
            originalPositions[i] = dollTransform.position;
            originalRotations[i] = dollTransform.rotation;
        }
    }

    public override void Activate()
    {
        if (data == null)
        {
            return;
        }

        data.isActive = true;
        data.isResolved = false;
        isFailStateTriggered = false;

        for (int i = 0; i < lastHitTimes.Length; i++)
        {
            lastHitTimes[i] = -999f;
        }

        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
        }

        activationCoroutine = StartCoroutine(ActivateDolls());
    }

    public override void Resolve()
    {
        if (data == null)
        {
            return;
        }

        data.isActive = false;
        data.isResolved = true;

        StopAllDollCoroutines();
        StartCoroutine(ReturnDollsToStart());
        EventBus.OnAnomalyResolved?.Invoke(data.id);
    }

    public override void TriggerFailState()
    {
        if (isFailStateTriggered)
        {
            return;
        }

        isFailStateTriggered = true;

        if (data != null)
        {
            data.isActive = false;
        }

        StopAllDollCoroutines();
        // Immediately return dolls to start positions
        for (int i = 0; i < dolls.Length; i++)
        {
            if (dolls[i] == null ||
                dolls[i].dollGameObject == null) continue;
            dolls[i].dollGameObject.transform.position =
                originalPositions[i];
            dolls[i].dollGameObject.transform.rotation =
                originalRotations[i];
        }


        FreezeAllDolls();

        if (data != null)
        {
            EventBus.OnAnomalyFailState?.Invoke(data.id);
        }

        StartCoroutine(BlackoutAndReset(2.0f));
    }

    public override void ResetAnomaly()
    {
        StopAllDollCoroutines();
        StopAllCoroutines();

        isFailStateTriggered = false;

        for (int i = 0; i < lastHitTimes.Length; i++)
        {
            lastHitTimes[i] = -999f;
        }

        if (dolls != null)
        {
            int restoreCount = Mathf.Min(dolls.Length, originalPositions.Length);
            for (int i = 0; i < restoreCount; i++)
            {
                if (dolls[i] == null || dolls[i].dollGameObject == null)
                {
                    continue;
                }

                Transform dollTransform = dolls[i].dollGameObject.transform;
                dollTransform.position = originalPositions[i];
                dollTransform.rotation = originalRotations[i];
            }
        }

        if (data != null)
        {
            data.isActive = false;
            data.isResolved = false;
        }
    }

    private IEnumerator ActivateDolls()
    {
        if (dolls == null)
        {
            activationCoroutine = null;
            yield break;
        }

        for (int i = 0; i < dolls.Length; i++)
        {
            if (dolls[i] == null || dolls[i].dollGameObject == null)
            {
                continue;
            }

            float speed = baseMovementSpeed + (i * speedIncreasePerDoll);

            if (i < dollMovementCoroutines.Length && dollMovementCoroutines[i] != null)
            {
                StopCoroutine(dollMovementCoroutines[i]);
            }

            if (i < dollMovementCoroutines.Length)
            {
                dollMovementCoroutines[i] = StartCoroutine(MoveDoll(i, speed));
            }

            yield return new WaitForSeconds(activationInterval);
        }

        activationCoroutine = null;
    }

    private IEnumerator MoveDoll(int dollIndex, float speed)
    {
        if (dolls == null || dollIndex < 0 || dollIndex >= dolls.Length)
        {
            yield break;
        }

        DollAgent doll = dolls[dollIndex];
        if (doll == null || doll.dollGameObject == null)
        {
            yield break;
        }

        if (doll.waypoints == null || doll.waypoints.Length == 0)
        {
            yield break;
        }

        int currentWaypointIndex = 0;

        while (data != null && data.isActive)
        {
            if (currentWaypointIndex >= doll.waypoints.Length)
            {
                currentWaypointIndex = doll.waypoints.Length - 1;
            }

            Transform targetWaypoint = doll.waypoints[currentWaypointIndex];
            if (targetWaypoint == null)
            {
                yield return null;
                continue;
            }

            Transform dollTransform = doll.dollGameObject.transform;
            Vector3 targetPos = targetWaypoint.position;
            Vector3 currentPos = dollTransform.position;
            float distToWaypoint = Vector3.Distance(currentPos, targetPos);

            if (distToWaypoint > 0.1f)
            {
                Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);
                dollTransform.position = newPos;

                Vector3 dir = (targetPos - currentPos).normalized;
                if (dir != Vector3.zero)
                {
                    // dollTransform.rotation = Quaternion.LookRotation(dir);
                    dollTransform.rotation = originalRotations[dollIndex];
                }
            }
            else
            {
                currentWaypointIndex++;
            }

            if (playerObject != null && dollIndex < lastHitTimes.Length)
            {
                float dist = Vector3.Distance(dollTransform.position, playerObject.transform.position);
                if (dist <= contactRadius && Time.time - lastHitTimes[dollIndex] >= hitCooldown)
                {
                    lastHitTimes[dollIndex] = Time.time;
                    DealDamageToPlayer();
                }
            }

            yield return null;
        }

        if (dollIndex >= 0 && dollIndex < dollMovementCoroutines.Length)
        {
            dollMovementCoroutines[dollIndex] = null;
        }
    }

    private IEnumerator ReturnDollsToStart()
    {
        float t = 0f;
        const float returnDuration = 1.5f;

        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float lerpT = t / returnDuration;

            if (dolls != null)
            {
                int returnCount = Mathf.Min(dolls.Length, originalPositions.Length);
                for (int i = 0; i < returnCount; i++)
                {
                    if (dolls[i] == null || dolls[i].dollGameObject == null)
                    {
                        continue;
                    }

                    Transform dollTransform = dolls[i].dollGameObject.transform;
                    dollTransform.position = Vector3.Lerp(dollTransform.position, originalPositions[i], lerpT);
                    dollTransform.rotation = Quaternion.Lerp(dollTransform.rotation, originalRotations[i], lerpT);
                }
            }

            yield return null;
        }

        if (dolls == null)
        {
            yield break;
        }

        int finalCount = Mathf.Min(dolls.Length, originalPositions.Length);
        for (int i = 0; i < finalCount; i++)
        {
            if (dolls[i] == null || dolls[i].dollGameObject == null)
            {
                continue;
            }

            Transform dollTransform = dolls[i].dollGameObject.transform;
            dollTransform.position = originalPositions[i];
            dollTransform.rotation = originalRotations[i];
        }
    }

    private void StopAllDollCoroutines()
    {
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }

        if (dollMovementCoroutines == null)
        {
            return;
        }

        for (int i = 0; i < dollMovementCoroutines.Length; i++)
        {
            if (dollMovementCoroutines[i] != null)
            {
                StopCoroutine(dollMovementCoroutines[i]);
                dollMovementCoroutines[i] = null;
            }
        }
    }

    private void FreezeAllDolls()
    {
        if (data != null)
        {
            data.isActive = false;
        }
    }
}
