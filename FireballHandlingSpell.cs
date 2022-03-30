using System.Collections;
using ThunderRoad;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using SnippetCode;
using System.Linq;
using System.Collections.Generic;

namespace FireballHandling
{
    public class FireballHandlingSpell : SpellCastCharge
    {

        public override void Load(SpellCaster spellCaster, Level level)
        {
            base.Load(spellCaster, level);
        }


        public override void Fire(bool active)
        {
            base.Fire(active);
            if (active)
            {

            }
            else
            {
                if (currentCharge >= 0.3f && (!PlayerControl.GetHand(Side.Right).gripPressed && FireballHandling.isRightHandCasting || !PlayerControl.GetHand(Side.Left).gripPressed && FireballHandling.isLeftHandCasting))
                {
                    SpawnItemProjectile();
                }
            }
        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();
        }

        private void SpawnItemProjectile()
        {
            Catalog.GetData<ItemData>("DynamicProjectileFireballHandling").SpawnAsync(projectile =>
            {
                projectile.disallowDespawn = true;
                projectile.transform.position = spellCaster.ragdollHand.grip.position + 0.2f * spellCaster.ragdollHand.PointDir();
                projectile.rb.useGravity = false;
                projectile.rb.velocity = Vector3.zero;
                foreach (CollisionHandler collisionHandler in projectile.collisionHandlers)
                {
                    foreach (Damager damager in collisionHandler.damagers)
                        damager.Load(Catalog.GetData<DamagerData>("Fireball"), collisionHandler);
                }
                ItemMagicProjectile component = projectile.GetComponent<ItemMagicProjectile>();
                if (component)
                {
                    component.guided = false;
                    component.speed = 0;
                    component.Fire(Vector3.zero, Catalog.GetData<EffectData>("SpellFireball"));
                }
                projectile.isThrowed = true;
                projectile.isFlying = true;
                projectile.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
                FireballHandling.AddToList(projectile, spellCaster.ragdollHand.side);
            });
        }
        public override void Unload()
        {
            base.Unload();
        }
    }
}