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
    public class FireballHandling : MonoBehaviour
    {
        // For spell
        public static List<Item> projectileList = new List<Item>();
        public static List<Item> projectileListRightHand = new List<Item>();
        public static List<Item> projectileListLeftHand = new List<Item>();

        public static Vector3 positionCenterPointRight;
        public static Vector3 deltaPositionCenterPointRight;
        public static Vector3 distanceCenterFromGripRight;
        public static Vector3 positionCenterPointLeft;
        public static Vector3 deltaPositionCenterPointLeft;
        public static Vector3 distanceCenterFromGripLeft;
        public Vector3 positionOfBody;
        public Vector3 velocity;
        public bool previousStateButtonUsePressedRight = false;
        public bool currentStateButtonUsePressedRight = false;
        public bool previousStateButtonUsePressedLeft = false;
        public bool currentStateButtonUsePressedLeft = false;
        public static bool isCenterPointCreatedRight = false;
        public static bool isCenterPointCreatedLeft = false;
        public static bool leftHandEquipped = false;
        public static bool rightHandEquipped = false;
        public static bool isLeftHandCasting = false;
        public static bool isRightHandCasting = false;
        private bool moveAllProjectile = false;
        private const string spellName = "FireballHandlingId";
        private FireballHandlingVFX fireballHandlingVFX;

        // Center point FX
        public GameObject centerPointGameObject;
        public Transform centerPointTrans;


        public void Awake()
        {

        }

        public void Start()
        {
            // NAH use it like a class ! (one for each hand)
            fireballHandlingVFX = GameManager.local.gameObject.AddComponent<FireballHandlingVFX>();
        }

        public void FixedUpdate()
        {
            if (fireballHandlingVFX == null)
            {
                fireballHandlingVFX = GameManager.local.gameObject.AddComponent<FireballHandlingVFX>();
            }
            if (Player.currentCreature != null)
            {
                if (spellName == Player.currentCreature.handLeft?.caster?.spellInstance?.id)
                {
                    leftHandEquipped = true;
                }
                else
                {
                    leftHandEquipped = false;
                }
                if (spellName == Player.currentCreature.handRight?.caster?.spellInstance?.id)
                {
                    rightHandEquipped = true;
                }
                else
                {
                    rightHandEquipped = false;
                }
                if (Player.currentCreature.handLeft?.caster?.isFiring == true)
                {
                    isLeftHandCasting = true;
                }
                else
                {
                    isLeftHandCasting = false;
                }
                if (Player.currentCreature.handRight?.caster?.isFiring == true)
                {
                    isRightHandCasting = true;
                }
                else
                {
                    isRightHandCasting = false;
                }
            }
        }

        public void Update()
        {
            if (Player.local?.creature?.isKilled == true)
            {
                foreach (Item elements in projectileList)
                {
                    elements.Despawn();
                }
                projectileList.Clear();
                projectileListRightHand.Clear();
                projectileListLeftHand.Clear();
            }
            GameManager.local.StartCoroutine(ManagedTimeSpawned());
            //GameManager.local.StartCoroutine(ManagedRemoveList());

            if (leftHandEquipped)
            {
                if (PlayerControl.GetHand(Side.Left).gripPressed == true && !isLeftHandCasting && Player.currentCreature.handLeft.grabbedHandle == null && Player.currentCreature.handLeft.caster?.telekinesis?.catchedHandle == false)
                {
                    currentStateButtonUsePressedLeft = PlayerControl.GetHand(Side.Left).gripPressed;
                    // At rising edge
                    if (currentStateButtonUsePressedLeft != previousStateButtonUsePressedLeft && previousStateButtonUsePressedLeft == false && isCenterPointCreatedLeft == false)
                    {
                        CreateCenterPointLeft();
                        previousStateButtonUsePressedLeft = currentStateButtonUsePressedLeft;
                    }
                    if (isCenterPointCreatedLeft)
                    {
                        deltaPositionCenterPointLeft = positionCenterPointLeft - positionOfBody + Player.local.creature.transform.position;
                        distanceCenterFromGripLeft = Player.currentCreature.handLeft.grip.position - deltaPositionCenterPointLeft;
                        // Movement mode
                        MoveProjectiles(distanceCenterFromGripLeft, Side.Left);
                    }
                }
            }

            if (rightHandEquipped)
            {
                if (PlayerControl.GetHand(Side.Right).gripPressed == true && isRightHandCasting && Player.currentCreature.handRight.grabbedHandle == null && Player.currentCreature.handRight.caster?.telekinesis?.catchedHandle == false)
                {
                    currentStateButtonUsePressedRight = PlayerControl.GetHand(Side.Right).gripPressed;
                    // At rising edge
                    if (currentStateButtonUsePressedRight != previousStateButtonUsePressedRight && previousStateButtonUsePressedRight == false && isCenterPointCreatedRight == false)
                    {
                        CreateCenterPointRight();
                        previousStateButtonUsePressedRight = currentStateButtonUsePressedRight;
                    }
                    if (isCenterPointCreatedRight)
                    {
                        deltaPositionCenterPointRight = positionCenterPointRight - positionOfBody + Player.local.creature.transform.position;
                        distanceCenterFromGripRight = Player.currentCreature.handRight.grip.position - deltaPositionCenterPointRight;
                        // Movement mode
                        moveAllProjectile = true;
                        MoveAllProjectiles(distanceCenterFromGripRight);
                    }
                    else
                    {
                        moveAllProjectile = false;
                    }
                }
                if (PlayerControl.GetHand(Side.Right).gripPressed == true && !isRightHandCasting && Player.currentCreature.handRight.grabbedHandle == null && Player.currentCreature.handRight.caster?.telekinesis?.catchedHandle == false)
                {
                    currentStateButtonUsePressedRight = PlayerControl.GetHand(Side.Right).gripPressed;
                    // At rising edge
                    if (currentStateButtonUsePressedRight != previousStateButtonUsePressedRight && previousStateButtonUsePressedRight == false && isCenterPointCreatedRight == false)
                    {
                        CreateCenterPointRight();
                        previousStateButtonUsePressedRight = currentStateButtonUsePressedRight;
                    }
                    if (isCenterPointCreatedRight)
                    {
                        deltaPositionCenterPointRight = positionCenterPointRight - positionOfBody + Player.local.creature.transform.position;
                        distanceCenterFromGripRight = Player.currentCreature.handRight.grip.position - deltaPositionCenterPointRight;
                        // Movement mode
                        MoveProjectiles(distanceCenterFromGripRight, Side.Right);
                    }
                    moveAllProjectile = false;
                }
                if (PlayerControl.GetHand(Side.Right).gripPressed == false || !isRightHandCasting)
                {
                    moveAllProjectile = false;
                }
            }
            if (leftHandEquipped || rightHandEquipped)
            {
                if ((PlayerControl.GetHand(Side.Right).gripPressed == false || Player.currentCreature.handRight.grabbedHandle != null || Player.currentCreature.handRight.caster?.telekinesis?.catchedHandle == true) && moveAllProjectile == false)
                {
                    previousStateButtonUsePressedRight = false;
                    isCenterPointCreatedRight = false;
                    StopProjectiles(Side.Right);
                }
                if ((PlayerControl.GetHand(Side.Left).gripPressed == false || Player.currentCreature.handLeft.grabbedHandle != null || Player.currentCreature.handLeft.caster?.telekinesis?.catchedHandle == true) && moveAllProjectile == false)
                {
                    previousStateButtonUsePressedLeft = false;
                    isCenterPointCreatedLeft = false;
                    StopProjectiles(Side.Left);
                }
            }
            if (!leftHandEquipped && moveAllProjectile == false)
            {
                previousStateButtonUsePressedLeft = false;
                isCenterPointCreatedLeft = false;
                StopProjectiles(Side.Left);
            }
            if (!rightHandEquipped)
            {
                moveAllProjectile = false;
                previousStateButtonUsePressedRight = false;
                isCenterPointCreatedRight = false;
                StopProjectiles(Side.Right);
            }
        }

        private void CreateCenterPointRight()
        {
            positionCenterPointRight = Player.currentCreature.handRight.grip.position;
            positionOfBody = Player.local.creature.transform.position;
            isCenterPointCreatedRight = true;
        }
        private void CreateCenterPointLeft()
        {
            positionCenterPointLeft = Player.currentCreature.handLeft.grip.position;
            positionOfBody = Player.local.creature.transform.position;
            isCenterPointCreatedLeft = true;
        }

        public static void AddToList(Item itemProjectile, Side side)
        {
            itemProjectile.OnDespawnEvent += () => RemoveFromList(itemProjectile);
            itemProjectile.ignoredRagdoll = Player.currentCreature.ragdoll;
            if (side == Side.Left)
            {
                projectileListLeftHand.Add(itemProjectile);
            }
            if (side == Side.Right)
            {
                projectileListRightHand.Add(itemProjectile);
            }
            projectileList.Add(itemProjectile);
        }

        public void MoveAllProjectiles(Vector3 direction)
        {
            foreach (Item elements in projectileList)
            {
                elements.Throw();
                elements.rb.velocity = direction * 40.0f;
            }
        }

        public void MoveProjectiles(Vector3 direction, Side side)
        {
            if (side == Side.Left)
            {
                foreach (Item elements in projectileListLeftHand)
                {
                    elements.Throw();
                    elements.rb.velocity = direction * 40.0f;
                }
            }
            if (side == Side.Right)
            {
                foreach (Item elements in projectileListRightHand)
                {
                    elements.Throw();
                    elements.rb.velocity = direction * 40.0f;
                }
            }
        }

        public void StopProjectiles(Side side)
        {
            if (side == Side.Left)
            {
                foreach (Item elements in projectileListLeftHand)
                {
                    elements.rb.velocity = Vector3.zero;
                    elements.isFlying = true;
                    elements.isThrowed = true;
                    elements.rb.useGravity = false;
                }
            }
            if (side == Side.Right)
            {
                foreach (Item elements in projectileListRightHand)
                {
                    elements.rb.velocity = Vector3.zero;
                    elements.isFlying = true;
                    elements.isThrowed = true;
                    elements.rb.useGravity = false;
                }
            }
        }

        public static void RemoveFromList(Item itemProjectile)
        {
            for (int index = projectileList.Count - 1; index >= 0; --index)
            {
                if (itemProjectile.GetHashCode() == projectileList[index].GetHashCode())
                {
                    projectileList.RemoveAt(index);
                }
            }
            if (projectileListLeftHand.Count != 0)
            {
                for (int index = projectileListLeftHand.Count - 1; index >= 0; --index)
                {
                    if (itemProjectile.GetHashCode() == projectileListLeftHand[index].GetHashCode())
                    {
                        projectileListLeftHand.RemoveAt(index);
                    }
                }
            }
            if (projectileListRightHand.Count != 0)
            {
                for (int index = projectileListRightHand.Count - 1; index >= 0; --index)
                {
                    if (itemProjectile.GetHashCode() == projectileListRightHand[index].GetHashCode())
                    {
                        projectileListRightHand.RemoveAt(index);
                    }
                }
            }
        }
        // Break is ugly
        IEnumerator ManagedTimeSpawned()
        {
            if (projectileList.Count != 0)
            {
                foreach (Item elements in projectileList)
                {
                    //Debug.Log("FireballHandling : Time : " + Time.time + "; elements spawn time : " + elements.spawnTime.ToString() + " ;");
                    if ((Time.time - elements.spawnTime) >= (60.0f * 3.0f))
                    {
                        RemoveFromList(elements);
                        elements.Despawn();
                        break;
                    }
                }
            }
            yield return null;
        }

        /*IEnumerator ManagedRemoveList()
        {
            if (projectileList.Count != 0)
            {
                foreach (Item elements in projectileList)
                {
                    FireballHandlingFireballClass component = elements.GetComponent<FireballHandlingFireballClass>();
                    if (component.remove == true)
                    {
                        RemoveFromList(elements);
                        elements.Despawn();
                        break;
                    }
                }
            }
            yield return null;
        }*/
    }
}