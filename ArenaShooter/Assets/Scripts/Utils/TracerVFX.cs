using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TracerVFX : MonoBehaviour
{
    public float life = 0.05f;
    public float width = 0.01f;

    LineRenderer lr;
    float t;

    public void Show(Vector3 start, Vector3 end)
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        t = life;
        gameObject.SetActive(true);
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (lr)
        {
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.View; // billboard-ish
        }
        gameObject.SetActive(false);
    }

    void Update()
    {
        if ((t -= Time.deltaTime) <= 0f)
            gameObject.SetActive(false);
    }
}
