using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Bullet : MonoBehaviour, IShootable
{
    [SerializeField] private Rigidbody myRigidbody;

    [SerializeField] private float shootSpeed;

    protected Transform tr;
    protected Transform magazineTr;

    protected virtual void Awake()
    {
        tr = transform;
        myRigidbody = myRigidbody ? myRigidbody : GetComponent<Rigidbody>();
    }

    public virtual void Shoot(Vector3 target)
    {
        myRigidbody.isKinematic = false;
        tr.position = magazineTr.position;
        gameObject.SetActive(true);
        Vector3 dir = (target - myRigidbody.position).normalized;
        tr.rotation = Quaternion.LookRotation(dir);
        myRigidbody.velocity = dir * shootSpeed;
    }

    public virtual void HitTarget(Vector3 position)
    {
        myRigidbody.isKinematic = true;
    }

    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }

    public virtual void Setup(Transform magazine, float shootSpeed)
    {
        tr = transform;
        gameObject.SetActive(false);
        magazineTr = magazine;
        this.shootSpeed = shootSpeed;
        tr.parent = null;
    }


    internal virtual void OnCreated()
    {
        gameObject.SetActive(false);
    }

    public virtual void SetSeedType(PlantType type)
    {
       
    }

    public abstract void OnTriggerEnter(Collider other);


    public abstract void OnCollisionEnter(Collision collision);
    
}
