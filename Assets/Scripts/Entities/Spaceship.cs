using System;
using UnityEngine;

namespace Entities
{
    public class Spaceship : Entity
    {
        [SerializeField] protected float speed;
        [SerializeField] private GameObject missilePrefab;
        [SerializeField] private Sprite missileSprite;
        [SerializeField] private int missileLives;
        [SerializeField] private float missileSpeed;
        [SerializeField] private LayerMask layerToHit;
        private Missile _missile;
        public void Shoot(Vector2 direction, string myLayer)
        {
            if (_missile) return;
            _missile = Instantiate(missilePrefab, transform.position, Quaternion.identity).GetComponent<Missile>();
            _missile.Setup(missileLives, missileSprite, direction, missileSpeed, Sr.color, layerToHit, myLayer);
        }
    }
}