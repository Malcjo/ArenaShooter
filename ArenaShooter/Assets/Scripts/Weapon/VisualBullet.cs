using UnityEngine;

public class VisualBullet : MonoBehaviour
{
    Vector3 _dir;
    float _speed;
    float _life;
    float _t;

    // optional: fade or scale out
    public AnimationCurve scaleOverLife = AnimationCurve.Linear(0, 1, 1, 1);

    public void Init(Vector3 startPos, Vector3 dir, float speed, float lifeSeconds)
    {
        transform.position = startPos;
        _dir = dir.normalized;
        _speed = speed;
        _life = Mathf.Max(0.01f, lifeSeconds);
        _t = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        _t += dt;
        if (_t >= _life) { Destroy(gameObject); return; }

        transform.position += _dir * _speed * dt;
        /*
        if (scaleOverLife != null)
        {
            float k = Mathf.Clamp01(_t / _life);
            float s = scaleOverLife.Evaluate(k);
            transform.localScale = new Vector3(s, s, s);
        }
        */
    }
}
