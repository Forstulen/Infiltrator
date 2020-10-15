using UnityEngine;
using System.Collections;

abstract public class AttackBaseScript : MonoBehaviour {

    [System.NonSerialized]
    public bool IsAttacking;
    [System.NonSerialized]
    public bool IsWalking;
    [System.NonSerialized]
    public bool IsRunning;
    [System.NonSerialized]
    public bool IsWandering;
    [System.NonSerialized]
    public bool IsRotating;

    protected void AttackTag()
    {
        this.gameObject.tag = "RedEnemy";
    }

    protected void AlertTag()
    {
        this.gameObject.tag = "YellowEnemy";
    }

    protected void NonAttackTag()
    {
        this.gameObject.tag = "Enemy";
    }

}
