using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [System.Serializable]
    public class ScatterableObject
    {
        public Transform target;
        [HideInInspector] public Vector3 originalPosition;
        [HideInInspector] public Quaternion originalRotation;
        public Vector3 scatterBoundsCenter;
        public Vector3 scatterBoundsSize = new Vector3(3f, 0f, 3f);
    }

    [SerializeField] private ScatterableObject[] scatterableObjects;

    private void Awake()
    {
        if (scatterableObjects == null)
        {
            return;
        }

        foreach (ScatterableObject scatterableObject in scatterableObjects)
        {
            if (scatterableObject == null || scatterableObject.target == null)
            {
                continue;
            }

            scatterableObject.originalPosition = scatterableObject.target.position;
            scatterableObject.originalRotation = scatterableObject.target.rotation;
        }
    }

    private void OnEnable()
    {
        EventBus.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        EventBus.OnGameStateChanged -= HandleGameStateChanged;
    }

    public void ScatterAllObjects()
    {
        if (scatterableObjects == null)
        {
            return;
        }

        foreach (ScatterableObject scatterableObject in scatterableObjects)
        {
            if (scatterableObject == null || scatterableObject.target == null)
            {
                continue;
            }

            float randomX = Random.Range(
                scatterableObject.scatterBoundsCenter.x - scatterableObject.scatterBoundsSize.x * 0.5f,
                scatterableObject.scatterBoundsCenter.x + scatterableObject.scatterBoundsSize.x * 0.5f);
            float randomZ = Random.Range(
                scatterableObject.scatterBoundsCenter.z - scatterableObject.scatterBoundsSize.z * 0.5f,
                scatterableObject.scatterBoundsCenter.z + scatterableObject.scatterBoundsSize.z * 0.5f);

            scatterableObject.target.position = new Vector3(
                randomX,
                scatterableObject.originalPosition.y,
                randomZ);
            scatterableObject.target.rotation = Random.rotation;
        }
    }

    public void ResetScene()
    {
        if (scatterableObjects == null)
        {
            return;
        }

        foreach (ScatterableObject scatterableObject in scatterableObjects)
        {
            if (scatterableObject == null || scatterableObject.target == null)
            {
                continue;
            }

            scatterableObject.target.position = scatterableObject.originalPosition;
            scatterableObject.target.rotation = scatterableObject.originalRotation;
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.RunStart)
        {
            ResetScene();
        }
    }
}
