using System.Collections.Generic;
using UnityEngine;

namespace NooMiBetter
{
    public sealed class RagdollController : MonoBehaviour
    {
        public Rigidbody Torso { get; private set; }
        public Rigidbody LeftHand { get; private set; }
        public Rigidbody RightHand { get; private set; }
        public bool IsAttached { get; private set; }
        private Rigidbody barAnchor;
        private readonly List<Rigidbody> parts = new List<Rigidbody>();
        private readonly List<ConfigurableJoint> handJoints = new List<ConfigurableJoint>();

        public void Build(Vector3 origin, Transform parent, Material material)
        {
            if (Torso != null) return;
            var anchorObject = new GameObject("Bar Anchor Body");
            anchorObject.transform.position = new Vector3(0, 1.45f, 0);
            anchorObject.transform.SetParent(parent, true);
            barAnchor = anchorObject.AddComponent<Rigidbody>();
            barAnchor.isKinematic = true;
            Torso = Part("Torso", PrimitiveType.Capsule, origin, new Vector3(.34f,.52f,.24f), 2.2f, material);
            var head = Part("Head", PrimitiveType.Sphere, origin + Vector3.up*.72f, new Vector3(.25f,.25f,.25f), .45f, material);
            Link(Torso, head, origin + Vector3.up*.48f, new Vector3(25,25,25));
            var leftUpperArm = Part("Left Upper Arm", PrimitiveType.Capsule, origin + new Vector3(-.38f,.25f,0), new Vector3(.16f,.36f,.16f), .55f, material);
            var leftForearm = Part("Left Forearm", PrimitiveType.Capsule, origin + new Vector3(-.48f,.68f,0), new Vector3(.13f,.32f,.13f), .4f, material);
            LeftHand = Part("Left Hand", PrimitiveType.Sphere, origin + new Vector3(-.5f,1.02f,0), new Vector3(.17f,.17f,.17f), .2f, material);
            var rightUpperArm = Part("Right Upper Arm", PrimitiveType.Capsule, origin + new Vector3(.38f,.25f,0), new Vector3(.16f,.36f,.16f), .55f, material);
            var rightForearm = Part("Right Forearm", PrimitiveType.Capsule, origin + new Vector3(.48f,.68f,0), new Vector3(.13f,.32f,.13f), .4f, material);
            RightHand = Part("Right Hand", PrimitiveType.Sphere, origin + new Vector3(.5f,1.02f,0), new Vector3(.17f,.17f,.17f), .2f, material);
            Link(Torso,leftUpperArm,origin+new Vector3(-.25f,.43f,0),new Vector3(80,80,45));
            Link(leftUpperArm,leftForearm,origin+new Vector3(-.43f,.48f,0),new Vector3(110,10,10));
            Link(leftForearm,LeftHand,origin+new Vector3(-.5f,.88f,0),new Vector3(60,60,60));
            Link(Torso,rightUpperArm,origin+new Vector3(.25f,.43f,0),new Vector3(80,80,45));
            Link(rightUpperArm,rightForearm,origin+new Vector3(.43f,.48f,0),new Vector3(110,10,10));
            Link(rightForearm,RightHand,origin+new Vector3(.5f,.88f,0),new Vector3(60,60,60));
            var leftThigh=Part("Left Thigh",PrimitiveType.Capsule,origin+new Vector3(-.16f,-.55f,0),new Vector3(.2f,.42f,.2f),.9f,material);
            var leftShin=Part("Left Shin",PrimitiveType.Capsule,origin+new Vector3(-.16f,-1.02f,0),new Vector3(.15f,.42f,.15f),.65f,material);
            var rightThigh=Part("Right Thigh",PrimitiveType.Capsule,origin+new Vector3(.16f,-.55f,0),new Vector3(.2f,.42f,.2f),.9f,material);
            var rightShin=Part("Right Shin",PrimitiveType.Capsule,origin+new Vector3(.16f,-1.02f,0),new Vector3(.15f,.42f,.15f),.65f,material);
            Link(Torso,leftThigh,origin+new Vector3(-.16f,-.28f,0),new Vector3(55,45,30));
            Link(leftThigh,leftShin,origin+new Vector3(-.16f,-.78f,0),new Vector3(120,10,10));
            Link(Torso,rightThigh,origin+new Vector3(.16f,-.28f,0),new Vector3(55,45,30));
            Link(rightThigh,rightShin,origin+new Vector3(.16f,-.78f,0),new Vector3(120,10,10));
            AttachHands();
        }

        private Rigidbody Part(string name, PrimitiveType type, Vector3 position, Vector3 scale, float mass, Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=position;go.transform.localScale=scale;
            go.GetComponent<Renderer>().material=material;
            var rb=go.AddComponent<Rigidbody>();rb.mass=mass;rb.interpolation=RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode=CollisionDetectionMode.Continuous;
            parts.Add(rb);return rb;
        }
        private void Link(Rigidbody a,Rigidbody b,Vector3 worldAnchor,Vector3 limits)
        {
            var joint=b.gameObject.AddComponent<ConfigurableJoint>();joint.connectedBody=a;joint.autoConfigureConnectedAnchor=false;
            joint.anchor=b.transform.InverseTransformPoint(worldAnchor);joint.connectedAnchor=a.transform.InverseTransformPoint(worldAnchor);
            joint.xMotion=joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Limited;
            var angular=new SoftJointLimit{limit=limits.x};joint.lowAngularXLimit=angular;joint.highAngularXLimit=angular;
            joint.angularYLimit=new SoftJointLimit{limit=limits.y};joint.angularZLimit=new SoftJointLimit{limit=limits.z};
            joint.enablePreprocessing=false;
        }
        private void AttachHands()
        {
            Attach(LeftHand);Attach(RightHand);IsAttached=true;
        }
        private void Attach(Rigidbody hand)
        {
            var joint=hand.gameObject.AddComponent<ConfigurableJoint>();joint.connectedBody=barAnchor;
            joint.autoConfigureConnectedAnchor=false;joint.anchor=Vector3.zero;
            joint.connectedAnchor=hand.transform.position-barAnchor.position;
            joint.xMotion=joint.yMotion=joint.zMotion=ConfigurableJointMotion.Limited;
            joint.linearLimit=new SoftJointLimit{limit=.12f};
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Free;
            handJoints.Add(joint);
        }
        public void Release()
        {
            foreach(var joint in handJoints) if(joint) Destroy(joint);
            handJoints.Clear();IsAttached=false;
        }
        public void Grab()
        {
            if(IsAttached)return;
            AttachHands();
            foreach(var rb in parts){rb.velocity=Vector3.zero;rb.angularVelocity=Vector3.zero;}
        }
        public void AddTrickImpulse(float torque)
        {
            foreach(var rb in parts) rb.AddTorque(Vector3.forward*torque*(rb==Torso?1f:.15f),ForceMode.Impulse);
            Torso.AddForce(Vector3.up*1.8f,ForceMode.Impulse);
        }
        public void ResetBody(Vector3 origin)
        {
            Release();
            foreach(var rb in parts){rb.velocity=Vector3.zero;rb.angularVelocity=Vector3.zero;}
            Torso.position=origin;Torso.rotation=Quaternion.identity;
            // Reposition each limb relative to torso after removing joint forces.
            foreach(var rb in parts) {
                if(rb==Torso) continue;
                Vector3 offset=rb.transform.position - Torso.position;
                rb.position=Torso.position+offset;
            }
            Grab();
        }
        public float LowestY
        {
            get { float y=float.MaxValue;foreach(var rb in parts) y=Mathf.Min(y,rb.position.y);return y; }
        }
        public Vector3 Position { get { return Torso ? Torso.position : Vector3.zero; } }
    }
}
