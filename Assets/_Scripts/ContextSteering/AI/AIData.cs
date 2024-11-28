using System.Collections.Generic;
using UnityEngine;

public class AIData : MonoBehaviour
{
    public List<Transform> targets = null;
    public Collider2D[] obstacles = null;

    public Transform currentTarget;

    [SerializeField]
    public float maxChaseDistance = 10f;  // Default chase distance, adjust as needed

    public int GetTargetsCount() => targets == null ? 0 : targets.Count;
}
