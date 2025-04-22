using Bas.Pennings.DevTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneManager : AbstractSingleton<CloneManager>
{
    [SerializeField] private float _cloneSpawningDelay = 1;

    [Header("References")]
    [SerializeField] private GameObject _clonePrefab;
    [SerializeField] private Transform _clonesParentObject;

    private List<GameObject> cloneObjects = new();
    private Coroutine cloneSpawningRoutine;
    private int cloneCount = 1; // Should be replaced with the how many iterations there are.

    private void Start() => StartCloneSpawning();

    private void OnValidate()
    {
        _clonesParentObject = _clonesParentObject != null ? _clonesParentObject : transform;
        if (_cloneSpawningDelay <= 0) _cloneSpawningDelay = .01f;
    }

    public void ResetClones()
    {
        StopCloneSpawning();
        DestroyClones();
        StartCloneSpawning();
        cloneCount++;
    }

    private void DestroyClones()
    {
        foreach (var clone in cloneObjects) Destroy(clone);
        cloneObjects = new();
    }

    private void StopCloneSpawning()
    {
        if (cloneSpawningRoutine != null)
        {
            StopCoroutine(cloneSpawningRoutine);
            cloneSpawningRoutine = null;
        }
    }

    private void StartCloneSpawning()
    {
        cloneSpawningRoutine = StartCoroutine(InstantiateClonesWithDelay());

        IEnumerator InstantiateClonesWithDelay()
        {
            for (int i = 0; i < cloneCount; i++, i++)
            {
                var clone = InstantiateClone(_clonesParentObject);
                cloneObjects.Add(clone);
                yield return new WaitForSeconds(_cloneSpawningDelay);
            }
        }
    }

    private GameObject InstantiateClone(Transform transform)
        => Instantiate(
            _clonePrefab,
            transform.position,
            transform.rotation,
            _clonesParentObject.transform);
}
