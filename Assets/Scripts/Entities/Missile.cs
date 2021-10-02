using System.Collections;
using UnityEngine;

namespace Entities
{
    public class Missile : Entity
    {
        private Vector3 _direction;
        private LayerMask _layerToHit;
        private float _speed;
        private Transform _transform;

        private void Update()
        {
            if (Dead) return;
            var bounds = Box.bounds;
            var hit = Physics2D.Linecast(bounds.min, bounds.max, _layerToHit);
            if (!hit) return;
            OnHit(hit.transform.GetComponent<Entity>(), hit.point);
        }

        private void FixedUpdate()
        {
            if (Dead) return;
            _transform.position += _direction * (_speed * Time.deltaTime);
        }

        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }

        private void OnHit(Entity entity, Vector2 point)
        {
            switch (entity)
            {
                case Spaceship _:
                    entity.TakeDamage(lives);
                    Destroy(gameObject);
                    break;
                case Missile _:
                    entity.transform.position = point;
                    _transform.position = point;
                    entity.TakeDamage(lives);
                    TakeDamage(lives);
                    break;
                case Bunker _:
                    _transform.position = point;
                    TakeDamage(lives);
                    break;
            }
        }

        public void Setup(int lives, Sprite sprite, Vector2 direction, float speed, Color color, LayerMask layerToHit, string myLayer)
        {
            base.Setup(lives, sprite, color);
            _transform = transform;
            _direction = direction;
            _speed = speed;
            _layerToHit = layerToHit;
            gameObject.layer = LayerMask.NameToLayer(myLayer);
        }
    }
}