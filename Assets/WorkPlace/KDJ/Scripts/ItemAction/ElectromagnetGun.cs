using UnityEngine;

public class ElectromagnetGun : ToolAction
{
    [SerializeField] private GameObject _electroEffect;

    private Collider[] _colls = new Collider[10];
    private LayerMask _layerMask = 1 << 8;
    private bool _isMining = false;
    private GameObject _electroEffectInstance;

    private void Update()
    {
        if (InputManager.Instance.IsUsingTool && _electroEffectInstance == null)
        {
            _electroEffectInstance = Instantiate(_electroEffect, PlayerManager.Instance.InHandItem.transform.position, Quaternion.identity);
        }
        else if(!InputManager.Instance.IsUsingTool && _electroEffectInstance != null)
        {
            Destroy(_electroEffectInstance);
        }

        if (_electroEffectInstance != null)
        {
            _electroEffectInstance.transform.position = PlayerManager.Instance.InHandItem.transform.position;
        }
    }

    public override void Action(int power)
    {
        // ±¤¹° Ã¼±¼ ·ÎÁ÷
        int count = Physics.OverlapSphereNonAlloc(PlayerManager.Instance.InHandItem.transform.position
            + PlayerManager.Instance.Player.transform.forward * 1.5f, 3f, _colls, _layerMask);

        if (count > 0)
        {
            _isMining = true;
            for (int i = 0; i < count; i++)
            {
                bool s = _colls[i].TryGetComponent<MineableResource>(out MineableResource ore);
                if (!s)
                {
                    continue;
                }
                ore.TakeMiningDamage(power);
            }
        }
        else
        {
            _isMining = false;
        }

        PlayerManager.Instance.Player.PlayerInteraction.InteractAllNearItem();
    }

    private void OnDrawGizmos()
    {
        if (PlayerManager.Instance.InHandItem == null)
            return;

        if (_isMining)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(PlayerManager.Instance.InHandItem.transform.position
                + PlayerManager.Instance.Player.transform.forward * 1.5f, 3f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(PlayerManager.Instance.InHandItem.transform.position
                + PlayerManager.Instance.Player.transform.forward * 1.5f, 3f);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(PlayerManager.Instance.Player.transform.position, 4f);
    }
}

