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

        // Formations
        private GameObject centerFormation;
        private Transform centerFormationTran;
        private bool centerFormationCreated;
        private bool newInputForFormation;
        private bool rotationFormationEnabled = false;


        private int numberOfProjectile = 0;
        private int numFormation = 0;
        private bool startFormation = false;
        private bool finishFormation = false;
        private int numberInFormation = 0;
        private float speedRotation = 5;
        private float durationOfMvt = 2.0f;
        private float radiusFormationCircle;
        private float radiusLimit = 0.75f;
        private bool addRadiusOfProjectileForCircle = true;

        private float valueX, valueY, valueZ;
        private float oriX, oriY, oriZ;
        private Vector3 rotationAxis;
        private Vector3 oriPosition;
        private Vector3 savePosition;
        private float radiusProjectile = 0.0f;

        public void Awake()
        {
            centerFormation = new GameObject();
        }

        public void Start()
        {
            // NAH use it like a class ! (one for each hand)
            if (Player.currentCreature != null)
            {
                fireballHandlingVFX = GameManager.local.gameObject.AddComponent<FireballHandlingVFX>();
            }
        }

        public void FixedUpdate()
        {
            if (fireballHandlingVFX == null)
            {
                if (Player.currentCreature != null)
                {
                    fireballHandlingVFX = GameManager.local.gameObject.AddComponent<FireballHandlingVFX>();
                }
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
                if (PlayerControl.GetHand(Side.Left).gripPressed == true && isLeftHandCasting && Player.currentCreature.handLeft.grabbedHandle == null && Player.currentCreature.handLeft.caster?.telekinesis?.catchedHandle == false)
                {
                    // Works
                    currentStateButtonUsePressedLeft = PlayerControl.GetHand(Side.Left).gripPressed && isLeftHandCasting;
                    // At rising edge
                    if (currentStateButtonUsePressedLeft != previousStateButtonUsePressedLeft && previousStateButtonUsePressedLeft == false && isCenterPointCreatedLeft == false)
                    {
                        newInputForFormation = true;
                        CreateCenterFormation();
                        previousStateButtonUsePressedLeft = currentStateButtonUsePressedLeft;
                    }
                    // WEIRD BEHAVIOR AFTER FORMATIONS !
                }
                if (PlayerControl.GetHand(Side.Left).gripPressed == true && !isLeftHandCasting && Player.currentCreature.handLeft.grabbedHandle == null && Player.currentCreature.handLeft.caster?.telekinesis?.catchedHandle == false)
                {
                    currentStateButtonUsePressedLeft = PlayerControl.GetHand(Side.Left).gripPressed && !isLeftHandCasting;
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
            if(projectileList.Count != 0 && centerFormationCreated == true)
            {
                MasterFormation();
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
            itemProjectile.mainCollisionHandler.OnCollisionStartEvent += (_) => RemoveFromList(itemProjectile);
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
            Debug.Log("FireballHandling : ListTotal : " + projectileList.Count.ToString() + ";");
            Debug.Log("FireballHandling : ListRightHand : " + projectileListRightHand.Count.ToString() + ";");
            Debug.Log("FireballHandling : ListLeftHand : " + projectileListLeftHand.Count.ToString() + ";");
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
            if (projectileList.Count != 0)
            {
                for (int index = projectileList.Count - 1; index >= 0; --index)
                {
                    if (itemProjectile.GetHashCode() == projectileList[index].GetHashCode())
                    {
                        projectileList.RemoveAt(index);
                    }
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

        private void CreateCenterFormation()
        {
            centerFormationTran = centerFormation.transform;
            centerFormationTran.position = Player.local.head.transform.position + Player.local.head.transform.forward * 2f;
            centerFormationCreated = true;
        }

        private void MasterFormation()
        {
            if (newInputForFormation == true)
            {
                oriX = centerFormationTran.position.x;
                oriY = centerFormationTran.position.y;
                oriZ = centerFormationTran.position.z;
                //Debug.Log("FireballHandling : Position centerFormation : Position X : " + oriX.ToString() + "; Position Y : " + oriY.ToString() + "; Position Z : " + oriZ.ToString() + ";");
                oriPosition = centerFormationTran.position;
                rotationAxis = Player.local.head.transform.forward;
                radiusProjectile = .15f;
                radiusLimit = 0.3f + radiusProjectile / 2f;
                startFormation = true;
                numberOfProjectile = projectileList.Count();
                finishFormation = false;
                numberInFormation = 0;
                CalcFormation();
                newInputForFormation = false;
            }
            if (startFormation == true)
            {
                switch (numFormation)
                {
                    case 0:
                        ResetList();
                        break;
                    case 1:
                        FormationCube();
                        centerFormationCreated = false;
                        break;
                    case 2:
                        FormationCircle();
                        centerFormationCreated = false;
                        break;
                    default:
                        ResetList();
                        break;
                }
            }

        }

        private void CalcFormation()
        {
            numFormation++;
            if (numFormation > 2 || numFormation < 0)
            {
                numFormation = 0;
            }
        }

        private void FormationCube()
        {
            speedRotation = 5;
            if (startFormation == true && projectileList.Count != 0)
            {
                if (finishFormation == false)
                {
                    int numI = 0;
                    int numJ = 0;
                    int numH = 0;
                    int[] tab = new int[3];
                    tab = CalculateDimensionsForCube();
                    numI = tab[0];
                    numJ = tab[1];
                    numH = tab[2];
                    // number in one depth
                    for (int i = 0; i < numI; i++)
                    {
                        // number in one column
                        for (int j = 0; j < numJ; j++)
                        {
                            // number in one line
                            for (int h = 0; h < numH; h++)
                            {
                                if (i <= 0)
                                {
                                    valueX = 0;
                                }
                                else if (i % 2 == 1 && i != 0)
                                {
                                    valueX = (i + 1) / 2;
                                }
                                else
                                {
                                    valueX = -i / 2;
                                }
                                if (j <= 0)
                                {
                                    valueY = 0;
                                }
                                else if (j % 2 == 1 && j != 0)
                                {
                                    valueY = (j + 1) / 2;
                                }
                                else
                                {
                                    valueY = -j / 2;
                                }
                                if (h <= 0)
                                {
                                    valueZ = 0;
                                }
                                else if (h % 2 == 1 && h != 0)
                                {
                                    valueZ = (h + 1) / 2;
                                }
                                else
                                {
                                    valueZ = -h / 2;
                                }
                                if (numberInFormation == 0)
                                {
                                    savePosition = new Vector3(oriX + valueX * 0.3f, oriY + valueY * 0.3f, oriZ + valueZ * 0.3f);
                                }
                                if (numberInFormation < numberOfProjectile)
                                {
                                    StartCoroutine(LerpMovement(new Vector3(oriX + valueX * 0.3f, oriY + valueY * 0.3f, oriZ + valueZ * 0.3f), projectileList[numberInFormation]));
                                    numberInFormation++;
                                }
                            }
                        }
                    }
                }
                if (savePosition != null)
                {
                    if (projectileList[0].transform.position == savePosition)
                    {
                        finishFormation = true;
                    }
                }
                if (finishFormation == true && rotationFormationEnabled == true)
                {
                    RotationProjectiles();
                }
            }
        }


        private void FormationCircle()
        {
            speedRotation = 5;
            if (startFormation == true)
            {
                int numI = 0;
                int numJ = 0;
                int[] tab = new int[2];
                if (finishFormation == false)
                {
                    tab = CalculateDimensionsForCircle();
                    numI = tab[0];
                    numJ = tab[1];
                    Debug.Log("FireballHandling : Circle : numI : " + numI.ToString() + "; numJ : " + numJ.ToString() + ";");
                    Debug.Log("FireballHandling : Circle : radiusFormation : " + radiusFormationCircle.ToString() + "; radiusLimit : " + radiusLimit.ToString() + "; radiusProjectile : " + radiusProjectile.ToString() + ";");
                    for (int i = 0; i < numI; i++)
                    {
                        for (int j = 0; j < numJ; j++)
                        {
                            if (i <= 0)
                            {
                                valueX = 0;
                            }
                            else if (i % 2 == 1 && i != 0)
                            {
                                valueX = (i + 1) / 2;
                            }
                            else
                            {
                                valueX = -i / 2;
                            }
                            Vector3 pos = RandomCircle(oriPosition, radiusFormationCircle, numJ, j);
                            pos.x = oriX + valueX * 0.3f;
                            // make the object face the center
                            Quaternion rot = Quaternion.FromToRotation(Vector3.right, oriPosition - pos);
                            if (numberInFormation == 0)
                            {
                                savePosition = pos;
                            }
                            if (numberInFormation < numberOfProjectile)
                            {
                                StartCoroutine(LerpMovement(pos, projectileList[numberInFormation]));
                                numberInFormation++;
                            }
                        }
                    }
                    finishFormation = true;
                }
                if (savePosition != null)
                {
                    if (projectileList[0].transform.position == savePosition)
                    {
                        finishFormation = true;
                    }
                }
                if (finishFormation == true && rotationFormationEnabled == true)
                {
                    RotationProjectiles();
                }
            }
        }
        private void ResetList()
        {
            if (projectileList.Count != 0)
            {
                finishFormation = false;
                startFormation = false;
                numberInFormation = 0;
                speedRotation = 0;
            }
        }

        private void RotationProjectiles()
        {
            foreach (Item projectile in projectileList)
            {
                projectile.transform.RotateAround(oriPosition, rotationAxis, 10 * Time.deltaTime * speedRotation);
                //gameObject.transform.position = oriPosition + Quaternion.AngleAxis(Time.time * speedRotation, rotationAxis) * upAxis * Vector3.Distance(oriPosition, gameObject.transform.position);
            }
        }
        private Vector3 RandomCircle(Vector3 center, float radius, int numMaxObject, int numberObject)
        {
            int ang = 360 / numMaxObject * numberObject;
            Vector3 pos;
            pos.x = center.x;
            pos.y = center.y + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
            pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
            return pos;
        }
        private int[] CalculateDimensionsForCircle()
        {
            if (addRadiusOfProjectileForCircle == true)
            {
                radiusFormationCircle = (numberOfProjectile / 2 / Mathf.PI) * (radiusProjectile * 2);
            }
            else
            {
                radiusFormationCircle = (numberOfProjectile / 2 / Mathf.PI) * radiusProjectile;
            }
            int[] dimensions = new int[2];
            // dimensions[0] = depth
            // dimensions[1] = number in circle

            dimensions[0] = Mathf.FloorToInt(radiusFormationCircle / radiusLimit) + 1;
            dimensions[1] = numberOfProjectile / dimensions[0] + 1;
            radiusFormationCircle /= dimensions[0];

            return dimensions;

        }

        private int[] CalculateDimensionsForCube()
        {
            int[] dimensions = new int[3];
            // dimensions[0] = depth
            // dimensions[1] = column
            // dimensions[2] = line

            if (numberOfProjectile >= 1 && numberOfProjectile <= 6 ||
                numberOfProjectile == 9 ||
                numberOfProjectile >= 10 && numberOfProjectile <= 20)
            {
                dimensions[0] = 1;
            }
            if (numberOfProjectile >= 7 && numberOfProjectile <= 8 ||
                numberOfProjectile >= 21 && numberOfProjectile <= 24 ||
                numberOfProjectile >= 28 && numberOfProjectile <= 32 ||
                numberOfProjectile >= 37 && numberOfProjectile <= 40 ||
                numberOfProjectile >= 49 && numberOfProjectile <= 50)
            {
                dimensions[0] = 2;
            }
            if (numberOfProjectile >= 25 && numberOfProjectile <= 27 ||
                numberOfProjectile >= 33 && numberOfProjectile <= 36 ||
                numberOfProjectile >= 41 && numberOfProjectile <= 48 ||
                numberOfProjectile >= 51 && numberOfProjectile <= 60 ||
                numberOfProjectile >= 65 && numberOfProjectile <= 75)
            {
                dimensions[0] = 3;
            }
            if (numberOfProjectile >= 61 && numberOfProjectile <= 64)
            {
                dimensions[0] = 4;
            }
            if (numberOfProjectile >= 76)
            {
                dimensions[0] = 5;
            }


            if (numberOfProjectile >= 1 && numberOfProjectile <= 3)
            {
                dimensions[1] = 1;
            }
            if (numberOfProjectile >= 4 && numberOfProjectile <= 8)
            {
                dimensions[1] = 2;
            }
            if (numberOfProjectile >= 9 && numberOfProjectile <= 15 ||
                numberOfProjectile >= 21 && numberOfProjectile <= 30 ||
                numberOfProjectile >= 33 && numberOfProjectile <= 36 ||
                numberOfProjectile >= 41 && numberOfProjectile <= 45)
            {
                dimensions[1] = 3;
            }
            if (numberOfProjectile >= 16 && numberOfProjectile <= 20 ||
                numberOfProjectile >= 31 && numberOfProjectile <= 32 ||
                numberOfProjectile >= 37 && numberOfProjectile <= 40 ||
                numberOfProjectile >= 46 && numberOfProjectile <= 48 ||
                numberOfProjectile >= 51 && numberOfProjectile <= 64)
            {
                dimensions[1] = 4;
            }
            if (numberOfProjectile >= 49 && numberOfProjectile <= 50 ||
                numberOfProjectile >= 65 && numberOfProjectile <= 75 ||
                numberOfProjectile >= 76)
            {
                dimensions[1] = 5;
            }


            if (numberOfProjectile == 1)
            {
                dimensions[2] = 1;
            }
            if (numberOfProjectile == 2 ||
                numberOfProjectile == 4 ||
                numberOfProjectile >= 7 && numberOfProjectile <= 8)
            {
                dimensions[2] = 2;
            }
            if (numberOfProjectile == 3 ||
                numberOfProjectile >= 5 && numberOfProjectile <= 6 ||
                numberOfProjectile == 9 ||
                numberOfProjectile >= 25 && numberOfProjectile <= 27)
            {
                dimensions[2] = 3;
            }
            if (numberOfProjectile >= 10 && numberOfProjectile <= 12 ||
                numberOfProjectile == 16 ||
                numberOfProjectile >= 21 && numberOfProjectile <= 24 ||
                numberOfProjectile >= 31 && numberOfProjectile <= 36 ||
                numberOfProjectile >= 46 && numberOfProjectile <= 48 ||
                numberOfProjectile >= 61 && numberOfProjectile <= 64)
            {
                dimensions[2] = 4;
            }
            if (numberOfProjectile >= 13 && numberOfProjectile <= 15 ||
                numberOfProjectile >= 17 && numberOfProjectile <= 20 ||
                numberOfProjectile >= 28 && numberOfProjectile <= 30 ||
                numberOfProjectile >= 37 && numberOfProjectile <= 45 ||
                numberOfProjectile >= 49 && numberOfProjectile <= 60 ||
                numberOfProjectile >= 65 && numberOfProjectile <= 75 ||
                numberOfProjectile >= 76 && numberOfProjectile <= 125)
            {
                dimensions[2] = 5;
            }


            if (numberOfProjectile >= 126 && numberOfProjectile <= 216)
            {
                dimensions[0] = 6;
                dimensions[1] = 6;
                dimensions[2] = 6;
            }
            if (numberOfProjectile >= 217 && numberOfProjectile <= 343)
            {
                dimensions[0] = 7;
                dimensions[1] = 7;
                dimensions[2] = 7;
            }
            if (numberOfProjectile >= 344 && numberOfProjectile <= 512)
            {
                dimensions[0] = 8;
                dimensions[1] = 8;
                dimensions[2] = 8;
            }
            if (numberOfProjectile >= 513 && numberOfProjectile <= 729)
            {
                dimensions[0] = 9;
                dimensions[1] = 9;
                dimensions[2] = 9;
            }
            if (numberOfProjectile >= 730 && numberOfProjectile <= 1000)
            {
                dimensions[0] = 10;
                dimensions[1] = 10;
                dimensions[2] = 10;
            }
            return dimensions;
        }

        private IEnumerator LerpMovement(Vector3 positionToReach, Item itemToMove)
        {
            //Debug.Log("FireballHandling : Layer : " + itemToMove.gameObject.layer.ToString());
            foreach (ColliderGroup colliderGroup in itemToMove.colliderGroups)
            {
                foreach (Collider collider in colliderGroup.colliders)
                {
                    collider.enabled = false;
                }
            }
            float time = 0;
            Vector3 positionOrigin = itemToMove.rb.position;
            if (positionToReach != positionOrigin)
            {
                while (time < durationOfMvt)
                {
                    itemToMove.rb.position = Vector3.Lerp(positionOrigin, positionToReach, time / durationOfMvt);
                    time += Time.deltaTime;
                    yield return null;
                }
            }
            itemToMove.rb.position = positionToReach;
            foreach (ColliderGroup colliderGroup in itemToMove.colliderGroups)
            {
                foreach (Collider collider in colliderGroup.colliders)
                {
                    collider.enabled = true;
                }
            }
        }
        /*IEnumerator ManagedRemoveList()
        {
            if (projectileList.Count != 0)
            {
                foreach (Item elements in projectileList)
                {
                    if (elements.rb.detectCollisions == true)
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