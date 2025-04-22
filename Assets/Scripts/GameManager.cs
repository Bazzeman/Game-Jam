using Bas.Pennings.DevTools;
using UnityEngine;

public class GameManager : AbstractSingleton<GameManager>
{
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playerParentObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }
    }

    private void OnValidate() => _playerParentObject = _playerParentObject != null ? _playerParentObject : transform;

    private void ResetPlayer()
    {
        Destroy(_playerObject);
        _playerObject = InstantiatePlayer(_playerParentObject);
    }

    private GameObject InstantiatePlayer(Transform transform)
        => Instantiate(
            _playerPrefab,
            transform.position,
            transform.rotation,
            _playerParentObject.transform);
}
