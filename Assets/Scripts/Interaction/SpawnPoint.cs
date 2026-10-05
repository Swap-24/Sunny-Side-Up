using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnPointID;
    [SerializeField] private bool isDefaultSpawn;

    public string SpawnPointID => spawnPointID;
    public bool IsDefaultSpawn => isDefaultSpawn;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.6f);
    }
}
