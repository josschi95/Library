using UnityEngine;

public class ParticleReturnToPool : MonoBehaviour
{
    new public string tag;
    public void OnParticleSystemStopped()
    {
        ObjectPooler.ReturnToPool_Static(tag, gameObject);
    }
}
