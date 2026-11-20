using DienGame.Client;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>Rewards are physical collectible sprites and work in all three class scenes.</summary>
    public sealed class WorldStageLoot : MonoBehaviour
    {
        private void OnEnable() { EnemyHealth.AnyEnemyDied+=EnemyDied; }
        private void OnDisable() { EnemyHealth.AnyEnemyDied-=EnemyDied; }
        private void EnemyDied(EnemyHealth enemy)
        {
            if(enemy==null) return;
            WorldInventory.RecordKill();
            int stage=Mathf.Clamp(LobbySessionService.SelectedStage,1,5);
            float roll=Random.value;
            if(roll<.07f) Spawn(enemy.transform.position,false,1);
            else if(roll<.38f) Spawn(enemy.transform.position,true,8+stage*5+Random.Range(0,10));
        }
        private static void Spawn(Vector3 position,bool isGold,int amount)
        {
            GameObject go=new GameObject(isGold?"Loot_Gold":"Loot_GearFragment");
            go.transform.position=new Vector3(position.x,position.y,-.01f);
            SpriteRenderer sr=go.AddComponent<SpriteRenderer>();
            sr.sprite=Resources.Load<Sprite>(isGold?"World/UI/CoinPickup":"World/UI/CrystalPickup");
            sr.sortingOrder=950;
            WorldPickup pickup=go.AddComponent<WorldPickup>();
            pickup.Configure(isGold,amount);
        }
    }

    public sealed class WorldPickup : MonoBehaviour
    {
        private bool _gold;
        private int _amount;
        private Transform _player;
        private Vector3 _base;
        private float _age;
        public void Configure(bool gold,int amount)
        {
            _gold=gold;_amount=amount;_base=transform.position;
        }
        private void Start()
        {
            PlayerMovement p=FindFirstObjectByType<PlayerMovement>();
            if(p!=null)_player=p.transform;
            Sprite s=GetComponent<SpriteRenderer>().sprite;
            if(s!=null) transform.localScale=Vector3.one*(.52f/(s.rect.width/s.pixelsPerUnit));
        }
        private void Update()
        {
            _age+=Time.deltaTime;
            transform.position=_base+new Vector3(0f,Mathf.Sin(_age*3f)*.08f,0f);
            if(_player==null)
            {
                PlayerMovement p=FindFirstObjectByType<PlayerMovement>();if(p!=null)_player=p.transform;
            }
            if(_player!=null && Vector2.Distance(transform.position,_player.position)<.93f)
            {
                if(_gold)MetaProgressService.GrantGold(_amount);
                else WorldInventory.AddFragments(_amount);
                Destroy(gameObject);
            }
            if(_age>35f) Destroy(gameObject);
        }
    }
}
